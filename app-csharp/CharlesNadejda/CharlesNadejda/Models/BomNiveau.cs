using System;

namespace CharlesNadejda.Models
{
    public class BomNiveau
    {
        public int      Id           { get; set; }
        public int      IdContexte   { get; set; }
        public int      Ordre        { get; set; }
        public string   Nom          { get; set; }
        public string   Description  { get; set; }
        public DateTime DateCreation { get; set; }

        // Jointures — remontées depuis bom_contextes → activites
        public string NomContexte { get; set; }
        public int    IdActivite  { get; set; }
        public string ActiviteNom { get; set; }

        public override string ToString() => $"Niveau {Ordre} — {Nom}";
    }
}
