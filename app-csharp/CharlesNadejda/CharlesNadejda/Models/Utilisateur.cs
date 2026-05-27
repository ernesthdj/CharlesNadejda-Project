namespace CharlesNadejda.Models
{
    // 📌 SCRIPT DEFENSE — Étape 0.1 : Modèle POCO + ToString() affiché dans StatusBar
    public class Utilisateur
    {
        public int    Id      { get; set; }
        public string Nom     { get; set; }
        public string Prenom  { get; set; }
        public string Email   { get; set; }
        public string Role    { get; set; }

        public override string ToString() => $"{Prenom} {Nom} ({Role})";
    }
}
