using System;
using System.Collections.Generic;

namespace CharlesNadejda.Models
{
    public class CommandeWeb
    {
        public int       Id                 { get; set; }
        public int       IdClient           { get; set; }
        public string    Statut             { get; set; }   // panier, payee, annulee
        public decimal   TotalTtc           { get; set; }
        public string    AdresseLivraison   { get; set; }
        public DateTime? DateCommande       { get; set; }
        public DateTime  DateCreation       { get; set; }

        // Jointures — chargées par le DAL
        public string NomClient    { get; set; }   // clients.nom
        public string PrenomClient { get; set; }   // clients.prenom
        public string EmailClient  { get; set; }   // clients.email

        // Calculé
        public int NbArticles { get; set; }

        // Chargées optionnellement
        public List<CommandeWebLigne> Lignes { get; set; } = new List<CommandeWebLigne>();

        public string NomCompletClient => $"{PrenomClient} {NomClient}";

        public override string ToString() => $"Commande #{Id} — {NomCompletClient}";
    }
}
