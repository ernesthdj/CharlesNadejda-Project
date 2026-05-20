using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    /// <summary>
    /// Accès en lecture seule à la VIEW vue_stock_global.
    /// Fournit des filtres par activité, contexte et niveau.
    ///
    /// Règle de filtrage par activité :
    ///   - lots           : id_stock IN (activites_stocks WHERE id_activite = @id)
    ///   - produits       : id_activite = @id (direct, v11)
    /// </summary>
    public static class VueStockGlobalDAL
    {
        // Fragment SQL commun : JOIN activites pour récupérer le nom de l'activité.
        // Pour les lots (id_activite IS NULL), LEFT JOIN retourne NULL → NomActivite = null.
        private const string SELECT_BASE = @"
            SELECT vsg.*, a.nom AS nom_activite
            FROM vue_stock_global vsg
            LEFT JOIN activites a ON a.id = vsg.id_activite";

        /// <summary>Tout le stock, trié par type puis DLC croissante.</summary>
        public static List<VueStockGlobal> GetAll()
            => Execute(SELECT_BASE + @"
                ORDER BY vsg.type_stock, vsg.date_dlc ASC");

        /// <summary>
        /// Stock accessible à une activité donnée.
        /// Lots : filtre via activites_stocks (M:N stock ↔ activité).
        /// Produits fabriqués : filtre via id_activite direct (v11).
        /// </summary>
        public static List<VueStockGlobal> GetByActivite(int idActivite)
            => Execute(SELECT_BASE + @"
                WHERE (    vsg.type_stock = 'lot_ingredient'
                       AND vsg.id_stock IN (
                               SELECT id_stock FROM activites_stocks
                               WHERE id_activite = @p))
                   OR (    vsg.type_stock = 'produit_fabrique'
                       AND vsg.id_activite = @p)
                ORDER BY vsg.type_stock, vsg.date_dlc ASC",
                ("@p", (object)idActivite));

        /// <summary>
        /// Ingrédients réservés pour un contexte précis.
        /// Retourne uniquement les lots avec réservation active pour ce contexte.
        /// </summary>
        public static List<VueStockGlobal> GetByContexte(int idContexte)
            => Execute(@"
                SELECT vsg.*, a.nom AS nom_activite
                FROM vue_stock_global vsg
                LEFT JOIN activites a ON a.id = vsg.id_activite
                JOIN bom_reservations br
                    ON  br.id_lot      = vsg.id_entree
                    AND br.id_contexte = @p
                    AND br.actif       = 1
                WHERE vsg.type_stock = 'lot_ingredient'
                ORDER BY vsg.date_dlc ASC",
                ("@p", (object)idContexte));

        /// <summary>Stock fabriqué d'un niveau BOM donné.</summary>
        public static List<VueStockGlobal> GetByNiveau(int idNiveau)
            => Execute(SELECT_BASE + @"
                WHERE vsg.id_niveau = @p
                ORDER BY vsg.date_dlc ASC",
                ("@p", (object)idNiveau));

        // ── Agrégation par fiche ingrédient ──────────────────────────────

        /// <summary>
        /// Agrège les lots par fiche ingrédient : 1 ligne = 1 ingrédient (somme de tous ses lots).
        /// Les produits fabriqués passent tels quels.
        /// </summary>
        public static List<VueStockGlobal> AgregerParFiche(List<VueStockGlobal> source)
        {
            var result = new List<VueStockGlobal>();

            // Produits fabriqués : pas d'agrégation
            result.AddRange(source.Where(l => !l.EstLot));

            // Lots : grouper par IdFicheIngredient
            var groupes = source
                .Where(l => l.EstLot && l.IdFicheIngredient.HasValue)
                .GroupBy(l => l.IdFicheIngredient.Value);

            foreach (var g in groupes)
            {
                var premier  = g.First();
                var totalQte = g.Sum(l => l.QuantiteTotale);
                var totalRes = g.Sum(l => l.QuantiteReservee);

                decimal coutMoyen = totalQte > 0
                    ? g.Sum(l => l.CoutUnitaire * l.QuantiteTotale) / totalQte
                    : 0m;

                result.Add(new VueStockGlobal
                {
                    TypeStock             = premier.TypeStock,
                    IdEntree              = premier.IdEntree,
                    Nom                   = premier.Nom,
                    Unite                 = premier.Unite,
                    QuantiteTotale        = totalQte,
                    QuantiteReservee      = totalRes,
                    QuantiteDispoReelle   = totalQte - totalRes,
                    CoutUnitaire          = coutMoyen,
                    PrixConditionnement   = null,
                    QteParConditionnement = premier.QteParConditionnement,
                    ConditionnementLabel  = premier.ConditionnementLabel,
                    DateDlc               = g.Any(l => l.DateDlc.HasValue)
                                             ? g.Where(l => l.DateDlc.HasValue).Min(l => l.DateDlc.Value)
                                             : (DateTime?)null,
                    IdStock               = premier.IdStock,
                    StockNom              = premier.StockNom,
                    IdFicheIngredient     = g.Key,
                    IdActivite            = null,
                    NomActivite           = null,
                    IdContexte            = null,
                    IdNiveau              = null,
                    IdFicheBom            = null
                });
            }

            // Lots sans fiche (cas improbable)
            result.AddRange(source.Where(l => l.EstLot && !l.IdFicheIngredient.HasValue));

            return result;
        }

        // ── Helpers privés ────────────────────────────────────────────────

        private static List<VueStockGlobal> Execute(string sql,
            params (string Name, object Value)[] parameters)
        {
            var list = new List<VueStockGlobal>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                foreach (var (name, value) in parameters)
                    cmd.Parameters.AddWithValue(name, value);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        private static VueStockGlobal Map(MySqlDataReader r) => new VueStockGlobal
        {
            TypeStock           = r["type_stock"].ToString(),
            IdEntree            = Convert.ToInt32(r["id_entree"]),
            Nom                 = r["nom"].ToString(),
            Unite               = r["unite"].ToString(),
            QuantiteTotale      = Convert.ToDecimal(r["quantite_totale"]),
            QuantiteReservee    = Convert.ToDecimal(r["quantite_reservee"]),
            QuantiteDispoReelle = Convert.ToDecimal(r["quantite_dispo_reelle"]),
            CoutUnitaire        = Convert.ToDecimal(r["cout_unitaire"]),
            PrixConditionnement = r["prix_conditionnement"] == DBNull.Value
                                    ? (decimal?)null : Convert.ToDecimal(r["prix_conditionnement"]),
            QteParConditionnement = r["qte_par_conditionnement"] == DBNull.Value
                                    ? (decimal?)null : Convert.ToDecimal(r["qte_par_conditionnement"]),
            ConditionnementLabel = r["conditionnement_label"] == DBNull.Value
                                    ? null : r["conditionnement_label"].ToString(),
            DateDlc             = r["date_dlc"]      == DBNull.Value
                                    ? (DateTime?)null : (DateTime)r["date_dlc"],
            IdStock             = r["id_stock"]      == DBNull.Value
                                    ? (int?)null : Convert.ToInt32(r["id_stock"]),
            StockNom            = r["stock_nom"]     == DBNull.Value
                                    ? null : r["stock_nom"].ToString(),
            IdFicheIngredient   = r["id_fiche_ingredient"] == DBNull.Value
                                    ? (int?)null : Convert.ToInt32(r["id_fiche_ingredient"]),
            IdActivite          = r["id_activite"]   == DBNull.Value
                                    ? (int?)null : Convert.ToInt32(r["id_activite"]),
            NomActivite         = r["nom_activite"]  == DBNull.Value
                                    ? null : r["nom_activite"].ToString(),
            IdContexte          = r["id_contexte"]   == DBNull.Value
                                    ? (int?)null : Convert.ToInt32(r["id_contexte"]),
            IdNiveau            = r["id_niveau"]     == DBNull.Value
                                    ? (int?)null : Convert.ToInt32(r["id_niveau"]),
            IdFicheBom          = r["id_fiche_bom"]  == DBNull.Value
                                    ? (int?)null : Convert.ToInt32(r["id_fiche_bom"])
        };
    }
}
