namespace CharlesNadejda.Navigation
{
    /// <summary>
    /// Parametres optionnels transmis lors d'un appel a <see cref="ScreenRouter.Navigate"/>.
    /// Permettent de passer du contexte supplementaire a l'ecran cible sans coupler les composants.
    /// </summary>
    public class NavigationParams
    {
        /// <summary>
        /// Force l'ecran Ressources a afficher un type specifique (ex: Ingredients, Fournisseurs).
        /// Utilise quand la navigation vient d'un raccourci plutot que de la sidebar.
        /// </summary>
        public RessourceType? RessourceType       { get; set; }
    }
}
