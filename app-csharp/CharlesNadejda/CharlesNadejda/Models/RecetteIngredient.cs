namespace CharlesNadejda.Models
{
    public class RecetteIngredient
    {
        public int     IdRecette      { get; set; }
        public int     IdIngredient   { get; set; }
        public string  NomIngredient  { get; set; }
        public string  UniteMesure    { get; set; }
        public decimal Quantite       { get; set; }
        public decimal PrixUnitaire   { get; set; }   // prix_achat_reference au moment

        public decimal SousTotal => Quantite * PrixUnitaire;
    }
}
