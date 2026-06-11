using System;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Entrée de stock d'un produit fabriqué (table bom_stocks).
    /// Chaque ligne représente un lot de production avec sa quantité disponible et son coût unitaire.
    /// Consommé en FIFO lors des productions de niveau supérieur.
    /// </summary>
    public class BomStock
    {
        // ── Colonnes DB (table bom_stocks) ────────────────────────

        public int       Id                  { get; set; }
        /// <summary>FK vers bom_niveaux — niveau de la hiérarchie BOM.</summary>
        public int       IdNiveau            { get; set; }
        /// <summary>FK vers bom_fiches — quelle recette a produit ce stock.</summary>
        public int       IdFiche             { get; set; }
        /// <summary>FK vers bom_productions — quelle production a créé cette entrée.</summary>
        public int       IdProduction        { get; set; }
        public decimal   QuantiteDisponible  { get; set; }
        /// <summary>Coût par unité produite, calculé lors de la production (FIFO).</summary>
        public decimal   CoutUnitaire        { get; set; }
        public DateTime  DateProduction      { get; set; }
        /// <summary>Date Limite de Consommation — null si pas de péremption.</summary>
        public DateTime? DateDlc             { get; set; }
        public DateTime  DateCreation        { get; set; }
        /// <summary>Discriminant direct — FK vers bom_contextes (ajouté en v11).</summary>
        public int    IdContexte  { get; set; }
        /// <summary>Discriminant direct — FK vers activites (ajouté en v11).</summary>
        public int    IdActivite  { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par BomStockDAL via JOIN bom_fiches.nom.</summary>
        public string NomFiche    { get; set; }
        /// <summary>Chargé par BomStockDAL via JOIN bom_fiches.unite_output.</summary>
        public string UniteOutput { get; set; }
        /// <summary>Chargé par BomStockDAL via JOIN bom_niveaux.nom.</summary>
        public string NomNiveau   { get; set; }
        /// <summary>Chargé par BomStockDAL via JOIN bom_niveaux.ordre.</summary>
        public int    OrdreNiveau { get; set; }
        /// <summary>Chargé par BomStockDAL via JOIN bom_contextes.nom.</summary>
        public string NomContexte { get; set; }
        /// <summary>Chargé par BomStockDAL via JOIN activites.nom.</summary>
        public string NomActivite { get; set; }
        /// <summary>Chargé par BomStockDAL via JOIN bom_fiches.stock_cible — pour la jauge.</summary>
        public decimal? StockCible       { get; set; }
        /// <summary>Chargé par BomStockDAL via sous-requête SUM sur tous les lots de la même fiche.</summary>
        public decimal  TotalDispoFiche  { get; set; }

        // ── Propriétés calculées ──────────────────────────────────

        /// <summary>Ratio stock total de la fiche / stock cible (0..N). Null si pas de cible.</summary>
        public double? StockRatio =>
            StockCible.HasValue && StockCible.Value > 0
                ? (double)(TotalDispoFiche / StockCible.Value)
                : (double?)null;

        /// <summary>Vrai si la DLC est dépassée (comparé à DateTime.Today).</summary>
        public bool    EstPerime  => DateDlc.HasValue && DateDlc.Value < DateTime.Today;
        /// <summary>Valeur totale de cette entrée = QuantiteDisponible * CoutUnitaire.</summary>
        public decimal CoutTotal  => QuantiteDisponible * CoutUnitaire;

        public override string ToString() =>
            $"{NomFiche} — {QuantiteDisponible} {UniteOutput} [{NomNiveau}]";
    }
}
