namespace CharlesNadejda.Models
{
    public class Parfum
    {
        public int    Id          { get; set; }
        public string Nom         { get; set; }
        public string Description { get; set; }
        public string TypeParfum  { get; set; }
        public string CouleurHex  { get; set; }
        public int?   IdRecette   { get; set; }
        public string NomRecette  { get; set; }   // jointure
        public bool   Disponible  { get; set; }

        public override string ToString() => Nom;
    }
}
