using System;

namespace CharlesNadejda.Models
{
    public class Lot
    {
        public int      Id                      { get; set; }
        public int      IdFicheIngredient       { get; set; }
        public string   NomIngredient           { get; set; }   // jointure
        public string   UniteMesure             { get; set; }   // jointure — unité de base ('g','ml','piece')
        public string   ConditionnementLabel    { get; set; }   // jointure — label du conditionnement
        public decimal  QteParConditionnement   { get; set; }   // jointure — qté en base par conditionnement
        public decimal  NbConditionnements      { get; set; }   // nombre de colis achetés (ex: 5 sacs)
        public string   NumeroLot               { get; set; }
        public int?     IdFournisseur            { get; set; }
        public string   NomFournisseur          { get; set; }   // jointure
        public DateTime DateAchat               { get; set; }
        public DateTime? DatePeremption         { get; set; }
        /// <summary>Quantité totale en unité de base (g/ml/pce) = NbConditionnements × QteParConditionnement.</summary>
        public decimal  QuantiteInitiale        { get; set; }
        public decimal  QuantiteDisponible      { get; set; }
        public decimal  PrixUnitaire            { get; set; }   // prix HTVA par conditionnement
        public decimal  PrixAchatReel           { get; set; }   // total HTVA = NbConditionnements × PrixUnitaire
        public decimal  TvaPct                  { get; set; }   // taux de TVA en % (0 = exonéré)
        public string   ReferenceFacture        { get; set; }
        public string   Notes                   { get; set; }
    }
}
