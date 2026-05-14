using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class IngredientDAL
    {
        /// <summary>
        /// idStock    : 0 = tous / filtre par stock physique
        /// idActivite : 0 = tous / filtre par tous les stocks liés à cette activité
        /// </summary>
        public static List<Ingredient> GetAll(int idStock = 0, int idActivite = 0)
        {
            var list = new List<Ingredient>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT fi.id, fi.nom, fi.marque, fi.unite_mesure, fi.type_physique, fi.densite,
                           fi.conditionnement_label, fi.qte_par_conditionnement,
                           fi.prix_achat_reference, fi.seuil_alerte_stock, fi.stock_cible,
                           fi.id_fournisseur_defaut, fi.id_stock, fi.actif,
                           f.nom  AS nom_fournisseur,
                           s.nom  AS nom_stock,
                           COALESCE(SUM(l.quantite_disponible), 0) AS stock_actuel
                    FROM fiches_ingredients fi
                    LEFT  JOIN fournisseurs      f ON f.id = fi.id_fournisseur_defaut
                    INNER JOIN stocks            s ON s.id = fi.id_stock
                    LEFT  JOIN lots_ingredients  l ON l.id_fiche_ingredient = fi.id
                    WHERE fi.actif = 1";

                if (idStock > 0)
                {
                    cmd.CommandText += " AND fi.id_stock = @idStock";
                    cmd.Parameters.AddWithValue("@idStock", idStock);
                }
                else if (idActivite > 0)
                {
                    cmd.CommandText += @" AND fi.id_stock IN (
                        SELECT id_stock FROM activites_stocks WHERE id_activite = @idActivite)";
                    cmd.Parameters.AddWithValue("@idActivite", idActivite);
                }

                cmd.CommandText += " GROUP BY fi.id ORDER BY fi.nom";

                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

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

        public static void Insert(Ingredient i)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO fiches_ingredients
                        (nom, marque, unite_mesure, type_physique, densite,
                         conditionnement_label, qte_par_conditionnement,
                         prix_achat_reference, seuil_alerte_stock, stock_cible,
                         id_fournisseur_defaut, id_stock, actif)
                    VALUES (@nom, @marque, @unite, @type_physique, @densite,
                            @condLabel, @condQte,
                            @prix, @seuil, @stockCible, @fournisseur, @idStock, 1)";
                Bind(cmd, i);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Update(Ingredient i)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE fiches_ingredients
                    SET nom=@nom, marque=@marque, unite_mesure=@unite,
                        type_physique=@type_physique, densite=@densite,
                        conditionnement_label=@condLabel, qte_par_conditionnement=@condQte,
                        prix_achat_reference=@prix, seuil_alerte_stock=@seuil,
                        stock_cible=@stockCible,
                        id_fournisseur_defaut=@fournisseur, id_stock=@idStock
                    WHERE id=@id";
                Bind(cmd, i);
                cmd.Parameters.AddWithValue("@id", i.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM fiches_ingredients WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        private static void Bind(MySqlCommand cmd, Ingredient i)
        {
            cmd.Parameters.AddWithValue("@nom",          i.Nom);
            cmd.Parameters.AddWithValue("@marque",       i.Marque ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@unite",        i.UniteMesure);
            cmd.Parameters.AddWithValue("@type_physique",i.TypePhysique ?? "solide");
            cmd.Parameters.AddWithValue("@densite",      i.Densite.HasValue ? (object)i.Densite.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@condLabel",    i.ConditionnementLabel ?? "");
            cmd.Parameters.AddWithValue("@condQte",      i.QteParConditionnement);
            cmd.Parameters.AddWithValue("@prix",         i.PrixAchatReference);
            cmd.Parameters.AddWithValue("@seuil",        i.SeuilAlerteStock.HasValue ? (object)i.SeuilAlerteStock.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@stockCible",   i.StockCible.HasValue ? (object)i.StockCible.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@fournisseur",  i.IdFournisseurDefaut.HasValue ? (object)i.IdFournisseurDefaut.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@idStock",       i.IdStock);
        }

        private static Ingredient Map(MySqlDataReader r) => new Ingredient
        {
            Id                    = (int)r["id"],
            Nom                   = r["nom"].ToString(),
            Marque                = r["marque"]               == DBNull.Value ? null : r["marque"].ToString(),
            UniteMesure           = r["unite_mesure"].ToString(),
            TypePhysique          = r["type_physique"].ToString(),
            Densite               = r["densite"]              == DBNull.Value ? (decimal?)null : (decimal)r["densite"],
            ConditionnementLabel  = r["conditionnement_label"].ToString(),
            QteParConditionnement = (decimal)r["qte_par_conditionnement"],
            PrixAchatReference    = (decimal)r["prix_achat_reference"],
            SeuilAlerteStock      = r["seuil_alerte_stock"]   == DBNull.Value ? (decimal?)null : (decimal)r["seuil_alerte_stock"],
            StockCible            = r["stock_cible"]           == DBNull.Value ? (decimal?)null : (decimal)r["stock_cible"],
            IdFournisseurDefaut   = r["id_fournisseur_defaut"] == DBNull.Value ? (int?)null : (int)r["id_fournisseur_defaut"],
            NomFournisseur        = r["nom_fournisseur"]      == DBNull.Value ? null : r["nom_fournisseur"].ToString(),
            IdStock               = (int)r["id_stock"],
            StockNom              = r["nom_stock"].ToString(),
            Actif                 = Convert.ToBoolean(r["actif"]),
            StockActuel           = (decimal)r["stock_actuel"]
        };
    }
}
