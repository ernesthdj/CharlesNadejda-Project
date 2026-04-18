using System;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Modèle de lecture seule mappant la VIEW vue_stock_global.
    /// Unifie lots_ingredients (matières premières) et bom_stocks (produits fabriqués).
    /// Toute modification passe par LotDAL ou BomStockDAL — jamais via ce modèle.
    /// </summary>
    public class VueStockGlobal
    {
        public string    TypeStock           { get; set; } // "lot_ingredient" | "produit_fabrique"
        public int       IdEntree            { get; set; }
        public string    Nom                 { get; set; }
        public string    Unite               { get; set; }
        public decimal   QuantiteTotale      { get; set; }
        public decimal   QuantiteReservee    { get; set; }
        public decimal   QuantiteDispoReelle { get; set; }
        public decimal   CoutUnitaire        { get; set; }
        public DateTime? DateDlc             { get; set; }

        // Lots uniquement (NULL pour produits fabriqués)
        public int?      IdStock             { get; set; }
        public string    StockNom            { get; set; }

        // Produits fabriqués uniquement (NULL pour lots)
        public int?      IdActivite          { get; set; }
        public string    NomActivite         { get; set; }
        public int?      IdContexte          { get; set; }
        public int?      IdNiveau            { get; set; }
        public int?      IdFicheBom          { get; set; }

        public bool EstLot              => TypeStock == "lot_ingredient";
        public bool EstEnAlerte         => QuantiteDispoReelle <= 0;
        public bool ADesReservations    => QuantiteReservee > 0;
    }
}
