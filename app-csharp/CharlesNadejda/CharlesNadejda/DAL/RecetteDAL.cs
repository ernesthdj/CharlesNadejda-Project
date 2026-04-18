using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class RecetteDAL
    {
        public static List<Recette> GetAll()
        {
            var list = new List<Recette>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT id, nom, description, type_rendement, rendement_quantite,
                           temps_preparation, actif
                    FROM fiches_recettes WHERE actif = 1 ORDER BY nom";
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapHeader(r));
            }
            return list;
        }

        public static Recette GetById(int id)
        {
            Recette recette = null;
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT id, nom, description, type_rendement, rendement_quantite,
                           temps_preparation, actif
                    FROM fiches_recettes WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                    if (r.Read()) recette = MapHeader(r);
            }
            if (recette != null)
                recette.Ingredients = GetIngredients(recette.Id);
            return recette;
        }

        public static List<RecetteIngredient> GetIngredients(int idRecette)
        {
            var list = new List<RecetteIngredient>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT ri.id_recette, ri.id_fiche_ingredient, fi.nom AS nom_ingredient,
                           fi.unite_mesure, ri.quantite, fi.prix_achat_reference
                    FROM recettes_ingredients ri
                    INNER JOIN fiches_ingredients fi ON fi.id = ri.id_fiche_ingredient
                    WHERE ri.id_recette = @id";
                cmd.Parameters.AddWithValue("@id", idRecette);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new RecetteIngredient
                        {
                            IdRecette     = (int)r["id_recette"],
                            IdIngredient  = (int)r["id_fiche_ingredient"],
                            NomIngredient = r["nom_ingredient"].ToString(),
                            UniteMesure   = r["unite_mesure"].ToString(),
                            Quantite      = (decimal)r["quantite"],
                            PrixUnitaire  = (decimal)r["prix_achat_reference"]
                        });
            }
            return list;
        }

        public static bool NomExiste(string nom, int excludeId = 0)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM fiches_recettes WHERE nom = @nom AND id <> @id";
                cmd.Parameters.AddWithValue("@nom", nom);
                cmd.Parameters.AddWithValue("@id",  excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static int Insert(Recette r)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO fiches_recettes (nom, description, type_rendement, rendement_quantite, temps_preparation, actif)
                    VALUES (@nom, @desc, @type, @rendement, @temps, 1)";
                BindHeader(cmd, r);
                cmd.ExecuteNonQuery();
                return (int)cmd.LastInsertedId;
            }
        }

        public static void Update(Recette r)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE fiches_recettes
                    SET nom=@nom, description=@desc, type_rendement=@type,
                        rendement_quantite=@rendement, temps_preparation=@temps
                    WHERE id=@id";
                BindHeader(cmd, r);
                cmd.Parameters.AddWithValue("@id", r.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public static void SetIngredients(int idRecette, List<RecetteIngredient> items)
        {
            using (var conn = DbHelper.GetConnection())
            {
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var del = conn.CreateCommand())
                        {
                            del.Transaction = tx;
                            del.CommandText = "DELETE FROM recettes_ingredients WHERE id_recette = @id";
                            del.Parameters.AddWithValue("@id", idRecette);
                            del.ExecuteNonQuery();
                        }

                        foreach (var item in items)
                        {
                            using (var ins = conn.CreateCommand())
                            {
                                ins.Transaction = tx;
                                ins.CommandText = @"INSERT INTO recettes_ingredients (id_recette, id_fiche_ingredient, quantite)
                                                    VALUES (@idR, @idI, @qte)";
                                ins.Parameters.AddWithValue("@idR", idRecette);
                                ins.Parameters.AddWithValue("@idI", item.IdIngredient);
                                ins.Parameters.AddWithValue("@qte", item.Quantite);
                                ins.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM fiches_recettes WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        private static void BindHeader(MySqlCommand cmd, Recette r)
        {
            cmd.Parameters.AddWithValue("@nom",      r.Nom);
            cmd.Parameters.AddWithValue("@desc",     r.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@type",     r.TypeRendement ?? "par_lot");
            cmd.Parameters.AddWithValue("@rendement",r.RendementQuantite);
            cmd.Parameters.AddWithValue("@temps",    r.TempsPreparation.HasValue ? (object)r.TempsPreparation.Value : DBNull.Value);
        }

        private static Recette MapHeader(MySqlDataReader r) => new Recette
        {
            Id                = (int)r["id"],
            Nom               = r["nom"].ToString(),
            Description       = r["description"]      == DBNull.Value ? null : r["description"].ToString(),
            TypeRendement     = r["type_rendement"].ToString(),
            RendementQuantite = (decimal)r["rendement_quantite"],
            TempsPreparation  = r["temps_preparation"] == DBNull.Value ? (int?)null : (int)r["temps_preparation"],
            Actif             = Convert.ToBoolean(r["actif"])
        };
    }
}
