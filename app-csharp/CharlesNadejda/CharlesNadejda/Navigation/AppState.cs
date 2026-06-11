using System;
using CharlesNadejda.Models;

namespace CharlesNadejda.Navigation
{
    /// <summary>
    /// Etat global de navigation de l'application — source de verite unique.
    ///
    /// Centralise la selection courante : activite, contexte BOM, niveau, ecran,
    /// type de ressource, et index de ligne DGV pour la restauration de selection.
    ///
    /// Flux de mutation (cascade) :
    ///   SetActivite → reset contexte/niveau → StateChanged
    ///   SetContexte → reset niveau          → StateChanged
    ///   SetNiveau                            → StateChanged
    ///   SetRessource                         → StateChanged
    ///
    /// Consomme : FrmPrincipal (lecture), ScreenRouter (lecture/ecriture ActiveScreen),
    ///            tous les ecrans inline (lecture du contexte actif).
    /// </summary>
    public class AppState
    {
        // ── Selection hierarchique : Activite → Contexte → Niveau ───

        /// <summary>Activite (branche metier) selectionnee a l'onboarding. Peuplee par SetActivite().</summary>
        public Activite    ActiveActivite  { get; private set; }

        /// <summary>Contexte BOM actif (ex: "Pralines"). Peuple au clic sur un contexte dans l'ecran Production.</summary>
        public BomContexte ActiveContexte  { get; private set; }

        /// <summary>Niveau BOM actif dans le contexte courant. Peuple au clic sur un niveau dans l'arbre.</summary>
        public BomNiveau   ActiveNiveau    { get; private set; }

        // ── Navigation ──────────────────────────────────────────────

        /// <summary>Ecran actuellement affiche. Mis a jour par <see cref="ScreenRouter.Navigate"/>.</summary>
        public ScreenId      ActiveScreen    { get; internal set; } = ScreenId.Onboarding;

        /// <summary>Type de ressource actif dans l'ecran Ressources (ingredients, fournisseurs, stocks, etc.).</summary>
        public RessourceType RessourceActive { get; private set; }

        // ── Restauration de selection DGV ───────────────────────────

        /// <summary>Index de la ligne selectionnee dans le DGV des fiches BOM. -1 = aucune selection.</summary>
        public int DgvFichesRowIndex    { get; set; } = -1;

        /// <summary>Index de la ligne selectionnee dans le DGV des ressources. -1 = aucune selection.</summary>
        public int DgvRessourceRowIndex { get; set; } = -1;

        /// <summary>US-08 : filtre alertes pour navigation contextuelle depuis les StatCards du Hub.</summary>
        public bool FiltreAlertesSeulement { get; private set; }

        /// <summary>Emis apres chaque mutation d'etat — les ecrans s'y abonnent pour se rafraichir.</summary>
        public event EventHandler StateChanged;

        /// <summary>
        /// Change l'activite active. Si l'activite change, reset le contexte, le niveau et l'index DGV.
        /// </summary>
        public void SetActivite(Activite a)
        {
            bool changed = ActiveActivite?.Id != a?.Id;
            ActiveActivite = a;
            if (changed) { ActiveContexte = null; ActiveNiveau = null; DgvFichesRowIndex = -1; }
            RaiseChanged();
        }

        /// <summary>Change le contexte BOM actif. Reset le niveau et l'index DGV fiches.</summary>
        public void SetContexte(BomContexte c)
        {
            ActiveContexte = c;
            ActiveNiveau   = null;
            DgvFichesRowIndex = -1;
            RaiseChanged();
        }

        /// <summary>Change le niveau BOM actif dans le contexte courant.</summary>
        public void SetNiveau(BomNiveau n)
        {
            ActiveNiveau = n;
            RaiseChanged();
        }

        /// <summary>Change le type de ressource actif et reset l'index DGV ressources.</summary>
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
