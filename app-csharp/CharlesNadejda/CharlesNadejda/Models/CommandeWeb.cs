using System;
using System.Collections.Generic;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Commande passée via le site e-commerce Laravel.
    /// Lecture seule côté ERP C# — les commandes sont créées par le site web.
    /// </summary>
    public class CommandeWeb
    {
        // ── Constantes de statut ──────────────────────────────────

        /// <summary>Panier en cours de constitution (pas encore validé).</summary>
        public const string StatutPanier  = "panier";
        /// <summary>Commande validée et payée.</summary>
        public const string StatutPayee   = "payee";
        /// <summary>Commande annulée (par le client ou l'admin).</summary>
        public const string StatutAnnulee = "annulee";

        // ── Colonnes DB (table commandes_web) ─────────────────────

        public int       Id                 { get; set; }
        public int       IdClient           { get; set; }
        /// <summary>Statut de la commande : panier, payee, annulee. Voir constantes StatutXxx.</summary>
        public string    Statut             { get; set; }
        public decimal   TotalTtc           { get; set; }
        public string    AdresseLivraison   { get; set; }
        public DateTime? DateCommande       { get; set; }
        public DateTime  DateCreation       { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par CommandeWebDAL via JOIN clients.nom.</summary>
        public string NomClient    { get; set; }
        /// <summary>Chargé par CommandeWebDAL via JOIN clients.prenom.</summary>
        public string PrenomClient { get; set; }
        /// <summary>Chargé par CommandeWebDAL via JOIN clients.email.</summary>
        public string EmailClient  { get; set; }
        /// <summary>Chargé par CommandeWebDAL via sous-requête COUNT sur commandes_web_lignes.</summary>
        public int    NbArticles     { get; set; }
        /// <summary>Chargé par CommandeWebDAL via GROUP_CONCAT (ex: "Baguette x11, Pain x2").</summary>
        public string ResumeArticles { get; set; }

        /// <summary>Lignes détaillées — chargées optionnellement par CommandeWebDAL.GetById().</summary>
        public List<CommandeWebLigne> Lignes { get; set; } = new List<CommandeWebLigne>();

        // ── Propriétés calculées ──────────────────────────────────

        /// <summary>Nom complet = Prénom + Nom du client.</summary>
        public string NomCompletClient => $"{PrenomClient} {NomClient}";

        public override string ToString() => $"Commande #{Id} — {NomCompletClient}";
    }
}
