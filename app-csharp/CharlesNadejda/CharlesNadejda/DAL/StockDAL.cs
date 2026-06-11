using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    /// <summary>
    /// CRUD sur les emplacements de stockage physiques (table stocks).
    /// Gère aussi la liaison M:N avec les activités via activites_stocks (LierActivite/DelierActivite).
    /// </summary>
    public static class StockDAL
    {
        // ── SELECT ──────────────────────────────────────────────────

        public static List<Stock> GetAll(bool includeInactifs = false)
        {
            var list = new List<Stock>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, nom, description, actif, date_creation FROM stocks";
                if (!includeInactifs) cmd.CommandText += " WHERE actif = 1";
                cmd.CommandText += " ORDER BY nom";
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public static Stock GetById(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, nom, description, actif, date_creation FROM stocks WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                    if (r.Read()) return Map(r);
            }
            return null;
        }

        /// <summary>Retourne les stocks liés à une activité (via activites_stocks).</summary>
        public static List<Stock> GetByActivite(int idActivite)
        {
            var list = new List<Stock>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT s.id, s.nom, s.description, s.actif, s.date_creation
                    FROM stocks s
                    INNER JOIN activites_stocks acs ON acs.id_stock = s.id
                    WHERE acs.id_activite = @idActivite AND s.actif = 1
                    ORDER BY s.nom";
                cmd.Parameters.AddWithValue("@idActivite", idActivite);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public static bool NomExiste(string nom, int excludeId = 0)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM stocks WHERE nom = @nom AND id <> @id";
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@id",  excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ── INSERT / UPDATE / DELETE ─────────────────────────────────

        /// <summary>Insère un stock et retourne son ID généré.</summary>
        public static int Insert(Stock s)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO stocks (nom, description, actif) VALUES (@nom, @desc, 1)";
                Bind(cmd, s);
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public static void Update(Stock s)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE stocks SET nom = @nom, description = @desc WHERE id = @id";
                Bind(cmd, s);
                cmd.Parameters.AddWithValue("@id", s.Id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Suppression physique — bloquée si le stock contient des ingrédients.
        /// </summary>
        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                // Vérification : lots_ingredients liés à ce stock
                cmd.CommandText = @"SELECT COUNT(*) FROM lots_ingredients WHERE id_stock = @id";
                cmd.Parameters.AddWithValue("@id", id);
                int nb = Convert.ToInt32(cmd.ExecuteScalar());
                if (nb > 0)
                    throw new InvalidOperationException(
                        $"Impossible de supprimer : ce stock contient {nb} lot(s) d'ingrédients.\n" +
                        "Déplacez ou supprimez les lots avant de supprimer le stock.");

                // Vérification 2 : lots avec du stock disponible
                cmd.CommandText = @"SELECT COUNT(*) FROM lots_ingredients
                                    WHERE id_stock = @id AND quantite_disponible > 0";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", id);
                int nbLots = Convert.ToInt32(cmd.ExecuteScalar());
                if (nbLots > 0)
                    throw new InvalidOperationException(
                        $"Impossible de supprimer : ce stock contient {nbLots} lot(s) d'ingrédients actifs.");

                cmd.CommandText = "DELETE FROM stocks WHERE id = @id";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ── Liaison M:N activites_stocks ─────────────────────────────

        /// <summary>Lie une activité à un stock (jonction M:N).</summary>
        public static void LierActivite(int idActivite, int idStock)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                cmd.CommandText = @"INSERT IGNORE INTO activites_stocks (id_activite, id_stock)
                                    VALUES (@idActivite, @idStock)";
                cmd.Parameters.AddWithValue("@idActivite", idActivite);
                cmd.Parameters.AddWithValue("@idStock",    idStock);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Délie une activité d'un stock.</summary>
        public static void DelierActivite(int idActivite, int idStock)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM activites_stocks WHERE id_activite = @idActivite AND id_stock = @idStock";
                cmd.Parameters.AddWithValue("@idActivite", idActivite);
                cmd.Parameters.AddWithValue("@idStock",    idStock);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Retourne les ids des activités liées à un stock via activites_stocks.
        /// Utilisé par FrmStocks pour initialiser les checkboxes du panel de liaison.
        /// </summary>
        /// <param name="idStock">Id du stock.</param>
        /// <returns>Liste des ids d'activités liées.</returns>
        public static List<int> GetActivitesLiees(int idStock)
        {
            var list = new List<int>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id_activite FROM activites_stocks WHERE id_stock = @id";
                cmd.Parameters.AddWithValue("@id", idStock);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Convert.ToInt32(r["id_activite"]));
            }
            return list;
        }

        /// <summary>
        /// Vérifie si le stock contient des fiches d'ingrédients ou des lots actifs.
        /// Retourne true si la suppression doit être bloquée.
        /// </summary>
        public static bool ContientDonnees(int idStock)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT
                        (SELECT COUNT(*) FROM lots_ingredients WHERE id_stock = @id) +
                        (SELECT COUNT(*) FROM lots_ingredients
                         WHERE id_stock = @id AND quantite_disponible > 0)
                    AS total";
                cmd.Parameters.AddWithValue("@id", idStock);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ── Helpers privés ───────────────────────────────────────────

        private static void Bind(MySqlCommand cmd, Stock s)
        {
            cmd.Parameters.AddWithValue("@nom",  s.Nom);
            cmd.Parameters.AddWithValue("@desc", s.Description ?? (object)DBNull.Value);
        }

        private static Stock Map(MySqlDataReader r) => new Stock
        {
            Id           = (int)r["id"],
            Nom          = r["nom"].ToString(),
            Description  = r["description"] == DBNull.Value ? null : r["description"].ToString(),
            Actif        = Convert.ToBoolean(r["actif"]),
            DateCreation = (DateTime)r["date_creation"]
        };
    }
}
