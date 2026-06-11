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
        /// idStock : 0 = tous / filtre par stock physique (via lots_ingredients)
        /// Les fiches sont globales — le stock est assigné au lot, pas à la fiche.
        /// </summary>
        public static List<Ingredient> GetAll(int idStock = 0)
        {
            var list = new List<Ingredient>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT fi.id, fi.nom, fi.marque, fi.description, fi.unite_mesure, fi.type_physique, fi.densite,
                           fi.conditionnement_label, fi.qte_par_conditionnement, fi.nb_par_lot,
                           fi.prix_achat_reference, fi.seuil_alerte_stock, fi.stock_cible,
                           fi.id_fournisseur_defaut, fi.actif,
                           f.nom  AS nom_fournisseur,
                           COALESCE(SUM(l.quantite_disponible), 0) AS stock_actuel
                    FROM fiches_ingredients fi
                    LEFT  JOIN fournisseurs      f ON f.id = fi.id_fournisseur_defaut
                    LEFT  JOIN lots_ingredients  l ON l.id_fiche_ingredient = fi.id
                    WHERE fi.actif = 1";

                if (idStock > 0)
                {
                    cmd.CommandText += @" AND fi.id IN (
                        SELECT DISTINCT id_fiche_ingredient FROM lots_ingredients WHERE id_stock = @idStock)";
                    cmd.Parameters.AddWithValue("@idStock", idStock);
                }

                cmd.CommandText += " GROUP BY fi.id ORDER BY fi.nom";

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
                           fi.id_fournisseur_defaut, fi.actif,
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
                         id_fournisseur_defaut, actif)
                    VALUES (@nom, @marque, @desc, @unite, @type_physique, @densite,
                            @condLabel, @condQte, @nbLot,
                            @prix, @seuil, @stockCible, @fournisseur, 1)";
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
                        id_fournisseur_defaut=@fournisseur
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
            using (var cmd = conn.CreateCommand())
            {
                // Vérifier les lots actifs
                cmd.CommandText = @"
                    SELECT COUNT(*) FROM lots_ingredients
                    WHERE id_fiche_ingredient = @id AND quantite_disponible > 0";
                cmd.Parameters.AddWithValue("@id", id);
                int nbLots = Convert.ToInt32(cmd.ExecuteScalar());
                if (nbLots > 0)
                    throw new InvalidOperationException(
                        $"Impossible de supprimer : {nbLots} lot(s) avec du stock disponible.");

                // Vérifier les références dans les fiches BOM
                cmd.CommandText = @"
                    SELECT COUNT(*) FROM bom_fiches_lignes
                    WHERE id_input_ingredient = @id";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", id);
                int nbLignes = Convert.ToInt32(cmd.ExecuteScalar());
                if (nbLignes > 0)
                    throw new InvalidOperationException(
                        $"Impossible de supprimer : cet ingrédient est utilisé dans {nbLignes} fiche(s) BOM.");

                cmd.CommandText = "DELETE FROM fiches_ingredients WHERE id = @id";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
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
            cmd.Parameters.AddWithValue("@fournisseur",  i.IdFournisseurDefaut.HasValue ? (object)i.IdFournisseurDefaut.Value : DBNull.Value);
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
            NomFournisseur        = r["nom_fournisseur"]      == DBNull.Value ? null : r["nom_fournisseur"].ToString(),
            Actif                 = Convert.ToBoolean(r["actif"]),
            StockActuel           = (decimal)r["stock_actuel"]
        };
    }
}
