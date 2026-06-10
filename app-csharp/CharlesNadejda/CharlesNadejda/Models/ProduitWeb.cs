using System;

namespace CharlesNadejda.Models
{
    public class ProduitWeb
    {
        public int      Id              { get; set; }
        public int      IdBomFiche      { get; set; }
        public int?     IdCategorie     { get; set; }
        public string   NomCommercial   { get; set; }
        public string   Description     { get; set; }
        public decimal  PrixVente       { get; set; }
        public string   ImagePath       { get; set; }
        public bool     EnVente         { get; set; }
        public int      OrdreAffichage  { get; set; }
        public DateTime DateCreation    { get; set; }

        // Jointures — chargées par le DAL
        public string  NomFiche        { get; set; }   // bom_fiches.nom
        public string  NomCategorie    { get; set; }   // categories_web.nom

        // Infos de la fiche BOM pour convertir le stock brut en unités
        public decimal QuantiteOutputBatch { get; set; }  // bom_fiches.quantite_output (ex: 250 pour 250g/baguette)
        public string  UniteOutput         { get; set; }  // bom_fiches.unite_output (ex: "g")

        // Calculé par le DAL : SUM(bom_stocks.quantite_disponible) — en unité brute (g, ml...)
        public decimal StockDisponible { get; set; }

        // Nombre d'unités de produit en stock (ex: 2750g ÷ 250g = 11 baguettes)
        public int StockUnites => QuantiteOutputBatch > 0
            ? (int)(StockDisponible / QuantiteOutputBatch) : 0;

        public bool EstEnStock => StockDisponible > 0;

        public override string ToString() => NomCommercial;
    }
}
