using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    /// <summary>
    /// CRUD sur les fiches d'ingrédients (table fiches_ingredients).
    /// Chaque SELECT agrège le stock actuel depuis lots_ingredients via COALESCE(SUM).
    /// </summary>
    public static class IngredientDAL
    {
        // ── SELECT ──────────────────────────────────────────────────

        /// <summary>
        /// Charge les fiches ingrédients selon deux modes conceptuellement distincts :
        ///
        /// Mode Fiches (stockReelSeulement = false) — la fiche EST la définition de l'ingrédient.
        ///   Pas de JOIN lots_ingredients : une fiche n'a pas de quantité, comme une classe n'a pas d'état.
        ///   stock_actuel = 0 (non pertinent). Filtre chip = id_stock_defaut.
        ///
        /// Mode Stock réel (stockReelSeulement = true) — les lots sont les instances physiques.
        ///   JOIN lots_ingredients pour agréger les quantités disponibles.
        ///   Filtre chip = lots présents avec quantite_disponible > 0 dans ce stock.
        /// </summary>
        public static List<Ingredient> GetAll(int idStock = 0, bool stockReelSeulement = false)
        {
            var list = new List<Ingredient>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                if (!stockReelSeulement)
                {
                    // ── Mode Fiches : référentiel pur, sans calcul de stock ──────────────
                    cmd.CommandText = @"
                        SELECT fi.id, fi.nom, fi.marque, fi.description, fi.unite_mesure, fi.type_physique, fi.densite,
                               fi.conditionnement_label, fi.qte_par_conditionnement, fi.nb_par_lot,
                               fi.prix_achat_reference, fi.seuil_alerte_stock, fi.stock_cible,
                               fi.id_fournisseur_defaut, fi.id_stock_defaut,
                               fi.dlc_jours_reference, fi.qualite_label, fi.actif,
                               f.nom AS nom_fournisseur,
                               0     AS stock_actuel
                        FROM fiches_ingredients fi
                        LEFT JOIN fournisseurs f ON f.id = fi.id_fournisseur_defaut
                        WHERE fi.actif = 1";

                    if (idStock > 0)
                    {
                        cmd.CommandText += " AND fi.id_stock_defaut = @idStock";
                        cmd.Parameters.AddWithValue("@idStock", idStock);
                    }

                    cmd.CommandText += " ORDER BY fi.nom";
                }
                else
                {
                    // ── Mode Stock réel : instances physiques avec quantités ─────────────
                    cmd.CommandText = @"
                        SELECT fi.id, fi.nom, fi.marque, fi.description, fi.unite_mesure, fi.type_physique, fi.densite,
                               fi.conditionnement_label, fi.qte_par_conditionnement, fi.nb_par_lot,
                               fi.prix_achat_reference, fi.seuil_alerte_stock, fi.stock_cible,
                               fi.id_fournisseur_defaut, fi.id_stock_defaut,
                               fi.dlc_jours_reference, fi.qualite_label, fi.actif,
                               f.nom AS nom_fournisseur,
                               COALESCE(SUM(l.quantite_disponible), 0) AS stock_actuel
                        FROM fiches_ingredients fi
                        LEFT JOIN fournisseurs     f ON f.id = fi.id_fournisseur_defaut
                        LEFT JOIN lots_ingredients l ON l.id_fiche_ingredient = fi.id
                        WHERE fi.actif = 1";

                    if (idStock > 0)
                    {
                        cmd.CommandText += @" AND fi.id IN (
                            SELECT DISTINCT id_fiche_ingredient FROM lots_ingredients
                            WHERE id_stock = @idStock AND quantite_disponible > 0)";
                        cmd.Parameters.AddWithValue("@idStock", idStock);
                    }

                    cmd.CommandText += " GROUP BY fi.id";

                    if (idStock == 0)
                        cmd.CommandText += " HAVING stock_actuel > 0";

                    cmd.CommandText += " ORDER BY fi.nom";
                }

                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        /// <summary>
        /// Retourne les ingrédients disponibles pour un achat dans le contexte d'une activité :
        ///   - Sans aucun lot nulle part (nouveaux, jamais achetés → Vodka fraîchement créée)
        ///   - OU avec au moins un lot dans un stock lié à cette activité
        /// Exclut les ingrédients achetés uniquement pour d'autres activités (Farine → Boulangerie).
        /// </summary>
        public static List<Ingredient> GetAllForAchat(int idActivite)
        {
            var list = new List<Ingredient>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT fi.id, fi.nom, fi.marque, fi.description, fi.unite_mesure, fi.type_physique, fi.densite,
                           fi.conditionnement_label, fi.qte_par_conditionnement, fi.nb_par_lot,
                           fi.prix_achat_reference, fi.seuil_alerte_stock, fi.stock_cible,
                           fi.id_fournisseur_defaut, fi.id_stock_defaut,
                           fi.dlc_jours_reference, fi.qualite_label, fi.actif,
                           f.nom  AS nom_fournisseur,
                           COALESCE(SUM(l.quantite_disponible), 0) AS stock_actuel
                    FROM fiches_ingredients fi
                    LEFT  JOIN fournisseurs     f ON f.id = fi.id_fournisseur_defaut
                    LEFT  JOIN lots_ingredients l ON l.id_fiche_ingredient = fi.id
                    WHERE fi.actif = 1
                      AND (
                            -- Jamais acheté nulle part : fiche neuve, disponible à l'achat
                            NOT EXISTS (
                                SELECT 1 FROM lots_ingredients
                                WHERE id_fiche_ingredient = fi.id
                            )
                            OR
                            -- Déjà acheté dans un stock de cette activité
                            EXISTS (
                                SELECT 1 FROM lots_ingredients l2
                                INNER JOIN activites_stocks acs ON acs.id_stock = l2.id_stock
                                WHERE l2.id_fiche_ingredient = fi.id
                                  AND acs.id_activite = @idActivite
                            )
                          )
                    GROUP BY fi.id ORDER BY fi.nom";
                cmd.Parameters.AddWithValue("@idActivite", idActivite);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        /// <summary>
        /// Retourne les ingrédients dont au moins un lot se trouve dans un stock lié à l'activité.
        /// Le stock_actuel est la somme des quantités uniquement dans ces stocks (pas globale).
        /// </summary>
        public static List<Ingredient> GetAllByActivite(int idActivite)
        {
            var list = new List<Ingredient>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                // INNER JOIN sur lots_ingredients + activites_stocks :
                // - seuls les ingrédients ayant des lots dans les stocks de cette activité apparaissent
                // - SUM ne comptabilise que les lots de ces stocks (pas les autres stocks)
                cmd.CommandText = @"
                    SELECT fi.id, fi.nom, fi.marque, fi.description, fi.unite_mesure, fi.type_physique, fi.densite,
                           fi.conditionnement_label, fi.qte_par_conditionnement, fi.nb_par_lot,
                           fi.prix_achat_reference, fi.seuil_alerte_stock, fi.stock_cible,
                           fi.id_fournisseur_defaut, fi.id_stock_defaut,
                           fi.dlc_jours_reference, fi.qualite_label, fi.actif,
                           f.nom  AS nom_fournisseur,
                           COALESCE(SUM(l.quantite_disponible), 0) AS stock_actuel
                    FROM fiches_ingredients fi
                    LEFT  JOIN fournisseurs      f   ON f.id  = fi.id_fournisseur_defaut
                    INNER JOIN lots_ingredients  l   ON l.id_fiche_ingredient = fi.id
                    INNER JOIN activites_stocks  acs ON acs.id_stock = l.id_stock
                    WHERE fi.actif = 1 AND acs.id_activite = @idActivite
                    GROUP BY fi.id ORDER BY fi.nom";
                cmd.Parameters.AddWithValue("@idActivite", idActivite);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        /// <summary>Retourne un ingrédient par son ID, ou null si introuvable.</summary>
        public static Ingredient GetById(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT fi.id, fi.nom, fi.marque, fi.description, fi.unite_mesure, fi.type_physique, fi.densite,
                           fi.conditionnement_label, fi.qte_par_conditionnement, fi.nb_par_lot,
                           fi.prix_achat_reference, fi.seuil_alerte_stock, fi.stock_cible,
                           fi.id_fournisseur_defaut, fi.id_stock_defaut,
                           fi.dlc_jours_reference, fi.qualite_label, fi.actif,
                           f.nom  AS nom_fournisseur,
                           COALESCE(SUM(l.quantite_disponible), 0) AS stock_actuel
                    FROM fiches_ingredients fi
                    LEFT  JOIN fournisseurs      f ON f.id = fi.id_fournisseur_defaut
                    LEFT  JOIN lots_ingredients  l ON l.id_fiche_ingredient = fi.id
                    WHERE fi.id = @id
                    GROUP BY fi.id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? Map(r) : null;
            }
        }

        /// <summary>Vérifie l'unicité du nom dans le référentiel ingrédients.</summary>
        public static bool NomExiste(string nom, int excludeId = 0)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM fiches_ingredients WHERE nom = @nom AND id <> @id";
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@id",  excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ── INSERT / UPDATE / DELETE ─────────────────────────────────

        /// <summary>Insère un ingrédient et retourne son ID généré.</summary>
        public static int Insert(Ingredient i)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO fiches_ingredients
                        (nom, marque, description, unite_mesure, type_physique, densite,
                         conditionnement_label, qte_par_conditionnement, nb_par_lot,
                         prix_achat_reference, seuil_alerte_stock, stock_cible,
                         id_fournisseur_defaut, id_stock_defaut, dlc_jours_reference, qualite_label, actif)
                    VALUES (@nom, @marque, @desc, @unite, @type_physique, @densite,
                            @condLabel, @condQte, @nbLot,
                            @prix, @seuil, @stockCible, @fournisseur, @idStockDefaut, @dlcJours, @qualite, 1)";
                Bind(cmd, i);
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        /// <summary>Met à jour toutes les colonnes d'un ingrédient existant.</summary>
        public static void Update(Ingredient i)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE fiches_ingredients
                    SET nom=@nom, marque=@marque, description=@desc, unite_mesure=@unite,
                        type_physique=@type_physique, densite=@densite,
                        conditionnement_label=@condLabel, qte_par_conditionnement=@condQte,
                        nb_par_lot=@nbLot,
                        prix_achat_reference=@prix, seuil_alerte_stock=@seuil,
                        stock_cible=@stockCible,
                        id_fournisseur_defaut=@fournisseur,
                        id_stock_defaut=@idStockDefaut,
                        dlc_jours_reference=@dlcJours, qualite_label=@qualite
                    WHERE id=@id";
                Bind(cmd, i);
                cmd.Parameters.AddWithValue("@id", i.Id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Supprime un ingrédient. Bloqué si des lots actifs ou des références BOM existent.</summary>
        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    // Vérifier les lots actifs
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            SELECT COUNT(*) FROM lots_ingredients
                            WHERE id_fiche_ingredient = @id AND quantite_disponible > 0";
                        cmd.Parameters.AddWithValue("@id", id);
                        int nbLots = Convert.ToInt32(cmd.ExecuteScalar());
                        if (nbLots > 0)
                            throw new InvalidOperationException(
                                $"Impossible de supprimer : {nbLots} lot(s) avec du stock disponible.");
                    }

                    // Vérifier les références dans les fiches BOM
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            SELECT COUNT(*) FROM bom_fiches_lignes
                            WHERE id_input_ingredient = @id";
                        cmd.Parameters.AddWithValue("@id", id);
                        int nbLignes = Convert.ToInt32(cmd.ExecuteScalar());
                        if (nbLignes > 0)
                            throw new InvalidOperationException(
                                $"Impossible de supprimer : cet ingrédient est utilisé dans {nbLignes} fiche(s) BOM.");
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = "DELETE FROM fiches_ingredients WHERE id = @id";
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        // ── Helpers privés ───────────────────────────────────────────

        /// <summary>Lie les paramètres SQL communs à INSERT et UPDATE.</summary>
        private static void Bind(MySqlCommand cmd, Ingredient i)
        {
            cmd.Parameters.AddWithValue("@nom",          i.Nom);
            cmd.Parameters.AddWithValue("@marque",       i.Marque ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@desc",         i.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@unite",        i.UniteMesure);
            cmd.Parameters.AddWithValue("@type_physique",i.TypePhysique ?? "solide");
            cmd.Parameters.AddWithValue("@densite",      i.Densite.HasValue ? (object)i.Densite.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@condLabel",    i.ConditionnementLabel ?? "");
            cmd.Parameters.AddWithValue("@condQte",      i.QteParConditionnement);
            cmd.Parameters.AddWithValue("@nbLot",        i.NbParLot);
            cmd.Parameters.AddWithValue("@prix",         i.PrixAchatReference);
            cmd.Parameters.AddWithValue("@seuil",        i.SeuilAlerteStock.HasValue ? (object)i.SeuilAlerteStock.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@stockCible",   i.StockCible.HasValue ? (object)i.StockCible.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@fournisseur",    i.IdFournisseurDefaut.HasValue ? (object)i.IdFournisseurDefaut.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@idStockDefaut", i.IdStockDefaut.HasValue       ? (object)i.IdStockDefaut.Value       : DBNull.Value);
            cmd.Parameters.AddWithValue("@dlcJours",     i.DlcJoursReference.HasValue ? (object)i.DlcJoursReference.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@qualite",      i.QualiteLabel ?? (object)DBNull.Value);
        }

        private static Ingredient Map(MySqlDataReader r) => new Ingredient
        {
            Id                    = (int)r["id"],
            Nom                   = r["nom"].ToString(),
            Marque                = r["marque"]               == DBNull.Value ? null : r["marque"].ToString(),
            Description           = r["description"]          == DBNull.Value ? null : r["description"].ToString(),
            UniteMesure           = r["unite_mesure"].ToString(),
            TypePhysique          = r["type_physique"].ToString(),
            Densite               = r["densite"]              == DBNull.Value ? (decimal?)null : (decimal)r["densite"],
            ConditionnementLabel  = r["conditionnement_label"].ToString(),
            QteParConditionnement = (decimal)r["qte_par_conditionnement"],
            NbParLot              = (int)r["nb_par_lot"],
            PrixAchatReference    = (decimal)r["prix_achat_reference"],
            SeuilAlerteStock      = r["seuil_alerte_stock"]   == DBNull.Value ? (decimal?)null : (decimal)r["seuil_alerte_stock"],
            StockCible            = r["stock_cible"]           == DBNull.Value ? (decimal?)null : (decimal)r["stock_cible"],
            IdFournisseurDefaut   = r["id_fournisseur_defaut"] == DBNull.Value ? (int?)null : (int)r["id_fournisseur_defaut"],
            IdStockDefaut        = r["id_stock_defaut"]       == DBNull.Value ? (int?)null : (int)r["id_stock_defaut"],
            DlcJoursReference     = r["dlc_jours_reference"]  == DBNull.Value ? (int?)null : Convert.ToInt32(r["dlc_jours_reference"]),
            QualiteLabel          = r["qualite_label"]         == DBNull.Value ? null : r["qualite_label"].ToString(),
            NomFournisseur        = r["nom_fournisseur"]      == DBNull.Value ? null : r["nom_fournisseur"].ToString(),
            Actif                 = Convert.ToBoolean(r["actif"]),
            StockActuel           = Convert.ToDecimal(r["stock_actuel"])
        };
    }
}
