using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class BomContexteDAL
    {
        private const string SELECT_BASE = @"
            SELECT c.id, c.nom, c.description, c.id_activite, c.actif, c.date_creation,
                   a.nom AS nom_activite
            FROM bom_contextes c
            INNER JOIN activites a ON a.id = c.id_activite";

        /// <summary>idActivite : 0 = tous</summary>
        public static List<BomContexte> GetAll(int idActivite = 0)
        {
            var list = new List<BomContexte>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_BASE + " WHERE c.actif = 1";
                if (idActivite > 0)
                {
                    cmd.CommandText += " AND c.id_activite = @idActivite";
                    cmd.Parameters.AddWithValue("@idActivite", idActivite);
                }
                cmd.CommandText += " ORDER BY a.nom, c.nom";
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public static BomContexte GetById(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_BASE + " WHERE c.id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                    if (r.Read()) return Map(r);
            }
            return null;
        }

        public static bool NomExiste(string nom, int idActivite, int excludeId = 0)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT COUNT(*) FROM bom_contextes
                    WHERE nom = @nom AND id_activite = @idActivite AND id <> @id";
                cmd.Parameters.AddWithValue("@nom",        nom);
                cmd.Parameters.AddWithValue("@idActivite", idActivite);
                cmd.Parameters.AddWithValue("@id",         excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        /// <summary>
        /// Insère un contexte avec ses niveaux en une seule transaction.
        /// nomsNiveaux : liste ordonnée — index 0 = N1. N1 "Ingrédients" garanti minimum.
        /// </summary>
        public static int InsertAvecNiveaux(BomContexte c, List<string> nomsNiveaux)
        {
            using (var conn = DbHelper.GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int idContexte;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            INSERT INTO bom_contextes (nom, description, id_activite, actif)
                            VALUES (@nom, @desc, @idActivite, 1)";
                        Bind(cmd, c);
                        cmd.ExecuteNonQuery();
                        idContexte = (int)cmd.LastInsertedId;
                    }

                    string nomN1 = (nomsNiveaux != null && nomsNiveaux.Count > 0
                                    && !string.IsNullOrWhiteSpace(nomsNiveaux[0]))
                        ? nomsNiveaux[0] : "Ingrédients";

                    var tousLesNoms = new List<string> { nomN1 };
                    if (nomsNiveaux != null)
                        for (int i = 1; i < nomsNiveaux.Count; i++)
                            if (!string.IsNullOrWhiteSpace(nomsNiveaux[i]))
                                tousLesNoms.Add(nomsNiveaux[i].Trim());

                    for (int i = 0; i < tousLesNoms.Count; i++)
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tx;
                            cmd.CommandText = @"
                                INSERT INTO bom_niveaux (id_contexte, ordre, nom, description)
                                VALUES (@idCtx, @ordre, @nom, NULL)";
                            cmd.Parameters.AddWithValue("@idCtx", idContexte);
                            cmd.Parameters.AddWithValue("@ordre", i + 1);
                            cmd.Parameters.AddWithValue("@nom",   tousLesNoms[i]);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                    return idContexte;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        public static int Insert(BomContexte c) => InsertAvecNiveaux(c, null);

        public static void Update(BomContexte c)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE bom_contextes
                    SET nom=@nom, description=@desc, id_activite=@idActivite
                    WHERE id=@id";
                Bind(cmd, c);
                cmd.Parameters.AddWithValue("@id", c.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM bom_contextes WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        private static void Bind(MySqlCommand cmd, BomContexte c)
        {
            cmd.Parameters.AddWithValue("@nom",        c.Nom);
            cmd.Parameters.AddWithValue("@desc",       c.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@idActivite", c.IdActivite);
        }

        private static BomContexte Map(MySqlDataReader r) => new BomContexte
        {
            Id           = (int)r["id"],
            Nom          = r["nom"].ToString(),
            Description  = r["description"] == DBNull.Value ? null : r["description"].ToString(),
            IdActivite   = (int)r["id_activite"],
            ActiviteNom  = r["nom_activite"].ToString(),
            Actif        = Convert.ToBoolean(r["actif"]),
            DateCreation = (DateTime)r["date_creation"]
        };
    }
}
