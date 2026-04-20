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

        public ScreenRouter(AppState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public void Navigate(ScreenId screen, NavigationParams parms = null)
        {
            _state.ActiveScreen = screen;
            var p = parms ?? new NavigationParams();
            switch (screen)
            {
                case ScreenId.Onboarding:      OnOnboarding?.Invoke(p);      break;
                case ScreenId.Hub:             OnHub?.Invoke(p);             break;
                case ScreenId.ContexteNiveaux: OnContexteNiveaux?.Invoke(p); break;
                case ScreenId.Ressources:      OnRessources?.Invoke(p);      break;
                case ScreenId.Production:      OnProduction?.Invoke(p);      break;
            }
        }
    }
}
