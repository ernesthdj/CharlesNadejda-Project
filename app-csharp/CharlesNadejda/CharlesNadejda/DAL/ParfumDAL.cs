using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class ParfumDAL
    {
        public static List<Parfum> GetAll()
        {
            var list = new List<Parfum>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT p.id, p.nom, p.description, p.type_parfum, p.couleur_hex,
                           p.id_recette, r.nom AS nom_recette, p.disponible
                    FROM parfums p
                    LEFT JOIN fiches_recettes r ON r.id = p.id_recette
                    ORDER BY p.nom";
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
                cmd.CommandText = "SELECT COUNT(*) FROM parfums WHERE nom = @nom AND id <> @id";
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@id",  excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static void Insert(Parfum p)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO parfums (nom, description, type_parfum, couleur_hex, id_recette, disponible)
                                    VALUES (@nom, @desc, @type, @couleur, @idRecette, @dispo)";
                Bind(cmd, p);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Update(Parfum p)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"UPDATE parfums SET nom=@nom, description=@desc, type_parfum=@type,
                                    couleur_hex=@couleur, id_recette=@idRecette, disponible=@dispo
                                    WHERE id=@id";
                Bind(cmd, p);
                cmd.Parameters.AddWithValue("@id", p.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM parfums WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        private static void Bind(MySqlCommand cmd, Parfum p)
        {
            cmd.Parameters.AddWithValue("@nom",      p.Nom);
            cmd.Parameters.AddWithValue("@desc",     p.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@type",     p.TypeParfum  ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@couleur",  p.CouleurHex  ?? "#6F4E37");
            cmd.Parameters.AddWithValue("@idRecette",p.IdRecette.HasValue ? (object)p.IdRecette.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@dispo",    p.Disponible ? 1 : 0);
        }

        private static Parfum Map(MySqlDataReader r) => new Parfum
        {
            Id          = (int)r["id"],
            Nom         = r["nom"].ToString(),
            Description = r["description"] == DBNull.Value ? null : r["description"].ToString(),
            TypeParfum  = r["type_parfum"]  == DBNull.Value ? null : r["type_parfum"].ToString(),
            CouleurHex  = r["couleur_hex"].ToString(),
            IdRecette   = r["id_recette"]   == DBNull.Value ? (int?)null : (int)r["id_recette"],
            NomRecette  = r["nom_recette"]  == DBNull.Value ? null : r["nom_recette"].ToString(),
            Disponible  = Convert.ToBoolean(r["disponible"])
        };
    }
}
