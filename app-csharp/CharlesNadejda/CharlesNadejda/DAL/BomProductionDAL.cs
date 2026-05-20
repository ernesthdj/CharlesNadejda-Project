using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    /// <summary>
    /// Moteur de production BOM (Bill of Materials).
    /// Gère la validation de stock, la consommation FIFO (First In First Out)
    /// et la création des enregistrements de production dans une transaction atomique.
    /// </summary>
    public static class BomProductionDAL
    {
        /// <summary>Tolérance d'arrondi pour la consommation FIFO (évite les faux négatifs sur reste flottant).</summary>
        private const decimal TOLERANCE_ARRONDI = 0.0001m;

        public static List<BomProduction> GetByNiveau(int idNiveau)
        {
            var list = new List<BomProduction>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT p.id, p.id_niveau, p.id_fiche, p.quantite_produite,
                           p.cout_ingredients, p.cout_unitaire, p.date_production, p.notes,
                           f.nom AS nom_fiche,
                           n.nom AS nom_niveau, n.ordre,
                           c.nom AS nom_contexte
                    FROM bom_productions p
                    INNER JOIN bom_fiches    f ON f.id = p.id_fiche
                    INNER JOIN bom_niveaux   n ON n.id = p.id_niveau
                    INNER JOIN bom_contextes c ON c.id = n.id_contexte
                    WHERE p.id_niveau = @idNiveau
                    ORDER BY p.date_production DESC";
                cmd.Parameters.AddWithValue("@idNiveau", idNiveau);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapHeader(r));
            }
            return list;
        }

        public static List<BomProduction> GetRecentByActivite(int idActivite, int limit = 10)
        {
            var list = new List<BomProduction>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT p.id, p.id_niveau, p.id_fiche, p.quantite_produite,
                           p.cout_ingredients, p.cout_unitaire, p.date_production, p.notes,
                           f.nom AS nom_fiche,
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
        /// Retourne les productions du jour pour une activité.
        /// Filtre sur date_production = CURDATE() côté MySQL.
        /// </summary>
        /// <param name="idActivite">Id de l'activité.</param>
        /// <returns>Liste des BomProduction du jour, triée par date_production DESC.</returns>
        public static List<BomProduction> GetDuJourByActivite(int idActivite)
        {
            var list = new List<BomProduction>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT p.id, p.id_niveau, p.id_fiche, p.quantite_produite,
                           p.cout_ingredients, p.cout_unitaire, p.date_production, p.notes,
                           f.nom AS nom_fiche,
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
        /// Vérifie si le stock N-1 est suffisant pour produire quantiteCible unités
        /// de la fiche dans le niveau donné.
        /// Retourne une liste vide si tout est disponible,
        /// ou une liste de BomManque décrivant chaque pénurie.
        /// </summary>
        public static List<BomManque> VerifierDisponibilite(int idNiveau, int idFiche, decimal quantiteCible)
        {
            var niveau = BomNiveauDAL.GetById(idNiveau);
            var fiche  = BomFicheDAL.GetById(idFiche);
            if (niveau == null || fiche == null) return new List<BomManque>();
            return VerifierDisponibiliteLignes(fiche.Lignes, quantiteCible);
        }

        /// <summary>
        /// Surcharge interne utilisée par Executer() — opère sur les lignes déjà chargées,
        /// évitant le double chargement niveau/fiche et travaillant dans la même transaction logique.
        /// </summary>
        private static List<BomManque> VerifierDisponibiliteLignes(
            List<BomFicheLigne> lignes, decimal quantiteCible)
        {
            var manques = new List<BomManque>();
            decimal multiplicateur = quantiteCible;

            foreach (var ligne in lignes)
            {
                decimal qteNecessaire = ligne.Quantite * multiplicateur;
                decimal qteDisponible;

                if (ligne.TypeInput == "ingredient")
                {
                    decimal qteNecessaireBase = UnitConvertisseur.Convertir(
                        qteNecessaire, ligne.UniteMesure, ligne.UniteMesureInput);
                    qteDisponible = BomStockDAL.GetDisponibleIngredient(ligne.IdInputIngredient.Value);

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

        // ── Simulation complète ───────────────────────────────────────────

        /// <summary>
        /// Retourne TOUTES les lignes requises pour la production (pas seulement les pénuries).
        /// Chaque BomManque a Manque = 0 si le stock est suffisant, Manque > 0 sinon.
        /// Utilisé par FrmBomProductionSimulation pour afficher un tableau complet avec code couleur.
        /// </summary>
        public static List<BomManque> Simuler(int idNiveau, int idFiche, decimal quantiteCible)
        {
            var niveau = BomNiveauDAL.GetById(idNiveau);
            var fiche  = BomFicheDAL.GetById(idFiche);
            if (niveau == null || fiche == null) return new List<BomManque>();
            return SimulerLignes(fiche.Lignes, quantiteCible);
        }

        /// <summary>
        /// Logique interne de simulation sur des lignes déjà chargées.
        /// Retourne toutes les lignes avec qté nécessaire et disponible (Manque calculé par le Model).
        /// </summary>
        private static List<BomManque> SimulerLignes(List<BomFicheLigne> lignes, decimal quantiteCible)
        {
            var résultat = new List<BomManque>();
            decimal multiplicateur = quantiteCible;

            foreach (var ligne in lignes)
            {
                decimal qteNecessaire = ligne.Quantite * multiplicateur;
                decimal qteDisponible;

                // Conversion vers unité native du stock (commune aux deux types)
                decimal qteNecessaireConv = UnitConvertisseur.Convertir(
                    qteNecessaire, ligne.UniteMesure, ligne.UniteMesureInput);

                if (ligne.TypeInput == "ingredient")
                {
                    qteDisponible = BomStockDAL.GetDisponibleIngredient(ligne.IdInputIngredient.Value);
                }
                else
                {
                    int idNiveauSource = GetIdNiveauDeFiche(ligne.IdInputFiche.Value);
                    qteDisponible = idNiveauSource > 0
                        ? BomStockDAL.GetDisponible(idNiveauSource, ligne.IdInputFiche.Value)
                        : 0;
                }

                résultat.Add(new BomManque
                {
                    NomInput           = ligne.NomInput,
                    Unite              = ligne.UniteMesureInput,
                    QuantiteNecessaire = qteNecessaireConv,
                    QuantiteDisponible = qteDisponible
                });
            }
            return résultat;
        }

        // ── Exécution de production ───────────────────────────────────────

        /// <summary>
        /// Exécute une production dans une transaction MySQL atomique :
        /// 1. Vérifie la disponibilité (lève une exception si stock insuffisant).
        /// 2. Insère bom_productions.
        /// 3. Consomme le stock N-1 en FIFO, insère bom_productions_lignes.
        /// 4. Crée l'entrée de stock N dans bom_stocks.
        /// Retourne l'id de la production créée.
        /// </summary>
        public static int Executer(int idNiveau, int idFiche, decimal quantiteCible, string notes = null, int delaiConservationJours = 0)
        {
            using (var conn = DbHelper.GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    // Charger niveau et fiche UNE SEULE FOIS (évite double requête)
                    var niveau = BomNiveauDAL.GetById(idNiveau);
                    var fiche  = BomFicheDAL.GetById(idFiche);

                    if (niveau == null)
                        throw new InvalidOperationException($"Le niveau BOM (id={idNiveau}) n'existe plus — production annulée.");
                    if (fiche == null)
                        throw new InvalidOperationException($"La fiche BOM (id={idFiche}) n'existe plus — production annulée.");

                    // TICKET-01 FIX : Vérification avec les lignes déjà chargées
                    // Note : les sous-requêtes de stock ouvrent encore leur propre connexion,
                    // mais niveau/fiche sont chargés une seule fois et la transaction protège les écritures.
                    var manques = VerifierDisponibiliteLignes(fiche.Lignes, quantiteCible);
                    if (manques.Count > 0)
                    {
                        var details = string.Join("\n", manques);
                        throw new InvalidOperationException(
                            $"Stock insuffisant pour lancer la production :\n{details}");
                    }

                    // quantiteCible = nombre de batches. Quantité réellement produite = batches × QuantiteOutput.
                    decimal multiplicateur = quantiteCible;
                    decimal qteProduite    = quantiteCible * fiche.QuantiteOutput;

                    decimal coutTotalIngredients = 0;

                    // 1. Insérer l'enregistrement de production (cout_unitaire calculé après)
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
                        idProduction = (int)cmd.LastInsertedId;
                    }

                    // 2. Consommer le stock N-1 pour chaque ligne de la fiche
                    foreach (var ligne in fiche.Lignes)
                    {
                        decimal aConommer = ligne.Quantite * multiplicateur;
                        coutTotalIngredients += ConsumeStock(conn, tx, ligne, aConommer,
                                                             idProduction, niveau);
                    }

                    // 3. Mettre à jour cout_ingredients et cout_unitaire dans bom_productions
                    // coutUnitaire = coût par unité output (ex : coût par kg produit)
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

                    // 4. Créer l'entrée bom_stocks pour le niveau N
                    // id_contexte et id_activite déduits depuis niveau (v11 — jamais de saisie manuelle)
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        // DLC = date production + délai en jours (NULL si délai = 0)
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

                    tx.Commit();
                    return idProduction;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        // ── Helpers privés ────────────────────────────────────────────────

        /// <summary>
        /// Consomme aConommer unités de stock pour une ligne de fiche, en FIFO.
        /// Insère les bom_productions_lignes correspondantes.
        /// Retourne le coût total consommé pour cette ligne.
        /// </summary>
        private static decimal ConsumeStock(MySqlConnection conn, MySqlTransaction tx,
                                             BomFicheLigne ligne, decimal aConommer,
                                             int idProduction, BomNiveau niveau)
        {
            decimal coutLigne = 0;
            // Convertir aConommer (dans ligne.UniteMesure) vers l'unité native du stock (ligne.UniteMesureInput).
            // Pour ingredient  : UniteMesureInput = unité de base du lot (g, ml, piece).
            // Pour fiche       : UniteMesureInput = UniteOutput de la fiche source (ex: kg).
            // La conversion s'applique dans les deux cas — les stocks N-1 sont toujours
            // dans l'unité UniteMesureInput, jamais dans ligne.UniteMesure.
            decimal restant = UnitConvertisseur.Convertir(
                aConommer, ligne.UniteMesure, ligne.UniteMesureInput);

            if (ligne.TypeInput == "ingredient")
            {
                var lots = BomStockDAL.GetLotsDispoFIFO(ligne.IdInputIngredient.Value);
                foreach (var (idLot, dispo, prixUnit) in lots)
                {
                    if (restant <= 0) break;
                    decimal pris = Math.Min(restant, dispo);

                    // Décrémenter le lot
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

                    // TICKET-08 : libérer les réservations actives sur ce lot (dans la même transaction)
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

                    // Traçabilité
                    InsertLigne(conn, tx, idProduction, "lot_ingredient", idLot, null, pris, prixUnit);

                    coutLigne += pris * prixUnit;
                    restant   -= pris;
                }

                // Guard : si tout le stock a été épuisé sans couvrir le besoin,
                // la production est incohérente — on refuse.
                if (restant > TOLERANCE_ARRONDI)
                    throw new InvalidOperationException(
                        $"Stock insuffisant pour « {ligne.NomInput} » : " +
                        $"il manque {restant:F4} {ligne.UniteMesureInput} après épuisement FIFO.");
            }
            else
            {
                int idNiveauSource = GetIdNiveauDeFiche(ligne.IdInputFiche.Value);
                var stocks = BomStockDAL.GetBomStocksFIFO(idNiveauSource, ligne.IdInputFiche.Value);

                foreach (var (idStock, dispo, coutUnit) in stocks)
                {
                    if (restant <= 0) break;
                    decimal pris = Math.Min(restant, dispo);

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

                    InsertLigne(conn, tx, idProduction, "bom_stock", null, idStock, pris, coutUnit);

                    coutLigne += pris * coutUnit;
                    restant   -= pris;
                }

                // Guard : idem pour les produits intermédiaires (bom_stocks)
                if (restant > TOLERANCE_ARRONDI)
                    throw new InvalidOperationException(
                        $"Stock insuffisant pour « {ligne.NomInput} » : " +
                        $"il manque {restant:F4} {ligne.UniteMesureInput} après épuisement FIFO.");
            }
            return coutLigne;
        }

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
                cmd.Parameters.AddWithValue("@idLot",   idLot.HasValue   ? (object)idLot.Value   : DBNull.Value);
                cmd.Parameters.AddWithValue("@idStock", idStock.HasValue ? (object)idStock.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@qte",     quantite);
                cmd.Parameters.AddWithValue("@cout",    coutUnit);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Retourne l'id du niveau auquel appartient la fiche donnée.
        /// Utilisé pour consommer le stock du bon niveau source, quel que soit son ordre
        /// (un niveau N peut référencer n'importe quel niveau inférieur, pas seulement N-1).
        /// Retourne 0 si la fiche n'existe pas.
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

        private static BomProduction MapHeader(MySqlDataReader r) => new BomProduction
        {
            Id               = (int)r["id"],
            IdNiveau         = (int)r["id_niveau"],
            IdFiche          = (int)r["id_fiche"],
            QuantiteProduite = (decimal)r["quantite_produite"],
            CoutIngredients  = (decimal)r["cout_ingredients"],
            CoutUnitaire     = (decimal)r["cout_unitaire"],
            DateProduction   = (DateTime)r["date_production"],
            Notes            = r["notes"]       == DBNull.Value ? null : r["notes"].ToString(),
            NomFiche         = r["nom_fiche"].ToString(),
            NomNiveau        = r["nom_niveau"].ToString(),
            OrdreNiveau      = Convert.ToInt32(r["ordre"]),
            NomContexte      = r["nom_contexte"].ToString()
        };
    }
}
