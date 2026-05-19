namespace CharlesNadejda.Navigation
{
    /// <summary>
    /// Identifiants des éléments de navigation dans la sidebar ERP.
    /// Chaque item est mappé vers un ScreenId + éventuellement un RessourceType
    /// dans FrmPrincipal.OnSidebarNavigation().
    /// </summary>
    public enum NavItemId
    {
        // ── Workflow ─────────────────────────────────────────────
        Hub,
        Production,
        Planning,
        DevisPatisserie,

        // ── Stock & Achats ───────────────────────────────────────
        StocksLiaisons,
        VueStockGlobal,
        Mouvements,
        AchatsLots,
        Fournisseurs,

        // ── Référentiels ─────────────────────────────────────────
        FichesBom,
        Ingredients,
        NiveauxContextes,
        Parametres,

        // ── Boutique en ligne ────────────────────────────────────
        BoutiqueWeb
    }
}
