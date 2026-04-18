using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class FournisseurDAL
    {
        public static List<Fournisseur> GetAll()
        {
            var list = new List<Fournisseur>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, nom, contact, email, telephone, adresse, notes FROM fournisseurs ORDER BY nom";
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
                cmd.CommandText = "SELECT COUNT(*) FROM fournisseurs WHERE nom = @nom AND id <> @id";
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@id",  excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static void Insert(Fournisseur f)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO fournisseurs (nom, contact, email, telephone, adresse, notes)
                                    VALUES (@nom, @contact, @email, @tel, @adresse, @notes)";
                Bind(cmd, f);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Update(Fournisseur f)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"UPDATE fournisseurs SET nom=@nom, contact=@contact, email=@email,
                                    telephone=@tel, adresse=@adresse, notes=@notes WHERE id=@id";
                Bind(cmd, f);
                cmd.Parameters.AddWithValue("@id", f.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM fournisseurs WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        private static void Bind(MySqlCommand cmd, Fournisseur f)
        {
            cmd.Parameters.AddWithValue("@nom",     f.Nom);
            cmd.Parameters.AddWithValue("@contact", f.Contact   ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@email",   f.Email     ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@tel",     f.Telephone ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@adresse", f.Adresse   ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@notes",   f.Notes     ?? (object)DBNull.Value);
        }

        private static Fournisseur Map(MySqlDataReader r) => new Fournisseur
        {
            Id        = (int)r["id"],
            Nom       = r["nom"].ToString(),
            Contact   = r["contact"]   == DBNull.Value ? null : r["contact"].ToString(),
            Email     = r["email"]     == DBNull.Value ? null : r["email"].ToString(),
            Telephone = r["telephone"] == DBNull.Value ? null : r["telephone"].ToString(),
            Adresse   = r["adresse"]   == DBNull.Value ? null : r["adresse"].ToString(),
            Notes     = r["notes"]     == DBNull.Value ? null : r["notes"].ToString()
        };
    }
}
