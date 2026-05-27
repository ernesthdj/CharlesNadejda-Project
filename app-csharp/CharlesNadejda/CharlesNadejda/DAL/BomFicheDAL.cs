using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class BomFicheDAL
    {
        private const string SELECT_HEADER = @"
            SELECT f.id, f.id_niveau, f.nom, f.description,
                   f.unite_output, f.quantite_output, f.temps_preparation, f.stock_cible, f.actif, f.date_creation,
                   n.nom AS nom_niveau, n.ordre AS ordre_niveau,
                   c.id  AS id_contexte, c.nom AS nom_contexte, c.id_activite,
                   a.nom AS nom_activite
            FROM bom_fiches f
            INNER JOIN bom_niveaux   n ON n.id = f.id_niveau
            INNER JOIN bom_contextes c ON c.id = n.id_contexte
            INNER JOIN activites     a ON a.id = c.id_activite";

        /// <summary>
        /// Retourne toutes les fiches d'un niveau donné.
        /// Méthode principale — une fiche appartient à un niveau précis.
        /// </summary>
        public static List<BomFiche> GetByNiveau(int idNiveau)
        {
            var list = new List<BomFiche>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_HEADER + @"
                    WHERE f.id_niveau = @idNiveau AND f.actif = 1
                    ORDER BY f.nom";
                cmd.Parameters.AddWithValue("@idNiveau", idNiveau);
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapHeader(r));
            }
            return list;
        }

        /// <summary>
        /// Retourne toutes les fiches d'une activité — utilisé par FrmBomProductionSimulation.
        /// Préférer GetByNiveau quand possible.
        /// idActivite : 0 = tous
        /// </summary>
        public static List<BomFiche> GetAll(int idActivite = 0)
        {
            var list = new List<BomFiche>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_HEADER + " WHERE f.actif = 1";

                if (idActivite > 0)
                {
                    cmd.CommandText += " AND c.id_activite = @idActivite";
                    cmd.Parameters.AddWithValue("@idActivite", idActivite);
                }

                cmd.CommandText += " ORDER BY c.nom, n.ordre, f.nom";

                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(MapHeader(r));
            }
            return list;
        }

        public static BomFiche GetById(int id, bool avecLignes = true)
        {
            BomFiche fiche = null;
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = SELECT_HEADER + " WHERE f.id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                    if (r.Read()) fiche = MapHeader(r);
            }
            if (fiche != null && avecLignes)
                fiche.Lignes = BomFicheLigneDAL.GetByFiche(fiche.Id);
            return fiche;
        }

        /// <summary>Vérifie l'unicité du nom dans le scope du niveau.</summary>
        public static bool NomExiste(string nom, int idNiveau, int excludeId = 0)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT COUNT(*) FROM bom_fiches
                    WHERE nom = @nom AND id_niveau = @idNiveau AND id <> @id";
                cmd.Parameters.AddWithValue("@nom",      nom);
                cmd.Parameters.AddWithValue("@idNiveau", idNiveau);
                cmd.Parameters.AddWithValue("@id",       excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        /// <summary>Insère la fiche et ses lignes dans une transaction.</summary>
        public static int Insert(BomFiche f)
        {
            using (var conn = DbHelper.GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int idFiche;
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            INSERT INTO bom_fiches
                                (id_niveau, nom, description, unite_output, quantite_output, temps_preparation, stock_cible, actif)
                            VALUES (@idNiveau, @nom, @desc, @unite, @qte, @temps, @stockCible, 1)";
                        BindHeader(cmd, f);
                        cmd.ExecuteNonQuery();
                        idFiche = (int)cmd.LastInsertedId;
                    }
                    InsertLignes(conn, tx, idFiche, f.Lignes);
                    tx.Commit();
                    return idFiche;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        /// <summary>Met à jour la fiche et remplace toutes ses lignes.</summary>
        public static void Update(BomFiche f)
        {
            using (var conn = DbHelper.GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            UPDATE bom_fiches
                            SET id_niveau=@idNiveau, nom=@nom, description=@desc,
                                unite_output=@unite, quantite_output=@qte, temps_preparation=@temps,
                                stock_cible=@stockCible
                            WHERE id=@id";
                        BindHeader(cmd, f);
                        cmd.Parameters.AddWithValue("@id", f.Id);
                        cmd.ExecuteNonQuery();
                    }
                    using (var del = conn.CreateCommand())
                    {
                        del.Transaction = tx;
                        del.CommandText = "DELETE FROM bom_fiches_lignes WHERE id_fiche = @id";
                        del.Parameters.AddWithValue("@id", f.Id);
                        del.ExecuteNonQuery();
                    }
                    InsertLignes(conn, tx, f.Id, f.Lignes);
                    tx.Commit();
                }
                catch { tx.Rollback(); throw; }
            }
        }

        /// <summary>
        /// Retourne le nombre de fiches actives par id_niveau pour un contexte donné.
        /// Une seule requête au lieu de N — utilisé pour afficher les badges de comptage.
        /// </summary>
        public static Dictionary<int, int> GetCountsByContexte(int idContexte)
        {
            var dict = new Dictionary<int, int>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT f.id_niveau, COUNT(*) AS cnt
                    FROM bom_fiches f
                    INNER JOIN bom_niveaux n ON n.id = f.id_niveau
                    WHERE n.id_contexte = @idContexte AND f.actif = 1
                    GROUP BY f.id_niveau";
                cmd.Parameters.AddWithValue("@idContexte", idContexte);
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        dict[Convert.ToInt32(r["id_niveau"])] = Convert.ToInt32(r["cnt"]);
            }
            return dict;
        }

        public static void Delete(int id)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                // Vérifier si cette fiche est consommée par des fiches de niveau supérieur
                cmd.CommandText = @"
                    SELECT COUNT(*) FROM bom_fiches_lignes
                    WHERE id_input_fiche = @id";
                cmd.Parameters.AddWithValue("@id", id);
                int nbRef = Convert.ToInt32(cmd.ExecuteScalar());
                if (nbRef > 0)
                    throw new InvalidOperationException(
                        $"Impossible de supprimer : cette fiche est référencée dans {nbRef} fiche(s) de niveau supérieur.");

                // Vérifier les productions existantes
                cmd.CommandText = "SELECT COUNT(*) FROM bom_productions WHERE id_fiche = @id";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", id);
                int nbProd = Convert.ToInt32(cmd.ExecuteScalar());
                if (nbProd > 0)
                    throw new InvalidOperationException(
                        $"Impossible de supprimer : {nbProd} production(s) enregistrée(s) avec cette fiche.");

                cmd.CommandText = "DELETE FROM bom_fiches WHERE id = @id";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Duplique une fiche BOM dans le même niveau.
        /// Le nom de la copie est "Copie de [nom original]".
        /// Si ce nom existe déjà, suffixe numérique (2), (3)...
        /// Retourne l'id de la nouvelle fiche.
        /// </summary>
        /// <param name="idFiche">Id de la fiche source à dupliquer.</param>
        /// <returns>Id de la fiche copiée (int).</returns>
        public static int Duplicate(int idFiche)
        {
            var source = GetById(idFiche, avecLignes: true);
            if (source == null)
                throw new ArgumentException($"Fiche {idFiche} introuvable.");

            // Générer un nom unique dans le scope du même niveau
            string nomBase = $"Copie de {source.Nom}";
            string nom     = nomBase;
            int    suffixe = 2;
            while (NomExiste(nom, source.IdNiveau))
                nom = $"{nomBase} ({suffixe++})";

            var copie = new BomFiche
            {
                IdNiveau         = source.IdNiveau,
                Nom              = nom,
                Description      = source.Description,
                UniteOutput      = source.UniteOutput,
                QuantiteOutput   = source.QuantiteOutput,
                TempsPreparation = source.TempsPreparation,
                StockCible       = source.StockCible,
                Lignes           = source.Lignes   // Insert() réinsère chaque ligne
            };
            return Insert(copie);
        }

        // ── Helpers privés ───────────────────────────────────────────────

        internal static void InsertLignes(MySqlConnection conn, MySqlTransaction tx,
                                          int idFiche, List<BomFicheLigne> lignes)
        {
            foreach (var l in lignes)
            {
                if (l.TypeInput == "ingredient" && !l.IdInputIngredient.HasValue)
                    throw new ArgumentException($"Ligne '{l.NomInput}' : type_input='ingredient' mais IdInputIngredient est null.");
                if (l.TypeInput == "fiche" && !l.IdInputFiche.HasValue)
                    throw new ArgumentException($"Ligne '{l.NomInput}' : type_input='fiche' mais IdInputFiche est null.");

                using (var ins = conn.CreateCommand())
                {
                    ins.Transaction = tx;
                    ins.CommandText = @"
                        INSERT INTO bom_fiches_lignes
                            (id_fiche, type_input, id_input_ingredient, id_input_fiche, quantite, unite_mesure)
                        VALUES (@idFiche, @type, @idIngr, @idFicheInput, @qte, @unite)";
                    ins.Parameters.AddWithValue("@idFiche",      idFiche);
                    ins.Parameters.AddWithValue("@type",         l.TypeInput);
                    ins.Parameters.AddWithValue("@idIngr",       l.IdInputIngredient.HasValue ? (object)l.IdInputIngredient.Value : DBNull.Value);
                    ins.Parameters.AddWithValue("@idFicheInput", l.IdInputFiche.HasValue      ? (object)l.IdInputFiche.Value      : DBNull.Value);
                    ins.Parameters.AddWithValue("@qte",          l.Quantite);
                    ins.Parameters.AddWithValue("@unite",        l.UniteMesure);
                    ins.ExecuteNonQuery();
                }
            }
        }

        private static void BindHeader(MySqlCommand cmd, BomFiche f)
        {
            cmd.Parameters.AddWithValue("@idNiveau", f.IdNiveau);
            cmd.Parameters.AddWithValue("@nom",      f.Nom);
            cmd.Parameters.AddWithValue("@desc",     f.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@unite",    f.UniteOutput ?? "piece");
            cmd.Parameters.AddWithValue("@qte",      f.QuantiteOutput);
            cmd.Parameters.AddWithValue("@temps",    f.TempsPreparation.HasValue ? (object)f.TempsPreparation.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@stockCible", f.StockCible.HasValue ? (object)f.StockCible.Value : DBNull.Value);
        }

        private static BomFiche MapHeader(MySqlDataReader r) => new BomFiche
        {
            Id               = (int)r["id"],
            IdNiveau         = (int)r["id_niveau"],
            Nom              = r["nom"].ToString(),
            Description      = r["description"]      == DBNull.Value ? null : r["description"].ToString(),
            UniteOutput      = r["unite_output"].ToString(),
            QuantiteOutput   = (decimal)r["quantite_output"],
            TempsPreparation = r["temps_preparation"] == DBNull.Value ? (int?)null : (int)r["temps_preparation"],
            StockCible       = r["stock_cible"] == DBNull.Value ? (decimal?)null : (decimal)r["stock_cible"],
            Actif            = Convert.ToBoolean(r["actif"]),
            DateCreation     = (DateTime)r["date_creation"],
            NomNiveau        = r["nom_niveau"].ToString(),
            OrdreNiveau      = Convert.ToInt32(r["ordre_niveau"]),
            IdContexte       = (int)r["id_contexte"],
            NomContexte      = r["nom_contexte"].ToString(),
            IdActivite       = (int)r["id_activite"],
            ActiviteNom      = r["nom_activite"].ToString()
        };
    }
}
