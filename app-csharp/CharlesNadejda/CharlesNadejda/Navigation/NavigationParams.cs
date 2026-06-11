namespace CharlesNadejda.Navigation
{
    /// <summary>
    /// Parametres optionnels transmis lors d'un appel a <see cref="ScreenRouter.Navigate"/>.
    /// Permettent de passer du contexte supplementaire a l'ecran cible sans coupler les composants.
    /// </summary>
    public class NavigationParams
    {
        /// <summary>
        /// ID de l'entite vers laquelle scroller dans le DGV apres construction de l'ecran.
        /// Utilise par les ecrans Ressources et Production pour restaurer la selection apres un CRUD.
        /// </summary>
        public int?          ScrollToId           { get; set; }

        /// <summary>
        /// Entite metier a pre-charger dans l'ecran cible (ex: FicheIngredient pour ouvrir un edit).
        /// Type object pour rester generique — l'ecran cible fait le cast vers le bon Model.
        /// </summary>
        public object        Entity               { get; set; }

        /// <summary>
        /// Si true, l'ecran s'ouvre directement en mode edition sur l'entite passee dans Entity.
        /// </summary>
        public bool          IsEdit               { get; set; }

        /// <summary>
        /// Force l'ecran Ressources a afficher un type specifique (ex: Ingredients, Fournisseurs).
        /// Utilise quand la navigation vient d'un raccourci plutot que de la sidebar.
        /// </summary>
        public RessourceType? RessourceType       { get; set; }

        /// <summary>
        /// US-08 : si true, l'ecran Ingredients n'affiche que les fiches dont le stock est en alerte.
        /// Positionne lors de la navigation depuis les StatCards du Hub.
        /// </summary>
        public bool          FiltreAlertesSeulement { get; set; }
    }
}
