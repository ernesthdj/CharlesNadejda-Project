namespace CharlesNadejda.Models
{
    /// <summary>
    /// Utilisateur authentifié de l'ERP (table utilisateurs).
    /// Seul le rôle "admin" peut se connecter à l'application C#.
    /// L'authentification passe par BCrypt (compatible avec le site Laravel).
    /// </summary>
    public class Utilisateur
    {
        // ── Colonnes DB (table utilisateurs) ──────────────────────

        public int    Id      { get; set; }
        public string Nom     { get; set; }
        public string Prenom  { get; set; }
        public string Email   { get; set; }
        /// <summary>Rôle de l'utilisateur : "admin" ou "client".</summary>
        public string Role    { get; set; }

        public override string ToString() => $"{Prenom} {Nom} ({Role})";
    }
}
