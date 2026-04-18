using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class BomNiveauDAL
    {
        private const string SELECT_BASE = @"
            SELECT n.id, n.id_contexte, n.ordre, n.nom, n.description, n.date_creation,
                   c.nom AS nom_contexte, c.id_activite,
                   a.nom AS nom_activite
            FROM bom_niveaux n
            INNER JOIN bom_contextes c ON c.id = n.id_contexte
            INNER JOIN activites     a ON a.id = c.id_activite";

        public static List<BomNiveau> GetByContexte(int idContexte)
        {
            var list = new List<BomNiveau>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_BASE + " WHERE n.id_contexte = @idCtx ORDER BY n.ordre";
                cmd.Parameters.AddWithValue("@idCtx", idContexte);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public static BomNiveau GetById(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_BASE + " WHERE n.id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                    if (r.Read()) return Map(r);
            }
            return null;
        }

        /// <summary>Retourne l'ordre maximum des niveaux du contexte.</summary>
        public static int GetOrdreMax(int idContexte)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COALESCE(MAX(ordre), 0) FROM bom_niveaux WHERE id_contexte = @id";
                cmd.Parameters.AddWithValue("@id", idContexte);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static int Insert(BomNiveau n)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO bom_niveaux (id_contexte, ordre, nom, description)
                    VALUES (@idCtx,
                            (SELECT COALESCE(MAX(ordre),0)+1 FROM bom_niveaux AS sub WHERE sub.id_contexte = @idCtx),
                            @nom, @desc)";
                Bind(cmd, n);
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public static void Update(BomNiveau n)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE bom_niveaux SET nom=@nom, description=@desc WHERE id=@id";
                cmd.Parameters.AddWithValue("@nom",  n.Nom);
                cmd.Parameters.AddWithValue("@desc", n.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@id",   n.Id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Supprime uniquement si ce niveau est le dernier (ordre le plus élevé) du contexte.
        /// Lève une exception métier sinon.
        /// </summary>
        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT COUNT(*) FROM bom_niveaux
                    WHERE id_contexte = (SELECT id_contexte FROM bom_niveaux WHERE id = @id)
                      AND ordre > (SELECT ordre FROM bom_niveaux WHERE id = @id)";
                cmd.Parameters.AddWithValue("@id", id);
                int niveauxSupérieurs = Convert.ToInt32(cmd.ExecuteScalar());

                if (niveauxSupérieurs > 0)
                    throw new InvalidOperationException(
                        "Impossible de supprimer un niveau intermédiaire. " +
                        "Supprimez d'abord les niveaux supérieurs.");

                cmd.CommandText = "DELETE FROM bom_niveaux WHERE id = @id";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public static BomNiveau GetByContexteEtOrdre(int idContexte, int ordre)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_BASE + " WHERE n.id_contexte = @idCtx AND n.ordre = @ordre";
                cmd.Parameters.AddWithValue("@idCtx", idContexte);
                cmd.Parameters.AddWithValue("@ordre", ordre);
                using (var r = cmd.ExecuteReader())
                    if (r.Read()) return Map(r);
            }
            return null;
        }

        private static void Bind(MySqlCommand cmd, BomNiveau n)
        {
            cmd.Parameters.AddWithValue("@idCtx", n.IdContexte);
            cmd.Parameters.AddWithValue("@nom",   n.Nom);
            cmd.Parameters.AddWithValue("@desc",  n.Description ?? (object)DBNull.Value);
        }

        private static BomNiveau Map(MySqlDataReader r) => new BomNiveau
        {
            Id           = (int)r["id"],
            IdContexte   = (int)r["id_contexte"],
            Ordre        = Convert.ToInt32(r["ordre"]),
            Nom          = r["nom"].ToString(),
            Description  = r["description"] == DBNull.Value ? null : r["description"].ToString(),
            DateCreation = (DateTime)r["date_creation"],
            NomContexte  = r["nom_contexte"].ToString(),
            IdActivite   = (int)r["id_activite"],
            ActiviteNom  = r["nom_activite"].ToString()
        };
    }
}
