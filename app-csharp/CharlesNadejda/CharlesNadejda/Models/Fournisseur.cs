namespace CharlesNadejda.Models
{
    public class Fournisseur
    {
        public int    Id        { get; set; }
        public string Nom       { get; set; }
        public string Contact   { get; set; }
        public string Email     { get; set; }
        public string Telephone { get; set; }
        public string Adresse   { get; set; }
        public string Notes     { get; set; }

        public override string ToString() => Nom;
    }
}
