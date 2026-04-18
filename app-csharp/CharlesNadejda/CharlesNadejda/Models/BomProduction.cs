using System;
using System.Collections.Generic;

namespace CharlesNadejda.Models
{
    public class BomProduction
    {
        public int      Id               { get; set; }
        public int      IdNiveau         { get; set; }
        public int      IdFiche          { get; set; }
        public decimal  QuantiteProduite { get; set; }
        public decimal  CoutIngredients  { get; set; }
        public decimal  CoutUnitaire     { get; set; }
        public DateTime DateProduction   { get; set; }
        public string   Notes            { get; set; }

        // Jointures
        public string NomFiche    { get; set; }
        public string NomNiveau   { get; set; }
        public int    OrdreNiveau { get; set; }
        public string NomContexte { get; set; }

        // Chargées optionnellement par le DAL
        public List<BomProductionLigne> Lignes { get; set; } = new List<BomProductionLigne>();

        public override string ToString() =>
            $"{DateProduction:dd/MM/yyyy} — {NomFiche} × {QuantiteProduite} [{NomNiveau}]";
    }
}
