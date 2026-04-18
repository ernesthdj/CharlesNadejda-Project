using System;
using System.Collections.Generic;

namespace CharlesNadejda.Models
{
    public class BomContexte
    {
        public int      Id           { get; set; }
        public string   Nom          { get; set; }
        public string   Description  { get; set; }
        public int      IdActivite   { get; set; }
        public bool     Actif        { get; set; }
        public DateTime DateCreation { get; set; }

        // Chargé par jointure dans le DAL (activites.nom)
        public string ActiviteNom { get; set; }

        // Chargés optionnellement par le DAL
        public List<BomNiveau> Niveaux { get; set; } = new List<BomNiveau>();

        public override string ToString() => $"{Nom} ({ActiviteNom})";
    }
}
