using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    /// <summary>
    /// DAL lecture seule pour les commandes web.
    /// Les commandes sont créées par le site Laravel, consultées par l'admin dans l'ERP.
    /// </summary>
    public static class CommandeWebDAL
    {
        private const string SELECT_BASE = @"
            SELECT cmd.id, cmd.id_client, cmd.statut, cmd.total_ttc,
                   cmd.adresse_livraison, cmd.date_commande, cmd.date_creation,
                   cl.nom       AS nom_client,
                   cl.prenom    AS prenom_client,
                   cl.email     AS email_client,
                   (SELECT COUNT(*) FROM commandes_web_lignes l WHERE l.id_commande = cmd.id) AS nb_articles
            FROM commandes_web cmd
            INNER JOIN clients cl ON cl.id = cmd.id_client";

        // ── SELECT ──────────────────────────────────────────────

        /// <summary>
        /// Liste les commandes (hors paniers en cours).
        /// Filtre optionnel par statut ('payee', 'annulee').
        /// </summary>
        public static List<CommandeWeb> GetAll(string filtreStatut = null)
        {
            var list = new List<CommandeWeb>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                var where = "WHERE cmd.statut <> 'panier'";
                if (!string.IsNullOrEmpty(filtreStatut))
                {
                    where += " AND cmd.statut = @statut";
                    cmd.Parameters.AddWithValue("@statut", filtreStatut);
                }

                cmd.CommandText = SELECT_BASE + " " + where + @"
                    ORDER BY cmd.date_commande DESC";
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapHeader(r));
            }
            return list;
        }

        /// <summary>
        /// Charge une commande avec ses lignes détaillées.
        /// </summary>
        public static CommandeWeb GetById(int id)
        {
            CommandeWeb commande = null;

            using (var conn = DbHelper.GetConnection())
            {
                // Header
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = SELECT_BASE + " WHERE cmd.id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        if (r.Read()) commande = MapHeader(r);
                }

                if (commande == null) return null;

                // Lignes
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT l.id, l.id_commande, l.id_produit_web,
                               l.quantite, l.prix_unitaire, l.sous_total,
                               p.nom_commercial AS nom_produit
                        FROM commandes_web_lignes l
                        INNER JOIN produits_web p ON p.id = l.id_produit_web
                        WHERE l.id_commande = @id
                        ORDER BY l.id";
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) commande.Lignes.Add(MapLigne(r));
                }
            }
            return commande;
        }

        /// <summary>
        /// Statistiques rapides pour le dashboard.
        /// </summary>
        public static int GetCountByStatut(string statut)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM commandes_web WHERE statut = @statut";
                cmd.Parameters.AddWithValue("@statut", statut);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // ── HELPERS ─────────────────────────────────────────────

        private static CommandeWeb MapHeader(MySqlDataReader r) => new CommandeWeb
        {
            Id               = (int)r["id"],
            IdClient         = (int)r["id_client"],
            Statut           = r["statut"].ToString(),
            TotalTtc         = (decimal)r["total_ttc"],
            AdresseLivraison = r["adresse_livraison"] == DBNull.Value ? null : r["adresse_livraison"].ToString(),
            DateCommande     = r["date_commande"]     == DBNull.Value ? (DateTime?)null : (DateTime)r["date_commande"],
            DateCreation     = (DateTime)r["date_creation"],
            NomClient        = r["nom_client"].ToString(),
            PrenomClient     = r["prenom_client"].ToString(),
            EmailClient      = r["email_client"].ToString(),
            NbArticles       = Convert.ToInt32(r["nb_articles"])
        };

        private static CommandeWebLigne MapLigne(MySqlDataReader r) => new CommandeWebLigne
        {
            Id            = (int)r["id"],
            IdCommande    = (int)r["id_commande"],
            IdProduitWeb  = (int)r["id_produit_web"],
            Quantite      = (int)r["quantite"],
            PrixUnitaire  = (decimal)r["prix_unitaire"],
            SousTotal     = (decimal)r["sous_total"],
            NomProduit    = r["nom_produit"].ToString()
        };
    }
}
