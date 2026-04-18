using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class BomStockDAL
    {
        /// <summary>Liste tout le stock d'un niveau donné.</summary>
        public static List<BomStock> GetByNiveau(int idNiveau)
        {
            var list = new List<BomStock>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT s.id, s.id_niveau, s.id_contexte, s.id_activite,
                           s.id_fiche, s.id_production,
                           s.quantite_disponible, s.cout_unitaire,
                           s.date_production, s.date_dlc, s.date_creation,
                           f.nom AS nom_fiche, f.unite_output,
                           n.nom AS nom_niveau, n.ordre,
                           c.nom AS nom_contexte,
                           a.nom AS nom_activite
                    FROM bom_stocks s
                    INNER JOIN bom_fiches    f ON f.id  = s.id_fiche
                    INNER JOIN bom_niveaux   n ON n.id  = s.id_niveau
                    INNER JOIN bom_contextes c ON c.id  = s.id_contexte
                    INNER JOIN activites     a ON a.id  = s.id_activite
                    WHERE s.id_niveau = @idNiveau AND s.quantite_disponible > 0
                    ORDER BY s.date_production ASC";  // FIFO (First In First Out)
                cmd.Parameters.AddWithValue("@idNiveau", idNiveau);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        /// <summary>
        /// Retourne la quantité totale disponible pour une fiche dans un niveau.
        /// </summary>
        public static decimal GetDisponible(int idNiveau, int idFiche)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT COALESCE(SUM(quantite_disponible), 0)
                    FROM bom_stocks
                    WHERE id_niveau = @idNiveau AND id_fiche = @idFiche";
                cmd.Parameters.AddWithValue("@idNiveau", idNiveau);
                cmd.Parameters.AddWithValue("@idFiche",  idFiche);
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// Retourne la quantité disponible réelle d'un ingrédient (tous lots confondus),
        /// déduction faite de toutes les réservations actives tous contextes.
        /// Formule : SUM(lot.quantite_disponible) - SUM(reservations actives)
        /// </summary>
        public static decimal GetDisponibleIngredient(int idFicheIngredient)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT
                        COALESCE(SUM(l.quantite_disponible), 0)
                        - COALESCE((
                            SELECT SUM(r.quantite_reservee)
                            FROM bom_reservations r
                            INNER JOIN lots_ingredients lr ON lr.id = r.id_lot
                            WHERE lr.id_fiche_ingredient = @idFi AND r.actif = 1
                          ), 0)
                    FROM lots_ingredients l
                    WHERE l.id_fiche_ingredient = @idFi";
                cmd.Parameters.AddWithValue("@idFi", idFicheIngredient);
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// Retourne les lots d'un ingrédient triés FIFO (date_achat ASC),
        /// avec disponibilité nette (déduction réservations par lot).
        /// Utilisé par BomProductionDAL pour la consommation réelle.
        /// </summary>
        public static List<(int IdLot, decimal DispoNette, decimal PrixUnitaire)> GetLotsDispoFIFO(int idFicheIngredient)
        {
            var result = new List<(int, decimal, decimal)>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT l.id,
                           l.quantite_disponible
                           - COALESCE((SELECT SUM(r.quantite_reservee)
                                       FROM bom_reservations r
                                       WHERE r.id_lot = l.id AND r.actif = 1), 0) AS dispo_nette,
                           l.prix_unitaire / NULLIF(fi.qte_par_conditionnement, 0) AS prix_unitaire_base
                    FROM lots_ingredients l
                    INNER JOIN fiches_ingredients fi ON fi.id = l.id_fiche_ingredient
                    WHERE l.id_fiche_ingredient = @idFi
                    ORDER BY l.date_achat ASC";
                cmd.Parameters.AddWithValue("@idFi", idFicheIngredient);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                    {
                        decimal dispo = (decimal)r["dispo_nette"];
                        if (dispo > 0)
                            result.Add(((int)r["id"], dispo, (decimal)r["prix_unitaire_base"]));
                    }
            }
            return result;
        }

        /// <summary>
        /// Retourne les lots de bom_stocks pour une fiche dans un niveau, triés FIFO.
        /// Utilisé pour la consommation au niveau N-1 lors d'une production au niveau N.
        /// </summary>
        public static List<(int IdStock, decimal Dispo, decimal CoutUnitaire)> GetBomStocksFIFO(int idNiveau, int idFiche)
        {
            var result = new List<(int, decimal, decimal)>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT id, quantite_disponible, cout_unitaire
                    FROM bom_stocks
                    WHERE id_niveau = @idNiveau AND id_fiche = @idFiche
                      AND quantite_disponible > 0
                    ORDER BY date_production ASC";
                cmd.Parameters.AddWithValue("@idNiveau", idNiveau);
                cmd.Parameters.AddWithValue("@idFiche",  idFiche);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        result.Add(((int)r["id"], (decimal)r["quantite_disponible"], (decimal)r["cout_unitaire"]));
            }
            return result;
        }

        private static BomStock Map(MySqlDataReader r) => new BomStock
        {
            Id                 = (int)r["id"],
            IdNiveau           = (int)r["id_niveau"],
            IdContexte         = (int)r["id_contexte"],
            IdActivite         = (int)r["id_activite"],
            IdFiche            = (int)r["id_fiche"],
            IdProduction       = (int)r["id_production"],
            QuantiteDisponible = (decimal)r["quantite_disponible"],
            CoutUnitaire       = (decimal)r["cout_unitaire"],
            DateProduction     = (DateTime)r["date_production"],
            DateDlc            = r["date_dlc"]  == DBNull.Value ? (DateTime?)null : (DateTime)r["date_dlc"],
            DateCreation       = (DateTime)r["date_creation"],
            NomFiche           = r["nom_fiche"].ToString(),
            UniteOutput        = r["unite_output"].ToString(),
            NomNiveau          = r["nom_niveau"].ToString(),
            OrdreNiveau        = Convert.ToInt32(r["ordre"]),
            NomContexte        = r["nom_contexte"].ToString(),
            NomActivite        = r["nom_activite"].ToString()
        };
    }
}
