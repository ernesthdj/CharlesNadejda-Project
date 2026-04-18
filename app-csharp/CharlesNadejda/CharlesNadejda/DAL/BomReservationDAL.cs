using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class BomReservationDAL
    {
        public static List<BomReservation> GetByContexte(int idContexte)
        {
            var list = new List<BomReservation>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT r.id, r.id_lot, r.id_contexte, r.quantite_reservee,
                           r.date_reservation, r.notes, r.actif,
                           fi.nom AS nom_ingredient, fi.unite_mesure,
                           c.nom  AS nom_contexte
                    FROM bom_reservations r
                    INNER JOIN lots_ingredients l  ON l.id  = r.id_lot
                    INNER JOIN fiches_ingredients fi ON fi.id = l.id_fiche_ingredient
                    INNER JOIN bom_contextes c      ON c.id  = r.id_contexte
                    WHERE r.id_contexte = @idCtx AND r.actif = 1
                    ORDER BY fi.nom";
                cmd.Parameters.AddWithValue("@idCtx", idContexte);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        /// <summary>
        /// Retourne la quantité totale réservée (tous contextes confondus) pour un lot donné.
        /// Utilisé pour calculer la disponibilité réelle : dispo = lot.quantite_disponible - résultat.
        /// </summary>
        public static decimal GetTotalReservePourLot(int idLot)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT COALESCE(SUM(quantite_reservee), 0)
                    FROM bom_reservations
                    WHERE id_lot = @idLot AND actif = 1";
                cmd.Parameters.AddWithValue("@idLot", idLot);
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        public static int Insert(BomReservation res)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO bom_reservations (id_lot, id_contexte, quantite_reservee, notes, actif)
                    VALUES (@idLot, @idCtx, @qte, @notes, 1)";
                Bind(cmd, res);
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public static void Update(BomReservation res)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE bom_reservations
                    SET quantite_reservee=@qte, notes=@notes
                    WHERE id=@id";
                cmd.Parameters.AddWithValue("@qte",   res.QuantiteReservee);
                cmd.Parameters.AddWithValue("@notes", res.Notes ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@id",    res.Id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Désactive une réservation (soft delete — libère la quantité réservée).</summary>
        public static void Liberer(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE bom_reservations SET actif = 0 WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Libère toutes les réservations actives d'un contexte.</summary>
        public static void LibererToutContexte(int idContexte)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE bom_reservations SET actif = 0 WHERE id_contexte = @id";
                cmd.Parameters.AddWithValue("@id", idContexte);
                cmd.ExecuteNonQuery();
            }
        }

        private static void Bind(MySqlCommand cmd, BomReservation res)
        {
            cmd.Parameters.AddWithValue("@idLot", res.IdLot);
            cmd.Parameters.AddWithValue("@idCtx", res.IdContexte);
            cmd.Parameters.AddWithValue("@qte",   res.QuantiteReservee);
            cmd.Parameters.AddWithValue("@notes", res.Notes ?? (object)DBNull.Value);
        }

        private static BomReservation Map(MySqlDataReader r) => new BomReservation
        {
            Id               = (int)r["id"],
            IdLot            = (int)r["id_lot"],
            IdContexte       = (int)r["id_contexte"],
            QuantiteReservee = (decimal)r["quantite_reservee"],
            DateReservation  = (DateTime)r["date_reservation"],
            Notes            = r["notes"]         == DBNull.Value ? null : r["notes"].ToString(),
            Actif            = Convert.ToBoolean(r["actif"]),
            NomIngredient    = r["nom_ingredient"].ToString(),
            UniteMesure      = r["unite_mesure"].ToString(),
            NomContexte      = r["nom_contexte"].ToString()
        };
    }
}
