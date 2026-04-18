using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class ProduitDAL
    {
        public static List<Produit> GetAll()
        {
            var list = new List<Produit>();

            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT p.id, p.id_categorie, c.nom AS nom_categorie,
                           p.nom, p.description, p.prix_ttc, p.prix_promo,
                           p.stock, p.disponible, p.image_url
                    FROM produits p
                    INNER JOIN categories c ON c.id = p.id_categorie
                    ORDER BY c.ordre_affichage, c.nom, p.nom";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(Map(reader));
                }
            }

            return list;
        }

        public static Produit GetById(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT p.id, p.id_categorie, c.nom AS nom_categorie,
                           p.nom, p.description, p.prix_ttc, p.prix_promo,
                           p.stock, p.disponible, p.image_url
                    FROM produits p
                    INNER JOIN categories c ON c.id = p.id_categorie
                    WHERE p.id = @id";

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
                cmd.CommandText = "SELECT COUNT(*) FROM produits WHERE nom = @nom AND id <> @excludeId";
                cmd.Parameters.AddWithValue("@nom",       nom);
                cmd.Parameters.AddWithValue("@excludeId", excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static void Insert(Produit p)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO produits (id_categorie, nom, description, prix_ttc, prix_promo, stock, disponible, image_url)
                    VALUES (@idCat, @nom, @desc, @prixTTC, @prixPromo, @stock, @disponible, @imageUrl)";

                BindParams(cmd, p);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Update(Produit p)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE produits
                    SET id_categorie = @idCat, nom = @nom, description = @desc,
                        prix_ttc = @prixTTC, prix_promo = @prixPromo,
                        stock = @stock, disponible = @disponible, image_url = @imageUrl
                    WHERE id = @id";

                BindParams(cmd, p);
                cmd.Parameters.AddWithValue("@id", p.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM produits WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        private static void BindParams(MySqlCommand cmd, Produit p)
        {
            cmd.Parameters.AddWithValue("@idCat",      p.IdCategorie);
            cmd.Parameters.AddWithValue("@nom",        p.Nom);
            cmd.Parameters.AddWithValue("@desc",       p.Description  ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@prixTTC",    p.PrixTTC);
            cmd.Parameters.AddWithValue("@prixPromo",  p.PrixPromo.HasValue ? (object)p.PrixPromo.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@stock",      p.Stock);
            cmd.Parameters.AddWithValue("@disponible", p.Disponible ? 1 : 0);
            cmd.Parameters.AddWithValue("@imageUrl",   p.ImageUrl ?? (object)DBNull.Value);
        }

        private static Produit Map(MySqlDataReader r) => new Produit
        {
            Id           = (int)r["id"],
            IdCategorie  = (int)r["id_categorie"],
            NomCategorie = r["nom_categorie"].ToString(),
            Nom          = r["nom"].ToString(),
            Description  = r["description"] == DBNull.Value ? null : r["description"].ToString(),
            PrixTTC      = (decimal)r["prix_ttc"],
            PrixPromo    = r["prix_promo"] == DBNull.Value ? (decimal?)null : (decimal)r["prix_promo"],
            Stock        = (int)r["stock"],
            Disponible   = Convert.ToBoolean(r["disponible"]),
            ImageUrl     = r["image_url"] == DBNull.Value ? null : r["image_url"].ToString()
        };
    }
}
