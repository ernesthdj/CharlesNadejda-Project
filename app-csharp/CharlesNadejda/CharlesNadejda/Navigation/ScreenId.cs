namespace CharlesNadejda.Navigation
{
    /// <summary>
    /// Identifie chaque ecran navigable de l'application ERP.
    /// Utilise par <see cref="ScreenRouter"/> pour determiner quel panneau afficher
    /// et par <see cref="AppState.ActiveScreen"/> pour memoriser l'ecran actif.
    /// </summary>
    public enum ScreenId
    {
        /// <summary>Ecran de demarrage : selection de l'activite (branche metier).</summary>
        Onboarding,

        /// <summary>Dashboard principal avec StatCards (alertes stock, productions recentes, KPI).</summary>
        Hub,

        /// <summary>
        /// Ecran de gestion Contexte/Niveaux BOM.
        /// Fusionne avec Production dans un seul panneau depuis v19.
        /// </summary>
        ContexteNiveaux,

        /// <summary>
        /// Ecran generique pour les referentiels (ingredients, fournisseurs, stocks, lots, vue stock).
        /// Le contenu affiche depend de <see cref="AppState.RessourceActive"/>.
        /// </summary>
        Ressources,

        /// <summary>
        /// Ecran de production BOM : selection contexte, niveaux, fiches, lancement de production.
        /// Partage le meme callback que ContexteNiveaux dans le ScreenRouter.
        /// </summary>
        Production,

        // ── Phase 2+ — placeholders (ecrans non encore implementes) ──

        /// <summary>Planning de production (previsionnel). Non implemente — affiche un placeholder.</summary>
        Planning,

        /// <summary>Devis patisserie personnalises pour les clients. Non implemente — affiche un placeholder.</summary>
        DevisPatisserie,

        /// <summary>Historique des mouvements de stock (entrees/sorties). Non implemente — affiche un placeholder.</summary>
        Mouvements,

        /// <summary>Ecran de parametres et configuration de l'application.</summary>
        Parametres,

        /// <summary>Mini CMS pour la boutique en ligne : gestion des produits web, categories et commandes.</summary>
        BoutiqueWeb
    }
}
