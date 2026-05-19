namespace CharlesNadejda.Models
{
    public class CategorieWeb
    {
        public int    Id              { get; set; }
        public string Nom             { get; set; }
        public string Description     { get; set; }
        public int    OrdreAffichage  { get; set; }
        public bool   Actif           { get; set; }

        // Calculé par le DAL : nombre de produits publiés dans cette catégorie
        public int NbProduits { get; set; }

        public override string ToString() => Nom;
    }
}
