using System;

namespace CharlesNadejda.Navigation
{
    public class ScreenRouter
    {
        private readonly AppState _state;

        public Action<NavigationParams> OnOnboarding      { get; set; }
        public Action<NavigationParams> OnHub             { get; set; }
        public Action<NavigationParams> OnContexteNiveaux { get; set; }
        public Action<NavigationParams> OnRessources      { get; set; }
        public Action<NavigationParams> OnProduction      { get; set; }
        public Action<NavigationParams> OnPlaceholder     { get; set; }

        // Guard de re-navigation : mémorise le dernier écran instancié
        private ScreenId?      _lastScreen;
        private int            _lastContexteId  = -1;
        private RessourceType  _lastRessource;

        public ScreenRouter(AppState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        /// <summary>
        /// Invalide le guard — à appeler quand on veut forcer un rechargement
        /// (ex : après suppression d'un contexte, changement d'activité).
        /// </summary>
        public void Invalidate() => _lastScreen = null;

        public void Navigate(ScreenId screen, NavigationParams parms = null)
        {
            // Guard singleton : évite de reconstruire un écran déjà affiché à l'identique.
            if (_lastScreen == screen)
            {
                if (screen == ScreenId.ContexteNiveaux
                    && _lastContexteId == (_state.ActiveContexte?.Id ?? -1))
                    return;

                if (screen == ScreenId.Ressources
                    && _lastRessource == _state.RessourceActive)
                    return;

                if (screen == ScreenId.Hub)
                    return;
            }

            _lastScreen      = screen;
            _lastContexteId  = _state.ActiveContexte?.Id ?? -1;
            _lastRessource   = _state.RessourceActive;

            _state.ActiveScreen = screen;
            var p = parms ?? new NavigationParams();
            switch (screen)
            {
                case ScreenId.Onboarding:      OnOnboarding?.Invoke(p);      break;
                case ScreenId.Hub:             OnHub?.Invoke(p);             break;
                case ScreenId.ContexteNiveaux: OnContexteNiveaux?.Invoke(p); break;
                case ScreenId.Ressources:      OnRessources?.Invoke(p);      break;
                case ScreenId.Production:      OnProduction?.Invoke(p);      break;
                case ScreenId.Planning:
                case ScreenId.DevisPatisserie:
                case ScreenId.Mouvements:
                case ScreenId.Parametres:      OnPlaceholder?.Invoke(p);     break;
            }
        }
    }
}
