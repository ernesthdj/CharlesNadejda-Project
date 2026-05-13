using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class ActiviteDAL
    {
        /// <summary>Retourne toutes les activités actives, triées par nom.</summary>
        public static List<Activite> GetAll(bool includeInactifs = false)
        {
            var list = new List<Activite>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, nom, description, actif, date_creation FROM activites";
                if (!includeInactifs) cmd.CommandText += " WHERE actif = 1";
                cmd.CommandText += " ORDER BY nom";
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public static Activite GetById(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, nom, description, actif, date_creation FROM activites WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                    if (r.Read()) return Map(r);
            }
            return null;
        }

        public static bool NomExiste(string nom, int excludeId = 0)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM activites WHERE nom = @nom AND id <> @id";
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@id",  excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static int Insert(Activite a)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO activites (nom, description, actif)
                    VALUES (@nom, @desc, 1)";
                Bind(cmd, a);
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public static void Update(Activite a)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE activites SET nom=@nom, description=@desc WHERE id=@id";
                Bind(cmd, a);
                cmd.Parameters.AddWithValue("@id", a.Id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Suppression physique avec cascade complète :
        /// → fiches_ingredients (+ leurs lots, lignes recettes, lignes BOM)
        /// → bom_contextes (+ leurs bom_niveaux)
        /// Géré intégralement par les FK CASCADE en DB (migration v9).
        /// </summary>
        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM activites WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Désactivation soft — lève une exception si des contextes actifs y sont rattachés.
        /// </summary>
        public static void Desactiver(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM bom_contextes WHERE id_activite = @id AND actif = 1";
                cmd.Parameters.AddWithValue("@id", id);
                int nbContextes = Convert.ToInt32(cmd.ExecuteScalar());
                if (nbContextes > 0)
                    throw new InvalidOperationException(
                        $"Impossible de désactiver : {nbContextes} contexte(s) actif(s) rattaché(s) à cette activité.");

                // Vérifier les ingrédients liés via activites_stocks (v10 : id_activite → id_stock + jonction)
                cmd.CommandText = @"
                    SELECT COUNT(*)
                    FROM fiches_ingredients fi
                    JOIN stocks s            ON s.id = fi.id_stock
                    JOIN activites_stocks ast ON ast.id_stock = s.id
                    WHERE ast.id_activite = @id AND fi.actif = 1";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", id);
                int nbIngredients = Convert.ToInt32(cmd.ExecuteScalar());
                if (nbIngredients > 0)
                    throw new InvalidOperationException(
                        $"Impossible de désactiver : {nbIngredients} ingrédient(s) actif(s) rattaché(s) à cette activité.");

                cmd.CommandText = "UPDATE activites SET actif = 0 WHERE id = @id";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        private static void Bind(MySqlCommand cmd, Activite a)
        {
            cmd.Parameters.AddWithValue("@nom",  a.Nom);
            cmd.Parameters.AddWithValue("@desc", a.Description ?? (object)DBNull.Value);
        }

        private static Activite Map(MySqlDataReader r) => new Activite
        {
            Id           = (int)r["id"],
            Nom          = r["nom"].ToString(),
            Description  = r["description"] == DBNull.Value ? null : r["description"].ToString(),
            Actif        = Convert.ToBoolean(r["actif"]),
            DateCreation = (DateTime)r["date_creation"]
        };
    }
}
