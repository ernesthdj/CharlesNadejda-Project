namespace CharlesNadejda.Models
{
    public class Produit
    {
        public int     Id            { get; set; }
        public int     IdCategorie   { get; set; }
        public string  NomCategorie  { get; set; }   // champ de jointure, lecture seule
        public string  Nom           { get; set; }
        public string  Description   { get; set; }
        public decimal PrixTTC       { get; set; }
        public decimal? PrixPromo    { get; set; }
        public int     Stock         { get; set; }
        public bool    Disponible    { get; set; }
        public string  ImageUrl      { get; set; }

        public override string ToString() => Nom;
    }
}
