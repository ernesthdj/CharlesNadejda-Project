namespace CharlesNadejda.Models
{
    public class BomProductionLigne
    {
        public int     Id                   { get; set; }
        public int     IdProduction         { get; set; }
        public string  TypeSource           { get; set; }   // lot_ingredient | bom_stock
        public int?    IdLotIngredient      { get; set; }
        public int?    IdBomStock           { get; set; }
        public decimal QuantiteConsommee    { get; set; }
        public decimal CoutUnitaireMoment   { get; set; }

        // Jointures
        public string NomSource    { get; set; }   // nom ingrédient ou nom fiche
        public string UniteSource  { get; set; }

        public decimal SousTotal => QuantiteConsommee * CoutUnitaireMoment;

        public override string ToString() =>
            $"{QuantiteConsommee} {UniteSource} de {NomSource}";
    }
}
