using System;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Lot d'achat d'un ingrédient (table lots_ingredients).
    /// Représente un achat physique avec quantité, prix, DLC et traçabilité fournisseur.
    /// Consommé en FIFO par le moteur de production BOM.
    /// </summary>
    public class Lot
    {
        // ── Colonnes DB (table lots_ingredients) ──────────────────

        public int      Id                      { get; set; }
        /// <summary>FK vers fiches_ingredients — quel ingrédient ce lot concerne.</summary>
        public int      IdFicheIngredient       { get; set; }
        /// <summary>FK vers stocks — dans quel emplacement physique ce lot est rangé.</summary>
        public int      IdStock                 { get; set; }
        /// <summary>Nombre de conditionnements achetés (ex: 5 sacs).</summary>
        public decimal  NbConditionnements      { get; set; }
        public string   NumeroLot               { get; set; }
        public int?     IdFournisseur            { get; set; }
        public DateTime DateAchat               { get; set; }
        public DateTime? DatePeremption         { get; set; }
        /// <summary>Quantité totale en unité de base (g/ml/pce) = NbConditionnements * QteParConditionnement.</summary>
        public decimal  QuantiteInitiale        { get; set; }
        public decimal  QuantiteDisponible      { get; set; }
        /// <summary>Prix HTVA par conditionnement.</summary>
        public decimal  PrixUnitaire            { get; set; }
        /// <summary>Total HTVA de l'achat = NbConditionnements * PrixUnitaire.</summary>
        public decimal  PrixAchatReel           { get; set; }
        /// <summary>Taux de TVA en % (0 = exonéré).</summary>
        public decimal  TvaPct                  { get; set; }
        public string   ReferenceFacture        { get; set; }
        public string   Notes                   { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par LotDAL via JOIN stocks.nom.</summary>
        public string   StockNom                { get; set; }
        /// <summary>Chargé par LotDAL via JOIN fiches_ingredients.nom.</summary>
        public string   NomIngredient           { get; set; }
        /// <summary>Chargé par LotDAL via JOIN fiches_ingredients.unite_mesure — unité de base ('g','ml','piece').</summary>
        public string   UniteMesure             { get; set; }
        /// <summary>Chargé par LotDAL via JOIN fiches_ingredients.conditionnement_label.</summary>
        public string   ConditionnementLabel    { get; set; }
        /// <summary>Chargé par LotDAL via JOIN fiches_ingredients.qte_par_conditionnement.</summary>
        public decimal  QteParConditionnement   { get; set; }
        /// <summary>Chargé par LotDAL via LEFT JOIN fournisseurs.nom.</summary>
        public string   NomFournisseur          { get; set; }

        // ── Propriétés calculées ──────────────────────────────────

        /// <summary>Prix par unité de base (€/g, €/ml, €/pce) = PrixUnitaire / QteParConditionnement. Protégé division par zéro.</summary>
        public decimal  PrixUnitaireBase        => QteParConditionnement > 0 ? PrixUnitaire / QteParConditionnement : 0;
    }
}
