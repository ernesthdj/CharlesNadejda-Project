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

        // Calculé par le DAL : SUM(bom_stocks.quantite_disponible)
        public decimal StockDisponible { get; set; }

        public bool EstEnStock => StockDisponible > 0;

        public override string ToString() => NomCommercial;
    }
}
