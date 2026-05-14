using System;

namespace CharlesNadejda.Models
{
    public class Ingredient
    {
        public int      Id                      { get; set; }
        public string   Nom                     { get; set; }
        public string   Marque                  { get; set; }
        public string   Description             { get; set; }
        /// <summary>Unité de base atomique : 'g', 'ml' ou 'piece'. Toujours l'unité de stockage interne.</summary>
        public string   UniteMesure             { get; set; }
        public string   TypePhysique            { get; set; }   // solide | liquide | poudre | piece
        public decimal? Densite                 { get; set; }   // g/ml — obligatoire si liquide ou poudre
        /// <summary>Label du conditionnement commercial (ex: "Sac 10 kg", "Brique 1 L").</summary>
        public string   ConditionnementLabel    { get; set; }
        /// <summary>Quantité en unité de base par conditionnement (ex: 10000 pour Sac 10 kg en grammes).</summary>
        public decimal  QteParConditionnement   { get; set; } = 1m;
        /// <summary>Prix de référence par conditionnement (€/sac, €/bouteille…).</summary>
        public decimal  PrixAchatReference      { get; set; }
        public decimal? SeuilAlerteStock        { get; set; }
        /// <summary>Stock cible (100% de la jauge) en unité de base. Paramétré par l'utilisateur.</summary>
        public decimal? StockCible              { get; set; }
        public int?     IdFournisseurDefaut      { get; set; }
        public string   NomFournisseur          { get; set; }   // jointure fournisseurs
        public int      IdStock                 { get; set; }
        public string   StockNom                { get; set; }   // jointure stocks
        public bool     Actif                   { get; set; }

        /// <summary>Coût par unité de base (€/g, €/ml, €/pce), calculé depuis le prix de référence.</summary>
        public decimal PrixParUniteBase =>
            QteParConditionnement > 0 ? PrixAchatReference / QteParConditionnement : PrixAchatReference;

        public bool EstEnAlerte =>
            SeuilAlerteStock.HasValue && StockActuel <= SeuilAlerteStock.Value;

        // Calculé depuis les lots — rempli par le DAL si besoin
        public decimal StockActuel { get; set; }

        /// <summary>Nombre de pièces (conditionnements) en stock, arrondi vers le bas.</summary>
        public decimal StockPieces =>
            QteParConditionnement > 0 ? Math.Floor(StockActuel / QteParConditionnement) : 0;

        /// <summary>Ratio stock actuel / stock cible (0..N). Null si pas de cible définie.</summary>
        public double? StockRatio =>
            StockCible.HasValue && StockCible.Value > 0
                ? (double)(StockActuel / StockCible.Value)
                : (double?)null;

        public override string ToString() => $"{Nom} — {ConditionnementLabel} ({UniteMesure})";
    }
}
