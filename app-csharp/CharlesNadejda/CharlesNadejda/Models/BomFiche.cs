using System;
using System.Collections.Generic;
using System.Linq;

namespace CharlesNadejda.Models
{
    public class BomFiche
    {
        public int      Id               { get; set; }
        public int      IdNiveau         { get; set; }   // niveau auquel appartient cette fiche
        public string   Nom              { get; set; }
        public string   Description      { get; set; }
        public string   UniteOutput      { get; set; }   // kg, g, l, ml, cl, piece
        public decimal  QuantiteOutput   { get; set; }   // quantité produite par exécution
        public int?     TempsPreparation { get; set; }   // minutes
        public bool     Actif            { get; set; }
        public DateTime DateCreation     { get; set; }

        // Jointures — chargées par le DAL via bom_niveaux → bom_contextes → activites
        public string NomNiveau   { get; set; }
        public int    OrdreNiveau { get; set; }
        public int    IdContexte  { get; set; }
        public string NomContexte { get; set; }
        public int    IdActivite  { get; set; }
        public string ActiviteNom { get; set; }

        // Chargées optionnellement par le DAL
        public List<BomFicheLigne> Lignes { get; set; } = new List<BomFicheLigne>();

        // Coût total des inputs pour une exécution (calculé depuis les lignes + prix référence)
        public decimal CoutBatch    => Lignes.Sum(l => l.SousTotal);
        public decimal CoutUnitaire => QuantiteOutput > 0 ? CoutBatch / QuantiteOutput : 0;

        public override string ToString() => Nom;
    }
}
