using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;
using CharlesNadejda.Forms.Shell;
using CharlesNadejda.Navigation;

namespace CharlesNadejda.Forms
{
    // ════════════════════════════════════════════════════════════════
    //  FrmPrincipal — Shell principal, Single-Form Application (SFA)
    //
    //  Ce fichier partial contient : constructeur, layout shell
    //  (sidebar, toolbar, status bar, panneau droit), palette locale,
    //  helpers UI partages, et branchement du ScreenRouter.
    //
    //  Les ecrans metier sont dans les fichiers partial dedies :
    //    .Hub.cs         — Dashboard / tableau de bord
    //    .Production.cs  — Production BOM (3 colonnes Kanban)
    //    .BoutiqueWeb.cs — Mini CMS boutique en ligne
    //    .Parametres.cs  — Configuration utilisateur
    //
    //  Architecture SFA : au lieu d'ouvrir plusieurs fenetres,
    //  tous les ecrans s'affichent dans _pnlDroit (Panel2 du SplitContainer).
    //  La sidebar a gauche pilote la navigation via ScreenRouter.
    // ════════════════════════════════════════════════════════════════
    /// <summary>
    /// Hub principal — Single-Form Application (SFA).
    /// Ressources et production s'affichent inline dans Panel2 du SplitContainer.
    /// TICKET-22 : les CRUD BOM Edit (contextes, niveaux, achats) sont inline via ShowFormInline.
    /// </summary>
    public partial class FrmPrincipal : Form
    {
        // L'utilisateur connecté — je le garde en readonly car il ne change jamais pendant la session.
        // Si l'user veut changer de compte, c'est Application.Restart() → retour au login.
        private readonly Utilisateur  _utilisateur;

        // AppState centralise TOUT l'état de navigation : activité active, contexte actif,
        // écran courant, type de ressource, filtres... C'est le "cerveau" de l'état global.
        // Chaque écran lit l'AppState pour savoir quoi afficher.
        private readonly AppState     _state = new AppState();

        // Le router est le cerveau de la navigation — il décide quel écran afficher
        // en fonction du ScreenId demandé. Il a aussi un guard anti-doublon :
        // si je suis déjà sur Production et que je reclique Production, il ignore.
        private          ScreenRouter _router;
        // _niveauxListe supprimé — chargement local dans Production

        // ── Palette — alias locaux vers AppColors (source de vérité unique) ──
        // J'utilise des alias static readonly pour éviter de taper AppColors.ChocoBrand partout.
        // La source de vérité reste AppColors — si je change une couleur là-bas, ça se propage ici.
        private static readonly Color CHOCO_BRAND  = AppColors.ChocoBrand;
        private static readonly Color CHOCO_MED    = AppColors.ChocoMed;
        private static readonly Color CHOCO_ABYSS  = AppColors.ChocoAbyss;
        private static readonly Color CHOCO_DARK   = AppColors.ChocoDark;
        private static readonly Color OR           = AppColors.Or;
        private static readonly Color CREME        = AppColors.Creme;
        private static readonly Color CREME_WARM   = AppColors.CremeWarm;
        private static readonly Color CREME_BG     = AppColors.CremeBg;
        private static readonly Color SIDEBAR_TXT  = AppColors.SidebarTxt;
        private static readonly Color SIDEBAR_META = AppColors.SidebarMeta;
        private static readonly Color BORDER_CLR   = AppColors.Border;
        private static readonly Color GREEN_OK     = AppColors.GreenOk;
        private static readonly Color RED_CRIT     = AppColors.RedCrit;
        private static readonly Color ORG_WARN     = AppColors.OrgWarn;

        // ── Shell ERP ─────────────────────────────────────────────────────
        // Les 3 composants du "shell" — le cadre fixe qui ne change jamais :
        // - TitleBar : bandeau du haut avec nom utilisateur + titre écran
        // - Sidebar : rail de navigation à gauche (activités, contextes, menu)
        // - StatusBar : barre d'état en bas (activité courante, infos rapides)
        private TitleBarPanel            _titleBar;
        private SidebarPanel             _sidebar;
        private AppStatusBar     _statusBar;

        // ── Panneau droit ─────────────────────────────────────────────────
        // C'est ICI que tout le contenu s'affiche — chaque écran (Hub, Production,
        // Ressources, etc.) est injecté dans ce Panel via ClearAndDisposePanel() + Controls.Add().
        // C'est le "viewport" de l'app.
        private Panel _pnlDroit;

