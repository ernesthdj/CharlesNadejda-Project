using System;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Fiche ingrédient (matière première).
    /// Représente la définition d'un ingrédient dans le référentiel — pas un lot physique.
    /// Le stock réel est agrégé depuis les lots_ingredients via le DAL.
    /// </summary>
    public class Ingredient
    {
        // ── Colonnes DB (table fiches_ingredients) ────────────────

        public int      Id                      { get; set; }
        public string   Nom                     { get; set; }
        public string   Marque                  { get; set; }
        public string   Description             { get; set; }
        /// <summary>Unité de base atomique : 'g', 'ml' ou 'piece'. Toujours l'unité de stockage interne.</summary>
        public string   UniteMesure             { get; set; }
        /// <summary>Nature physique : solide, liquide, poudre ou piece. Conditionne la densité obligatoire.</summary>
        public string   TypePhysique            { get; set; }
        /// <summary>Densité en g/ml — obligatoire si liquide ou poudre, pour les conversions de volume.</summary>
        public decimal? Densite                 { get; set; }
        /// <summary>Label du conditionnement commercial (ex: "Sac 10 kg", "Brique 1 L").</summary>
        public string   ConditionnementLabel    { get; set; }
        /// <summary>Quantité en unité de base par conditionnement (ex: 10000 pour Sac 10 kg en grammes).</summary>
        public decimal  QteParConditionnement   { get; set; } = 1m;
        /// <summary>Nombre de conditionnements par lot d'achat (ex: 6 briques, 4 sacs). Défaut = 1.</summary>
        public int      NbParLot                { get; set; } = 1;
        /// <summary>Prix de référence par conditionnement (€/sac, €/bouteille…).</summary>
        public decimal  PrixAchatReference      { get; set; }
        /// <summary>Seuil en unité de base en dessous duquel l'alerte stock se déclenche.</summary>
        public decimal? SeuilAlerteStock        { get; set; }
        /// <summary>Stock cible (100% de la jauge) en unité de base. Paramétré par l'utilisateur.</summary>
        public decimal? StockCible              { get; set; }
        public int?     IdFournisseurDefaut      { get; set; }
        /// <summary>Stock de rangement par défaut (FK nullable vers stocks). Utilisé par le filtre chip "Fiches" de FrmIngredients.</summary>
        public int?     IdStockDefaut            { get; set; }
        /// <summary>Duree de conservation par defaut en jours (nullable si non defini).</summary>
        public int?     DlcJoursReference       { get; set; }
        /// <summary>Label qualite (ex: "Bio", "AOP", "Grand Cru"). Nullable.</summary>
        public string   QualiteLabel            { get; set; }
        public bool     Actif                   { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par IngredientDAL via LEFT JOIN fournisseurs.</summary>
        public string   NomFournisseur          { get; set; }

        /// <summary>Chargé par IngredientDAL via COALESCE(SUM(lots_ingredients.quantite_disponible)).</summary>
        public decimal StockActuel { get; set; }

        // ── Propriétés calculées ──────────────────────────────────

        /// <summary>Quantité par conditionnement avec son unité, pour affichage — ex: "700 ml", "1 kg", "500 g".</summary>
        public string QteCondLabel =>
            $"{QteParConditionnement:0.##} {UniteMesure}";

        /// <summary>Prix par unité de base = PrixAchatReference / QteParConditionnement. Protégé contre division par zéro.</summary>
        public decimal PrixParUniteBase =>
            QteParConditionnement > 0 ? PrixAchatReference / QteParConditionnement : PrixAchatReference;

        /// <summary>Vrai si le stock actuel est inférieur ou égal au seuil d'alerte configuré.</summary>
        public bool EstEnAlerte =>
            SeuilAlerteStock.HasValue && StockActuel <= SeuilAlerteStock.Value;

        /// <summary>Nombre de conditionnements complets en stock = Floor(StockActuel / QteParConditionnement).</summary>
        public decimal StockPieces =>
            QteParConditionnement > 0 ? Math.Floor(StockActuel / QteParConditionnement) : 0;

        /// <summary>Stock cible exprimé en nombre de conditionnements = StockCible / QteParConditionnement.
        /// Null si aucun stock cible défini. Cohérent avec la saisie dans FrmIngredientEdit (nudStockCible en pièces).</summary>
        public decimal? StockCiblePieces =>
            StockCible.HasValue && QteParConditionnement > 0
                ? Math.Round(StockCible.Value / QteParConditionnement)
                : (decimal?)null;

        /// <summary>Ratio stock actuel / stock cible (0..N). Null si pas de cible définie.</summary>
        public double? StockRatio =>
            StockCible.HasValue && StockCible.Value > 0
                ? (double)(StockActuel / StockCible.Value)
                : (double?)null;

        public override string ToString() => $"{Nom} — {ConditionnementLabel} ({UniteMesure})";
    }
}
