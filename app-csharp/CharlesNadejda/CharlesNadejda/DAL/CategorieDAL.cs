using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class CategorieDAL
    {
        public static List<Categorie> GetAll()
        {
            var list = new List<Categorie>();

            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT id, nom, description, ordre_affichage
                    FROM categories
                    ORDER BY ordre_affichage, nom";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(Map(reader));
                }
            }

            return list;
        }

        public static Categorie GetById(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, nom, description, ordre_affichage FROM categories WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                    return reader.Read() ? Map(reader) : null;
            }
        }

        public static bool NomExiste(string nom, int excludeId = 0)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM categories WHERE nom = @nom AND id <> @excludeId";
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@excludeId", excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static void Insert(Categorie c)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO categories (nom, description, ordre_affichage)
                    VALUES (@nom, @description, @ordre)";

                cmd.Parameters.AddWithValue("@nom",         c.Nom);
                cmd.Parameters.AddWithValue("@description", c.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ordre",       c.OrdreAffichage);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Update(Categorie c)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE categories
                    SET nom = @nom, description = @description, ordre_affichage = @ordre
                    WHERE id = @id";

                cmd.Parameters.AddWithValue("@nom",         c.Nom);
                cmd.Parameters.AddWithValue("@description", c.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ordre",       c.OrdreAffichage);
                cmd.Parameters.AddWithValue("@id",          c.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM categories WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        private static Categorie Map(MySqlDataReader r) => new Categorie
        {
            Id             = (int)r["id"],
            Nom            = r["nom"].ToString(),
            Description    = r["description"] == DBNull.Value ? null : r["description"].ToString(),
            OrdreAffichage = (int)r["ordre_affichage"]
        };
    }
}