        // ── Contexte screen — supprimé (fusionné dans Production) ────────

        // ════════════════════════════════════════════════════════════════
        //  Constructeur / Load
        // ════════════════════════════════════════════════════════════════

        // Le constructeur reçoit l'utilisateur authentifié depuis FrmLogin.
        // Je crée le router ici (pas dans Load) parce que j'en ai besoin
        // dès que le shell est construit — avant même que les données soient chargées.
        public FrmPrincipal(Utilisateur utilisateur)
        {
            _utilisateur = utilisateur;
            _router      = new ScreenRouter(_state);
            InitializeComponent();
        }

        // FrmPrincipal_Load — c'est le vrai point de départ de l'app après le login.
        // L'ordre est critique : 1) construire la coquille visuelle, 2) câbler le router,
        // 3) charger les données (activités → contextes → navigation initiale).
        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            BuildShell();
            InitRouter();
            ChargerActivites();
        }

        // ════════════════════════════════════════════════════════════════
        //  Router — câblage des écrans inline
        // ════════════════════════════════════════════════════════════════

        // InitRouter câble chaque ScreenId à sa méthode d'affichage.
        // C'est une table de dispatch : quand le router reçoit "Production",
        // il appelle ShowProductionScreen. Quand il reçoit "Hub", ShowHubScreen, etc.
        // Le paramètre NavigationParams (p) est passé systématiquement même si pas toujours utilisé.
        private void InitRouter()
        {
            _router.OnOnboarding      = p => ShowOnboarding();
            _router.OnHub             = p => ShowHubScreen();
            _router.OnRessources      = p => ShowRessourceScreen(_state.RessourceActive, p);
            _router.OnProduction      = p => ShowProductionScreen(p);
            _router.OnPlaceholder     = p => ShowPlaceholder(null);
            _router.OnBoutiqueWeb     = p => ShowBoutiqueWebScreen();
            _router.OnParametres      = p => ShowParametresScreen();
        }

        /// <summary>
        /// Point d'entrée unique pour toute navigation.
        /// <para>• <paramref name="stateSetup"/> : action optionnelle exécutée avant la navigation
        ///   (ex. SetRessource, SetContexte) — permet d'éviter le pattern { setup(); router.Navigate(); }.</para>
        /// <para>• <paramref name="forceRefresh"/> : invalide le guard singleton pour forcer une
        ///   reconstruction (à utiliser après toute opération CRUD).</para>
        /// Le guard de re-navigation est centralisé dans <see cref="ScreenRouter"/> — si on est déjà
        /// sur le même écran avec le même état, l'appel est ignoré.
        /// </summary>
        // NavigateTo est ma façade de navigation — TOUT passe par là.
        // Le pattern stateSetup est malin : au lieu de faire 2 appels séparés
        // (_state.SetRessource(X); _router.Navigate(Y)), je fais tout en un seul appel.
        // forceRefresh sert après un CRUD (ajout/modif/suppression) pour reconstruire l'écran.
        private void NavigateTo(ScreenId screen, Action stateSetup = null, bool forceRefresh = false)
        {
            // Si un stateSetup est fourni, je l'exécute AVANT la navigation
            // pour que le state soit à jour quand l'écran se construit
            stateSetup?.Invoke();

            // Invalider le guard = forcer la reconstruction même si on est "déjà" sur cet écran
            if (forceRefresh) _router.Invalidate();

            _router.Navigate(screen);
        }

        // ════════════════════════════════════════════════════════════════
        //  Construction du shell ERP
        // ════════════════════════════════════════════════════════════════

        // BuildShell monte la structure fixe de l'interface — le cadre qui ne change jamais.
        // L'ordre d'ajout dans Controls est crucial en WinForms avec DockStyle :
        // WinForms dock en LIFO → Fill doit être ajouté EN PREMIER, puis Bottom, Left, Top.
        // Si je me trompe dans l'ordre, la sidebar peut se retrouver SOUS la titlebar, etc.
        private void BuildShell()
        {
            _titleBar = new TitleBarPanel(_utilisateur);
            _statusBar = new AppStatusBar();

            // La sidebar émet des événements que je câble ici —
            // chaque événement correspond à une action utilisateur dans le rail gauche
            _sidebar = new SidebarPanel();
            _sidebar.NavigationRequested      += OnSidebarNavigation;   // Clic sur un item de menu
            _sidebar.ActivityChanged          += OnActivityChanged;     // Changement d'activité dans le dropdown
            _sidebar.ManageActivitiesRequested += OnManageActivities;   // Bouton "Gérer les activités"
            _sidebar.NewContextRequested       += OnNewContext;         // Bouton "+" pour nouveau contexte
            _sidebar.ContextChanged            += OnContextChanged;     // Sélection d'un autre contexte
            _sidebar.EditContextRequested      += OnEditContext;        // Clic droit → Modifier un contexte
            _sidebar.DeleteContextRequested    += OnDeleteContext;      // Clic droit → Supprimer un contexte

            // Le panneau droit = le viewport principal. Dock Fill = il prend tout l'espace restant.
            // AutoScroll = true pour que le contenu scrolle si trop grand.
            _pnlDroit = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = CREME_WARM, Padding = new Padding(0)
            };

