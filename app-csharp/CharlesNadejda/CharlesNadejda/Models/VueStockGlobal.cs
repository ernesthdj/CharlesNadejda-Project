using System;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Modèle de lecture seule mappant la VIEW vue_stock_global.
    /// Unifie lots_ingredients (matières premières) et bom_stocks (produits fabriqués).
    /// Toute modification passe par LotDAL ou BomStockDAL — jamais via ce modèle.
    /// </summary>
    public class VueStockGlobal
    {
        // ── Constantes de type stock ──────────────────────────────

        /// <summary>Type : lot de matière première (table lots_ingredients).</summary>
        public const string TypeLotIngredient   = "lot_ingredient";
        /// <summary>Type : produit issu de la fabrication BOM (table bom_stocks).</summary>
        public const string TypeProduitFabrique = "produit_fabrique";

        // ── Colonnes de la VIEW ───────────────────────────────────

        /// <summary>Discriminant : "lot_ingredient" ou "produit_fabrique". Voir constantes TypeXxx.</summary>
        public string    TypeStock           { get; set; }
        public int       IdEntree            { get; set; }
        public string    Nom                 { get; set; }
        public string    Unite               { get; set; }
        public decimal   QuantiteTotale      { get; set; }
        public decimal   QuantiteReservee    { get; set; }
        public decimal   QuantiteDispoReelle { get; set; }
        /// <summary>Prix par unité de base (€/g, €/ml, €/pce).</summary>
        public decimal   CoutUnitaire        { get; set; }
        /// <summary>Prix d'un conditionnement — lots uniquement, null pour produits fabriqués.</summary>
        public decimal?  PrixConditionnement { get; set; }
        /// <summary>Quantité en unité de base par conditionnement — lots uniquement.</summary>
        public decimal?  QteParConditionnement { get; set; }
        public string    ConditionnementLabel { get; set; }
        public DateTime? DateDlc             { get; set; }

        // ── Champs spécifiques lots (null pour produits fabriqués) ─

        public int?      IdStock             { get; set; }
        public string    StockNom            { get; set; }
        /// <summary>ID de la fiche ingrédient — utilisé pour l'agrégation par fiche.</summary>
        public int?      IdFicheIngredient   { get; set; }

        // ── Champs spécifiques produits fabriqués (null pour lots) ─

        public int?      IdActivite          { get; set; }
        /// <summary>Chargé par VueStockGlobalDAL via LEFT JOIN activites.nom.</summary>
        public string    NomActivite         { get; set; }
        public int?      IdContexte          { get; set; }
        public int?      IdNiveau            { get; set; }
        public int?      IdFicheBom          { get; set; }

        // ── Propriétés calculées ──────────────────────────────────

        /// <summary>Vrai si c'est un lot d'ingrédient (vs produit fabriqué).</summary>
        public bool EstLot              => TypeStock == TypeLotIngredient;
        /// <summary>Vrai si la quantité disponible réelle est épuisée (rupture de stock).</summary>
        public bool EstEnRupture        => QuantiteDispoReelle <= 0;
        /// <summary>Vrai si des réservations sont actives sur cette entrée.</summary>
        public bool ADesReservations    => QuantiteReservee > 0;

        public override string ToString() => $"{Nom} — {QuantiteDispoReelle} {Unite}";
    }
}
