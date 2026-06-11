namespace CharlesNadejda.Navigation
{
    /// <summary>
    /// Identifiants des elements de navigation dans la sidebar ERP.
    /// Chaque item est mappe vers un <see cref="ScreenId"/> + eventuellement un <see cref="RessourceType"/>
    /// dans FrmPrincipal.OnSidebarNavigation().
    /// </summary>
    public enum NavItemId
    {
        // ── Workflow ─────────────────────────────────────────────

        /// <summary>Dashboard principal — mappe vers ScreenId.Hub.</summary>
        Hub,

        /// <summary>Ecran de production BOM — mappe vers ScreenId.Production.</summary>
        Production,

        /// <summary>Planning previsionnel — mappe vers ScreenId.Planning (placeholder).</summary>
        Planning,

        /// <summary>Devis patisserie — mappe vers ScreenId.DevisPatisserie (placeholder).</summary>
        DevisPatisserie,

        // ── Stock & Achats ───────────────────────────────────────

        /// <summary>Gestion des emplacements de stock et liaisons activite/stock — RessourceType.Stocks.</summary>
        StocksLiaisons,

        /// <summary>Vue consolidee de tout le stock (matieres + fabriques) — RessourceType.VueStock.</summary>
        VueStockGlobal,

        /// <summary>Historique des mouvements de stock — mappe vers ScreenId.Mouvements (placeholder).</summary>
        Mouvements,

        /// <summary>Gestion des achats (lots d'ingredients) — RessourceType.Achats.</summary>
        AchatsLots,

        /// <summary>Repertoire fournisseurs — RessourceType.Fournisseurs.</summary>
        Fournisseurs,

        // ── Referentiels ─────────────────────────────────────────

        /// <summary>Catalogue des fiches ingredients (matieres premieres) — RessourceType.Ingredients.</summary>
        Ingredients,

        /// <summary>Parametres et configuration de l'application.</summary>
        Parametres,

        // ── Boutique en ligne ────────────────────────────────────

        /// <summary>Mini CMS boutique web : produits, categories, commandes.</summary>
        BoutiqueWeb
    }
}
