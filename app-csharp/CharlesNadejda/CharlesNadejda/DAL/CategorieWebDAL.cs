using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class CategorieWebDAL
    {
        // ── SELECT ──────────────────────────────────────────────

        public static List<CategorieWeb> GetAll(bool includeInactifs = false)
        {
            var list = new List<CategorieWeb>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT c.*,
                           COUNT(p.id) AS nb_produits
                    FROM categories_web c
                    LEFT JOIN produits_web p ON p.id_categorie = c.id AND p.en_vente = 1
                    " + (includeInactifs ? "" : "WHERE c.actif = 1") + @"
                    GROUP BY c.id
                    ORDER BY c.ordre_affichage, c.nom";
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public static CategorieWeb GetById(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT c.*,
                           COUNT(p.id) AS nb_produits
                    FROM categories_web c
                    LEFT JOIN produits_web p ON p.id_categorie = c.id AND p.en_vente = 1
                    WHERE c.id = @id
                    GROUP BY c.id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? Map(r) : null;
            }
        }

        public static bool NomExiste(string nom, int excludeId = 0)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM categories_web WHERE nom = @nom AND id <> @id";
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@id",  excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ── INSERT ──────────────────────────────────────────────

        public static int Insert(CategorieWeb c)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO categories_web (nom, description, ordre_affichage, actif)
                    VALUES (@nom, @desc, @ordre, @actif)";
                Bind(cmd, c);
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        // ── UPDATE ──────────────────────────────────────────────

        public static void Update(CategorieWeb c)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE categories_web
                    SET nom = @nom, description = @desc,
                        ordre_affichage = @ordre, actif = @actif
                    WHERE id = @id";
                Bind(cmd, c);
                cmd.Parameters.AddWithValue("@id", c.Id);
                cmd.ExecuteNonQuery();
            }
        }

        // ── DELETE ──────────────────────────────────────────────

        /// <summary>
        /// Supprime une catégorie. Les produits liés passent à id_categorie = NULL (FK SET NULL en DB).
        /// </summary>
        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM categories_web WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ── HELPERS ─────────────────────────────────────────────

        private static void Bind(MySqlCommand cmd, CategorieWeb c)
        {
            cmd.Parameters.AddWithValue("@nom",   c.Nom);
            cmd.Parameters.AddWithValue("@desc",  c.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ordre", c.OrdreAffichage);
            cmd.Parameters.AddWithValue("@actif", c.Actif);
        }

        private static CategorieWeb Map(MySqlDataReader r) => new CategorieWeb
        {
            Id             = (int)r["id"],
            Nom            = r["nom"].ToString(),
            Description    = r["description"]     == DBNull.Value ? null : r["description"].ToString(),
            OrdreAffichage = Convert.ToInt32(r["ordre_affichage"]),
            Actif          = Convert.ToBoolean(r["actif"]),
            NbProduits     = Convert.ToInt32(r["nb_produits"])
        };
    }
}
