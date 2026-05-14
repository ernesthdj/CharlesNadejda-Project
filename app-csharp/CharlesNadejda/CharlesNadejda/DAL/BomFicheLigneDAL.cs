using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;
using System;

namespace CharlesNadejda.DAL
{
    public static class BomFicheLigneDAL
    {
        /// <summary>
        /// Charge toutes les lignes d'une fiche avec les infos de l'input référencé (jointure).
        /// </summary>
        public static List<BomFicheLigne> GetByFiche(int idFiche)
        {
            var list = new List<BomFicheLigne>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                // Ligne type 'ingredient' → jointure fiches_ingredients
                // Ligne type 'fiche'      → jointure bom_fiches
                // COALESCE résout les deux cas en une seule requête
                cmd.CommandText = @"
                    SELECT l.id, l.id_fiche, l.type_input,
                           l.id_input_ingredient, l.id_input_fiche,
                           l.quantite, l.unite_mesure,
                           COALESCE(fi.nom,  bf.nom)                   AS nom_input,
                           COALESCE(fi.unite_mesure, bf.unite_output)  AS unite_input,
                           -- Prix par unité de base : pour les ingrédients, prix_achat_reference / qte_par_conditionnement
                           -- Pour les fiches (type='fiche'), le coût unitaire réel est calculé à la production
                           COALESCE(
                               fi.prix_achat_reference / NULLIF(fi.qte_par_conditionnement, 0),
                               0
                           ) AS prix_ref
                    FROM bom_fiches_lignes l
                    LEFT JOIN fiches_ingredients fi ON fi.id = l.id_input_ingredient
                    LEFT JOIN bom_fiches         bf ON bf.id = l.id_input_fiche
                    WHERE l.id_fiche = @idFiche";
                cmd.Parameters.AddWithValue("@idFiche", idFiche);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        /// <summary>
        /// Retourne les noms des fiches qui consomment un ingrédient donné.
        /// </summary>
        public static List<string> GetFichesUtilisant(int idIngredient)
        {
            var list = new List<string>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT DISTINCT f.nom
                    FROM bom_fiches_lignes l
                    INNER JOIN bom_fiches f ON f.id = l.id_fiche
                    WHERE l.id_input_ingredient = @id
                    ORDER BY f.nom";
                cmd.Parameters.AddWithValue("@id", idIngredient);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(r["nom"].ToString());
            }
            return list;
        }

        /// <summary>
        /// Retourne les noms des fiches qui consomment une fiche BOM donnée (niveau supérieur).
        /// </summary>
        public static List<string> GetFichesConsommant(int idFiche)
        {
            var list = new List<string>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT DISTINCT f.nom
                    FROM bom_fiches_lignes l
                    INNER JOIN bom_fiches f ON f.id = l.id_fiche
                    WHERE l.id_input_fiche = @id
                    ORDER BY f.nom";
                cmd.Parameters.AddWithValue("@id", idFiche);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(r["nom"].ToString());
            }
            return list;
        }

        private static BomFicheLigne Map(MySqlDataReader r) => new BomFicheLigne
        {
            Id                 = (int)r["id"],
            IdFiche            = (int)r["id_fiche"],
            TypeInput          = r["type_input"].ToString(),
            IdInputIngredient  = r["id_input_ingredient"] == DBNull.Value ? (int?)null : (int)r["id_input_ingredient"],
            IdInputFiche       = r["id_input_fiche"]       == DBNull.Value ? (int?)null : (int)r["id_input_fiche"],
            Quantite           = (decimal)r["quantite"],
            UniteMesure        = r["unite_mesure"].ToString(),
            NomInput           = r["nom_input"].ToString(),
            UniteMesureInput   = r["unite_input"].ToString(),
            PrixUnitaireRef    = (decimal)r["prix_ref"]
        };
    }
}
