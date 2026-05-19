using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class ProduitWebDAL
    {
        private const string SELECT_BASE = @"
            SELECT p.id, p.id_bom_fiche, p.id_categorie, p.nom_commercial,
                   p.description, p.prix_vente, p.image_path, p.en_vente,
                   p.ordre_affichage, p.date_creation,
                   f.nom          AS nom_fiche,
                   c.nom          AS nom_categorie,
                   COALESCE(SUM(bs.quantite_disponible), 0) AS stock_disponible
            FROM produits_web p
            INNER JOIN bom_fiches f       ON f.id = p.id_bom_fiche
            LEFT  JOIN categories_web c   ON c.id = p.id_categorie
            LEFT  JOIN bom_stocks bs      ON bs.id_fiche = p.id_bom_fiche
                                          AND bs.quantite_disponible > 0";

        private const string GROUP_ORDER = @"
            GROUP BY p.id
            ORDER BY p.ordre_affichage, p.nom_commercial";

        // ── SELECT ──────────────────────────────────────────────

        public static List<ProduitWeb> GetAll()
        {
            var list = new List<ProduitWeb>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_BASE + GROUP_ORDER;
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public static List<ProduitWeb> GetEnVente()
        {
            var list = new List<ProduitWeb>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_BASE + @"
                    WHERE p.en_vente = 1" + GROUP_ORDER;
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public static ProduitWeb GetById(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_BASE + @"
                    WHERE p.id = @id
                    GROUP BY p.id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? Map(r) : null;
            }
        }

        /// <summary>
        /// Retourne les fiches BOM qui n'ont pas encore été publiées en boutique.
        /// Utilisé par le ComboBox de sélection dans FrmProduitWebEdit.
        /// </summary>
        public static List<BomFiche> GetFichesNonPubliees()
        {
            var list = new List<BomFiche>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT f.id, f.nom, f.unite_output, f.quantite_output
                    FROM bom_fiches f
                    WHERE f.actif = 1
                      AND f.id NOT IN (SELECT id_bom_fiche FROM produits_web)
                    ORDER BY f.nom";
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new BomFiche
                        {
                            Id             = (int)r["id"],
                            Nom            = r["nom"].ToString(),
                            UniteOutput    = r["unite_output"].ToString(),
                            QuantiteOutput = (decimal)r["quantite_output"]
                        });
            }
            return list;
        }

        // ── INSERT ──────────────────────────────────────────────

        public static int Insert(ProduitWeb p)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO produits_web
                        (id_bom_fiche, id_categorie, nom_commercial, description,
                         prix_vente, image_path, en_vente, ordre_affichage)
                    VALUES
                        (@idFiche, @idCat, @nom, @desc,
                         @prix, @img, @enVente, @ordre)";
                Bind(cmd, p);
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        // ── UPDATE ──────────────────────────────────────────────

        public static void Update(ProduitWeb p)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE produits_web
                    SET id_categorie = @idCat, nom_commercial = @nom,
                        description = @desc, prix_vente = @prix,
                        image_path = @img, en_vente = @enVente,
                        ordre_affichage = @ordre
                    WHERE id = @id";
                Bind(cmd, p);
                cmd.Parameters.AddWithValue("@id", p.Id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Toggle publié / dépublié sans ouvrir le formulaire d'édition.
        /// </summary>
        public static void ToggleEnVente(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE produits_web SET en_vente = NOT en_vente WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ── DELETE ──────────────────────────────────────────────

        /// <summary>
        /// Vérifie qu'aucune commande ne référence ce produit avant suppression.
        /// </summary>
        public static bool PeutSupprimer(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT COUNT(*) FROM commandes_web_lignes
                    WHERE id_produit_web = @id";
                cmd.Parameters.AddWithValue("@id", id);
                return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
            }
        }

        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM produits_web WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ── HELPERS ─────────────────────────────────────────────

        private static void Bind(MySqlCommand cmd, ProduitWeb p)
        {
            cmd.Parameters.AddWithValue("@idFiche", p.IdBomFiche);
            cmd.Parameters.AddWithValue("@idCat",   p.IdCategorie.HasValue ? (object)p.IdCategorie.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@nom",     p.NomCommercial);
            cmd.Parameters.AddWithValue("@desc",    p.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@prix",    p.PrixVente);
            cmd.Parameters.AddWithValue("@img",     p.ImagePath   ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@enVente", p.EnVente);
            cmd.Parameters.AddWithValue("@ordre",   p.OrdreAffichage);
        }

        private static ProduitWeb Map(MySqlDataReader r) => new ProduitWeb
        {
            Id              = (int)r["id"],
            IdBomFiche      = (int)r["id_bom_fiche"],
            IdCategorie     = r["id_categorie"]    == DBNull.Value ? (int?)null : (int)r["id_categorie"],
            NomCommercial   = r["nom_commercial"].ToString(),
            Description     = r["description"]     == DBNull.Value ? null : r["description"].ToString(),
            PrixVente       = (decimal)r["prix_vente"],
            ImagePath       = r["image_path"]      == DBNull.Value ? null : r["image_path"].ToString(),
            EnVente         = Convert.ToBoolean(r["en_vente"]),
            OrdreAffichage  = Convert.ToInt32(r["ordre_affichage"]),
            DateCreation    = (DateTime)r["date_creation"],
            NomFiche        = r["nom_fiche"].ToString(),
            NomCategorie    = r["nom_categorie"]   == DBNull.Value ? null : r["nom_categorie"].ToString(),
            StockDisponible = Convert.ToDecimal(r["stock_disponible"])
        };
    }
}
