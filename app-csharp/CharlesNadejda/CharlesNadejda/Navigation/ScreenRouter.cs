using System;

namespace CharlesNadejda.Navigation
{
    // ScreenRouter — mon routeur de navigation central pour l'ERP
    // C'est lui qui décide quel écran afficher quand on clique dans la sidebar.
    // Il utilise un pattern "delegate callbacks" : chaque écran a un Action<NavigationParams>
    // que le MainForm enregistre, et le routeur l'invoque au bon moment.
    // Le guard singleton empêche de reconstruire un écran déjà affiché — optimisation importante
    // parce que chaque écran fait des requêtes SQL au chargement.
    public class ScreenRouter
    {
        // AppState contient l'état global de l'app (activité courante, contexte actif, écran actif)
        // Je le reçois par injection dans le constructeur — pas de static, c'est plus propre
        private readonly AppState _state;

        // Chaque propriété est un callback que le MainForm branche au démarrage.
        // Quand Navigate() est appelé avec ScreenId.Hub par exemple, OnHub est invoqué.
        // Le MainForm sait alors qu'il doit instancier et afficher le panel Hub.
        public Action<NavigationParams> OnOnboarding      { get; set; }
        public Action<NavigationParams> OnHub             { get; set; }
        public Action<NavigationParams> OnContexteNiveaux { get; set; }
        public Action<NavigationParams> OnRessources      { get; set; }
        public Action<NavigationParams> OnProduction      { get; set; }
        public Action<NavigationParams> OnPlaceholder     { get; set; }
        public Action<NavigationParams> OnBoutiqueWeb     { get; set; }
        public Action<NavigationParams> OnParametres      { get; set; }

        // Guard de re-navigation : je mémorise le dernier écran instancié
        // pour éviter de reconstruire un écran déjà affiché.
        // _lastScreen = quel ScreenId est actuellement visible
        private ScreenId?      _lastScreen;

        // _lastContexteId = l'id du contexte qui était actif au moment du dernier Navigate
        // Utile parce que Production et ContexteNiveaux dépendent du contexte sélectionné
        private int            _lastContexteId  = -1;

        // _lastRessource = le type de ressource actif au dernier Navigate (pour l'écran Ressources)
        private RessourceType  _lastRessource;

        // Constructeur — je refuse un AppState null, c'est une dépendance obligatoire
        public ScreenRouter(AppState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        /// <summary>
        /// Invalidate() remet le guard à null — ça force le prochain Navigate()
        /// à reconstruire l'écran même si c'est le même ScreenId.
        /// J'appelle ça après une suppression de contexte, un changement d'activité, etc.
        /// </summary>
        public void Invalidate() => _lastScreen = null;

        // Navigate() — le coeur du routeur. Reçoit un ScreenId et des params optionnels.
        // Vérifie le guard, met à jour l'état, et invoque le bon callback.
        public void Navigate(ScreenId screen, NavigationParams parms = null)
        {
            // Guard singleton : si on demande le même écran que celui déjà affiché,
            // on vérifie si le contexte ou la ressource ont changé avant de reconstruire.
            if (_lastScreen == screen)
            {
                // ContexteNiveaux et Production partagent le même callback (OnProduction)
                // donc je fusionne leur guard — si le contexte n'a pas changé, on skip
                if ((screen == ScreenId.ContexteNiveaux || screen == ScreenId.Production)
                    && _lastContexteId == (_state.ActiveContexte?.Id ?? -1))
                    return;

                // Pour Ressources, je vérifie si le type de ressource a changé
                // (ex : passer de "Ingrédients" à "Fournisseurs")
                if (screen == ScreenId.Ressources
                    && _lastRessource == _state.RessourceActive)
                    return;

                // Le Hub est toujours le même, pas besoin de reconstruire
                if (screen == ScreenId.Hub)
                    return;
            }

            // Mise à jour du guard — je sauvegarde l'état actuel pour le prochain appel
            _lastScreen      = screen;
            _lastContexteId  = _state.ActiveContexte?.Id ?? -1;
            _lastRessource   = _state.RessourceActive;

            // Mise à jour de l'état global — le MainForm peut lire ActiveScreen à tout moment
            _state.ActiveScreen = screen;

            // Création des params par défaut si non fournis
            var p = parms ?? new NavigationParams();

            // Switch sur le ScreenId — chaque case invoque le bon callback
            // Le ?. (null-conditional) protège si le callback n'a pas été branché
            switch (screen)
            {
                case ScreenId.Onboarding:      OnOnboarding?.Invoke(p);      break;
                case ScreenId.Hub:             OnHub?.Invoke(p);             break;
                case ScreenId.ContexteNiveaux: OnProduction?.Invoke(p); break;  // Fusionné → Production (même écran)
                case ScreenId.Ressources:      OnRessources?.Invoke(p);      break;
                case ScreenId.Production:      OnProduction?.Invoke(p);      break;
                case ScreenId.BoutiqueWeb:     OnBoutiqueWeb?.Invoke(p);     break;
                case ScreenId.Parametres:      OnParametres?.Invoke(p);      break;

                // Planning, DevisPatisserie et Mouvements ne sont pas encore implémentés
                // → ils tombent tous sur OnPlaceholder qui affiche un écran "Coming soon"
                case ScreenId.Planning:
                case ScreenId.DevisPatisserie:
                case ScreenId.Mouvements:      OnPlaceholder?.Invoke(p);     break;
            }
        }
    }
}
