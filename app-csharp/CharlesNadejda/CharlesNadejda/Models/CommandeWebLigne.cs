namespace CharlesNadejda.Models
{
    /// <summary>
    /// Ligne d'une commande web — un produit commandé avec sa quantité et son prix.
    /// Le sous-total est une colonne GENERATED côté MySQL (pas calculé en C#).
    /// </summary>
    public class CommandeWebLigne
    {
        // ── Colonnes DB (table commandes_web_lignes) ──────────────

        public int     Id             { get; set; }
        /// <summary>FK vers commandes_web.</summary>
        public int     IdCommande     { get; set; }
        /// <summary>FK vers produits_web.</summary>
        public int     IdProduitWeb   { get; set; }
        public int     Quantite       { get; set; }
        public decimal PrixUnitaire   { get; set; }
        /// <summary>Colonne GENERATED en DB = Quantite * PrixUnitaire.</summary>
        public decimal SousTotal      { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par CommandeWebDAL via JOIN produits_web.nom_commercial.</summary>
        public string NomProduit { get; set; }
    }
}
