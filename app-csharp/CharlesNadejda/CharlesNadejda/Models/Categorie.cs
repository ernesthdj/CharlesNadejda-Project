namespace CharlesNadejda.Models
{
    public class Categorie
    {
        public int    Id              { get; set; }
        public string Nom             { get; set; }
        public string Description     { get; set; }
        public int    OrdreAffichage  { get; set; }

        public override string ToString() => Nom;
    }
}
