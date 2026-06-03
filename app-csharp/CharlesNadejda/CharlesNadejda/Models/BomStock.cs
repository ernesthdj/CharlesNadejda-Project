using System;

namespace CharlesNadejda.Models
{
    public class BomStock
    {
        public int       Id                  { get; set; }
        public int       IdNiveau            { get; set; }
        public int       IdFiche             { get; set; }
        public int       IdProduction        { get; set; }
        public decimal   QuantiteDisponible  { get; set; }
        public decimal   CoutUnitaire        { get; set; }
        public DateTime  DateProduction      { get; set; }
        public DateTime? DateDlc             { get; set; }
        public DateTime  DateCreation        { get; set; }

        // Discriminants directs (v11)
        public int    IdContexte  { get; set; }
        public int    IdActivite  { get; set; }

        // Jointures
        public string NomFiche    { get; set; }
        public string UniteOutput { get; set; }
        public string NomNiveau   { get; set; }
        public int    OrdreNiveau { get; set; }
        public string NomContexte { get; set; }
        public string NomActivite { get; set; }

        // Stock cible de la fiche parente (pour jauge)
        public decimal? StockCible       { get; set; }
        public decimal  TotalDispoFiche  { get; set; }

        /// <summary>Ratio stock total de la fiche / stock cible (0..N). Null si pas de cible.</summary>
        public double? StockRatio =>
            StockCible.HasValue && StockCible.Value > 0
                ? (double)(TotalDispoFiche / StockCible.Value)
                : (double?)null;

        public bool    EstPerime  => DateDlc.HasValue && DateDlc.Value < DateTime.Today;
        public decimal CoutTotal  => QuantiteDisponible * CoutUnitaire;

        public override string ToString() =>
            $"{NomFiche} — {QuantiteDisponible} {UniteOutput} [{NomNiveau}]";
    }
}
