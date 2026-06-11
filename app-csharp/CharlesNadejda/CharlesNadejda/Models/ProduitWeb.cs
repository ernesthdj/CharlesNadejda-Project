using System;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Produit publié sur la boutique e-commerce.
    /// Fait le pont entre une fiche BOM (production) et le catalogue web.
    /// Le stock est calculé dynamiquement depuis bom_stocks.
    /// </summary>
    public class ProduitWeb
    {
        // ── Colonnes DB (table produits_web) ──────────────────────

        public int      Id              { get; set; }
        /// <summary>FK vers bom_fiches — la recette dont ce produit est issu.</summary>
        public int      IdBomFiche      { get; set; }
        /// <summary>FK vers categories_web — null si non catégorisé.</summary>
        public int?     IdCategorie     { get; set; }
        public string   NomCommercial   { get; set; }
        public string   Description     { get; set; }
        public decimal  PrixVente       { get; set; }
        /// <summary>URL ou chemin de l'image (Cloudinary ou local).</summary>
        public string   ImagePath       { get; set; }
        public bool     EnVente         { get; set; }
        public int      OrdreAffichage  { get; set; }
        public DateTime DateCreation    { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par ProduitWebDAL via JOIN bom_fiches.nom.</summary>
        public string  NomFiche             { get; set; }
        /// <summary>Chargé par ProduitWebDAL via LEFT JOIN categories_web.nom.</summary>
        public string  NomCategorie         { get; set; }
        /// <summary>Chargé par ProduitWebDAL via JOIN bom_fiches.quantite_output (ex: 250 pour 250g/baguette).</summary>
        public decimal QuantiteOutputBatch  { get; set; }
        /// <summary>Chargé par ProduitWebDAL via JOIN bom_fiches.unite_output (ex: "g").</summary>
        public string  UniteOutput          { get; set; }
        /// <summary>Chargé par ProduitWebDAL via COALESCE(SUM(bom_stocks.quantite_disponible)) — unité brute.</summary>
        public decimal StockDisponible      { get; set; }

        // ── Propriétés calculées ──────────────────────────────────

        /// <summary>Nombre d'unités vendables en stock = StockDisponible / QuantiteOutputBatch (ex: 2750g / 250g = 11 baguettes).</summary>
        public int StockUnites => QuantiteOutputBatch > 0
            ? (int)(StockDisponible / QuantiteOutputBatch) : 0;

        /// <summary>Vrai s'il reste du stock brut disponible.</summary>
        public bool EstEnStock => StockDisponible > 0;

        public override string ToString() => NomCommercial;
    }
}
