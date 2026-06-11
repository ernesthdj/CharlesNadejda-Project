namespace CharlesNadejda.Models
{
    /// <summary>
    /// Catégorie de produits pour la boutique web (ex: "Tablettes", "Bonbons").
    /// Un produit sans catégorie reste visible (id_categorie = NULL avec FK SET NULL).
    /// </summary>
    public class CategorieWeb
    {
        // ── Colonnes DB (table categories_web) ────────────────────

        public int    Id              { get; set; }
        public string Nom             { get; set; }
        public string Description     { get; set; }
        /// <summary>Ordre d'affichage dans le menu de la boutique (croissant).</summary>
        public int    OrdreAffichage  { get; set; }
        public bool   Actif           { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par CategorieWebDAL via COUNT(produits_web) WHERE en_vente = 1.</summary>
        public int NbProduits { get; set; }

        public override string ToString() => Nom;
    }
}
