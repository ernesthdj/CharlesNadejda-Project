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

        // ── Vérification de disponibilité ────────────────────────────────

        /// <summary>
        /// Vérifie si le stock N-1 est suffisant pour produire quantiteCible unités
        /// de la fiche dans le niveau donné.
        /// Retourne une liste vide si tout est disponible,
        /// ou une liste de BomManque décrivant chaque pénurie.
        /// </summary>
        public static List<BomManque> VerifierDisponibilite(int idNiveau, int idFiche, decimal quantiteCible)
        {
            var manques = new List<BomManque>();
            var niveau  = BomNiveauDAL.GetById(idNiveau);
            var fiche   = BomFicheDAL.GetById(idFiche);

            if (niveau == null || fiche == null) return manques;

            // quantiteCible = nombre de batches à exécuter.
            // Chaque batch produit fiche.QuantiteOutput unités.
            // Le multiplicateur est donc directement le nombre de batches.
            decimal multiplicateur = quantiteCible;

            foreach (var ligne in fiche.Lignes)
            {
                decimal qteNecessaire = ligne.Quantite * multiplicateur;
                decimal qteDisponible;

                if (ligne.TypeInput == "ingredient")
                {
                    // Niveau 1 : consomme le stock global d'ingrédients (moins les réservations)
                    // Stock toujours en unité de base — convertir qteNecessaire vers l'unité de base
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
                    // Niveau N (N > 1) : consomme le bom_stock du niveau précédent.
                    // bom_stocks stocke dans l'unité output de la fiche source (ligne.UniteMesureInput).
                    // qteNecessaire est dans ligne.UniteMesure — conversion obligatoire avant comparaison.
                    int idNiveauPrecedent = GetIdNiveauPrecedent(idNiveau, niveau.IdContexte, niveau.Ordre);
                    qteDisponible = idNiveauPrecedent > 0
                        ? BomStockDAL.GetDisponible(idNiveauPrecedent, ligne.IdInputFiche.Value)
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
            var résultat = new List<BomManque>();
            var niveau   = BomNiveauDAL.GetById(idNiveau);
            var fiche    = BomFicheDAL.GetById(idFiche);

            if (niveau == null || fiche == null) return résultat;

            // quantiteCible = nombre de batches (même sémantique que VerifierDisponibilite)
            decimal multiplicateur = quantiteCible;

            foreach (var ligne in fiche.Lignes)
            {
                decimal qteNecessaire = ligne.Quantite * multiplicateur;
                decimal qteDisponible;
                decimal qteNecessaireAffichee;
                string  uniteAffichee;

                if (ligne.TypeInput == "ingredient")
                {
                    // Convertir vers l'unité de base pour la comparaison avec le stock
                    qteNecessaireAffichee = UnitConvertisseur.Convertir(
                        qteNecessaire, ligne.UniteMesure, ligne.UniteMesureInput);
                    uniteAffichee = ligne.UniteMesureInput;
                    qteDisponible = BomStockDAL.GetDisponibleIngredient(ligne.IdInputIngredient.Value);
                }
                else
                {
                    // Même conversion que VerifierDisponibilite : ramener qteNecessaire
                    // dans l'unité du bom_stock (ligne.UniteMesureInput = unité output fiche source).
                    qteNecessaireAffichee = UnitConvertisseur.Convertir(
                        qteNecessaire, ligne.UniteMesure, ligne.UniteMesureInput);
                    uniteAffichee = ligne.UniteMesureInput;
                    int idNiveauPrecedent = GetIdNiveauPrecedent(idNiveau, niveau.IdContexte, niveau.Ordre);
                    qteDisponible = idNiveauPrecedent > 0
                        ? BomStockDAL.GetDisponible(idNiveauPrecedent, ligne.IdInputFiche.Value)
                        : 0;
                }

                résultat.Add(new BomManque
                {
                    NomInput           = ligne.NomInput,
                    Unite              = uniteAffichee,
                    QuantiteNecessaire = qteNecessaireAffichee,
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
        public static int Executer(int idNiveau, int idFiche, decimal quantiteCible, string notes = null)
        {
            // Vérification préalable (hors transaction pour ne pas bloquer inutilement)
            var manques = VerifierDisponibilite(idNiveau, idFiche, quantiteCible);
            if (manques.Count > 0)
            {
                var details = string.Join("\n", manques);
                throw new InvalidOperationException(
                    $"Stock insuffisant pour lancer la production :\n{details}");
            }

            var niveau = BomNiveauDAL.GetById(idNiveau);
            var fiche  = BomFicheDAL.GetById(idFiche);
            // quantiteCible = nombre de batches. Quantité réellement produite = batches × QuantiteOutput.
            decimal multiplicateur = quantiteCible;
            decimal qteProduite    = quantiteCible * fiche.QuantiteOutput;

            using (var conn = DbHelper.GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
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
                        cmd.CommandText = @"
                            INSERT INTO bom_stocks
                                (id_niveau, id_contexte, id_activite, id_fiche, id_production,
                                 quantite_disponible, cout_unitaire, date_production)
                            VALUES (@idNiv, @idCtx, @idAct, @idFiche, @idProd, @qte, @coutUnit, CURDATE())";
                        cmd.Parameters.AddWithValue("@idNiv",    idNiveau);
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

                    // Traçabilité
                    InsertLigne(conn, tx, idProduction, "lot_ingredient", idLot, null, pris, prixUnit);

                    coutLigne += pris * prixUnit;
                    restant   -= pris;
                }
            }
            else
            {
                int idNiveauPrecedent = GetIdNiveauPrecedent(niveau.Id, niveau.IdContexte, niveau.Ordre);
                var stocks = BomStockDAL.GetBomStocksFIFO(idNiveauPrecedent, ligne.IdInputFiche.Value);

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
        /// Retourne l'id du niveau d'ordre (ordreActuel - 1) dans le même contexte.
        /// Retourne 0 si aucun niveau précédent (ne devrait pas arriver pour ordre > 1).
        /// </summary>
        private static int GetIdNiveauPrecedent(int idNiveauActuel, int idContexte, int ordreActuel)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT id FROM bom_niveaux
                    WHERE id_contexte = @idCtx AND ordre = @ordre";
                cmd.Parameters.AddWithValue("@idCtx", idContexte);
                cmd.Parameters.AddWithValue("@ordre", ordreActuel - 1);
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
