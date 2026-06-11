namespace CharlesNadejda.Models
{
    /// <summary>
    /// Fournisseur de matières premières.
    /// Référencé par fiches_ingredients (fournisseur par défaut) et lots_ingredients (fournisseur du lot).
    /// </summary>
    public class Fournisseur
    {
        // ── Colonnes DB (table fournisseurs) ──────────────────────

        public int    Id        { get; set; }
        public string Nom       { get; set; }
        public string Contact   { get; set; }
        public string Email     { get; set; }
        public string Telephone { get; set; }
        public string Adresse   { get; set; }
        public string Notes     { get; set; }

        public override string ToString() => Nom;
    }
}
