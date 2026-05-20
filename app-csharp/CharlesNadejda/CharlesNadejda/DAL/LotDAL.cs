using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class LotDAL
    {
        private const string SELECT_BASE = @"
            SELECT l.id, l.id_fiche_ingredient, fi.nom AS nom_ingredient,
                   fi.unite_mesure, fi.conditionnement_label, fi.qte_par_conditionnement,
                   l.nb_conditionnements,
                   l.numero_lot, l.id_fournisseur, f.nom AS nom_fournisseur,
                   l.date_achat, l.date_peremption, l.quantite_initiale,
                   l.quantite_disponible, l.prix_unitaire, l.prix_achat_reel,
                   l.tva_pct, l.reference_facture, l.notes
            FROM lots_ingredients l
            INNER JOIN fiches_ingredients fi ON fi.id = l.id_fiche_ingredient
            LEFT JOIN fournisseurs f ON f.id = l.id_fournisseur";

        /// <summary>idActivite : 0 = tous / filtre via activites_stocks → fiches_ingredients</summary>
        public static List<Lot> GetAll(int idActivite = 0)
        {
            var list = new List<Lot>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_BASE + " WHERE 1=1";

                if (idActivite > 0)
                {
                    cmd.CommandText += @" AND fi.id_stock IN (
                        SELECT id_stock FROM activites_stocks WHERE id_activite = @idActivite)";
                    cmd.Parameters.AddWithValue("@idActivite", idActivite);
                }

                cmd.CommandText += " ORDER BY l.date_achat DESC";

                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        /// <summary>Tous les lots d'une fiche ingrédient (pour détail agrégé Vue Stock).</summary>
        public static List<Lot> GetByFicheIngredient(int idFiche)
        {
            var list = new List<Lot>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_BASE + @"
                    WHERE l.id_fiche_ingredient = @idFiche
                    ORDER BY l.date_peremption ASC, l.date_achat DESC";
                cmd.Parameters.AddWithValue("@idFiche", idFiche);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public static Lot GetById(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_BASE + " WHERE l.id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? Map(r) : null;
            }
        }

        public static void Insert(Lot lot)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO lots_ingredients
                        (id_fiche_ingredient, nb_conditionnements,
                         numero_lot, id_fournisseur, date_achat,
                         date_peremption, quantite_initiale, quantite_disponible,
                         prix_unitaire, prix_achat_reel, tva_pct, reference_facture, notes)
                    VALUES (@idFi, @nbCond,
                            @numeroLot, @idFourn, @dateAchat, @datePer,
                            @qteInit, @qteInit, @prixUnit, @prixTotal, @tvaPct, @refFact, @notes)";

                Bind(cmd, lot);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Update(Lot lot)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                // quantite_disponible = nouvelle_initiale - quantité déjà consommée
                // consommée = ancienne_initiale - ancienne_disponible (calculé côté SQL)
                cmd.CommandText = @"
                    UPDATE lots_ingredients SET
                        id_fiche_ingredient = @idFi,
                        nb_conditionnements = @nbCond,
                        numero_lot          = @numeroLot,
                        id_fournisseur      = @idFourn,
                        date_achat          = @dateAchat,
                        date_peremption     = @datePer,
                        quantite_initiale   = @qteInit,
                        quantite_disponible = GREATEST(0, @qteInit - (quantite_initiale - quantite_disponible)),
                        prix_unitaire       = @prixUnit,
                        prix_achat_reel     = @prixTotal,
                        tva_pct             = @tvaPct,
                        reference_facture   = @refFact,
                        notes               = @notes
                    WHERE id = @id";

                Bind(cmd, lot);
                cmd.Parameters.AddWithValue("@id", lot.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM lots_ingredients WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void Bind(MySqlCommand cmd, Lot lot)
        {
            cmd.Parameters.AddWithValue("@idFi",      lot.IdFicheIngredient);
            cmd.Parameters.AddWithValue("@nbCond",    lot.NbConditionnements);
            cmd.Parameters.AddWithValue("@numeroLot", lot.NumeroLot       ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@idFourn",   lot.IdFournisseur.HasValue ? (object)lot.IdFournisseur.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@dateAchat", lot.DateAchat);
            cmd.Parameters.AddWithValue("@datePer",   lot.DatePeremption.HasValue ? (object)lot.DatePeremption.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@qteInit",   lot.QuantiteInitiale);
            cmd.Parameters.AddWithValue("@prixUnit",  lot.PrixUnitaire);
            cmd.Parameters.AddWithValue("@prixTotal", lot.PrixAchatReel);
            cmd.Parameters.AddWithValue("@tvaPct",    lot.TvaPct);
            cmd.Parameters.AddWithValue("@refFact",   lot.ReferenceFacture ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@notes",     lot.Notes            ?? (object)DBNull.Value);
        }

        private static Lot Map(MySqlDataReader r) => new Lot
        {
            Id                    = (int)r["id"],
            IdFicheIngredient     = (int)r["id_fiche_ingredient"],
            NomIngredient         = r["nom_ingredient"].ToString(),
            UniteMesure           = r["unite_mesure"].ToString(),
            ConditionnementLabel  = r["conditionnement_label"].ToString(),
            QteParConditionnement = (decimal)r["qte_par_conditionnement"],
            NbConditionnements    = (decimal)r["nb_conditionnements"],
            NumeroLot             = r["numero_lot"]       == DBNull.Value ? null : r["numero_lot"].ToString(),
            IdFournisseur         = r["id_fournisseur"]   == DBNull.Value ? (int?)null : (int)r["id_fournisseur"],
            NomFournisseur        = r["nom_fournisseur"]  == DBNull.Value ? null : r["nom_fournisseur"].ToString(),
            DateAchat             = (DateTime)r["date_achat"],
            DatePeremption        = r["date_peremption"]  == DBNull.Value ? (DateTime?)null : (DateTime)r["date_peremption"],
            QuantiteInitiale      = (decimal)r["quantite_initiale"],
            QuantiteDisponible    = (decimal)r["quantite_disponible"],
            PrixUnitaire          = (decimal)r["prix_unitaire"],
            PrixAchatReel         = (decimal)r["prix_achat_reel"],
            TvaPct                = r["tva_pct"] == DBNull.Value ? 0m : (decimal)r["tva_pct"],
            ReferenceFacture      = r["reference_facture"] == DBNull.Value ? null : r["reference_facture"].ToString(),
            Notes                 = r["notes"]             == DBNull.Value ? null : r["notes"].ToString()
        };
    }
}
