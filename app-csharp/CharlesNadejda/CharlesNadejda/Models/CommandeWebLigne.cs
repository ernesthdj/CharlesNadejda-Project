namespace CharlesNadejda.Models
{
    public class CommandeWebLigne
    {
        public int     Id             { get; set; }
        public int     IdCommande     { get; set; }
        public int     IdProduitWeb   { get; set; }
        public int     Quantite       { get; set; }
        public decimal PrixUnitaire   { get; set; }
        public decimal SousTotal      { get; set; }   // colonne GENERATED en DB

        // Jointures
        public string NomProduit { get; set; }   // produits_web.nom_commercial
    }
}
