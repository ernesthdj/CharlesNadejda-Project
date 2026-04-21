using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;
using CharlesNadejda.Navigation;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Hub principal — Single-Form Application (SFA).
    /// Tout s'affiche dans Panel2 du SplitContainer ; aucun ShowDialog() de ressource/production.
    /// </summary>
    public partial class FrmPrincipal : Form
    {
        private readonly Utilisateur  _utilisateur;
        private readonly AppState     _state = new AppState();
        private          ScreenRouter _router;
        private List<BomNiveau>       _niveauxListe = new List<BomNiveau>();

        // ── Palette ──────────────────────────────────────────────────────
        private static readonly Color CHOCO_BRAND  = Color.FromArgb(61,  40,  23);
        private static readonly Color CHOCO_MED    = Color.FromArgb(111, 78,  55);
        private static readonly Color CHOCO_ABYSS  = Color.FromArgb(30,  15,   8);
        private static readonly Color CHOCO_DARK   = Color.FromArgb(44,  24,  16);
        private static readonly Color OR           = Color.FromArgb(212, 175,  55);
        private static readonly Color CREME        = Color.FromArgb(245, 230, 211);
        private static readonly Color CREME_WARM   = Color.FromArgb(253, 251, 246);
        private static readonly Color CREME_BG     = Color.FromArgb(236, 233, 216);
        private static readonly Color SIDEBAR_TXT  = Color.FromArgb(232, 217, 192);
        private static readonly Color SIDEBAR_META = Color.FromArgb(158, 123,  92);
        private static readonly Color BORDER_CLR   = Color.FromArgb(195, 185, 168);
        private static readonly Color GREEN_OK     = Color.FromArgb(92,  184,  92);
        private static readonly Color RED_CRIT     = Color.FromArgb(199,  44,  72);
        private static readonly Color ORG_WARN     = Color.FromArgb(211,  84,   0);

        // ── Sidebar controls ──────────────────────────────────────────────
        private SplitContainer _split;
        private ListBox        _lstActivites;
        private ListBox        _lstContextes;
        private ListBox        _lstNiveaux;
        private Panel          _pnlContextesFull;
        private Panel          _pnlNiveauxFull;
        private Panel          _pnlHdrContextes;
        private Panel          _pnlHdrNiveaux;
        private Panel          _pnlRes;

        // ── Panneau droit ─────────────────────────────────────────────────
        private Panel _pnlDroit;

        // ── Contexte screen ───────────────────────────────────────────────
        private Dictionary<int, Panel> _niveauPanels = new Dictionary<int, Panel>();
        private DataGridView           _dgvFiches;
        private DataGridView           _dgvStock;
        private Label                  _lblFichesHeader;
        private Button                 _btnNouveauNiveau;
        private Button                 _btnNouvFiche;
        private Button                 _btnModFiche;
        private Button                 _btnSupFiche;
        private Button                 _btnDupFiche;

        // ════════════════════════════════════════════════════════════════
        //  Constructeur / Load
        // ════════════════════════════════════════════════════════════════

        public FrmPrincipal(Utilisateur utilisateur)
        {
            _utilisateur = utilisateur;
            _router      = new ScreenRouter(_state);
            InitializeComponent();
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            lblUtilisateur.Text = $"Connecté : {_utilisateur}";
            BuildHub();
            InitRouter();
            ChargerActivites();
        }

        // ════════════════════════════════════════════════════════════════
        //  Router — câblage des écrans inline
        // ════════════════════════════════════════════════════════════════

        private void InitRouter()
        {
            _router.OnOnboarding      = p => ShowOnboarding();
            _router.OnHub             = p => ShowHubScreen();
            _router.OnContexteNiveaux = p => ShowContexteScreen();
            _router.OnRessources      = p => ShowRessourceScreen(_state.RessourceActive, p);
            _router.OnProduction      = p => ShowProductionScreen(p);
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
        private void NavigateTo(ScreenId screen, Action stateSetup = null, bool forceRefresh = false)
        {
            stateSetup?.Invoke();
            if (forceRefresh) _router.Invalidate();
            _router.Navigate(screen);
        }

        // ════════════════════════════════════════════════════════════════
        //  Construction du hub
        // ════════════════════════════════════════════════════════════════

        private void BuildHub()
        {
            // ── Barre titre gradient ──────────────────────────────────────
            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = CHOCO_BRAND };
            pnlTitle.Paint += (s, ev) =>
            {
                using (var br = new LinearGradientBrush(pnlTitle.ClientRectangle,
                    CHOCO_BRAND, CHOCO_MED, LinearGradientMode.Horizontal))
                    ev.Graphics.FillRectangle(br, pnlTitle.ClientRectangle);
            };

            var lblBrand = new Label
            {
                Text = "🍫  Charles & Nadejda  ·  ArtisaStock",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = OR, AutoSize = true, Location = new Point(12, 7)
            };

            var btnGererAct = new Button
            {
                Text = "⚙ Activités", Font = new Font("Segoe UI", 8.5F),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 255, 255, 255),
                ForeColor = CREME, Size = new Size(90, 22), Cursor = Cursors.Hand
            };
            btnGererAct.FlatAppearance.BorderColor = Color.FromArgb(140, OR);
            btnGererAct.FlatAppearance.BorderSize  = 1;
            btnGererAct.Click += (s, ev) => { new FrmActivites().ShowDialog(this); ChargerActivites(); };

            var btnNouvAct = new Button
            {
                Text = "+ Activité", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, BackColor = OR,
                ForeColor = CHOCO_BRAND, Size = new Size(80, 22), Cursor = Cursors.Hand
            };
            btnNouvAct.FlatAppearance.BorderColor = Color.FromArgb(168, 137, 30);
            btnNouvAct.FlatAppearance.BorderSize  = 1;
            btnNouvAct.Click += (s, ev) => { new FrmActivites().ShowDialog(this); ChargerActivites(); };

            pnlTitle.Resize += (s, ev) =>
            {
                btnGererAct.Location = new Point(pnlTitle.Width - 98,  6);
                btnNouvAct.Location  = new Point(pnlTitle.Width - 184, 6);
            };
            pnlTitle.Controls.Add(lblBrand);
            pnlTitle.Controls.Add(btnGererAct);
            pnlTitle.Controls.Add(btnNouvAct);
            this.Controls.Add(pnlTitle);
            pnlTitle.BringToFront();

            // ── SplitContainer ────────────────────────────────────────────
            _split = new SplitContainer
            {
                Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, BackColor = CHOCO_ABYSS
            };
            LayoutEventHandler firstLayout = null;
            firstLayout = (s, ev) =>
            {
                _split.Layout -= firstLayout;
                _split.IsSplitterFixed = true;
                _split.Panel1MinSize   = 220;
                _split.Panel2MinSize   = 300;
                if (_split.Width > 560)
                    _split.SplitterDistance = 260;
            };
            _split.Layout += firstLayout;
            this.Controls.Add(_split);
            _split.BringToFront();

            BuildLeftPanel();
            BuildRightPanel();
        }

        // ════════════════════════════════════════════════════════════════
        //  Rail gauche
        // ════════════════════════════════════════════════════════════════

        private void BuildLeftPanel()
        {
            _split.Panel1.BackColor = CHOCO_DARK;

            var pnlBrand = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = CHOCO_ABYSS };
            pnlBrand.Paint += (s, ev) =>
            {
                using (var pen = new Pen(CHOCO_BRAND, 1))
                    ev.Graphics.DrawLine(pen, 0, pnlBrand.Height - 1, pnlBrand.Width, pnlBrand.Height - 1);
            };
            pnlBrand.Controls.Add(new Label
            {
                Text = "ATELIER", Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, Location = new Point(14, 10), AutoSize = true
            });
            pnlBrand.Controls.Add(new Label
            {
                Text = "Charles & Nadejda", Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = CREME, Location = new Point(14, 24), AutoSize = true
            });

            // ── RESSOURCES (navigation inline — SFA) ──────────────────────
            var pnlHdrRes = MakeSectionHeader("Ressources");

            var resItems = new (string icon, string lbl, Action act)[]
            {
                ("📊", "Vue stock global",  () => NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.VueStock))),
                ("📦", "Stock/Liaisons",    () => NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Stocks))),
                ("🥣", "Fiches Ingrédients",() => NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Ingredients))),
                ("🏢", "Fournisseurs",      () => NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Fournisseurs))),
                ("🧾", "Achats / Lots",     () => NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Achats))),
            };
            _pnlRes = new Panel { Dock = DockStyle.Top, Height = resItems.Length * 34 + 4, BackColor = CHOCO_DARK };
            int ry = 4;
            foreach (var (icon, lbl, act) in resItems)
            {
                var btn = MakeSidebarButton(icon, lbl, act);
                btn.Location = new Point(0, ry);
                _pnlRes.Controls.Add(btn);
                ry += 34;
            }
            _split.Panel1.SizeChanged += (s, ev) =>
            {
                int w = _split.Panel1.ClientSize.Width;
                foreach (Control c in _pnlRes.Controls) c.Width = w;
            };

            var pnlSep1 = MakeSeparator();

            // ── ACTIVITÉS ─────────────────────────────────────────────────
            var pnlHdrAct = MakeSectionHeader("Activités");

            _lstActivites = new ListBox
            {
                Dock = DockStyle.Fill, BorderStyle = BorderStyle.None,
                BackColor = CHOCO_DARK, ForeColor = SIDEBAR_TXT,
                ItemHeight = 34, DrawMode = DrawMode.OwnerDrawFixed
            };
            _lstActivites.DrawItem             += LstActivites_DrawItem;
            _lstActivites.SelectedIndexChanged += LstActivites_SelectedIndexChanged;

            var pnlActSection = new Panel { Dock = DockStyle.Top, Height = 170, BackColor = CHOCO_DARK };
            pnlActSection.Controls.Add(_lstActivites);

            // ── CONTEXTES ─────────────────────────────────────────────────
            _pnlHdrContextes = MakeSectionHeader("Contextes");

            // US-02 : bouton "＋" dans l'en-tête Contextes (Fitts — position prévisible à droite)
            var btnNouvCtxSide = new Button
            {
                Text      = "＋", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = OR,
                Size      = new Size(26, 26), Cursor = Cursors.Hand
            };
            btnNouvCtxSide.FlatAppearance.BorderSize         = 0;
            btnNouvCtxSide.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 255, 255, 255);
            btnNouvCtxSide.Click += BtnNouveauContexte_Click;
            _pnlHdrContextes.Resize += (s, ev) => btnNouvCtxSide.Location = new Point(_pnlHdrContextes.Width - 28, 2);
            _pnlHdrContextes.Controls.Add(btnNouvCtxSide);
            btnNouvCtxSide.BringToFront();

            _lstContextes = new ListBox
            {
                Dock = DockStyle.Fill, BorderStyle = BorderStyle.None,
                BackColor = CHOCO_DARK, ForeColor = SIDEBAR_TXT,
                ItemHeight = 34, DrawMode = DrawMode.OwnerDrawFixed
            };
            _lstContextes.DrawItem             += LstContextes_DrawItem;
            _lstContextes.SelectedIndexChanged += LstContextes_SelectedIndexChanged;

            _pnlContextesFull = new Panel { Dock = DockStyle.Fill, BackColor = CHOCO_DARK, Visible = false };
            _pnlContextesFull.Controls.Add(_lstContextes);
            _pnlContextesFull.Controls.Add(_pnlHdrContextes);

            // ── NIVEAUX ───────────────────────────────────────────────────
            _pnlHdrNiveaux = MakeSectionHeader("Niveaux");

            _lstNiveaux = new ListBox
            {
                Dock = DockStyle.Fill, BorderStyle = BorderStyle.None,
                BackColor = CHOCO_DARK, ForeColor = SIDEBAR_TXT,
                ItemHeight = 34, DrawMode = DrawMode.OwnerDrawFixed
            };
            _lstNiveaux.DrawItem             += LstNiveaux_DrawItem;
            _lstNiveaux.SelectedIndexChanged += LstNiveaux_SelectedIndexChanged;

            _pnlNiveauxFull = new Panel { Dock = DockStyle.Bottom, Height = 170, BackColor = CHOCO_DARK, Visible = false };
            _pnlNiveauxFull.Controls.Add(_lstNiveaux);
            _pnlNiveauxFull.Controls.Add(_pnlHdrNiveaux);

            // ── Status connecté ───────────────────────────────────────────
            var pnlConnected = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = CHOCO_ABYSS };
            pnlConnected.Paint += (s, ev) =>
            {
                using (var pen = new Pen(CHOCO_BRAND, 1))
                    ev.Graphics.DrawLine(pen, 0, 0, pnlConnected.Width, 0);
            };
            pnlConnected.Controls.Add(new Label
            {
                Text = $"●  {_utilisateur.Prenom} {_utilisateur.Nom}",
                Font = new Font("Segoe UI", 8F), ForeColor = SIDEBAR_META,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            });

            // Ordre : Fill d'abord, Bottom avant Bottom, Top en dernier (topmost = dernier ajouté)
            _split.Panel1.Controls.Add(_pnlContextesFull);
            _split.Panel1.Controls.Add(_pnlNiveauxFull);
            _split.Panel1.Controls.Add(pnlConnected);
            _split.Panel1.Controls.Add(pnlActSection);
            _split.Panel1.Controls.Add(pnlHdrAct);
            _split.Panel1.Controls.Add(pnlSep1);
            _split.Panel1.Controls.Add(_pnlRes);
            _split.Panel1.Controls.Add(pnlHdrRes);
            _split.Panel1.Controls.Add(pnlBrand);
        }

        // ════════════════════════════════════════════════════════════════
        //  Panneau droit (conteneur)
        // ════════════════════════════════════════════════════════════════

        private void BuildRightPanel()
        {
            _pnlDroit = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = CREME_WARM, Padding = new Padding(0)
            };
            _split.Panel2.Controls.Add(_pnlDroit);
        }

        // ════════════════════════════════════════════════════════════════
        //  Chargement des données
        // ════════════════════════════════════════════════════════════════

        private void ChargerActivites()
        {
            if (_lstActivites == null) return;
            _lstActivites.Items.Clear();
            var acts = ActiviteDAL.GetAll();
            if (acts.Count == 0) { _state.SetActivite(null); ShowOnboarding(); return; }
            foreach (var a in acts) _lstActivites.Items.Add(a);
            int idx = _state.ActiveActivite != null
                ? Math.Max(0, acts.FindIndex(a => a.Id == _state.ActiveActivite.Id))
                : 0;
            _lstActivites.SelectedIndex = idx;
        }

        private void ChargerContextes()
        {
            _lstContextes.Items.Clear();
            if (_state.ActiveActivite == null) { _pnlContextesFull.Visible = false; return; }
            foreach (var c in BomContexteDAL.GetAll(_state.ActiveActivite.Id))
                _lstContextes.Items.Add(c);
            _pnlContextesFull.Visible = true;
            if (_lstContextes.Items.Count > 0 && _state.ActiveContexte != null)
            {
                for (int i = 0; i < _lstContextes.Items.Count; i++)
                    if (((BomContexte)_lstContextes.Items[i]).Id == _state.ActiveContexte.Id)
                    { _lstContextes.SelectedIndex = i; return; }
            }
            _state.SetContexte(null);
            ChargerNiveaux();
        }

        private void ChargerNiveaux()
        {
            _lstNiveaux.Items.Clear();
            if (_state.ActiveContexte == null) { _pnlNiveauxFull.Visible = false; return; }
            _niveauxListe = BomNiveauDAL.GetByContexte(_state.ActiveContexte.Id);
            foreach (var n in _niveauxListe) _lstNiveaux.Items.Add(n);
            int h = Math.Max(30, Math.Min(_niveauxListe.Count * 34, 170));
            _pnlNiveauxFull.Height  = h + 30;
            _pnlNiveauxFull.Visible = _niveauxListe.Count > 0;
        }

        // ════════════════════════════════════════════════════════════════
        //  Changements d'état (sidebar → droite)
        // ════════════════════════════════════════════════════════════════

        private void LstActivites_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(_lstActivites.SelectedItem is Activite act)) return;
            if (act.Id == _state.ActiveActivite?.Id) return; // même activité — évite ChargerContextes inutile
            _state.SetActivite(act);
            ChargerContextes();
            var cible = _state.ActiveContexte != null ? ScreenId.ContexteNiveaux : ScreenId.Hub;
            NavigateTo(cible, forceRefresh: true);
        }

        private void LstContextes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(_lstContextes.SelectedItem is BomContexte ctx)) return;
            if (ctx.Id == _state.ActiveContexte?.Id) return; // même contexte — évite ChargerNiveaux inutile
            _state.SetContexte(ctx);
            ChargerNiveaux();
            NavigateTo(ScreenId.ContexteNiveaux);
        }

        private void LstNiveaux_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(_lstNiveaux.SelectedItem is BomNiveau niv)) return;
            if (niv.Id == _state.ActiveNiveau?.Id) return; // même niveau — rien à faire
            _state.SetNiveau(niv);
            SyncNiveauSelection();
        }

        // ════════════════════════════════════════════════════════════════
        //  Écrans du panneau droit
        // ════════════════════════════════════════════════════════════════

        private void ShowOnboarding()
        {
            _pnlDroit.SuspendLayout();
            _pnlDroit.Controls.Clear();

            var pnlCenter = new Panel
            {
                Width = 480, Height = 300, Location = new Point(60, 60),
                BackColor = CREME, Margin = new Padding(0), Anchor = AnchorStyles.None
            };
            pnlCenter.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, pnlCenter.Width - 1, pnlCenter.Height - 1);
            };

            pnlCenter.Controls.Add(new Label
            {
                Text = "Bienvenue dans ArtisaStock",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, Location = new Point(28, 24),
                Size = new Size(420, 32), AutoSize = false
            });
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

            var btnCreer = new Button
            {
                Text = "⚡  Créer ma première activité",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = OR, ForeColor = CHOCO_BRAND, FlatStyle = FlatStyle.Flat,
                Location = new Point(28, 228), Size = new Size(264, 40), Cursor = Cursors.Hand
            };
            btnCreer.FlatAppearance.BorderColor = Color.FromArgb(168, 137, 30);
            btnCreer.Click += (s, ev) => { new FrmActivites().ShowDialog(this); ChargerActivites(); };
            pnlCenter.Controls.Add(btnCreer);

            _pnlDroit.Controls.Add(pnlCenter);
            _pnlDroit.ResumeLayout();
        }

        private void ShowHubScreen()
        {
            if (_state.ActiveActivite == null) { ShowOnboarding(); return; }
            _pnlDroit.SuspendLayout();
            _pnlDroit.Controls.Clear();

            List<Ingredient>    ings   = new List<Ingredient>();
            List<BomFiche>      fiches = new List<BomFiche>();
            List<BomProduction> prods  = new List<BomProduction>();
            try
            {
                ings   = IngredientDAL.GetAll(idActivite: _state.ActiveActivite.Id);
                fiches = BomFicheDAL.GetAll(idActivite: _state.ActiveActivite.Id);
                prods  = BomProductionDAL.GetRecentByActivite(_state.ActiveActivite.Id, 10);
            }
            catch (Exception ex)
            {
                // TICKET-10 : logguer au lieu d'avaler silencieusement — le hub s'affiche quand même
                Trace.TraceError("ShowHubScreen — chargement DAL : {0}", ex);
            }

            var alertes    = ings.Where(i => i.EstEnAlerte).ToList();
            int prods7j    = prods.Count(p => p.DateProduction >= DateTime.Now.AddDays(-7));
            decimal cout7j = prods.Where(p => p.DateProduction >= DateTime.Now.AddDays(-7))
                                  .Sum(p => p.CoutIngredients);

            // US-02 : vérifier si l'activité a des contextes pour afficher le message d'onboarding
            List<BomContexte> contextesDispo = new List<BomContexte>();
            try
            {
                contextesDispo = BomContexteDAL.GetAll(_state.ActiveActivite.Id);
            }
            catch (Exception ex)
            {
                // TICKET-10 : logguer — aucunContexte restera false, le hub s'affiche sans onboarding
                Trace.TraceError("ShowHubScreen — chargement contextes : {0}", ex);
            }
            bool aucunContexte = contextesDispo.Count == 0;

            var pnlHdr = new Panel
            {
                Dock = DockStyle.Top, Height = aucunContexte ? 76 : 56,
                BackColor = CREME_WARM, Padding = new Padding(20, 0, 20, 0)
            };
            pnlHdr.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, pnlHdr.Height - 1, pnlHdr.Width, pnlHdr.Height - 1);
            };
            pnlHdr.Controls.Add(new Label
            {
                Text = _state.ActiveActivite.Nom, Location = new Point(20, 8),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, AutoSize = true
            });
            pnlHdr.Controls.Add(new Label
            {
                Text = "Vue d'ensemble · Alertes, productions récentes, contextes actifs",
                Location = new Point(20, 32), Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = CHOCO_MED, AutoSize = true
            });

            // US-02 : message d'onboarding si aucun contexte de production
            if (aucunContexte)
            {
                pnlHdr.Controls.Add(new Label
                {
                    Text = "Aucun contexte de production — cliquez ＋ dans le rail gauche pour en créer un.",
                    Location = new Point(20, 54),
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                    ForeColor = CHOCO_MED, AutoSize = true
                });
            }

            var btnNouvProd = new Button
            {
                Text = "▶  Nouvelle production", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, BackColor = CHOCO_BRAND, ForeColor = Color.White,
                Size = new Size(148, 26), Cursor = Cursors.Hand
            };
            btnNouvProd.FlatAppearance.BorderColor = CHOCO_ABYSS;
            btnNouvProd.Click += (s, ev) => NavigateTo(ScreenId.Production);

            // US-09 : bouton "Rapport du jour" — visible seulement si productions > 0
            var btnRapport = new Button
            {
                Text      = "🖨 Rapport du jour",
                Font      = new Font("Segoe UI", 8.5F),
                FlatStyle = FlatStyle.Flat,
                BackColor = CHOCO_MED,
                ForeColor = Color.White,
                Size      = new Size(136, 26),
                Cursor    = Cursors.Hand,
                Visible   = prods.Count > 0
            };
            btnRapport.FlatAppearance.BorderColor = CHOCO_BRAND;
            btnRapport.FlatAppearance.BorderSize  = 1;
            btnRapport.Click += (s, ev) => GenererRapport();

            pnlHdr.Resize += (s, ev) =>
            {
                btnNouvProd.Location = new Point(pnlHdr.Width - 156, 14);
                btnRapport.Location  = new Point(pnlHdr.Width - 156 - 144, 14);
            };
            btnNouvProd.Location = new Point(900, 14);
            btnRapport.Location  = new Point(748, 14);
            pnlHdr.Controls.Add(btnNouvProd);
            pnlHdr.Controls.Add(btnRapport);

            var pnlStats = new Panel
            {
                Dock = DockStyle.Top, Height = 92,
                BackColor = CREME_WARM, Padding = new Padding(16, 12, 16, 8)
            };

            // US-08 : StatCards avec navigation contextuelle au clic
            var cardIngredients = MakeStatCard("🥣", "Ingrédients",      ings.Count.ToString(),    "",                                          "");
            var cardAlertes     = MakeStatCard("⚠",  "En alerte",        alertes.Count.ToString(), alertes.Count > 0
                                                                                                     ? string.Join(", ", alertes.Take(2).Select(i => i.Nom))
                                                                                                     : "Aucune alerte",              alertes.Count > 0 ? "danger" : "ok");
            var cardFiches      = MakeStatCard("🧪", "Fiches BOM",       fiches.Count.ToString(),  $"{_niveauxListe.Count} niveaux",             "gold");
            var cardProds       = MakeStatCard("▶",  "Productions · 7j", prods7j.ToString(),       $"Coût total {cout7j:F2} €",                  "success");

            // H3 — Ingrédients : navigation vers la liste complète
            cardIngredients.Cursor = Cursors.Hand;
            cardIngredients.Click += (s, ev) => {
                _state.SetFiltreAlertes(false);
                NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Ingredients));
            };

            // H4 — En alerte : navigation filtrée sur alertes (uniquement si alertes > 0)
            if (alertes.Count > 0)
            {
                cardAlertes.Cursor = Cursors.Hand;
                cardAlertes.Click += (s, ev) => {
                    _state.SetFiltreAlertes(true);
                    NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Ingredients));
                };
            }

            // H5 — Fiches BOM : navigation vers le premier contexte (si fiches > 0)
            if (fiches.Count > 0 && contextesDispo.Count > 0)
            {
                cardFiches.Cursor = Cursors.Hand;
                cardFiches.Click += (s, ev) => {
                    _state.SetContexte(contextesDispo[0]);
                    ChargerNiveaux();
                    NavigateTo(ScreenId.ContexteNiveaux);
                };
            }

            // H6 — Productions 7j : navigation vers l'écran production (si prods > 0)
            if (prods7j > 0)
            {
                cardProds.Cursor = Cursors.Hand;
                cardProds.Click += (s, ev) => NavigateTo(ScreenId.Production);
            }

            pnlStats.Resize += (s, ev) =>
            {
                int w = (pnlStats.ClientSize.Width - 32 - 12 * 3) / 4;
                for (int i = 0; i < pnlStats.Controls.Count; i++)
                    pnlStats.Controls[i].SetBounds(16 + i * (w + 12), 12, w, 68);
            };
            pnlStats.Controls.Add(cardIngredients);
            pnlStats.Controls.Add(cardAlertes);
            pnlStats.Controls.Add(cardFiches);
            pnlStats.Controls.Add(cardProds);

            var pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = CREME_WARM };

            var pnlAlerts = new Panel
            {
                Dock = DockStyle.Right, Width = 288,
                BackColor = CREME_WARM, Padding = new Padding(12, 8, 16, 8)
            };
            pnlAlerts.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, 0, 0, pnlAlerts.Height);
            };
            pnlAlerts.Controls.Add(new Label
            {
                Text = "ALERTES STOCK", Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, Location = new Point(12, 12), AutoSize = true
            });

            int ay = 34;
            if (alertes.Count == 0)
            {
                pnlAlerts.Controls.Add(new Label
                {
                    Text = "✓  Aucune alerte de stock",
                    Font = new Font("Segoe UI", 9F, FontStyle.Italic), ForeColor = GREEN_OK,
                    Location = new Point(12, ay), AutoSize = true
                });
            }
            else
            {
                foreach (var ing in alertes.Take(6))
                {
                    bool crit = ing.StockActuel <= 0 || (ing.SeuilAlerteStock.HasValue && ing.StockActuel <= ing.SeuilAlerteStock.Value * 0.5m);
                    var row = MakeAlertRow(
                        crit ? "crit" : "warn",
                        crit ? "⚠" : "🕑",
                        $"{ing.Nom} — {ing.StockActuel:F0} {ing.UniteMesure}",
                        ing.SeuilAlerteStock.HasValue ? $"Seuil : {ing.SeuilAlerteStock.Value:F0} {ing.UniteMesure}" : "");
                    row.Location = new Point(12, ay);
                    row.Width    = pnlAlerts.ClientSize.Width - 24;
                    pnlAlerts.Resize += (s, ev) => row.Width = pnlAlerts.ClientSize.Width - 24;
                    pnlAlerts.Controls.Add(row);
                    ay += row.Height + 6;
                }
            }

            var pnlProds = new Panel { Dock = DockStyle.Fill, BackColor = CREME_WARM, Padding = new Padding(16, 8, 12, 8) };
            pnlProds.Controls.Add(new Label
            {
                Text = "DERNIÈRES PRODUCTIONS", Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, Location = new Point(16, 12), AutoSize = true
            });

            var dgvProds = new DataGridView
            {
                Location = new Point(16, 36),
                Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Font = new Font("Segoe UI", 9F), BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, GridColor = BORDER_CLR,
                RowHeadersVisible = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false, MultiSelect = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 28
            };
            dgvProds.ColumnHeadersDefaultCellStyle.BackColor = CREME;
            dgvProds.ColumnHeadersDefaultCellStyle.ForeColor = CHOCO_BRAND;
            dgvProds.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dgvProds.DefaultCellStyle.SelectionBackColor      = CHOCO_BRAND;
            dgvProds.DefaultCellStyle.SelectionForeColor      = Color.White;
            pnlProds.Controls.Add(dgvProds);
            pnlProds.Resize += (s, ev) =>
            {
                dgvProds.Size = new Size(
                    pnlProds.ClientSize.Width - 28,
                    Math.Max(100, pnlProds.ClientSize.Height - 44));
            };

            if (prods.Count > 0)
            {
                dgvProds.DataSource = prods.Select(p => new
                {
                    Date    = p.DateProduction.ToString("dd/MM"),
                    Fiche   = p.NomFiche,
                    Niveau  = $"N{p.OrdreNiveau}",
                    Produit = $"{p.QuantiteProduite:F0}",
                    Cout    = $"{p.CoutIngredients:F2} \u20ac",
                }).ToList();
            }
            else
            {
                pnlProds.Controls.Add(new Label
                {
                    Text = "Aucune production enregistrée pour cette activité.",
                    Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                    ForeColor = CHOCO_MED, Location = new Point(16, 40), AutoSize = true
                });
            }

            pnlMain.Controls.Add(pnlProds);
            pnlMain.Controls.Add(pnlAlerts);

            _pnlDroit.Controls.Add(pnlMain);
            _pnlDroit.Controls.Add(pnlStats);
            _pnlDroit.Controls.Add(pnlHdr);

            _pnlDroit.ResumeLayout();
        }

        private void ShowContexteScreen()
        {
            if (_state.ActiveContexte == null) return;
            _niveauPanels.Clear();
            _dgvFiches        = null;
            _dgvStock         = null;
            _lblFichesHeader  = null;
            _btnNouveauNiveau = null;
            _btnNouvFiche     = null;
            _btnModFiche      = null;
            _btnSupFiche      = null;
            _btnDupFiche      = null;

            _pnlDroit.SuspendLayout();
            _pnlDroit.Controls.Clear();

            var pnlHdr = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = CREME_WARM };
            pnlHdr.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, pnlHdr.Height - 1, pnlHdr.Width, pnlHdr.Height - 1);
            };
            pnlHdr.Controls.Add(new Label
            {
                Text = _state.ActiveContexte.Nom, Location = new Point(20, 8),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = CHOCO_BRAND, AutoSize = true
            });
            pnlHdr.Controls.Add(new Label
            {
                Text = string.IsNullOrWhiteSpace(_state.ActiveContexte.Description)
                    ? $"Activité : {_state.ActiveContexte.ActiviteNom}"
                    : _state.ActiveContexte.Description,
                Location = new Point(20, 34), Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = CHOCO_MED, AutoSize = true
            });

            var btnModCtx = MakeActionButton("✎  Modifier",  CHOCO_MED, Color.White);
            var btnSupCtx = MakeActionButton("✕  Supprimer", RED_CRIT,  Color.White);
            btnModCtx.Click += BtnModifierContexte_Click;
            btnSupCtx.Click += BtnSupprimerContexte_Click;
            pnlHdr.Resize += (s, ev) =>
            {
                btnSupCtx.Location = new Point(pnlHdr.Width - 112, 16);
                btnModCtx.Location = new Point(pnlHdr.Width - 222, 16);
            };
            pnlHdr.Controls.Add(btnModCtx);
            pnlHdr.Controls.Add(btnSupCtx);

            var pnlNivLabel = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = CREME_WARM };
            pnlNivLabel.Controls.Add(new Label
            {
                Text = "NIVEAUX DU CONTEXTE", Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0)
            });

            var niveaux = BomNiveauDAL.GetByContexte(_state.ActiveContexte.Id);
            _niveauxListe = niveaux;
            int ordreMax = niveaux.Count > 0 ? niveaux.Max(n => n.Ordre) : 0;
            var pnlNivRows = new Panel { Dock = DockStyle.Top, BackColor = CREME_WARM, Padding = new Padding(16, 4, 16, 4) };
            int rowY = 4;
            foreach (var niv in niveaux.OrderByDescending(n => n.Ordre))
            {
                int ficheCount = 0;
                try { ficheCount = BomFicheDAL.GetByNiveau(niv.Id).Count; } catch { }
                var row = MakeNiveauRow(niv, ficheCount, ordreMax);
                row.Location = new Point(16, rowY);
                pnlNivRows.Controls.Add(row);
                rowY += row.Height + 4;
            }
            pnlNivRows.Height = rowY + 4;

            _btnNouveauNiveau = new Button
            {
                Text = "＋  Nouveau niveau", Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, BackColor = CHOCO_BRAND, ForeColor = Color.White,
                Size = new Size(156, 28), Location = new Point(16, rowY), Cursor = Cursors.Hand
            };
            _btnNouveauNiveau.FlatAppearance.BorderColor = CHOCO_ABYSS;
            _btnNouveauNiveau.Click += BtnAjouterNiveau_Click;
            pnlNivRows.Controls.Add(_btnNouveauNiveau);
            pnlNivRows.Height += 36;
            pnlNivRows.Resize += (s, ev) =>
            {
                int w = pnlNivRows.ClientSize.Width - 32;
                foreach (Control c in pnlNivRows.Controls.OfType<Panel>())
                    c.Width = Math.Max(300, w);
            };

            var pnlFiches = new Panel { Dock = DockStyle.Fill, BackColor = CREME_WARM };

            var pnlFichesTop = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = CREME_WARM, Padding = new Padding(16, 0, 16, 0) };
            pnlFichesTop.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, 0, pnlFichesTop.Width, 0);
            };
            _lblFichesHeader = new Label
            {
                Text = "Sélectionnez un niveau pour voir ses fiches BOM",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = CHOCO_MED, Location = new Point(16, 13), AutoSize = true
            };
            pnlFichesTop.Controls.Add(_lblFichesHeader);

            _btnNouvFiche = MakeActionButton("＋  Nouvelle fiche", CHOCO_BRAND, Color.White);
            _btnModFiche  = MakeActionButton("✎  Modifier",        CHOCO_MED,   Color.White);
            _btnSupFiche  = MakeActionButton("✕  Supprimer",       RED_CRIT,    Color.White);
            // US-07 : bouton Dupliquer — désactivé par défaut, activé sur sélection
            _btnDupFiche  = MakeActionButton("📋 Dupliquer",        CHOCO_MED,   Color.White);
            _btnDupFiche.Enabled = false;
            var btnNouvFiche = _btnNouvFiche;
            var btnModFiche  = _btnModFiche;
            var btnSupFiche  = _btnSupFiche;
            var btnDupFiche  = _btnDupFiche;

            btnNouvFiche.Click += (s, ev) => OuvrirFiche(null);
            btnModFiche.Click  += (s, ev) => OuvrirFiche(_dgvFiches?.CurrentRow?.DataBoundItem as BomFiche);
            btnSupFiche.Click  += (s, ev) => SupprimerFiche(_dgvFiches?.CurrentRow?.DataBoundItem as BomFiche);
            btnDupFiche.Click  += (s, ev) => DupliquerFiche(_dgvFiches?.CurrentRow?.DataBoundItem as BomFiche);

            pnlFichesTop.Resize += (s, ev) =>
            {
                btnSupFiche.Location  = new Point(pnlFichesTop.Width - 110, 8);
                btnModFiche.Location  = new Point(pnlFichesTop.Width - 220, 8);
                btnDupFiche.Location  = new Point(pnlFichesTop.Width - 330, 8);
                btnNouvFiche.Location = new Point(pnlFichesTop.Width - 448, 8);
            };
            pnlFichesTop.Controls.Add(btnNouvFiche);
            pnlFichesTop.Controls.Add(btnDupFiche);
            pnlFichesTop.Controls.Add(btnModFiche);
            pnlFichesTop.Controls.Add(btnSupFiche);

            _dgvFiches = new DataGridView
            {
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F),
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                GridColor = BORDER_CLR, RowHeadersVisible = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false, MultiSelect = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                ScrollBars = ScrollBars.Both,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 30
            };
            _dgvFiches.ColumnHeadersDefaultCellStyle.BackColor  = CREME; 
            _dgvFiches.ColumnHeadersDefaultCellStyle.ForeColor  = CHOCO_BRAND;
            _dgvFiches.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvFiches.DefaultCellStyle.SelectionBackColor       = CHOCO_BRAND;
            _dgvFiches.DefaultCellStyle.SelectionForeColor       = Color.White;
            _dgvFiches.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 246, 238);
            _dgvFiches.CellDoubleClick += (s, ev) =>
            {
                if (ev.RowIndex >= 0) OuvrirFiche(_dgvFiches.CurrentRow?.DataBoundItem as BomFiche);
            };
            // US-07 : activer/désactiver le bouton Dupliquer selon la sélection
            // ⚠ Capturer l'instance locale (pas le champ) — le champ est nullé lors d'une re-navigation
            // ce qui déclencherait une NRE via SelectionChanged sur l'ancien DGV encore en mémoire.
            var dgvFichesCapture = _dgvFiches;
            dgvFichesCapture.SelectionChanged += (s, ev) =>
            {
                if (btnDupFiche.IsDisposed) return;
                btnDupFiche.Enabled = dgvFichesCapture.CurrentRow?.DataBoundItem is BomFiche;
            };

            // ── Volet stock de production (panneau droit — sans header propre) ──
            _dgvStock = new DataGridView
            {
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F),
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                GridColor = BORDER_CLR, RowHeadersVisible = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false, MultiSelect = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                ScrollBars = ScrollBars.Both,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 30
            };
            _dgvStock.ColumnHeadersDefaultCellStyle.BackColor   = CREME;
            _dgvStock.ColumnHeadersDefaultCellStyle.ForeColor   = CHOCO_BRAND;
            _dgvStock.ColumnHeadersDefaultCellStyle.Font        = new Font("Segoe UI", 9F, FontStyle.Bold);
            _dgvStock.DefaultCellStyle.SelectionBackColor        = CHOCO_BRAND;
            _dgvStock.DefaultCellStyle.SelectionForeColor        = Color.White;
            _dgvStock.AlternatingRowsDefaultCellStyle.BackColor  = Color.FromArgb(250, 246, 238);

            // ── SplitContainer fiches | stock — splitter fixe à 50% ───────
            var splitFichesStock = new SplitContainer
            {
                Dock = DockStyle.Fill, Orientation = Orientation.Vertical,
                SplitterWidth = 4, BackColor = BORDER_CLR,
                IsSplitterFixed = true
            };
            splitFichesStock.Panel1.Controls.Add(_dgvFiches);
            splitFichesStock.Panel2.Controls.Add(_dgvStock);

            // Maintenir le splitter exactement à 50% lors de chaque redimensionnement
            splitFichesStock.Resize += (s, ev) =>
            {
                var sc = (SplitContainer)s;
                if (sc.Width > sc.SplitterWidth)
                    sc.SplitterDistance = (sc.Width - sc.SplitterWidth) / 2;
            };

            pnlFiches.Controls.Add(splitFichesStock);
            pnlFiches.Controls.Add(pnlFichesTop);

            _pnlDroit.Controls.Add(pnlFiches);
            _pnlDroit.Controls.Add(pnlNivRows);
            _pnlDroit.Controls.Add(pnlNivLabel);
            _pnlDroit.Controls.Add(pnlHdr);

            if (_state.ActiveNiveau != null && _niveauPanels.ContainsKey(_state.ActiveNiveau.Id))
                SelectNiveauRow(_state.ActiveNiveau);
            else if (niveaux.Count > 0)
                SelectNiveauRow(niveaux[0]);

            _pnlDroit.ResumeLayout();
        }

        // ════════════════════════════════════════════════════════════════
        //  Sélection niveau (synchronise sidebar ↔ droite)
        // ════════════════════════════════════════════════════════════════

        private void SyncNiveauSelection()
        {
            if (_state.ActiveNiveau == null) return;
            if (_niveauPanels.ContainsKey(_state.ActiveNiveau.Id))
                SelectNiveauRow(_state.ActiveNiveau);
        }

        private void SelectNiveauRow(BomNiveau niv)
        {
            _state.SetNiveau(niv);

            foreach (var kvp in _niveauPanels)
            {
                bool sel = kvp.Key == niv.Id;
                kvp.Value.BackColor = sel ? Color.FromArgb(255, 250, 229) : Color.White;
                kvp.Value.Invalidate();
            }

            for (int i = 0; i < _lstNiveaux.Items.Count; i++)
                if (((BomNiveau)_lstNiveaux.Items[i]).Id == niv.Id)
                {
                    _lstNiveaux.SelectedIndexChanged -= LstNiveaux_SelectedIndexChanged;
                    _lstNiveaux.SelectedIndex = i;
                    _lstNiveaux.SelectedIndexChanged += LstNiveaux_SelectedIndexChanged;
                    break;
                }

            if (_lblFichesHeader != null)
                _lblFichesHeader.Text = $"Fiches BOM du niveau N{niv.Ordre} — {niv.Nom}";
            ChargerFiches(niv);
        }

        private void ChargerFiches(BomNiveau niv)
        {
            if (_dgvFiches == null) return;
            try
            {
                _dgvFiches.DataSource = null;
                _dgvFiches.Columns.Clear();

                if (niv.Ordre == 1)
                {
                    // ── N1 : fiches ingrédients de l'activité (lecture seule) ───
                    if (_lblFichesHeader != null)
                        _lblFichesHeader.Text = "Fiches Ingrédients — " +
                            (_state.ActiveActivite?.Nom ?? _state.ActiveContexte?.ActiviteNom ?? "");

                    // Filtrer par activité via activites_stocks (JOIN sur id_stock)
                    _dgvFiches.DataSource = IngredientDAL.GetAll(idActivite: _state.ActiveActivite?.Id ?? 0);

                    if (_dgvFiches.Columns.Count > 0)
                    {
                        foreach (DataGridViewColumn col in _dgvFiches.Columns)
                            col.Visible = false;
                        int di = 0;
                        void ShowCol(string name, string header)
                        {
                            var col = _dgvFiches.Columns[name];
                            if (col == null) return;
                            col.Visible      = true;
                            col.HeaderText   = header;
                            col.DisplayIndex = di++;
                        }
                        ShowCol("Nom",                  "Ingrédient");
                        ShowCol("ConditionnementLabel", "Conditionnement");
                        ShowCol("UniteMesure",          "Unité base");
                        ShowCol("TypePhysique",         "Type physique");
                        ShowCol("NomFournisseur",       "Fournisseur");
                        _dgvFiches.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    }

                    // Boutons non applicables en mode lecture fiches ingrédients
                    if (_btnNouvFiche != null) _btnNouvFiche.Enabled = false;
                    if (_btnModFiche  != null) _btnModFiche.Enabled  = false;
                    if (_btnSupFiche  != null) _btnSupFiche.Enabled  = false;
                    if (_btnDupFiche  != null) _btnDupFiche.Enabled  = false;
                }
                else
                {
                    // ── N2+ : fiches BOM du niveau ─────────────────────────────
                    _dgvFiches.DataSource = BomFicheDAL.GetByNiveau(niv.Id);
                    if (_dgvFiches.Columns.Count > 0)
                    {
                        foreach (DataGridViewColumn col in _dgvFiches.Columns)
                            col.Visible = false;
                        int di = 0;
                        void ShowCol(string name, string header)
                        {
                            var col = _dgvFiches.Columns[name];
                            if (col == null) return;
                            col.Visible      = true;
                            col.HeaderText   = header;
                            col.DisplayIndex = di++;
                        }
                        // ↓ Modifier l'ordre ici pour changer l'ordre des colonnes
                        ShowCol("Nom",              "Fiche");
                        ShowCol("UniteOutput",      "Unité");
                        ShowCol("QuantiteOutput",   "Output / lot");
                        ShowCol("TempsPreparation", "Tps (min)");
                        ShowCol("DateCreation",     "Créée le");
                        _dgvFiches.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    }

                    // Réactiver les boutons BOM fiche
                    if (_btnNouvFiche != null) _btnNouvFiche.Enabled = true;
                    if (_btnModFiche  != null) _btnModFiche.Enabled  = true;
                    if (_btnSupFiche  != null) _btnSupFiche.Enabled  = true;
                    // Dupliquer reste géré par SelectionChanged
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement fiches : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            ChargerStockNiveau(niv);
        }

        private void ChargerStockNiveau(BomNiveau niv)
        {
            if (_dgvStock == null) return;
            try
            {
                _dgvStock.DataSource = null;
                _dgvStock.Columns.Clear();

                if (niv.Ordre == 1)
                {
                    // ── Niveau de base : ingrédients instanciés (stock disponible > 0) ──
                    _dgvStock.DataSource = IngredientDAL.GetAll(idActivite: _state.ActiveActivite?.Id ?? 0)
                        .Where(i => i.StockActuel > 0)
                        .ToList();
                    if (_dgvStock.Columns.Count > 0)
                    {
                        foreach (DataGridViewColumn col in _dgvStock.Columns)
                            col.Visible = false;
                        int di = 0;
                        void ShowCol(string name, string header)
                        {
                            var col = _dgvStock.Columns[name];
                            if (col == null) return;
                            col.Visible      = true;
                            col.HeaderText   = header;
                            col.DisplayIndex = di++;
                        }
                        // ↓ Modifier l'ordre ici pour changer l'ordre des colonnes
                        ShowCol("Nom",                 "Ingrédient");
                        ShowCol("StockActuel",         "Dispo");
                        ShowCol("UniteMesure",         "Unité base");
                        ShowCol("PrixAchatReference",  "€/cond.");
                        ShowCol("StockNom",            "Stock (lieu)");
                        ShowCol("ConditionnementLabel","Conditionnement");
                        _dgvStock.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    }
                }
                else
                {
                    // ── Niveaux supérieurs : bom_stocks produits à ce niveau ───
                    _dgvStock.DataSource = BomStockDAL.GetByNiveau(niv.Id);
                    if (_dgvStock.Columns.Count > 0)
                    {
                        foreach (DataGridViewColumn col in _dgvStock.Columns)
                            col.Visible = false;
                        int di = 0;
                        void ShowCol(string name, string header)
                        {
                            var col = _dgvStock.Columns[name];
                            if (col == null) return;
                            col.Visible      = true;
                            col.HeaderText   = header;
                            col.DisplayIndex = di++;
                        }
                        // ↓ Modifier l'ordre ici pour changer l'ordre des colonnes
                        ShowCol("NomFiche",           "Fiche");
                        ShowCol("QuantiteDisponible", "Qté dispo");
                        ShowCol("UniteOutput",        "Unité");
                        ShowCol("DateProduction",     "Produit le");
                        ShowCol("DateDlc",            "DLC");
                        ShowCol("CoutUnitaire",       "Coût/u");
                        ShowCol("CoutTotal",          "Coût/prod");
                        _dgvStock.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement stock niveau : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  DrawItem handlers
        // ════════════════════════════════════════════════════════════════

        private static void DrawCustomItem(DrawItemEventArgs e,
            string chipText, Color chipAccent, string label, string meta = null)
        {
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var br = new SolidBrush(selected ? Color.FromArgb(67, 45, 21) : CHOCO_DARK))
                e.Graphics.FillRectangle(br, e.Bounds);

            if (selected)
                using (var br = new SolidBrush(chipAccent))
                    e.Graphics.FillRectangle(br, e.Bounds.X, e.Bounds.Y, 3, e.Bounds.Height);

            int cx = e.Bounds.X + 10, cy = e.Bounds.Y + (e.Bounds.Height - 20) / 2;
            Color chipBg = selected ? chipAccent : Color.FromArgb(18, 255, 255, 255);
            Color chipFg = selected ? CHOCO_ABYSS : chipAccent;
            using (var br = new SolidBrush(chipBg))
                e.Graphics.FillRectangle(br, cx, cy, 22, 20);
            using (var f = new Font("Segoe UI", 7.5F, FontStyle.Bold))
            using (var br = new SolidBrush(chipFg))
                e.Graphics.DrawString(chipText, f, br,
                    new RectangleF(cx, cy, 22, 20),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            int lx = cx + 28, lw = e.Bounds.Width - lx - (meta != null ? 40 : 10) - e.Bounds.X;
            using (var f = new Font("Segoe UI", 10F, selected ? FontStyle.Bold : FontStyle.Regular))
            using (var br = new SolidBrush(selected ? Color.White : SIDEBAR_TXT))
                e.Graphics.DrawString(label, f, br,
                    new RectangleF(lx, e.Bounds.Y, lw, e.Bounds.Height),
                    new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });

            if (meta != null)
                using (var f = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var br = new SolidBrush(SIDEBAR_META))
                    e.Graphics.DrawString(meta, f, br,
                        new RectangleF(e.Bounds.Right - 40, e.Bounds.Y, 36, e.Bounds.Height),
                        new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
        }

        private void LstActivites_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var act = (Activite)_lstActivites.Items[e.Index];
            DrawCustomItem(e, act.Nom.Length > 0 ? act.Nom.Substring(0, 1).ToUpper() : "A", OR, act.Nom);
        }

        private void LstContextes_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var ctx = (BomContexte)_lstContextes.Items[e.Index];
            DrawCustomItem(e, ctx.Nom.Length > 0 ? ctx.Nom.Substring(0, 1).ToUpper() : "C", SIDEBAR_META, ctx.Nom);
        }

        private void LstNiveaux_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var niv = (BomNiveau)_lstNiveaux.Items[e.Index];
            DrawCustomItem(e, $"N{niv.Ordre}", GREEN_OK, niv.Nom);
        }

        // ════════════════════════════════════════════════════════════════
        //  Actions — Contextes
        // ════════════════════════════════════════════════════════════════

        private void BtnNouveauContexte_Click(object sender, EventArgs e)
        {
            if (_state.ActiveActivite == null)
            { MessageBox.Show("Sélectionnez une activité.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using (var frm = new FrmBomContexteEdit(null, _state.ActiveActivite))
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    ChargerContextes();
                    var cible = _state.ActiveContexte != null ? ScreenId.ContexteNiveaux : ScreenId.Hub;
                    NavigateTo(cible, forceRefresh: true);
                }
        }

        private void BtnModifierContexte_Click(object sender, EventArgs e)
        {
            if (_state.ActiveContexte == null) return;
            using (var frm = new FrmBomContexteEdit(_state.ActiveContexte, _state.ActiveActivite))
                if (frm.ShowDialog() == DialogResult.OK) ChargerContextes();
        }

        private void BtnSupprimerContexte_Click(object sender, EventArgs e)
        {
            if (_state.ActiveContexte == null) return;
            if (MessageBox.Show($"Supprimer « {_state.ActiveContexte.Nom} » et toutes ses données ?",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            try
            {
                BomContexteDAL.Delete(_state.ActiveContexte.Id);
                _state.SetContexte(null);
                ChargerContextes();
                NavigateTo(ScreenId.Hub, forceRefresh: true);
            }
            catch (Exception ex)
            { MessageBox.Show("Impossible : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ════════════════════════════════════════════════════════════════
        //  Actions — Niveaux
        // ════════════════════════════════════════════════════════════════

        private void BtnAjouterNiveau_Click(object sender, EventArgs e)
        {
            if (_state.ActiveContexte == null) return;
            var n = new BomNiveau
            {
                IdContexte = _state.ActiveContexte.Id,
                Ordre      = BomNiveauDAL.GetOrdreMax(_state.ActiveContexte.Id) + 1
            };
            using (var frm = new FrmBomNiveauEdit(n, false))
                if (frm.ShowDialog() == DialogResult.OK) { ChargerNiveaux(); NavigateTo(ScreenId.ContexteNiveaux, forceRefresh: true); }
        }

        private void SupprimerNiveau(BomNiveau niv)
        {
            if (MessageBox.Show($"Supprimer le niveau « {niv.Nom} » ?",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            try
            {
                BomNiveauDAL.Delete(niv.Id);
                if (_state.ActiveNiveau?.Id == niv.Id) _state.SetNiveau(null);
                ChargerNiveaux();
                NavigateTo(ScreenId.ContexteNiveaux, forceRefresh: true);
            }
            catch (InvalidOperationException ex)
            { MessageBox.Show(ex.Message, "Impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            catch (Exception ex)
            { MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ════════════════════════════════════════════════════════════════
        //  Actions — Fiches BOM
        // ════════════════════════════════════════════════════════════════

        private void OuvrirFiche(BomFiche fiche)
        {
            if (_state.ActiveNiveau == null)
            { MessageBox.Show("Sélectionnez un niveau.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using (var frm = new FrmBomFicheEdit(fiche, _state.ActiveNiveau))
                if (frm.ShowDialog() == DialogResult.OK) ChargerFiches(_state.ActiveNiveau);
        }

        private void SupprimerFiche(BomFiche fiche)
        {
            if (fiche == null)
            { MessageBox.Show("Sélectionnez une fiche.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show($"Supprimer « {fiche.Nom} » ?",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            try { BomFicheDAL.Delete(fiche.Id); ChargerFiches(_state.ActiveNiveau); }
            catch (Exception ex)
            { MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        /// <summary>
        /// US-07 : Duplique la fiche sélectionnée dans le même niveau via BomFicheDAL.Duplicate().
        /// Recharge la liste et sélectionne la copie dans le DGV.
        /// </summary>
        private void DupliquerFiche(BomFiche fiche)
        {
            if (fiche == null)
            { MessageBox.Show("Sélectionnez une fiche à dupliquer.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            if (MessageBox.Show($"Dupliquer « {fiche.Nom} » ?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1) != DialogResult.Yes) return;
            try
            {
                int idCopie = BomFicheDAL.Duplicate(fiche.Id);
                ChargerFiches(_state.ActiveNiveau);
                // Sélectionner la copie dans _dgvFiches
                foreach (DataGridViewRow row in _dgvFiches.Rows)
                    if (row.DataBoundItem is BomFiche f && f.Id == idCopie)
                    { _dgvFiches.CurrentCell = row.Cells[0]; break; }
            }
            catch (Exception ex)
            { MessageBox.Show("Erreur lors de la duplication : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ════════════════════════════════════════════════════════════════
        //  US-09 — Rapport du jour
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Génère et prévisualise un rapport du jour via PrintDocument + PrintPreviewDialog.
        /// Sections : en-tête activité · productions du jour · alertes stock · coût total.
        /// Police : Segoe UI 10pt corps, 9pt Bold sections.
        /// </summary>
        private void GenererRapport()
        {
            if (_state.ActiveActivite == null) return;

            List<BomProduction> prodsJour;
            List<Ingredient>    alertes;
            try
            {
                prodsJour = BomProductionDAL.GetDuJourByActivite(_state.ActiveActivite.Id);
                alertes   = IngredientDAL.GetAll(idActivite: _state.ActiveActivite.Id)
                                         .Where(i => i.EstEnAlerte).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la génération du rapport : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal coutJour = prodsJour.Sum(p => p.CoutIngredients);

            var doc = new System.Drawing.Printing.PrintDocument();
            doc.DocumentName = $"Rapport_{_state.ActiveActivite.Nom}_{DateTime.Today:yyyy-MM-dd}";

            doc.PrintPage += (s, ev) =>
            {
                var g      = ev.Graphics;
                var fontTitre    = new Font("Segoe UI", 13F, FontStyle.Bold);
                var fontSection  = new Font("Segoe UI", 9F,  FontStyle.Bold);
                var fontCorps    = new Font("Segoe UI", 10F, FontStyle.Regular);
                var fontMeta     = new Font("Segoe UI", 8F,  FontStyle.Italic);
                var brushNoir    = new SolidBrush(Color.FromArgb(44, 24, 16));
                var brushAccent  = new SolidBrush(Color.FromArgb(61, 40, 23));
                var brushMed     = new SolidBrush(Color.FromArgb(111, 78, 55));

                int margin = ev.MarginBounds.Left;
                int y      = ev.MarginBounds.Top;
                int width  = ev.MarginBounds.Width;

                // ── En-tête ──────────────────────────────────────────
                g.DrawString($"ARTISASTOCK — Rapport du jour", fontTitre, brushAccent,
                    new RectangleF(margin, y, width, 24));
                y += 28;
                g.DrawString($"Activité : {_state.ActiveActivite.Nom}   ·   {DateTime.Now:dd/MM/yyyy HH:mm}",
                    fontMeta, brushMed, new RectangleF(margin, y, width, 18));
                y += 24;

                using (var pen = new Pen(Color.FromArgb(195, 185, 168), 1))
                    g.DrawLine(pen, margin, y, margin + width, y);
                y += 10;

                // ── Section 1 : Productions du jour ──────────────────
                g.DrawString("PRODUCTIONS DU JOUR", fontSection, brushAccent,
                    new RectangleF(margin, y, width, 18));
                y += 22;

                if (prodsJour.Count == 0)
                {
                    g.DrawString("Aucune production ce jour.", fontMeta, brushMed,
                        new RectangleF(margin, y, width, 18));
                    y += 20;
                }
                else
                {
                    foreach (var p in prodsJour)
                    {
                        string ligne = $"  {p.DateProduction:HH:mm}  {p.NomFiche}  —  " +
                                       $"{p.QuantiteProduite:F0} {(p.NomNiveau ?? "")}  · " +
                                       $"Coût : {p.CoutIngredients:F2} €";
                        g.DrawString(ligne, fontCorps, brushNoir, new RectangleF(margin, y, width, 18));
                        y += 20;
                    }
                }
                y += 8;

                using (var pen = new Pen(Color.FromArgb(195, 185, 168), 1))
                    g.DrawLine(pen, margin, y, margin + width, y);
                y += 10;

                // ── Section 2 : Alertes stock ─────────────────────────
                g.DrawString("ALERTES STOCK", fontSection, brushAccent,
                    new RectangleF(margin, y, width, 18));
                y += 22;

                if (alertes.Count == 0)
                {
                    g.DrawString("✓  Aucune alerte de stock.", fontCorps, brushMed,
                        new RectangleF(margin, y, width, 18));
                    y += 20;
                }
                else
                {
                    foreach (var ing in alertes)
                    {
                        string ligne = $"  ⚠  {ing.Nom}  —  {ing.StockActuel:F0} {ing.UniteMesure}" +
                                       (ing.SeuilAlerteStock.HasValue
                                           ? $"  (seuil : {ing.SeuilAlerteStock.Value:F0} {ing.UniteMesure})"
                                           : "");
                        g.DrawString(ligne, fontCorps, brushNoir, new RectangleF(margin, y, width, 18));
                        y += 20;
                    }
                }
                y += 8;

                using (var pen = new Pen(Color.FromArgb(195, 185, 168), 1))
                    g.DrawLine(pen, margin, y, margin + width, y);
                y += 10;

                // ── Pied de page : coût total ─────────────────────────
                g.DrawString($"Coût total du jour : {coutJour:F2} €", fontSection, brushAccent,
                    new RectangleF(margin, y, width, 20));

                // Libérer les ressources GDI
                fontTitre.Dispose(); fontSection.Dispose(); fontCorps.Dispose(); fontMeta.Dispose();
                brushNoir.Dispose(); brushAccent.Dispose(); brushMed.Dispose();
            };

            using (var preview = new System.Windows.Forms.PrintPreviewDialog { Document = doc, Width = 900, Height = 700 })
                preview.ShowDialog(this);
        }

        // ════════════════════════════════════════════════════════════════
        //  Builder helpers
        // ════════════════════════════════════════════════════════════════

        private Panel MakeSectionHeader(string title)
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = CHOCO_DARK };
            pnl.Paint += (s, ev) =>
            {
                using (var pen = new Pen(Color.FromArgb(50, 255, 255, 255), 1))
                    ev.Graphics.DrawLine(pen, 12, pnl.Height - 1, pnl.Width - 12, pnl.Height - 1);
            };
            pnl.Controls.Add(new Label
            {
                Text = title.ToUpper(), Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(14, 0, 0, 0)
            });
            return pnl;
        }

        private Button MakeSidebarButton(string icon, string label, Action onClick)
        {
            var btn = new Button
            {
                Text      = $"{icon}  {label}",
                Font      = new Font("Segoe UI", 10F), FlatStyle = FlatStyle.Flat,
                Width     = _split?.Panel1.ClientSize.Width ?? 240, Height = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(12, 0, 0, 0),
                BackColor = CHOCO_DARK, ForeColor = SIDEBAR_TXT, Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize         = 0;
            btn.FlatAppearance.MouseOverBackColor = CHOCO_MED;
            btn.Click += (s, ev) => onClick();
            return btn;
        }

        private static Panel MakeSeparator()
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = CHOCO_DARK };
            pnl.Paint += (s, ev) =>
            {
                using (var pen = new Pen(Color.FromArgb(61, 40, 23), 1))
                    ev.Graphics.DrawLine(pen, 12, 3, pnl.Width - 12, 3);
            };
            return pnl;
        }

        private static Panel MakeStatCard(string icon, string label, string value, string delta, string tone)
        {
            Color bg     = Color.White;
            Color accent = CHOCO_BRAND;
            if (tone == "danger")  { bg = Color.FromArgb(255, 242, 242); accent = RED_CRIT; }
            if (tone == "gold")    { bg = Color.FromArgb(255, 252, 236); accent = OR;       }
            if (tone == "success") { bg = Color.FromArgb(242, 253, 242); accent = GREEN_OK; }

            var card = new Panel { BackColor = bg, Height = 68 };
            card.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using (var br = new SolidBrush(accent))
                    ev.Graphics.FillRectangle(br, 0, 0, 3, card.Height);
            };
            card.Controls.Add(new Label { Text = $"{icon} {label}", Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(120, 100, 80), Location = new Point(10, 8), AutoSize = true });
            card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = accent == OR ? Color.FromArgb(160, 130, 0) : accent, Location = new Point(10, 20), AutoSize = true });
            if (!string.IsNullOrEmpty(delta))
                card.Controls.Add(new Label { Text = delta, Font = new Font("Segoe UI", 7.5F), ForeColor = Color.FromArgb(120, 100, 80), Location = new Point(10, 50), AutoSize = true, MaximumSize = new Size(200, 0) });
            return card;
        }

        private static Panel MakeAlertRow(string tone, string icon, string title, string desc)
        {
            Color bg     = tone == "crit" ? Color.FromArgb(255, 242, 242) : Color.FromArgb(255, 248, 236);
            Color border = tone == "crit" ? RED_CRIT : ORG_WARN;
            var row = new Panel { Height = 52, BackColor = bg, Padding = new Padding(8, 6, 8, 6) };
            row.Paint += (s, ev) =>
            {
                using (var pen = new Pen(border, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1);
                using (var br = new SolidBrush(border))
                    ev.Graphics.FillRectangle(br, 0, 0, 4, row.Height);
            };
            row.Controls.Add(new Label { Text = icon, Font = new Font("Segoe UI", 13F), ForeColor = border, Location = new Point(10, 14), AutoSize = true });
            row.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = border, Location = new Point(30, 8), Size = new Size(row.Width - 38, 18) });
            if (!string.IsNullOrEmpty(desc))
                row.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 7.5F), ForeColor = Color.FromArgb(100, 80, 60), Location = new Point(30, 26), Size = new Size(row.Width - 38, 16) });
            return row;
        }

        private Panel MakeNiveauRow(BomNiveau niv, int ficheCount, int ordreMax)
        {
            bool locked = niv.Ordre == 0;
            var row = new Panel { Height = 64, BackColor = Color.White, Width = 600 };
            row.Paint += (s, ev) =>
            {
                bool sel  = _state.ActiveNiveau?.Id == niv.Id;
                Color leftC = locked ? Color.FromArgb(195, 165, 135) : sel ? OR : CHOCO_MED;
                Color brdC  = sel ? OR : BORDER_CLR;
                row.BackColor = sel ? Color.FromArgb(255, 250, 229) : Color.White;
                using (var pen = new Pen(brdC, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1);
                using (var br = new SolidBrush(leftC))
                    ev.Graphics.FillRectangle(br, 0, 0, 4, row.Height);
            };

            var lblOrdre = new Label
            {
                Text = $"N{niv.Ordre}", Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, Location = new Point(12, 10), Size = new Size(52, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var lblNom = new Label
            {
                Text = niv.Nom, Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, Location = new Point(68, 8), AutoSize = true
            };
            // US-11 : badge pill selon le statut du niveau
            Label pillBadge = null;
            if (niv.Ordre == ordreMax && ordreMax > 0)
            {
                pillBadge = new Label
                {
                    Text      = "top \u00b7 supprimable",
                    Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                    BackColor = OR,
                    ForeColor = CHOCO_BRAND,
                    AutoSize  = true,
                    Padding   = new Padding(6, 2, 6, 2),
                    Location  = new Point(68, 8)
                };
            }
            else if (niv.Ordre == 0)
            {
                pillBadge = new Label
                {
                    Text      = "verrouill\u00e9",
                    Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                    BackColor = CHOCO_MED,
                    ForeColor = Color.White,
                    AutoSize  = true,
                    Padding   = new Padding(6, 2, 6, 2),
                    Location  = new Point(68, 8)
                };
            }

            var lblDesc = new Label
            {
                Text = string.IsNullOrWhiteSpace(niv.Description)
                    ? (niv.Ordre == 0 ? "Stock global partagé — consommé par N1" : "")
                    : niv.Description,
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = CHOCO_MED, Location = new Point(68, 30), AutoSize = true, MaximumSize = new Size(400, 0)
            };
            var lblFiches = new Label
            {
                Text = ficheCount > 0 ? $"{ficheCount} fiches" : "—",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            row.Controls.Add(lblOrdre);
            row.Controls.Add(lblNom);
            if (pillBadge != null)
            {
                // Positionner le badge à droite du nom une fois la taille connue
                lblNom.SizeChanged += (s, ev) =>
                    pillBadge.Location = new Point(68 + lblNom.Width + 8, lblNom.Location.Y + 2);
                row.Controls.Add(pillBadge);
                pillBadge.BringToFront();
            }
            row.Controls.Add(lblDesc);
            row.Controls.Add(lblFiches);

            row.Resize += (s, ev) =>
            {
                lblFiches.SetBounds(row.Width - 100, 10, 88, 20);
                foreach (Control c in row.Controls.OfType<Label>())
                    if (c != lblOrdre && c != lblFiches) c.MaximumSize = new Size(row.Width - 200, 0);
            };

            if (!locked)
            {
                var btnFiches = new Button
                {
                    Text = "Fiches", Font = new Font("Segoe UI", 8F),
                    FlatStyle = FlatStyle.Flat, BackColor = CHOCO_BRAND, ForeColor = Color.White,
                    Size = new Size(64, 22), Location = new Point(68, 36), Cursor = Cursors.Hand
                };
                btnFiches.FlatAppearance.BorderSize = 0;
                btnFiches.Click += (s, ev) =>
                {
                    SelectNiveauRow(niv);
                    if (niv.Ordre == 1)
                    {
                        // N1 = ingrédients : raccourci vers FrmIngredients (fiches ingrédients)
                        new FrmIngredients(new Activite { Id = niv.IdActivite }).ShowDialog(this);
                    }
                    else
                    {
                        new FrmBomFiches(niv).ShowDialog(this);
                        ChargerFiches(niv);
                    }
                };

                var btnProd = new Button
                {
                    Text = "Produire", Font = new Font("Segoe UI", 8F),
                    FlatStyle = FlatStyle.Flat, BackColor = GREEN_OK, ForeColor = Color.White,
                    Size = new Size(64, 22), Location = new Point(138, 36), Cursor = Cursors.Hand
                };
                btnProd.FlatAppearance.BorderSize = 0;
                btnProd.Click += (s, ev) => { SelectNiveauRow(niv); NavigateTo(ScreenId.Production); };

                bool estTop = niv.Ordre == ordreMax;
                var btnDel = new Button
                {
                    Text    = "✕", Font = new Font("Segoe UI", 9F),
                    FlatStyle = FlatStyle.Flat, BackColor = Color.White,
                    ForeColor = estTop ? Color.FromArgb(160, 120, 100) : Color.FromArgb(200, 190, 180),
                    Size    = new Size(24, 22), Cursor = estTop ? Cursors.Hand : Cursors.Default,
                    Enabled = estTop
                };
                btnDel.FlatAppearance.BorderSize = 0;
                if (!estTop)
                {
                    var tip = new ToolTip();
                    tip.SetToolTip(btnDel, "Seul le niveau le plus haut (ordre max) est supprimable");
                }
                row.Resize += (s, ev) => btnDel.Location = new Point(row.Width - 30, 36);
                btnDel.Location = new Point(550, 36);
                btnDel.Click += (s, ev) => SupprimerNiveau(niv);

                row.Controls.Add(btnFiches);
                row.Controls.Add(btnProd);
                row.Controls.Add(btnDel);
            }

            row.Click += (s, ev) => SelectNiveauRow(niv);
            foreach (Control c in row.Controls) c.Click += (s, ev) => { };

            _niveauPanels[niv.Id] = row;
            row.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            return row;
        }

        private static Button MakeActionButton(string text, Color bg, Color fg)
        {
            var btn = new Button
            {
                Text = text, Font = new Font("Segoe UI", 8.5F),
                FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = fg,
                Size = new Size(102, 28), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ════════════════════════════════════════════════════════════════
        //  Écrans ressources & production — SFA (embed inline)
        // ════════════════════════════════════════════════════════════════

        private void ShowRessourceScreen(RessourceType type, NavigationParams p)
        {
            // US-08 : lire et réinitialiser le filtre alertes — évite la persistence entre navigations
            bool filtreAlertes = _state.FiltreAlertesSeulement;
            _state.SetFiltreAlertes(false);

            Form frm;
            switch (type)
            {
                case RessourceType.Fournisseurs: frm = new FrmFournisseurs();                     break;
                case RessourceType.Stocks:       frm = new FrmStocks();                           break;
                case RessourceType.Ingredients:  frm = new FrmIngredients(_state.ActiveActivite, filtreAlertes); break;
                case RessourceType.Achats:       frm = new FrmAchats(_state.ActiveActivite);       break;
                case RessourceType.VueStock:     frm = new FrmVueStock();                         break;
                // TICKET-17 : module Catalogue maintenant connecté
                case RessourceType.Categories:   frm = new FrmCategories();                       break;
                case RessourceType.Parfums:      frm = new FrmParfums();                          break;
                case RessourceType.Produits:     frm = new FrmProduits();                         break;
                default:                         return;
            }
            EmbedForm(frm);
        }

        private void ShowProductionScreen(NavigationParams p)
        {
            var frm = (_state.ActiveContexte != null && _state.ActiveNiveau != null)
                ? new FrmBomProductionSimulation(_state.ActiveContexte, _state.ActiveNiveau)
                : new FrmBomProductionSimulation(_state.ActiveActivite?.Id ?? 0);
            EmbedForm(frm);
        }

        /// <summary>
        /// Intègre un formulaire dans le panneau droit sans TopLevel (SFA).
        /// FormClosed déclenche le retour automatique à l'écran précédent.
        /// </summary>
        private void EmbedForm(Form frm)
        {
            _pnlDroit.SuspendLayout();
            _pnlDroit.Controls.Clear();
            frm.TopLevel        = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock            = DockStyle.Fill;
            frm.FormClosed     += (s, ev) =>
            {
                if (IsDisposed) return;
                if (_state.ActiveContexte != null)      NavigateTo(ScreenId.ContexteNiveaux, forceRefresh: true);
                else if (_state.ActiveActivite != null) NavigateTo(ScreenId.Hub,             forceRefresh: true);
                else                                    NavigateTo(ScreenId.Onboarding,      forceRefresh: true);
            };
            _pnlDroit.Controls.Add(frm);
            frm.Show();
            _pnlDroit.ResumeLayout();
        }

        // ════════════════════════════════════════════════════════════════
        //  Menu / Session
        // ════════════════════════════════════════════════════════════════

        // TICKET-17 : module Catalogue connecté — plus de PlaceholderWeb()
        private void menuCatCategories_Click(object sender, EventArgs e) =>
            NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Categories));

        private void menuCatParfums_Click(object sender, EventArgs e) =>
            NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Parfums));

        private void menuCatProduits_Click(object sender, EventArgs e) =>
            NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Produits));

        private void menuFournisseurs_Click(object sender, EventArgs e) =>
            NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Fournisseurs));
        private void menuCommandes_Click(object sender, EventArgs e) => PlaceholderWeb();

        private static void PlaceholderWeb() =>
            MessageBox.Show("Module Catalogue Web — à venir.", "En développement",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void menuDeconnexion_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Se déconnecter ?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Application.Restart() relance le processus complet → FrmLogin s'affiche à nouveau.
                // Toute donnée non persistée est perdue — acceptable pour ArtisaStock (pas de session mémoire critique).
                Application.Restart();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }

        private void FrmPrincipal_Resize(object sender, EventArgs e) { }

        // ════════════════════════════════════════════════════════════════
        //  Renderer menu sombre
        // ════════════════════════════════════════════════════════════════

        public class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer() : base(new DarkColorTable()) { }
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                using (var br = new SolidBrush((e.Item.Selected || e.Item.Pressed)
                    ? Color.FromArgb(212, 175, 55) : Color.FromArgb(61, 40, 23)))
                    e.Graphics.FillRectangle(br, new Rectangle(Point.Empty, e.Item.Size));
            }
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = (e.Item.Selected || e.Item.Pressed) ? Color.FromArgb(61, 40, 23) : Color.White;
                base.OnRenderItemText(e);
            }
        }

        private class DarkColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected              => Color.FromArgb(212, 175, 55);
            public override Color MenuItemBorder                => Color.Transparent;
            public override Color MenuBorder                    => Color.FromArgb(80, 55, 30);
            public override Color ToolStripDropDownBackground   => Color.FromArgb(50, 32, 18);
            public override Color ImageMarginGradientBegin      => Color.FromArgb(50, 32, 18);
            public override Color ImageMarginGradientMiddle     => Color.FromArgb(50, 32, 18);
            public override Color ImageMarginGradientEnd        => Color.FromArgb(50, 32, 18);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(212, 175, 55);
            public override Color MenuItemSelectedGradientEnd   => Color.FromArgb(212, 175, 55);
        }
    }
}
