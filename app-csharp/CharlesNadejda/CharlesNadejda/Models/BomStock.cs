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

        public bool EstPerime => DateDlc.HasValue && DateDlc.Value < DateTime.Today;

        public override string ToString() =>
            $"{NomFiche} — {QuantiteDisponible} {UniteOutput} [{NomNiveau}]";
    }
}
