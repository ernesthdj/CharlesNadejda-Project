namespace CharlesNadejda.Navigation
{
    /// <summary>
    /// Types de referentiels affiches dans l'ecran Ressources.
    /// Determine quel panneau est construit quand <see cref="ScreenId.Ressources"/> est navigue.
    /// La valeur est stockee dans <see cref="AppState.RessourceActive"/> et lue par le guard du ScreenRouter.
    /// </summary>
    public enum RessourceType
    {
        /// <summary>Gestion des emplacements de stock physiques et liaisons activite/stock.</summary>
        Stocks,

        /// <summary>Catalogue des fiches ingredients (matieres premieres) avec prix, conditionnement, seuils.</summary>
        Ingredients,

        /// <summary>Repertoire des fournisseurs (coordonnees, notes).</summary>
        Fournisseurs,

        /// <summary>Gestion des achats : lots d'ingredients avec tracabilite (date, prix, DLC, TVA).</summary>
        Achats,

        /// <summary>Vue consolidee en lecture seule de tout le stock (lots + produits fabriques).</summary>
        VueStock
    }
}