            // WinForms DockStyle order: Fill first, then Bottom, Left, Top
            // IMPORTANT : cet ordre est le contraire de l'intuition — Fill en premier !
            Controls.Add(_pnlDroit);    // Fill   → prend tout l'espace restant
            Controls.Add(_sidebar);     // Left   → rail gauche
            Controls.Add(_statusBar);   // Bottom → barre d'état en bas
            Controls.Add(_titleBar);    // Top    → bandeau titre en haut
        }

        // OnSidebarNavigation — quand l'utilisateur clique sur un item dans la sidebar.
        // C'est le gros switch de navigation : chaque NavItemId mappe vers un ScreenId.
        // Pour les ressources, je prépare aussi le type de ressource dans le state (SetRessource).
        private void OnSidebarNavigation(NavItemId id)
        {
            // D'abord, je mets à jour visuellement quel item est "actif" dans la sidebar
            _sidebar.SetActiveItem(id);

            switch (id)
            {
                case NavItemId.Hub:
                    NavigateTo(ScreenId.Hub);
                    break;
                case NavItemId.Production:
                    NavigateTo(ScreenId.Production);
                    break;

                // Chaque type de ressource navigue vers le même écran (Ressources)
                // mais avec un RessourceType différent → l'écran s'adapte
                case NavItemId.StocksLiaisons:
                    NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Stocks));
                    break;
                case NavItemId.VueStockGlobal:
                    NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.VueStock));
                    break;
                case NavItemId.AchatsLots:
                    NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Achats));
                    break;
                case NavItemId.Fournisseurs:
                    NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Fournisseurs));
                    break;
                case NavItemId.Ingredients:
                    NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Ingredients));
                    break;

                // Modules à venir — redirigent vers un placeholder pour l'instant
                case NavItemId.Planning:
                    NavigateTo(ScreenId.Planning);
                    break;
                case NavItemId.DevisPatisserie:
                    NavigateTo(ScreenId.DevisPatisserie);
                    break;
                case NavItemId.Mouvements:
                    NavigateTo(ScreenId.Mouvements);
                    break;
                case NavItemId.Parametres:
                    NavigateTo(ScreenId.Parametres);
                    break;
                case NavItemId.BoutiqueWeb:
                    NavigateTo(ScreenId.BoutiqueWeb);
                    break;
            }

            // Après chaque navigation, je mets à jour le titre et la barre d'état
            // pour refléter l'écran courant
            UpdateTitleBar();
            UpdateStatusBar();
        }

        // OnActivityChanged — déclenché quand l'utilisateur change d'activité dans le dropdown de la sidebar.
        // Une activité c'est le métier principal (ex: "Pâtisserie", "Boulangerie").
        // Changer d'activité = recharger tous les contextes liés + rediriger vers le bon écran.
        private void OnActivityChanged(Activite act)
        {
            // Guard : si null ou même activité déjà sélectionnée, je ne fais rien
            if (act == null || act.Id == _state.ActiveActivite?.Id) return;

            _state.SetActivite(act);
            ChargerContextes();

            // Si l'activité a au moins un contexte, je vais en Production. Sinon, retour au Hub.
            var cible = _state.ActiveContexte != null ? ScreenId.Production : ScreenId.Hub;
            NavigateTo(cible, forceRefresh: true);
            UpdateStatusBar();
        }

        // OnManageActivities — ouvre le formulaire de gestion des activités en mode modal (dialog).
        // Après fermeture, je recharge la liste car l'user a pu ajouter/modifier/supprimer.
        private void OnManageActivities()
        {
            using (var frm = new FrmActivites()) frm.ShowDialog(this);
            ChargerActivites();
        }

        // OnNewContext — quand l'utilisateur clique "+" pour créer un nouveau contexte de production.
        // Je délègue à BtnNouveauContexte_Click qui contient la logique complète.
        private void OnNewContext()
        {
            BtnNouveauContexte_Click(this, EventArgs.Empty);
        }

        // OnContextChanged — quand l'utilisateur sélectionne un autre contexte dans la sidebar.
        // Un contexte de production = une "ligne" de production (ex: "Gâteaux au chocolat Q4 2025").
        // Changer de contexte = reconstruire l'écran Production avec les fiches BOM de ce contexte.
        private void OnContextChanged(BomContexte ctx)
        {
            if (ctx == null || ctx.Id == _state.ActiveContexte?.Id) return;
            _state.SetContexte(ctx);
            _router.Invalidate();
            NavigateTo(ScreenId.Production, forceRefresh: true);
            UpdateTitleBar();
            UpdateStatusBar();
        }

        // OnEditContext — clic droit → "Modifier" sur un contexte dans la sidebar.
        // Ouvre FrmBomContexteEdit en mode édition (ctx existant passé au constructeur).
        // Si l'utilisateur valide (OK), je recharge les contextes et je reconstruis l'écran.
        private void OnEditContext(BomContexte ctx)
        {
            if (ctx == null) return;
            using (var frm = new FrmBomContexteEdit(ctx, _state.ActiveActivite))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    ChargerContextes();
                    _router.Invalidate();
                    NavigateTo(ScreenId.Production, forceRefresh: true);
                    UpdateTitleBar();
                }
            }
        }

        // OnDeleteContext — clic droit → "Supprimer" sur un contexte.
        // Confirmation obligatoire (MessageBox avec focus par défaut sur "Non" — sécurité UX).
        // Si le contexte supprimé était le contexte actif, je le reset à null et je redirige.
        private void OnDeleteContext(BomContexte ctx)
        {
            if (ctx == null) return;

            // Confirmation avec Button2 (Non) par défaut — on ne supprime pas par accident
            if (MessageBox.Show($"Supprimer « {ctx.Nom} » et toutes ses données ?",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            try
            {
                BomContexteDAL.Delete(ctx.Id);

                // Si l'activité a été supprimée, je bascule sur la première disponible
                if (_state.ActiveContexte?.Id == ctx.Id)
                    _state.SetContexte(null);

                ChargerContextes();

                // Après suppression : si un contexte reste, aller en Production. Sinon → Hub.
                var cible = _state.ActiveContexte != null ? ScreenId.Production : ScreenId.Hub;
                _router.Invalidate();
                NavigateTo(cible, forceRefresh: true);
                UpdateTitleBar();
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // UpdateTitleBar met à jour le titre affiché dans le bandeau du haut.
        // Chaque ScreenId a son libellé — si aucun match, fallback sur "ArtisaStock".
        private void UpdateTitleBar()
        {
            if (_titleBar == null) return;

            // Dictionnaire ScreenId → titre lisible — c'est plus propre qu'un switch
            var titles = new Dictionary<ScreenId, string>
            {
                { ScreenId.Onboarding,      "Bienvenue" },
                { ScreenId.Hub,             "Hub atelier" },
                { ScreenId.Ressources,      _state.RessourceActive.ToString() },
                { ScreenId.Production,      "Production" },
                { ScreenId.Planning,        "Planning" },
                { ScreenId.DevisPatisserie, "Devis pâtisserie" },
                { ScreenId.Mouvements,      "Mouvements" },
                { ScreenId.BoutiqueWeb,     "Boutique Web" },
                { ScreenId.Parametres,      "Paramètres" },
            };
            string title;
            titles.TryGetValue(_state.ActiveScreen, out title);
            _titleBar.SetTitle(title ?? "ArtisaStock");
        }

        // UpdateStatusBar propage l'état courant dans la barre d'état en bas.
        // Le ?. est important — au démarrage, la statusBar peut ne pas encore exister.
        private void UpdateStatusBar()
        {
            _statusBar?.UpdateState(_state);
        }

        // ════════════════════════════════════════════════════════════════
        //  Chargement des données
        // ════════════════════════════════════════════════════════════════

        // ChargerActivites — charge toutes les activités depuis la DB et met à jour la sidebar.
        // C'est le premier chargement de données au démarrage, et aussi appelé après chaque
        // modification d'activité (ajout, suppression via FrmActivites).
        // Si aucune activité n'existe, on affiche l'écran d'onboarding.
        private void ChargerActivites()
        {
            var acts = ActiviteDAL.GetAll();

            // Pas d'activité du tout → première utilisation, on montre l'onboarding
            if (acts.Count == 0) { _state.SetActivite(null); ShowOnboarding(); return; }

            // Je nourris la sidebar avec la liste complète des activités
            _sidebar.SetActivities(acts);

            // Si une activité était déjà sélectionnée, je vérifie qu'elle existe encore
            if (_state.ActiveActivite != null)
            {
                var match = acts.Find(a => a.Id == _state.ActiveActivite.Id);
                if (match != null)
                    _sidebar.SetSelectedActivity(match);
                else
                {
                    // L'activité a été supprimée entre-temps — fallback sur la première disponible
                    _state.SetActivite(acts[0]);
                    _sidebar.SetSelectedActivity(acts[0]);
                }
            }

            // Si aucune activité n'était encore sélectionnée (premier lancement), prendre la première
            if (_state.ActiveActivite == null)
            {
                _state.SetActivite(acts[0]);
                _sidebar.SetSelectedActivity(acts[0]);
            }

            // Charger les contextes de l'activité sélectionnée, puis naviguer vers le bon écran
            ChargerContextes();
            var cible = _state.ActiveContexte != null ? ScreenId.Production : ScreenId.Hub;
            NavigateTo(cible);
            UpdateStatusBar();
        }

        // ChargerContextes — charge les contextes de production liés à l'activité courante.
        // Un contexte = une "campagne" de production (ex: "Collection Noël 2025").
        // Si l'activité est nulle, je vide tout. Sinon je vérifie que le contexte actif
        // existe encore (il a pu être supprimé), et je sélectionne le premier par défaut.
        private void ChargerContextes()
        {
            // Pas d'activité → pas de contexte possible
            if (_state.ActiveActivite == null)
            {
                _state.SetContexte(null);
                _sidebar.SetContextes(null);
                return;
            }

            var contextes = BomContexteDAL.GetAll(_state.ActiveActivite.Id);

            // Logique de sélection du contexte actif :
            // - Si j'ai des contextes et rien de sélectionné → prendre le premier
            // - Si j'ai des contextes et un sélectionné → vérifier qu'il existe encore
            // - Si plus de contexte → null
            if (contextes.Count > 0 && _state.ActiveContexte == null)
                _state.SetContexte(contextes[0]);
            else if (contextes.Count > 0 && _state.ActiveContexte != null)
            {
                // Le contexte actif a peut-être été supprimé → fallback sur le premier
                if (!contextes.Any(c => c.Id == _state.ActiveContexte.Id))
                    _state.SetContexte(contextes[0]);
            }
            else
                _state.SetContexte(null);

            // Mettre à jour la sidebar avec les contextes disponibles + sélection courante
            _sidebar.SetContextes(contextes);
            if (_state.ActiveContexte != null)
                _sidebar.SetSelectedContext(_state.ActiveContexte);
        }

        // ChargerNiveaux supprimé — chargement local dans Production.cs

        // ════════════════════════════════════════════════════════════════
        //  Écrans du panneau droit
        // ════════════════════════════════════════════════════════════════

        // ShowOnboarding — écran de bienvenue pour les nouveaux utilisateurs.
        // S'affiche quand il n'y a AUCUNE activité dans la base.
        // Guide l'utilisateur pas à pas : créer un stock → créer une activité → lier → contexte.
        private void ShowOnboarding()
        {
            // SuspendLayout/ResumeLayout = j'empêche le redraw pendant que je construis l'UI
            // Sinon l'utilisateur verrait un clignotement moche à chaque ajout de contrôle
            _pnlDroit.SuspendLayout();

            // ClearAndDisposePanel() vide le panneau ET libère la mémoire des contrôles
            ClearAndDisposePanel();

            // Panneau central fixe — pas de Dock, position absolue pour un look "carte" centrée
            var pnlCenter = new Panel
            {
                Width = 480, Height = 300, Location = new Point(60, 60),
                BackColor = CREME, Margin = new Padding(0), Anchor = AnchorStyles.None
            };

            // Bordure dessinée manuellement via Paint — plus fin qu'un BorderStyle
            pnlCenter.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, pnlCenter.Width - 1, pnlCenter.Height - 1);
            };

            // Titre de bienvenue
            pnlCenter.Controls.Add(new Label
            {
                Text = "Bienvenue dans ArtisaStock",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, Location = new Point(28, 24),
                Size = new Size(420, 32), AutoSize = false
            });

            // Instructions pas à pas — le workflow de premier lancement
            pnlCenter.Controls.Add(new Label
            {
                Text = "Pour démarrer :\r\n\r\n" +
                       "  1.  \U0001f4e6  Créez un stock (lieu physique de stockage)\r\n" +
                       "  2.  \U0001f3af  Créez une activité (ce que vous produisez)\r\n" +
                       "  3.  \U0001f517  Liez vos stocks à l'activité\r\n" +
                       "  4.  \U0001f3ed  Créez un contexte de production",
                Font = new Font("Segoe UI", 9.5F), ForeColor = CHOCO_MED,
                Location = new Point(28, 64), Size = new Size(420, 130)
            });

            // US-10 : lien "créer un stock d'abord" — étape 1 du workflow
            // C'est un raccourci qui navigue directement vers l'écran Stocks
            var lnkStock = new LinkLabel
            {
                Text      = "\u2192 Créer un stock d'abord",
                Font      = new Font("Segoe UI", 9.5F, FontStyle.Underline),
                ForeColor = OR,
                Cursor    = Cursors.Hand,
                Location  = new Point(28, 196),
                AutoSize  = true
            };
            lnkStock.LinkClicked += (s, ev) =>
                NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Stocks));
            pnlCenter.Controls.Add(lnkStock);

            // CTA principal (Call To Action) — le gros bouton doré pour créer sa première activité
            var btnCreer = new Button
            {
                Text = "⚡  Créer ma première activité",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = OR, ForeColor = CHOCO_BRAND, FlatStyle = FlatStyle.Flat,
                Location = new Point(28, 228), Size = new Size(264, 40), Cursor = Cursors.Hand
            };
            btnCreer.FlatAppearance.BorderColor = Color.FromArgb(168, 137, 30);
            // Au clic, ouvrir le formulaire de création d'activité en modal
            // Si OK → recharger les activités (ce qui déclenchera la navigation vers le hub)
            btnCreer.Click += (s, ev) => { using (var frm = new FrmActiviteEdit()) { if (frm.ShowDialog(this) == DialogResult.OK) ChargerActivites(); } };
            pnlCenter.Controls.Add(btnCreer);

            _pnlDroit.Controls.Add(pnlCenter);
            _pnlDroit.ResumeLayout();
        }

        // ── Actions contextes (absorbées de Contexte.cs) ─────────────────

        // BtnNouveauContexte_Click — crée un nouveau contexte de production.
        // Vérifie d'abord qu'une activité est sélectionnée (sinon ça n'a pas de sens),
        // puis ouvre FrmBomContexteEdit en mode création (null = pas de contexte existant).
        private void BtnNouveauContexte_Click(object sender, EventArgs e)
        {
            if (_state.ActiveActivite == null)
            { MessageBox.Show("Sélectionnez une activité.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using (var frm = new FrmBomContexteEdit(null, _state.ActiveActivite))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    ChargerContextes();
                    UpdateStatusBar();
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Écrans ressources & production — SFA (embed inline)
        // ════════════════════════════════════════════════════════════════

        // ShowRessourceScreen — instancie le bon formulaire de ressource selon le type
        // et l'intègre dans le panneau droit via EmbedForm.
        // Chaque type de ressource a son propre Form dédié (Fournisseurs, Stocks, etc.)
        private void ShowRessourceScreen(RessourceType type, NavigationParams p)
        {
            // US-08 : lire et réinitialiser le filtre alertes — évite la persistence entre navigations.
            // Si l'utilisateur a cliqué "alertes" dans le Hub, le flag est true → je le lis et le reset.
            // Comme ça le filtre ne reste pas actif si l'user revient plus tard par la sidebar.
            bool filtreAlertes = _state.FiltreAlertesSeulement;
            _state.SetFiltreAlertes(false);

            // Chaque RessourceType → un Form spécifique, avec ses paramètres propres
            Form frm;
            switch (type)
            {
                case RessourceType.Fournisseurs: frm = new FrmFournisseurs();                     break;
                case RessourceType.Stocks:       frm = new FrmStocks();                           break;
                case RessourceType.Ingredients:  frm = new FrmIngredients(_state.ActiveActivite, filtreAlertes); break;
                case RessourceType.Achats:       frm = new FrmAchats(_state.ActiveActivite);       break;
                case RessourceType.VueStock:     frm = new FrmVueStock();                         break;
                default:                         return;
            }

            // Intégrer le formulaire dans le panneau droit (SFA pattern)
            EmbedForm(frm);
        }

        // ShowProductionScreen → déplacé dans FrmPrincipal.Production.cs (partial class)

        /// <summary>
        /// Intègre un formulaire dans le panneau droit sans TopLevel (SFA).
        /// FormClosed déclenche le retour automatique à l'écran précédent.
        /// </summary>
        // EmbedForm — le coeur du pattern SFA. Je prends un Form normal et je le transforme
        // en contrôle intégré : TopLevel=false pour qu'il ne soit plus une fenêtre indépendante,
        // FormBorderStyle=None pour virer la barre de titre, Dock=Fill pour remplir le panneau.
        // À la fermeture du Form embarqué, je reviens automatiquement à l'écran logique précédent.
        private void EmbedForm(Form frm)
        {
            _pnlDroit.SuspendLayout();
            ClearAndDisposePanel();

            // Ces 3 lignes transforment un Form classique en "contrôle embarqué"
            frm.TopLevel        = false;       // Plus une fenêtre indépendante
            frm.FormBorderStyle = FormBorderStyle.None;  // Pas de barre de titre
            frm.Dock            = DockStyle.Fill;        // Remplir tout le panneau

            // Quand le Form embarqué se ferme, je reviens automatiquement au bon écran
            // La priorité : Production > Hub > Onboarding (selon ce qui existe dans le state)
            frm.FormClosed     += (s, ev) =>
            {
                if (IsDisposed) return;
                if (_state.ActiveContexte != null)      NavigateTo(ScreenId.Production, forceRefresh: true);
                else if (_state.ActiveActivite != null) NavigateTo(ScreenId.Hub,             forceRefresh: true);
                else                                    NavigateTo(ScreenId.Onboarding,      forceRefresh: true);
            };
            _pnlDroit.Controls.Add(frm);
            frm.Show();
            _pnlDroit.ResumeLayout();
        }

        // ════════════════════════════════════════════════════════════════
        //  TICKET-22 — SFA : chargement inline des formulaires d'édition
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Charge un formulaire d'édition en mode inline (SFA) dans _pnlDroit,
        /// avec un bandeau "Retour" en haut. Le formulaire est disposé automatiquement
        /// à la fermeture. <paramref name="onClosed"/> est appelé avec le DialogResult
        /// pour que l'appelant puisse rafraîchir les données si OK.
        /// </summary>
        // ShowFormInline — variante de EmbedForm pour les formulaires d'édition (CRUD).
        // La différence : j'ajoute un bandeau "← Retour" en haut pour que l'utilisateur
        // puisse annuler et revenir. C'est le pattern "page d'édition" dans un SFA.
        // Le callback onClosed permet à l'appelant de savoir si l'user a validé (OK) ou annulé.
        private void ShowFormInline(Form frm, Action<DialogResult> onClosed)
        {
            frm.TopLevel        = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock            = DockStyle.Fill;

            _pnlDroit.SuspendLayout();
            ClearAndDisposePanel();

            // ── Bandeau Retour — un petit panel docké en haut avec bouton + titre ──
            var pnlRetour = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = CREME_WARM };

            // Ligne de séparation en bas du bandeau — dessinée via Paint
            pnlRetour.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, pnlRetour.Height - 1, pnlRetour.Width, pnlRetour.Height - 1);
            };

            // Bouton "← Retour" — ferme le formulaire avec Cancel si rien n'a été validé
            var btnRetour = new Button
            {
                Text = "←  Retour", Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat, BackColor = AppColors.GreyBtn, ForeColor = CHOCO_BRAND,
                Size = new Size(100, 30), Location = new Point(12, 5), Cursor = Cursors.Hand
            };
            btnRetour.FlatAppearance.BorderColor = BORDER_CLR;
            btnRetour.Click += (s, ev) =>
            {
                // Si l'utilisateur n'a pas validé (DialogResult.None), c'est un Cancel
                if (frm.DialogResult == DialogResult.None)
                    frm.DialogResult = DialogResult.Cancel;
                frm.Close();
            };
            pnlRetour.Controls.Add(btnRetour);

            // Titre du formulaire affiché dans le bandeau (ex: "Modifier le contexte")
            pnlRetour.Controls.Add(new Label
            {
                Text = frm.Text, Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, Location = new Point(120, 10), AutoSize = true
            });

            // À la fermeture du Form : restaurer la vue précédente + notifier l'appelant
            frm.FormClosed += (s, ev) =>
            {
                var result = frm.DialogResult;
                // Revenir à l'écran qui était actif avant l'édition
                NavigateTo(_state.ActiveScreen, forceRefresh: true);
                onClosed?.Invoke(result);
            };

            // Ordre d'ajout : Fill en premier (le form), puis Top (le bandeau retour)
            _pnlDroit.Controls.Add(frm);
            _pnlDroit.Controls.Add(pnlRetour);
            frm.Show();
            _pnlDroit.ResumeLayout();
        }

        /// <summary>
        /// Vide _pnlDroit ET dispose les contrôles enfants (Forms embarquées, DGVs, Panels).
        /// Sans Dispose, les Forms précédentes resteraient en mémoire avec leurs handlers DAL
        /// et seraient disposées en cascade à la fermeture de l'app (latence visible).
        /// </summary>
        // ClearAndDisposePanel — nettoyage obligatoire avant d'afficher un nouvel écran.
        // Controls.Clear() seul ne suffit PAS en WinForms : ça retire les contrôles du parent
        // mais ne les dispose pas → fuite mémoire. Ici je retire ET dispose chaque contrôle.
        // C'est critique parce que les Forms embarquées ont des connexions DAL ouvertes,
        // des événements câblés, des DGV avec des DataSources... tout ça doit être libéré.
        private void ClearAndDisposePanel()
        {
            DisposeAndClear(_pnlDroit.Controls);
        }

        /// <summary>
        /// Retire et dispose chaque contrôle d'une ControlCollection.
        /// Boucle while (pas foreach) car Dispose modifie la collection pendant l'itération.
        /// </summary>
        internal static void DisposeAndClear(Control.ControlCollection controls)
        {
            while (controls.Count > 0)
            {
                var c = controls[0];
                c.ContextMenuStrip?.Dispose();
                controls.RemoveAt(0);
                c.Dispose();
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Placeholder — module en développement
        // ════════════════════════════════════════════════════════════════

        // ShowPlaceholder — écran générique "module pas encore codé".
        // Affiché pour Planning, Devis, Mouvements, etc. qui ne sont pas encore implémentés.
        // Donne un feedback clair à l'utilisateur au lieu d'un crash ou d'un écran vide.
        private void ShowPlaceholder(string moduleName)
        {
            _pnlDroit.SuspendLayout();
            ClearAndDisposePanel();

            var pnlCenter = new Panel
            {
                Width = 400, Height = 200, Location = new Point(80, 80),
                BackColor = Color.White
            };
            pnlCenter.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, pnlCenter.Width - 1, pnlCenter.Height - 1);
            };
            pnlCenter.Controls.Add(new Label
            {
                Text = "\U0001f6a7  Module en développement",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, Location = new Point(28, 28),
                AutoSize = true
            });
            pnlCenter.Controls.Add(new Label
            {
                Text = "Ce module sera disponible dans une prochaine version.\nRevenez au Hub pour continuer.",
                Font = new Font("Segoe UI", 10F), ForeColor = CHOCO_MED,
                Location = new Point(28, 70), Size = new Size(340, 60)
            });

            // Bouton retour au Hub — pour ne pas laisser l'utilisateur coincé
            var btnRetour = new Button
            {
                Text = "\u2190 Retour au Hub", Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = OR, ForeColor = CHOCO_BRAND, FlatStyle = FlatStyle.Flat,
                Location = new Point(28, 140), Size = new Size(180, 36), Cursor = Cursors.Hand
            };
            btnRetour.FlatAppearance.BorderColor = Color.FromArgb(168, 137, 30);
            btnRetour.Click += (s, ev) => { _sidebar.SetActiveItem(NavItemId.Hub); NavigateTo(ScreenId.Hub, forceRefresh: true); UpdateTitleBar(); };
            pnlCenter.Controls.Add(btnRetour);

            _pnlDroit.Controls.Add(pnlCenter);
            _pnlDroit.ResumeLayout();
        }

        // OnFormClosed — quand le formulaire principal se ferme, toute l'app s'arrête.
        // Application.Exit() ferme proprement tous les threads et libère les ressources.
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }

        // Handler Resize vide — câblé dans le Designer mais pas utilisé pour l'instant.
        // Je le garde pour éviter un crash si le Designer essaie de l'appeler.
        private void FrmPrincipal_Resize(object sender, EventArgs e) { }
    }
}
