namespace CharlesNadejda.Models
{
    public class BomFicheLigne
    {
        public int     Id                  { get; set; }
        public int     IdFiche             { get; set; }
        public string  TypeInput           { get; set; }   // ingredient | fiche
        public int?    IdInputIngredient   { get; set; }
        public int?    IdInputFiche        { get; set; }
        public decimal Quantite            { get; set; }
        public string  UniteMesure         { get; set; }   // kg, g, l, ml, cl, piece

        // Jointures — nom et unité de l'input référencé
        public string NomInput         { get; set; }
        public string UniteMesureInput { get; set; }
        public decimal PrixUnitaireRef { get; set; }   // prix de référence pour estimation coût

        // Coût estimé de cette ligne (basé sur prix référence, pas le prix achat réel)
        public decimal SousTotal => Quantite * PrixUnitaireRef;

        public override string ToString() => $"{Quantite} {UniteMesure} de {NomInput}";
    }
}
