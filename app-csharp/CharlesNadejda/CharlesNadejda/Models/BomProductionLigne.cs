namespace CharlesNadejda.Models
{
    /// <summary>
    /// Ligne de traçabilité d'une production BOM.
    /// Enregistre quel lot ou stock a fourni quelle quantité à quel coût.
    /// Créée par BomProductionDAL.ConsumeStock() lors de la consommation FIFO.
    /// </summary>
    public class BomProductionLigne
    {
        // ── Constantes de type source ─────────────────────────────

        /// <summary>Source : lot de matière première (table lots_ingredients).</summary>
        public const string SourceLotIngredient = "lot_ingredient";
        /// <summary>Source : stock de produit intermédiaire (table bom_stocks).</summary>
        public const string SourceBomStock = "bom_stock";

        // ── Colonnes DB (table bom_productions_lignes) ────────────

        public int     Id                   { get; set; }
        /// <summary>FK vers bom_productions — la production parente.</summary>
        public int     IdProduction         { get; set; }
        /// <summary>Discriminant : "lot_ingredient" ou "bom_stock". Voir constantes SourceXxx.</summary>
        public string  TypeSource           { get; set; }
        /// <summary>FK vers lots_ingredients — rempli si TypeSource == "lot_ingredient".</summary>
        public int?    IdLotIngredient      { get; set; }
        /// <summary>FK vers bom_stocks — rempli si TypeSource == "bom_stock".</summary>
        public int?    IdBomStock           { get; set; }
        public decimal QuantiteConsommee    { get; set; }
        /// <summary>Prix unitaire au moment de la consommation (figé pour traçabilité).</summary>
        public decimal CoutUnitaireMoment   { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par le DAL — nom de l'ingrédient ou de la fiche source.</summary>
        public string NomSource    { get; set; }
        /// <summary>Chargé par le DAL — unité de la source (g, ml, piece, kg...).</summary>
        public string UniteSource  { get; set; }

        // ── Propriétés calculées ──────────────────────────────────

        /// <summary>Coût total de cette ligne = QuantiteConsommee * CoutUnitaireMoment.</summary>
        public decimal SousTotal => QuantiteConsommee * CoutUnitaireMoment;

        public override string ToString() =>
            $"{QuantiteConsommee} {UniteSource} de {NomSource}";
    }
}
