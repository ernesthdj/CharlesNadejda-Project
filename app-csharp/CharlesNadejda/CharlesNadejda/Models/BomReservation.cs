using System;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Réservation de stock sur un lot d'ingrédient pour un contexte de production.
    /// Permet de "bloquer" du stock avant la production effective.
    /// Libérée automatiquement lors de la consommation FIFO (BomProductionDAL.ConsumeStock).
    /// </summary>
    public class BomReservation
    {
        // ── Colonnes DB (table bom_reservations) ──────────────────

        public int      Id               { get; set; }
        /// <summary>FK vers lots_ingredients — le lot physique réservé.</summary>
        public int      IdLot            { get; set; }
        /// <summary>FK vers bom_contextes — pour quel contexte de production.</summary>
        public int      IdContexte       { get; set; }
        public decimal  QuantiteReservee { get; set; }
        public DateTime DateReservation  { get; set; }
        public string   Notes            { get; set; }
        /// <summary>Actif = réservation en cours. Passé à false lors de la libération (soft delete).</summary>
        public bool     Actif            { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par BomReservationDAL via JOIN fiches_ingredients.nom.</summary>
        public string NomIngredient { get; set; }
        /// <summary>Chargé par BomReservationDAL via JOIN fiches_ingredients.unite_mesure.</summary>
        public string UniteMesure   { get; set; }
        /// <summary>Chargé par BomReservationDAL via JOIN bom_contextes.nom.</summary>
        public string NomContexte   { get; set; }

        public override string ToString() =>
            $"{NomContexte} — {QuantiteReservee} {UniteMesure} de {NomIngredient}";
    }
}
