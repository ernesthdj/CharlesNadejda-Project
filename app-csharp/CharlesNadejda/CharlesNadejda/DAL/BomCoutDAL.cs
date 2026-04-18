using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    /// <summary>
    /// Calcul du coût de revient d'une production BOM (Bill of Materials) par règle de 3 récursive.
    ///
    /// Principe — règle de 3 inter-niveaux :
    ///   Si une fiche N produit Q_out [unité] et coûte C au total,
    ///   alors :
    ///     coût par unité    = C / Q_out
    ///     coût pour X unités = X × (C / Q_out)
    ///
    ///   Pour un input de type "fiche" (niveau N-1) consommé en quantité X :
    ///     nBatchesSource = X / Q_out_source          ← règle de 3
    ///     coûtInput      = nBatchesSource × C_source  ← appel récursif
    ///
    /// Remontée des prix depuis les ingrédients (N1) :
    ///   Chaque ingrédient a un prix par unité de base (€/g, €/ml, €/pce) issu de la
    ///   moyenne pondérée des lots disponibles.
    ///   Si aucun lot disponible : fallback sur prix_achat_reference / qte_par_conditionnement.
    ///
    /// Point d'entrée unique : CalculerCout(idFiche, nBatches).
    /// </summary>
    public static class BomCoutDAL
    {
        // ════════════════════════════════════════════════════════════
        //  Entrée publique
        // ════════════════════════════════════════════════════════════

        /// <summary>
        /// Calcule le coût de production d'une fiche pour <paramref name="nBatches"/> batches.
        ///
        /// Récursif multi-niveaux :
        ///   - Si une ligne est de type "ingredient" → prix moyen des lots (DB)
        ///   - Si une ligne est de type "fiche"      → appel récursif sur la fiche N-1
        ///
        /// Toutes les quantités sont converties dans l'unité de stockage (UniteMesureInput)
        /// via UnitConvertisseur avant calcul.
        /// </summary>
        /// <param name="idFiche">Id de la fiche BOM à évaluer.</param>
        /// <param name="nBatches">Nombre de batches à produire (peut être décimal).</param>
        public static RapportCout CalculerCout(int idFiche, decimal nBatches)
        {
            var fiche = BomFicheDAL.GetById(idFiche, avecLignes: true);
            if (fiche == null || fiche.Lignes == null)
                return new RapportCout { NomFiche = "Inconnue", NbBatches = nBatches };

            var rapport = new RapportCout
            {
                NomFiche       = fiche.Nom,
                NbBatches      = nBatches,
                QuantiteOutput = nBatches * fiche.QuantiteOutput,
                UniteOutput    = fiche.UniteOutput,
                Lignes         = new List<LigneCout>()
            };

            foreach (var ligne in fiche.Lignes)
            {
                // ── Quantité consommée totale (en ligne.UniteMesure) ─────────
                decimal qteConsommee = ligne.Quantite * nBatches;

                // ── Conversion vers l'unité de stockage (UniteMesureInput) ───
                // Pour ingredient : unité de base du lot (g, ml, pce)
                // Pour fiche      : unité output de la fiche source
                decimal qteStockage = UnitConvertisseur.Convertir(
                    qteConsommee, ligne.UniteMesure, ligne.UniteMesureInput);

                LigneCout lc;

                if (ligne.TypeInput == "ingredient")
                {
                    lc = CalculerLigneIngredient(ligne, qteStockage);
                }
                else
                {
                    lc = CalculerLigneFiche(ligne, qteStockage);
                }

                rapport.Lignes.Add(lc);
                rapport.CoutTotal += lc.SousTotal;
            }

            // Coût par unité de sortie (règle de 3 finale)
            rapport.CoutUnitaire = rapport.QuantiteOutput > 0
                ? rapport.CoutTotal / rapport.QuantiteOutput
                : 0m;

            return rapport;
        }

        // ════════════════════════════════════════════════════════════
        //  Calcul par type de ligne
        // ════════════════════════════════════════════════════════════

        /// <summary>
        /// Ligne ingredient : prix = moyenne pondérée des lots (€ par unité de base).
        /// Coût = qteStockage × prixMoyen.
        /// </summary>
        private static LigneCout CalculerLigneIngredient(BomFicheLigne ligne, decimal qteStockage)
        {
            decimal prixUnit = GetPrixMoyenIngredient(ligne.IdInputIngredient.Value);

            return new LigneCout
            {
                NomInput   = ligne.NomInput,
                TypeInput  = "ingredient",
                Quantite   = qteStockage,
                Unite      = ligne.UniteMesureInput,
                PrixUnit   = prixUnit,
                SousTotal  = qteStockage * prixUnit
            };
        }

        /// <summary>
        /// Ligne fiche (niveau N-1) : appel récursif via règle de 3.
        ///
        ///   nBatchesSource = qteStockage / ficheSource.QuantiteOutput
        ///   coût           = CalculerCout(ficheSource, nBatchesSource).CoutTotal
        ///   prixUnit       = coût / qteStockage  (€ par unité de stockage de la fiche source)
        /// </summary>
        private static LigneCout CalculerLigneFiche(BomFicheLigne ligne, decimal qteStockage)
        {
            var ficheSrc = BomFicheDAL.GetById(ligne.IdInputFiche.Value, avecLignes: false);

            if (ficheSrc == null || ficheSrc.QuantiteOutput <= 0)
                return new LigneCout
                {
                    NomInput  = ligne.NomInput,
                    TypeInput = "fiche",
                    Quantite  = qteStockage,
                    Unite     = ligne.UniteMesureInput,
                    PrixUnit  = 0m,
                    SousTotal = 0m
                };

            // Règle de 3 : combien de batches de la fiche source sont nécessaires ?
            decimal nBatchesSrc = qteStockage / ficheSrc.QuantiteOutput;

            // Calcul récursif du coût de la fiche source
            RapportCout detail = CalculerCout(ligne.IdInputFiche.Value, nBatchesSrc);

            decimal prixUnit = qteStockage > 0 ? detail.CoutTotal / qteStockage : 0m;

            return new LigneCout
            {
                NomInput    = ligne.NomInput,
                TypeInput   = "fiche",
                Quantite    = qteStockage,
                Unite       = ligne.UniteMesureInput,
                PrixUnit    = prixUnit,
                SousTotal   = detail.CoutTotal,
                DetailFiche = detail
            };
        }

        // ════════════════════════════════════════════════════════════
        //  Prix moyen pondéré d'un ingrédient
        // ════════════════════════════════════════════════════════════

        /// <summary>
        /// Retourne le coût moyen pondéré par unité de base (€/g, €/ml, €/pce).
        ///
        /// Calcul (lots disponibles) :
        ///   Σ(lot.quantite_disponible × lot.prix_unitaire / fi.qte_par_conditionnement)
        ///   ─────────────────────────────────────────────────────────────────────────────
        ///             Σ(lot.quantite_disponible)
        ///
        /// Fallback si aucun lot disponible :
        ///   fi.prix_achat_reference / fi.qte_par_conditionnement
        ///
        /// Fallback final : 0 (pas de données de prix).
        /// </summary>
        private static decimal GetPrixMoyenIngredient(int idFicheIngredient)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd  = conn.CreateCommand())
            {
                // Moyenne pondérée des lots disponibles.
                // Si aucun lot → prix de référence de la fiche ingrédient.
                cmd.CommandText = @"
                    SELECT
                        COALESCE(
                            /* Moyenne pondérée lots disponibles (€ / unité de base) */
                            SUM(l.quantite_disponible
                                * l.prix_unitaire
                                / NULLIF(fi.qte_par_conditionnement, 0))
                            / NULLIF(SUM(l.quantite_disponible), 0),

                            /* Fallback : prix de référence de la fiche ingrédient */
                            fi.prix_achat_reference
                            / NULLIF(fi.qte_par_conditionnement, 0),

                            /* Fallback final : 0 */
                            0
                        ) AS prix_base_moyen
                    FROM fiches_ingredients fi
                    LEFT JOIN lots_ingredients l
                        ON  l.id_fiche_ingredient = fi.id
                        AND l.quantite_disponible > 0
                    WHERE fi.id = @id
                    GROUP BY fi.id,
                             fi.prix_achat_reference,
                             fi.qte_par_conditionnement";

                cmd.Parameters.AddWithValue("@id", idFicheIngredient);

                var result = cmd.ExecuteScalar();
                return (result == null || result == DBNull.Value)
                    ? 0m
                    : Convert.ToDecimal(result);
            }
        }
    }
}
