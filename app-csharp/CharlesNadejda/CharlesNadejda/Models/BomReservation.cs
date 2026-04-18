using System;

namespace CharlesNadejda.Models
{
    public class BomReservation
    {
        public int      Id               { get; set; }
        public int      IdLot            { get; set; }
        public int      IdContexte       { get; set; }
        public decimal  QuantiteReservee { get; set; }
        public DateTime DateReservation  { get; set; }
        public string   Notes            { get; set; }
        public bool     Actif            { get; set; }

        // Jointures
        public string NomIngredient { get; set; }
        public string UniteMesure   { get; set; }
        public string NomContexte   { get; set; }

        public override string ToString() =>
            $"{NomContexte} — {QuantiteReservee} {UniteMesure} de {NomIngredient}";
    }
}
