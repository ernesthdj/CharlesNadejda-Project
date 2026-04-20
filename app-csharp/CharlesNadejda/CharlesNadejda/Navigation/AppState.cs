using System;
using CharlesNadejda.Models;

namespace CharlesNadejda.Navigation
{
    public class AppState
    {
        public Activite    ActiveActivite  { get; private set; }
        public BomContexte ActiveContexte  { get; private set; }
        public BomNiveau   ActiveNiveau    { get; private set; }

        public ScreenId      ActiveScreen    { get; internal set; } = ScreenId.Onboarding;
        public RessourceType RessourceActive { get; private set; }

        public int DgvFichesRowIndex    { get; set; } = -1;
        public int DgvRessourceRowIndex { get; set; } = -1;

        // US-08 : filtre alertes pour navigation contextuelle depuis les StatCards du Hub
        public bool FiltreAlertesSeulement { get; private set; }

        public event EventHandler StateChanged;

        public void SetActivite(Activite a)
        {
            bool changed = ActiveActivite?.Id != a?.Id;
            ActiveActivite = a;
            if (changed) { ActiveContexte = null; ActiveNiveau = null; DgvFichesRowIndex = -1; }
            RaiseChanged();
        }

        public void SetContexte(BomContexte c)
        {
            ActiveContexte = c;
            ActiveNiveau   = null;
            DgvFichesRowIndex = -1;
            RaiseChanged();
        }

        public void SetNiveau(BomNiveau n)
        {
            ActiveNiveau = n;
            RaiseChanged();
        }

        public void SetRessource(RessourceType type)
        {
            RessourceActive      = type;
            DgvRessourceRowIndex = -1;
            RaiseChanged();
        }

        /// <summary>
        /// US-08 : Définit le filtre alertes-seulement pour la navigation vers l'écran Ingrédients.
        /// Pas de RaiseChanged() ici — le filtre est lu au moment de la navigation.
        /// </summary>
        public void SetFiltreAlertes(bool alertesSeulement)
        {
            FiltreAlertesSeulement = alertesSeulement;
        }

        private void RaiseChanged() =>
            StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
