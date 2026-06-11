using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    // SCRIPT DEFENSE — Étape 4 : Executer — transaction atomique (vérif + INSERT + FIFO + bom_stocks)
    //                  Étape 4 : ConsumeStock — algorithme FIFO (lots triés par date_achat ASC)
    //                  Étape 4 : Simuler — calcul disponibilité avec jauges visuelles

    /// <summary>
    /// Moteur de production BOM (Bill of Materials — nomenclature de fabrication).
    /// C'est LE fichier central de la logique métier production.
    /// Il gère :
    /// - La lecture des productions passées (GetByNiveau, GetRecent, GetDuJour)
    /// - La vérification de disponibilité du stock avant production
    /// - La simulation complète (pour les jauges visuelles dans l'UI)
    /// - L'exécution atomique d'une production (vérif + INSERT + FIFO + stock)
    /// Tout passe par des requêtes paramétrées — jamais de concaténation SQL.
    /// </summary>
    public static class BomProductionDAL
    {
        /// <summary>
        /// Tolérance d'arrondi pour la consommation FIFO — j'utilise 0.0001
        /// pour éviter les faux négatifs quand il reste un tout petit résidu flottant
        /// (ex: 0.00000001 après des divisions successives). Sans ça, le guard
        /// "stock insuffisant" se déclencherait alors qu'on a bien tout consommé.
        /// </summary>
        private const decimal TOLERANCE_ARRONDI = 0.0001m;

        // GetByNiveau() — récupère toutes les productions d'un niveau BOM donné
        // Jointures sur bom_fiches, bom_niveaux et bom_contextes pour avoir les noms
        // Trié par date desc (les plus récentes en premier)
        public static List<BomProduction> GetByNiveau(int idNiveau)
        {
            var list = new List<BomProduction>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT p.id, p.id_niveau, p.id_fiche, p.quantite_produite,
                           p.cout_ingredients, p.cout_unitaire, p.date_production, p.notes,
                           f.nom AS nom_fiche, f.unite_output, f.quantite_output,
                           n.nom AS nom_niveau, n.ordre,
                           c.nom AS nom_contexte
                    FROM bom_productions p
                    INNER JOIN bom_fiches    f ON f.id = p.id_fiche
                    INNER JOIN bom_niveaux   n ON n.id = p.id_niveau
                    INNER JOIN bom_contextes c ON c.id = n.id_contexte
                    WHERE p.id_niveau = @idNiveau
                    ORDER BY p.date_production DESC";
                cmd.Parameters.AddWithValue("@idNiveau", idNiveau);
                // MapHeader() transforme chaque ligne SQL en objet BomProduction C#
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapHeader(r));
            }
            return list;
        }

        // GetRecentByActivite() — récupère les N dernières productions d'une activité
        // Utilisé sur le Hub pour afficher les productions récentes toutes fiches confondues
        // Le paramètre limit (par défaut 10) contrôle combien on ramène
        public static List<BomProduction> GetRecentByActivite(int idActivite, int limit = 10)
        {
            var list = new List<BomProduction>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT p.id, p.id_niveau, p.id_fiche, p.quantite_produite,
                           p.cout_ingredients, p.cout_unitaire, p.date_production, p.notes,
                           f.nom AS nom_fiche, f.unite_output, f.quantite_output,
                           n.nom AS nom_niveau, n.ordre,
                           c.nom AS nom_contexte
                    FROM bom_productions p
                    INNER JOIN bom_fiches    f ON f.id = p.id_fiche
                    INNER JOIN bom_niveaux   n ON n.id = p.id_niveau
                    INNER JOIN bom_contextes c ON c.id = n.id_contexte
                    WHERE c.id_activite = @idActivite
                    ORDER BY p.date_production DESC
                    LIMIT @limit";
                cmd.Parameters.AddWithValue("@idActivite", idActivite);
                cmd.Parameters.AddWithValue("@limit",      limit);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapHeader(r));
            }
            return list;
        }

        /// <summary>
        /// GetDuJourByActivite() — retourne les productions du jour pour une activité.
        /// Filtre sur date_production = CURDATE() côté MySQL (pas côté C#, pour éviter
        /// les décalages de timezone entre le serveur MySQL et le poste client).
        /// Utilisé sur le Hub dans le bloc "Productions du jour".
        /// </summary>
        public static List<BomProduction> GetDuJourByActivite(int idActivite)
        {
            var list = new List<BomProduction>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT p.id, p.id_niveau, p.id_fiche, p.quantite_produite,
                           p.cout_ingredients, p.cout_unitaire, p.date_production, p.notes,
                           f.nom AS nom_fiche, f.unite_output, f.quantite_output,
                           n.nom AS nom_niveau, n.ordre,
                           c.nom AS nom_contexte
                    FROM bom_productions p
                    INNER JOIN bom_fiches    f ON f.id = p.id_fiche
                    INNER JOIN bom_niveaux   n ON n.id = p.id_niveau
                    INNER JOIN bom_contextes c ON c.id = n.id_contexte
                    WHERE c.id_activite = @idActivite
                      AND DATE(p.date_production) = CURDATE()
                    ORDER BY p.date_production DESC";
                cmd.Parameters.AddWithValue("@idActivite", idActivite);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapHeader(r));
            }
            return list;
        }

        // ── Vérification de disponibilité ────────────────────────────────

        /// <summary>
        /// VerifierDisponibilite() — vérifie si le stock N-1 est suffisant pour produire
        /// quantiteCible batches de la fiche dans le niveau donné.
        /// Retourne une liste vide si tout est OK, ou une liste de BomManque décrivant
        /// chaque ingrédient/produit intermédiaire en pénurie.
        /// Version publique : charge le niveau et la fiche elle-même depuis le DAL.
        /// </summary>
        public static List<BomManque> VerifierDisponibilite(int idNiveau, int idFiche, decimal quantiteCible)
        {
            var niveau = BomNiveauDAL.GetById(idNiveau);
            var fiche  = BomFicheDAL.GetById(idFiche);
            if (niveau == null || fiche == null) return new List<BomManque>();
            return VerifierDisponibiliteLignes(fiche.Lignes, quantiteCible);
        }

        /// <summary>
        /// Surcharge interne — opère sur les lignes déjà chargées de la fiche,
        /// évitant le double chargement niveau/fiche. Utilisée par Executer()
        /// pour travailler dans la même transaction logique.
        /// Version SANS transaction : lit le stock avec des connexions séparées.
        /// </summary>
        private static List<BomManque> VerifierDisponibiliteLignes(
            List<BomFicheLigne> lignes, decimal quantiteCible)
        {
            var manques = new List<BomManque>();
            // multiplicateur = nombre de batches demandés
            decimal multiplicateur = quantiteCible;

            // Je parcours chaque ligne de la fiche (chaque ingrédient ou produit intermédiaire)
            foreach (var ligne in lignes)
            {
                // qteNecessaire = quantité par batch × nombre de batches
                decimal qteNecessaire = ligne.Quantite * multiplicateur;
                decimal qteDisponible;

                if (ligne.TypeInput == BomFiche.TypeInputIngredient)
                {
                    // Conversion de l'unité de la recette vers l'unité de base du stock
                    // Ex: la recette dit 500g mais le stock est en kg → je convertis
                    decimal qteNecessaireBase = UnitConvertisseur.Convertir(
                        qteNecessaire, ligne.UniteMesure, ligne.UniteMesureInput);
                    // Lecture du stock disponible pour cet ingrédient (tous lots confondus)
                    qteDisponible = BomStockDAL.GetDisponibleIngredient(ligne.IdInputIngredient.Value);

                    // Si pas assez en stock, j'ajoute un BomManque à la liste
                    if (qteDisponible < qteNecessaireBase)
                        manques.Add(new BomManque
                        {
                            NomInput           = ligne.NomInput,
                            Unite              = ligne.UniteMesureInput,
                            QuantiteNecessaire = qteNecessaireBase,
                            QuantiteDisponible = qteDisponible
                        });
                }
                else
                {
                    // Type "fiche" — c'est un produit intermédiaire (semi-fini d'un niveau inférieur)
                    // Je dois trouver dans quel niveau cette fiche source a été produite
                    int idNiveauSource = GetIdNiveauDeFiche(ligne.IdInputFiche.Value);
                    qteDisponible = idNiveauSource > 0
                        ? BomStockDAL.GetDisponible(idNiveauSource, ligne.IdInputFiche.Value)
                        : 0;

                    decimal qteNecessaireConv = UnitConvertisseur.Convertir(
                        qteNecessaire, ligne.UniteMesure, ligne.UniteMesureInput);

                    if (qteDisponible < qteNecessaireConv)
                        manques.Add(new BomManque
                        {
                            NomInput           = ligne.NomInput,
                            Unite              = ligne.UniteMesureInput,
                            QuantiteNecessaire = qteNecessaireConv,
                            QuantiteDisponible = qteDisponible
                        });
                }
            }
            return manques;
        }

        /// <summary>
        /// Surcharge transactionnelle — toutes les lectures de stock passent par conn/tx
        /// avec verrou pessimiste (FOR UPDATE dans BomStockDAL), éliminant la race condition
        /// TOCTOU (Time Of Check To Time Of Use — quand un autre process modifie le stock
        /// entre ma vérification et ma consommation).
        /// TICKET-01 FIX : c'est cette version qui est utilisée dans Executer().
        /// </summary>
        private static List<BomManque> VerifierDisponibiliteLignes(
            List<BomFicheLigne> lignes, decimal quantiteCible,
            MySqlConnection conn, MySqlTransaction tx)
        {
            var manques = new List<BomManque>();
            decimal multiplicateur = quantiteCible;

            // Même logique que la version sans transaction, mais avec conn/tx en plus
            // pour que les SELECT se fassent dans la même transaction que les UPDATE
            foreach (var ligne in lignes)
            {
                decimal qteNecessaire = ligne.Quantite * multiplicateur;
                decimal qteDisponible;

                if (ligne.TypeInput == BomFiche.TypeInputIngredient)
                {
                    decimal qteNecessaireBase = UnitConvertisseur.Convertir(
                        qteNecessaire, ligne.UniteMesure, ligne.UniteMesureInput);
                    // Lecture avec FOR UPDATE — verrouille les lignes lues jusqu'au COMMIT
                    qteDisponible = BomStockDAL.GetDisponibleIngredient(ligne.IdInputIngredient.Value, conn, tx);

                    if (qteDisponible < qteNecessaireBase)
                        manques.Add(new BomManque
                        {
                            NomInput           = ligne.NomInput,
                            Unite              = ligne.UniteMesureInput,
                            QuantiteNecessaire = qteNecessaireBase,
                            QuantiteDisponible = qteDisponible
                        });
                }
                else
                {
                    int idNiveauSource = GetIdNiveauDeFiche(ligne.IdInputFiche.Value, conn, tx);
                    qteDisponible = idNiveauSource > 0
                        ? BomStockDAL.GetDisponible(idNiveauSource, ligne.IdInputFiche.Value, conn, tx)
                        : 0;

                    decimal qteNecessaireConv = UnitConvertisseur.Convertir(
                        qteNecessaire, ligne.UniteMesure, ligne.UniteMesureInput);

                    if (qteDisponible < qteNecessaireConv)
                        manques.Add(new BomManque
                        {
                            NomInput           = ligne.NomInput,
                            Unite              = ligne.UniteMesureInput,
                            QuantiteNecessaire = qteNecessaireConv,
                            QuantiteDisponible = qteDisponible
                        });
                }
            }
            return manques;
        }

        // ── Simulation complète ───────────────────────────────────────────

        /// <summary>
        /// Simuler() — retourne TOUTES les lignes requises pour la production,
        /// pas seulement les pénuries. Chaque BomManque contient qté nécessaire
        /// ET qté disponible, ce qui permet à l'UI d'afficher des jauges colorées :
        /// - Vert : stock suffisant (QuantiteDisponible >= QuantiteNecessaire)
        /// - Rouge : pénurie (QuantiteDisponible < QuantiteNecessaire)
        /// Utilisé par FrmBomProductionSimulation pour le tableau de pré-production.
        /// </summary>
        public static List<BomManque> Simuler(int idNiveau, int idFiche, decimal quantiteCible)
        {
            var niveau = BomNiveauDAL.GetById(idNiveau);
            var fiche  = BomFicheDAL.GetById(idFiche);
            if (niveau == null || fiche == null) return new List<BomManque>();
            return SimulerLignes(fiche.Lignes, quantiteCible);
        }

        /// <summary>
        /// SimulerLignes() — logique interne de simulation sur des lignes déjà chargées.
        /// La différence avec VerifierDisponibiliteLignes : ici je retourne TOUTES les lignes,
        /// même celles où le stock est suffisant. Le champ Manque est calculé par le modèle
        /// BomManque (propriété calculée = max(0, Nécessaire - Disponible)).
        /// </summary>
        private static List<BomManque> SimulerLignes(List<BomFicheLigne> lignes, decimal quantiteCible)
        {
            var resultat = new List<BomManque>();
            decimal multiplicateur = quantiteCible;

            foreach (var ligne in lignes)
            {
                decimal qteNecessaire = ligne.Quantite * multiplicateur;
                decimal qteDisponible;

                // Conversion vers unité native du stock — commune aux deux types (ingredient/fiche)
                decimal qteNecessaireConv = UnitConvertisseur.Convertir(
                    qteNecessaire, ligne.UniteMesure, ligne.UniteMesureInput);

                if (ligne.TypeInput == BomFiche.TypeInputIngredient)
                {
                    // Lecture du stock ingrédient (somme de tous les lots disponibles)
                    qteDisponible = BomStockDAL.GetDisponibleIngredient(ligne.IdInputIngredient.Value);
                }
                else
                {
                    // Lecture du stock produit intermédiaire (bom_stocks du niveau source)
                    int idNiveauSource = GetIdNiveauDeFiche(ligne.IdInputFiche.Value);
                    qteDisponible = idNiveauSource > 0
                        ? BomStockDAL.GetDisponible(idNiveauSource, ligne.IdInputFiche.Value)
                        : 0;
                }

                // J'ajoute la ligne au résultat — même si le stock est suffisant
                // Le modèle BomManque calcule automatiquement le delta
                resultat.Add(new BomManque
                {
                    NomInput           = ligne.NomInput,
                    Unite              = ligne.UniteMesureInput,
                    QuantiteNecessaire = qteNecessaireConv,
                    QuantiteDisponible = qteDisponible
                });
            }
            return resultat;
        }

        // ── Exécution de production ───────────────────────────────────────

        /// <summary>
        /// Executer() — LE GROS MORCEAU. Exécute une production dans une transaction MySQL atomique.
        /// Les 4 étapes dans la transaction :
        ///   1. Vérifie la disponibilité (avec FOR UPDATE pour verrouiller les stocks)
        ///   2. Insère dans bom_productions (l'enregistrement de production)
        ///   3. Consomme le stock N-1 en FIFO via ConsumeStock() + insère bom_productions_lignes
        ///   4. Crée l'entrée de stock N dans bom_stocks (le produit fini est dispo)
        /// Si quoi que ce soit échoue → ROLLBACK complet, rien n'est modifié.
        /// Retourne l'id de la production créée.
        /// </summary>
        public static int Executer(int idNiveau, int idFiche, decimal quantiteCible, string notes = null, int delaiConservationJours = 0)
        {
            using (var conn = DbHelper.GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    // Charger niveau et fiche UNE SEULE FOIS — évite de refaire 2 requêtes
                    // dans VerifierDisponibilite + ConsumeStock
                    var niveau = BomNiveauDAL.GetById(idNiveau, conn, tx);
                    var fiche  = BomFicheDAL.GetById(idFiche, conn, tx);

                    // Guards : si le niveau ou la fiche ont été supprimés entre-temps
                    if (niveau == null)
                        throw new InvalidOperationException($"Le niveau BOM (id={idNiveau}) n'existe plus — production annulée.");
                    if (fiche == null)
                        throw new InvalidOperationException($"La fiche BOM (id={idFiche}) n'existe plus — production annulée.");

                    // TICKET-01 FIX COMPLET : vérification + lectures de stock dans la même
                    // transaction avec FOR UPDATE — élimine la race condition TOCTOU
                    // (un autre utilisateur ne peut pas consommer le stock entre ma vérif et mon UPDATE)
                    var manques = VerifierDisponibiliteLignes(fiche.Lignes, quantiteCible, conn, tx);
                    if (manques.Count > 0)
                    {
                        var details = string.Join("\n", manques);
                        throw new InvalidOperationException(
                            $"Stock insuffisant pour lancer la production :\n{details}");
                    }

                    // quantiteCible = nombre de batches demandés
                    // qteProduite = batches × QuantiteOutput de la fiche
                    // Ex: 2 batches de "Tablette 100g" (QuantiteOutput=10) → 20 tablettes produites
                    decimal multiplicateur = quantiteCible;
                    decimal qteProduite    = quantiteCible * fiche.QuantiteOutput;

                    // Accumulateur du coût total des ingrédients consommés
                    decimal coutTotalIngredients = 0;

                    // ÉTAPE 1 : Insérer l'enregistrement de production (cout à 0 pour l'instant)
                    // Je mettrai à jour le coût après avoir consommé tous les ingrédients
                    int idProduction;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            INSERT INTO bom_productions
                                (id_niveau, id_fiche, quantite_produite, cout_ingredients, cout_unitaire, notes)
                            VALUES (@idNiv, @idFiche, @qte, 0, 0, @notes)";
                        cmd.Parameters.AddWithValue("@idNiv",  idNiveau);
                        cmd.Parameters.AddWithValue("@idFiche", idFiche);
                        cmd.Parameters.AddWithValue("@qte",    qteProduite);   // batches × QuantiteOutput
                        cmd.Parameters.AddWithValue("@notes",  notes ?? (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                        // LastInsertedId = l'AUTO_INCREMENT de MySQL pour cette insertion
                        idProduction = (int)cmd.LastInsertedId;
                    }

                    // ÉTAPE 2 : Consommer le stock N-1 pour chaque ligne de la fiche
                    // ConsumeStock fait le FIFO : il prend les lots les plus anciens d'abord
                    // et retourne le coût total de ce qu'il a consommé pour cette ligne
                    foreach (var ligne in fiche.Lignes)
                    {
                        decimal aConommer = ligne.Quantite * multiplicateur;
                        coutTotalIngredients += ConsumeStock(conn, tx, ligne, aConommer,
                                                             idProduction, niveau);
                    }

                    // ÉTAPE 3 : Mettre à jour cout_ingredients et cout_unitaire dans bom_productions
                    // coutUnitaire = coût par unité produite (ex: coût par tablette de chocolat)
                    decimal coutUnitaire = qteProduite > 0 ? coutTotalIngredients / qteProduite : 0;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            UPDATE bom_productions
                            SET cout_ingredients=@cout, cout_unitaire=@coutUnit
                            WHERE id=@id";
                        cmd.Parameters.AddWithValue("@cout",     coutTotalIngredients);
                        cmd.Parameters.AddWithValue("@coutUnit", coutUnitaire);
                        cmd.Parameters.AddWithValue("@id",       idProduction);
                        cmd.ExecuteNonQuery();
                    }

                    // ÉTAPE 4 : Créer l'entrée bom_stocks pour le niveau N
                    // C'est ce qui rend le produit fini disponible en stock
                    // id_contexte et id_activite déduits depuis le niveau (jamais de saisie manuelle)
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        // DLC (Date Limite de Consommation) = date production + délai en jours
                        // Si delaiConservationJours = 0, DLC = NULL (pas de péremption)
                        string sqlDlc = delaiConservationJours > 0
                            ? "DATE_ADD(CURDATE(), INTERVAL @delaiJours DAY)"
                            : "NULL";

                        cmd.CommandText = $@"
                            INSERT INTO bom_stocks
                                (id_niveau, id_contexte, id_activite, id_fiche, id_production,
                                 quantite_disponible, cout_unitaire, date_production, date_dlc)
                            VALUES (@idNiv, @idCtx, @idAct, @idFiche, @idProd, @qte, @coutUnit, CURDATE(), {sqlDlc})";
                        cmd.Parameters.AddWithValue("@idNiv",    idNiveau);
                        if (delaiConservationJours > 0)
                            cmd.Parameters.AddWithValue("@delaiJours", delaiConservationJours);
                        cmd.Parameters.AddWithValue("@idCtx",    niveau.IdContexte);
                        cmd.Parameters.AddWithValue("@idAct",    niveau.IdActivite);
                        cmd.Parameters.AddWithValue("@idFiche",  idFiche);
                        cmd.Parameters.AddWithValue("@idProd",   idProduction);
                        cmd.Parameters.AddWithValue("@qte",      qteProduite);   // quantité réelle en stock
                        cmd.Parameters.AddWithValue("@coutUnit", coutUnitaire);
                        cmd.ExecuteNonQuery();
                    }

                    // Tout s'est bien passé — je commit la transaction
                    tx.Commit();
                    return idProduction;
                }
                catch
                {
                    // N'importe quelle erreur → ROLLBACK complet, rien n'est modifié en base
                    tx.Rollback();
                    throw;  // Je relance l'exception pour que l'UI puisse afficher le message
                }
            }
        }

        // ── Helpers privés ────────────────────────────────────────────────

        /// <summary>
        /// ConsumeStock() — consomme aConommer unités de stock pour une ligne de fiche, en FIFO
        /// (First In First Out — les lots les plus anciens sont consommés en premier).
        /// Pour chaque lot consommé, j'insère une ligne de traçabilité dans bom_productions_lignes.
        /// Retourne le coût total consommé pour cette ligne (somme de qté × prix unitaire par lot).
        /// C'est ici que la magie FIFO se passe — le coeur du moteur de production.
        /// </summary>
        private static decimal ConsumeStock(MySqlConnection conn, MySqlTransaction tx,
                                             BomFicheLigne ligne, decimal aConommer,
                                             int idProduction, BomNiveau niveau)
        {
            decimal coutLigne = 0;

            // Convertir aConommer (dans ligne.UniteMesure, ex: "g") vers l'unité native du stock
            // (ligne.UniteMesureInput, ex: "kg"). Les stocks sont TOUJOURS dans UniteMesureInput.
            // Pour ingredient : UniteMesureInput = unité de base du lot (g, ml, piece)
            // Pour fiche      : UniteMesureInput = UniteOutput de la fiche source (ex: kg)
            decimal restant = UnitConvertisseur.Convertir(
                aConommer, ligne.UniteMesure, ligne.UniteMesureInput);

            if (ligne.TypeInput == BomFiche.TypeInputIngredient)
            {
                // FIFO : je récupère les lots triés par date_achat ASC (les plus anciens d'abord)
                var lots = BomStockDAL.GetLotsDispoFIFO(ligne.IdInputIngredient.Value, conn, tx);

                // Je parcours les lots et je prends ce dont j'ai besoin dans chacun
                foreach (var (idLot, dispo, prixUnit) in lots)
                {
                    if (restant <= 0) break;  // J'ai tout ce qu'il me faut
                    decimal pris = Math.Min(restant, dispo);  // Je prends au max ce qui est dispo

                    // Décrémenter la quantité disponible du lot
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            UPDATE lots_ingredients
                            SET quantite_disponible = quantite_disponible - @pris
                            WHERE id = @id";
                        cmd.Parameters.AddWithValue("@pris", pris);
                        cmd.Parameters.AddWithValue("@id",   idLot);
                        cmd.ExecuteNonQuery();
                    }

                    // TICKET-08 : libérer les réservations actives sur ce lot
                    // (dans la même transaction pour la cohérence)
                    // Les réservations servaient à "bloquer" du stock avant la production effective
                    using (var cmdRes = conn.CreateCommand())
                    {
                        cmdRes.Transaction = tx;
                        cmdRes.CommandText = @"
                            UPDATE bom_reservations
                            SET actif = 0
                            WHERE id_lot = @idLot AND actif = 1";
                        cmdRes.Parameters.AddWithValue("@idLot", idLot);
                        cmdRes.ExecuteNonQuery();
                    }

                    // InsertLigne() — traçabilité : j'enregistre quel lot a fourni combien
                    InsertLigne(conn, tx, idProduction, BomProductionLigne.SourceLotIngredient, idLot, null, pris, prixUnit);

                    // Accumulation du coût : quantité prise × prix unitaire du lot
                    coutLigne += pris * prixUnit;
                    restant   -= pris;
                }

                // Guard final : si après avoir épuisé tous les lots il reste encore du besoin,
                // c'est une incohérence — le stock a été modifié entre la vérif et la conso
                // (ne devrait pas arriver grâce au FOR UPDATE, mais on sécurise)
                if (restant > TOLERANCE_ARRONDI)
                    throw new InvalidOperationException(
                        $"Stock insuffisant pour « {ligne.NomInput} » : " +
                        $"il manque {restant:F4} {ligne.UniteMesureInput} après épuisement FIFO.");
            }
            else
            {
                // Type "fiche" — consommation de produits intermédiaires (bom_stocks du niveau source)
                // Même logique FIFO mais sur la table bom_stocks au lieu de lots_ingredients
                int idNiveauSource = GetIdNiveauDeFiche(ligne.IdInputFiche.Value, conn, tx);
                var stocks = BomStockDAL.GetBomStocksFIFO(idNiveauSource, ligne.IdInputFiche.Value, conn, tx);

                foreach (var (idStock, dispo, coutUnit) in stocks)
                {
                    if (restant <= 0) break;
                    decimal pris = Math.Min(restant, dispo);

                    // Décrémenter le stock du produit intermédiaire
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            UPDATE bom_stocks
                            SET quantite_disponible = quantite_disponible - @pris
                            WHERE id = @id";
                        cmd.Parameters.AddWithValue("@pris", pris);
                        cmd.Parameters.AddWithValue("@id",   idStock);
                        cmd.ExecuteNonQuery();
                    }

                    // Traçabilité : quel stock intermédiaire a fourni combien
                    InsertLigne(conn, tx, idProduction, BomProductionLigne.SourceBomStock, null, idStock, pris, coutUnit);

                    coutLigne += pris * coutUnit;
                    restant   -= pris;
                }

                // Guard : idem pour les produits intermédiaires
                if (restant > TOLERANCE_ARRONDI)
                    throw new InvalidOperationException(
                        $"Stock insuffisant pour « {ligne.NomInput} » : " +
                        $"il manque {restant:F4} {ligne.UniteMesureInput} après épuisement FIFO.");
            }
            return coutLigne;
        }

        // InsertLigne() — insère une ligne de traçabilité dans bom_productions_lignes
        // Chaque ligne dit : "pour cette production, j'ai pris X unités du lot/stock Y au prix Z"
        // typeSource = BomProductionLigne.SourceLotIngredient ou .SourceBomStock selon la source
        private static void InsertLigne(MySqlConnection conn, MySqlTransaction tx,
                                         int idProduction, string typeSource,
                                         int? idLot, int? idStock,
                                         decimal quantite, decimal coutUnit)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO bom_productions_lignes
                        (id_production, type_source, id_lot_ingredient, id_bom_stock,
                         quantite_consommee, cout_unitaire_moment)
                    VALUES (@idProd, @type, @idLot, @idStock, @qte, @cout)";
                cmd.Parameters.AddWithValue("@idProd",  idProduction);
                cmd.Parameters.AddWithValue("@type",    typeSource);
                // Un des deux est NULL selon le type : lot OU stock, jamais les deux
                cmd.Parameters.AddWithValue("@idLot",   idLot.HasValue   ? (object)idLot.Value   : DBNull.Value);
                cmd.Parameters.AddWithValue("@idStock", idStock.HasValue ? (object)idStock.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@qte",     quantite);
                cmd.Parameters.AddWithValue("@cout",    coutUnit);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// GetIdNiveauDeFiche() — retourne l'id du niveau auquel appartient une fiche donnée.
        /// J'en ai besoin pour savoir dans quel niveau chercher le stock d'un produit intermédiaire.
        /// Un niveau N peut référencer n'importe quel niveau inférieur, pas seulement N-1.
        /// Retourne 0 si la fiche n'existe pas.
        /// Version sans transaction — crée sa propre connexion.
        /// </summary>
        private static int GetIdNiveauDeFiche(int idFiche)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id_niveau FROM bom_fiches WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", idFiche);
                var res = cmd.ExecuteScalar();
                return res == null ? 0 : Convert.ToInt32(res);
            }
        }

        /// <summary>
        /// Surcharge transactionnelle de GetIdNiveauDeFiche — utilise la connexion existante
        /// pour rester dans la même transaction MySQL (important pour la cohérence FIFO).
        /// </summary>
        private static int GetIdNiveauDeFiche(int idFiche, MySqlConnection conn, MySqlTransaction tx)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "SELECT id_niveau FROM bom_fiches WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", idFiche);
                var res = cmd.ExecuteScalar();
                return res == null ? 0 : Convert.ToInt32(res);
            }
        }

        // MapHeader() — transforme une ligne du MySqlDataReader en objet BomProduction C#
        // Mapping colonne par colonne avec gestion des DBNull pour les champs optionnels
        // Utilisé par tous les Get*() qui font des SELECT avec jointures
        private static BomProduction MapHeader(MySqlDataReader r) => new BomProduction
        {
            Id                  = (int)r["id"],
            IdNiveau            = (int)r["id_niveau"],
            IdFiche             = (int)r["id_fiche"],
            QuantiteProduite    = (decimal)r["quantite_produite"],
            CoutIngredients     = (decimal)r["cout_ingredients"],
            CoutUnitaire        = (decimal)r["cout_unitaire"],
            DateProduction      = (DateTime)r["date_production"],
            Notes               = r["notes"]       == DBNull.Value ? null : r["notes"].ToString(),
            NomFiche            = r["nom_fiche"].ToString(),
            UniteOutput         = r["unite_output"] == DBNull.Value ? "" : r["unite_output"].ToString(),
            QuantiteOutputBatch = r["quantite_output"] == DBNull.Value ? 0 : Convert.ToDecimal(r["quantite_output"]),
            NomNiveau           = r["nom_niveau"].ToString(),
            OrdreNiveau         = Convert.ToInt32(r["ordre"]),
            NomContexte         = r["nom_contexte"].ToString()
        };
    }
}
