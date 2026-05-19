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
    /// <summary>
    /// Hub principal — Single-Form Application (SFA).
    /// Ressources et production s'affichent inline dans Panel2 du SplitContainer.
    /// Les CRUD BOM (contextes, niveaux, fiches) utilisent encore ShowDialog — migration partielle.
    /// </summary>
    public partial class FrmPrincipal : Form
    {
        private readonly Utilisateur  _utilisateur;
        private readonly AppState     _state = new AppState();
        private          ScreenRouter _router;
        private List<BomNiveau>       _niveauxListe = new List<BomNiveau>();

        // ── Palette — alias locaux vers AppColors (source de vérité unique) ──
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
        private TitleBarPanel            _titleBar;
        private SidebarPanel             _sidebar;
        private AppStatusBar     _statusBar;

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
        private Button                 _btnNivProd;
        private Panel                  _pnlKanbanDetail;
        private Panel                  _pnlKanbanDetailContent;

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
            BuildShell();
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
            _router.OnPlaceholder     = p => ShowPlaceholder(null);
            _router.OnBoutiqueWeb     = p => ShowBoutiqueWebScreen();
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
        //  Construction du shell ERP
        // ════════════════════════════════════════════════════════════════

        private void BuildShell()
        {
            _titleBar = new TitleBarPanel(_utilisateur);
            _statusBar = new AppStatusBar();

            _sidebar = new SidebarPanel();
            _sidebar.NavigationRequested      += OnSidebarNavigation;
            _sidebar.ActivityChanged          += OnActivityChanged;
            _sidebar.ManageActivitiesRequested += OnManageActivities;
            _sidebar.NewContextRequested       += OnNewContext;

            _pnlDroit = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = CREME_WARM, Padding = new Padding(0)
            };

            // WinForms DockStyle order: Fill first, then Bottom, Left, Top
            Controls.Add(_pnlDroit);
            Controls.Add(_sidebar);
            Controls.Add(_statusBar);
            Controls.Add(_titleBar);
        }

        private void OnSidebarNavigation(NavItemId id)
        {
            _sidebar.SetActiveItem(id);
            switch (id)
            {
                case NavItemId.Hub:
                    NavigateTo(ScreenId.Hub);
                    break;
                case NavItemId.Production:
                    NavigateTo(ScreenId.Production);
                    break;
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
                case NavItemId.NiveauxContextes:
                    NavigateTo(ScreenId.ContexteNiveaux);
                    break;
                case NavItemId.FichesBom:
                    // Redirige vers le même écran que NiveauxContextes (rétrocompat)
                    NavigateTo(ScreenId.ContexteNiveaux);
                    break;
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
            UpdateTitleBar();
            UpdateStatusBar();
        }

        private void OnActivityChanged(Activite act)
        {
            if (act == null || act.Id == _state.ActiveActivite?.Id) return;
            _state.SetActivite(act);
            ChargerContextes();
            var cible = _state.ActiveContexte != null ? ScreenId.ContexteNiveaux : ScreenId.Hub;
            NavigateTo(cible, forceRefresh: true);
            UpdateStatusBar();
        }

        private void OnManageActivities()
        {
            using (var frm = new FrmActivites()) frm.ShowDialog(this);
            ChargerActivites();
        }

        private void OnNewContext()
        {
            BtnNouveauContexte_Click(this, EventArgs.Empty);
        }

        private void UpdateTitleBar()
        {
            if (_titleBar == null) return;
            var titles = new Dictionary<ScreenId, string>
            {
                { ScreenId.Onboarding,      "Bienvenue" },
                { ScreenId.Hub,             "Hub atelier" },
                { ScreenId.ContexteNiveaux, _state.ActiveContexte?.Nom ?? "Niveaux & contextes" },
                { ScreenId.Ressources,      _state.RessourceActive.ToString() },
                { ScreenId.Production,      "Production" },
                { ScreenId.Planning,        "Planning" },
                { ScreenId.DevisPatisserie, "Devis pâtisserie" },
                { ScreenId.Mouvements,      "Mouvements" },
                { ScreenId.Parametres,      "Paramètres" },
            };
            string title;
            titles.TryGetValue(_state.ActiveScreen, out title);
            _titleBar.SetTitle(title ?? "ArtisaStock");
        }

        private void UpdateStatusBar()
        {
            _statusBar?.UpdateState(_state);
        }

        // ════════════════════════════════════════════════════════════════
        //  Chargement des données
        // ════════════════════════════════════════════════════════════════

        private void ChargerActivites()
        {
            var acts = ActiviteDAL.GetAll();
            if (acts.Count == 0) { _state.SetActivite(null); ShowOnboarding(); return; }

            _sidebar.SetActivities(acts);

            if (_state.ActiveActivite != null)
            {
                var match = acts.Find(a => a.Id == _state.ActiveActivite.Id);
                if (match != null) _sidebar.SetSelectedActivity(match);
            }

            // If no activity set yet, trigger selection of first one
            if (_state.ActiveActivite == null)
            {
                _state.SetActivite(acts[0]);
                _sidebar.SetSelectedActivity(acts[0]);
            }

            ChargerContextes();
            var cible = _state.ActiveContexte != null ? ScreenId.ContexteNiveaux : ScreenId.Hub;
            NavigateTo(cible);
            UpdateStatusBar();
        }

        private void ChargerContextes()
        {
            if (_state.ActiveActivite == null) { _state.SetContexte(null); return; }
            var contextes = BomContexteDAL.GetAll(_state.ActiveActivite.Id);
            if (contextes.Count > 0 && _state.ActiveContexte == null)
                _state.SetContexte(contextes[0]);
            else if (contextes.Count > 0 && _state.ActiveContexte != null)
            {
                // Verify current context still exists
                if (!contextes.Any(c => c.Id == _state.ActiveContexte.Id))
                    _state.SetContexte(contextes[0]);
            }
            else
                _state.SetContexte(null);
            ChargerNiveaux();
        }

        private void ChargerNiveaux()
        {
            if (_state.ActiveContexte == null) { _niveauxListe = new List<BomNiveau>(); return; }
            _niveauxListe = BomNiveauDAL.GetByContexte(_state.ActiveContexte.Id);
        }

        // ════════════════════════════════════════════════════════════════
        //  Écrans du panneau droit
        // ════════════════════════════════════════════════════════════════

        private void ShowOnboarding()
        {
            _pnlDroit.SuspendLayout();
            ClearAndDisposePanel();

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
            btnCreer.Click += (s, ev) => { using (var frm = new FrmActivites()) frm.ShowDialog(this); ChargerActivites(); };
            pnlCenter.Controls.Add(btnCreer);

            _pnlDroit.Controls.Add(pnlCenter);
            _pnlDroit.ResumeLayout();
        }

        private void ShowHubScreen()
        {
            if (_state.ActiveActivite == null) { ShowOnboarding(); return; }
            _pnlDroit.SuspendLayout();
            ClearAndDisposePanel();

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
            _btnNivProd              = null;
            _pnlKanbanDetail        = null;
            _pnlKanbanDetailContent = null;

            _pnlDroit.SuspendLayout();
            ClearAndDisposePanel();

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

            // ══════════════════════════════════════════════════════════
            //  KANBAN — 3 colonnes : Niveaux | Fiches | Stock
            // ══════════════════════════════════════════════════════════

            var niveaux     = BomNiveauDAL.GetByContexte(_state.ActiveContexte.Id);
            _niveauxListe   = niveaux;
            int ordreMax    = niveaux.Count > 0 ? niveaux.Max(n => n.Ordre) : 0;
            var ficheCounts = BomFicheDAL.GetCountsByContexte(_state.ActiveContexte.Id);

            // ── Colonne 1 : Niveaux (gauche, fixe 220px) ──────────────
            var colNiveaux = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = CREME_WARM };
            var lblColNiv = MakeKanbanHeader("NIVEAUX", CHOCO_BRAND);
            var flowNiveaux = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, AutoScroll = true, BackColor = Color.Transparent,
                Padding = new Padding(8, 4, 8, 4)
            };
            foreach (var niv in niveaux.OrderByDescending(n => n.Ordre))
            {
                ficheCounts.TryGetValue(niv.Id, out int fc);
                var card = MakeNiveauCard(niv, fc, ordreMax);
                flowNiveaux.Controls.Add(card);
            }
            _btnNouveauNiveau = new Button
            {
                Text = "＋  Nouveau niveau", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = CHOCO_MED,
                Size = new Size(196, 32), Cursor = Cursors.Hand,
                Margin = new Padding(0, 4, 0, 0)
            };
            _btnNouveauNiveau.FlatAppearance.BorderColor = BORDER_CLR;
            _btnNouveauNiveau.Click += BtnAjouterNiveau_Click;
            flowNiveaux.Controls.Add(_btnNouveauNiveau);
            colNiveaux.Controls.Add(flowNiveaux);
            colNiveaux.Controls.Add(lblColNiv);

            // ── Colonne 2 : Fiches (Dock Left, taille auto) ──────────
            var pnlFiches = new Panel { Dock = DockStyle.Left, Width = 420, BackColor = CREME_WARM };
            var lblColFiches = MakeKanbanHeader("FICHES", OR);
            _lblFichesHeader = lblColFiches.Controls[0] as Label;

            var pnlFichesActions = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = CREME_WARM };

            _btnNivProd   = MakeSmallButton("▶ Produire", AppColors.Success, Color.White);
            _btnNivProd.Enabled = false;
            _btnNivProd.Click += (s, ev) => { if (_state.ActiveNiveau != null) NavigateTo(ScreenId.Production); };

            _btnNouvFiche = MakeSmallButton("＋ Nouvelle", CHOCO_BRAND, Color.White);
            _btnDupFiche  = MakeSmallButton("📋 Dupliquer", CHOCO_MED, Color.White);
            _btnModFiche  = MakeSmallButton("✎ Modifier",  CHOCO_MED, Color.White);
            _btnSupFiche  = MakeSmallButton("✕ Suppr.",     RED_CRIT,  Color.White);
            _btnDupFiche.Enabled = false;
            var btnNouvFiche = _btnNouvFiche;
            var btnModFiche  = _btnModFiche;
            var btnSupFiche  = _btnSupFiche;
            var btnDupFiche  = _btnDupFiche;

            btnNouvFiche.Click += (s, ev) => OuvrirFiche(null);
            btnModFiche.Click  += (s, ev) => OuvrirFiche(_dgvFiches?.CurrentRow?.DataBoundItem as BomFiche);
            btnSupFiche.Click  += (s, ev) => SupprimerFiche(_dgvFiches?.CurrentRow?.DataBoundItem as BomFiche);
            btnDupFiche.Click  += (s, ev) => DupliquerFiche(_dgvFiches?.CurrentRow?.DataBoundItem as BomFiche);

            var flowActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false, BackColor = Color.Transparent,
                Padding = new Padding(8, 4, 8, 0)
            };
            flowActions.Controls.Add(_btnNivProd);
            flowActions.Controls.Add(btnNouvFiche);
            flowActions.Controls.Add(btnDupFiche);
            flowActions.Controls.Add(btnModFiche);
            flowActions.Controls.Add(btnSupFiche);
            pnlFichesActions.Controls.Add(flowActions);

            _dgvFiches = new DataGridView
            {
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F),
                BackgroundColor = CREME_WARM, BorderStyle = BorderStyle.None,
                GridColor = BORDER_CLR, RowHeadersVisible = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false, AllowUserToResizeColumns = false,
                MultiSelect = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                ScrollBars = ScrollBars.Vertical,
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
            _dgvFiches.CellFormatting += DgvFiches_CellFormatting;
            var dgvFichesCapture = _dgvFiches;
            dgvFichesCapture.SelectionChanged += (s, ev) =>
            {
                if (btnDupFiche.IsDisposed) return;
                btnDupFiche.Enabled = dgvFichesCapture.CurrentRow?.DataBoundItem is BomFiche;
                ShowKanbanDetail();
            };

            pnlFiches.Controls.Add(_dgvFiches);
            pnlFiches.Controls.Add(pnlFichesActions);
            pnlFiches.Controls.Add(lblColFiches);

            // ── Colonne 3 : Stock (Dock Left, taille auto) ────────────
            var pnlStock = new Panel { Dock = DockStyle.Left, Width = 380, BackColor = CREME_WARM };
            var lblColStock = MakeKanbanHeader("STOCK", AppColors.Success);
            _dgvStock = new DataGridView
            {
                Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F),
                BackgroundColor = CREME_WARM, BorderStyle = BorderStyle.None,
                GridColor = BORDER_CLR, RowHeadersVisible = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false, AllowUserToResizeColumns = false,
                MultiSelect = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                ScrollBars = ScrollBars.Vertical,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 30
            };
            _dgvStock.ColumnHeadersDefaultCellStyle.BackColor   = CREME;
            _dgvStock.ColumnHeadersDefaultCellStyle.ForeColor   = CHOCO_BRAND;
            _dgvStock.ColumnHeadersDefaultCellStyle.Font        = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _dgvStock.DefaultCellStyle.SelectionBackColor        = CHOCO_BRAND;
            _dgvStock.DefaultCellStyle.SelectionForeColor        = Color.White;
            _dgvStock.AlternatingRowsDefaultCellStyle.BackColor  = Color.FromArgb(250, 246, 238);
            _dgvStock.CellFormatting += DgvStock_CellFormatting;
            _dgvStock.CellPainting   += DgvStock_CellPainting;
            _dgvStock.SelectionChanged += (s, ev) => ShowKanbanDetail();
            pnlStock.Controls.Add(_dgvStock);
            pnlStock.Controls.Add(lblColStock);

            // ── Colonne 4 : Volet détail (Fill = prend l'espace restant) ──
            _pnlKanbanDetail = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Color.FromArgb(252, 250, 246),
                Padding = new Padding(0)
            };
            var lblColDetail = MakeKanbanHeader("DÉTAIL", CHOCO_MED);
            _pnlKanbanDetailContent = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = Color.Transparent, Padding = new Padding(16, 12, 16, 12)
            };
            // Message par défaut
            _pnlKanbanDetailContent.Controls.Add(new Label
            {
                Text = "Sélectionnez un élément\npour voir ses détails",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = CHOCO_MED, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                Padding = new Padding(0, 40, 0, 0)
            });
            _pnlKanbanDetail.Controls.Add(_pnlKanbanDetailContent);
            _pnlKanbanDetail.Controls.Add(lblColDetail);

            // ── Séparateurs verticaux concrets entre colonnes ─────────
            var sep1 = MakeColumnSeparator();  // entre Niveaux et Fiches
            var sep2 = MakeColumnSeparator();  // entre Fiches et Stock
            var sep3 = MakeColumnSeparator();  // entre Stock et Détail

            // ── Assemblage (WinForms : Fill en 1er, puis Left de droite à gauche) ──
            _pnlDroit.Controls.Add(_pnlKanbanDetail);
            _pnlDroit.Controls.Add(sep3);
            _pnlDroit.Controls.Add(pnlStock);
            _pnlDroit.Controls.Add(sep2);
            _pnlDroit.Controls.Add(pnlFiches);
            _pnlDroit.Controls.Add(sep1);
            _pnlDroit.Controls.Add(colNiveaux);
            _pnlDroit.Controls.Add(pnlHdr);

            // Auto-dimensionner les colonnes après le 1er chargement
            var pnlFichesRef = pnlFiches;
            var pnlStockRef  = pnlStock;
            _dgvFiches.DataBindingComplete += (s, ev) =>
            {
                int w = 20; // scrollbar margin
                foreach (DataGridViewColumn c in _dgvFiches.Columns)
                    if (c.Visible) w += c.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
                pnlFichesRef.Width = Math.Max(300, Math.Min(w, 600));
            };
            _dgvStock.DataBindingComplete += (s, ev) =>
            {
                int w = 20;
                foreach (DataGridViewColumn c in _dgvStock.Columns)
                    if (c.Visible) w += c.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
                pnlStockRef.Width = Math.Max(280, Math.Min(w, 550));
            };

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
                kvp.Value.Invalidate();

            if (_lblFichesHeader != null)
                _lblFichesHeader.Text = $"FICHES — N{niv.Ordre} {niv.Nom}";
            if (_btnNivProd != null)
                _btnNivProd.Enabled = niv.Ordre > 1;
            ChargerFiches(niv);
        }

        // ════════════════════════════════════════════════════════════════
        //  Volet détail Kanban (colonne droite)
        // ════════════════════════════════════════════════════════════════

        private void ShowKanbanDetail()
        {
            if (_pnlKanbanDetailContent == null) return;
            _pnlKanbanDetailContent.SuspendLayout();
            _pnlKanbanDetailContent.Controls.Clear();

            // Priorité : sélection Stock > sélection Fiches
            if (_dgvStock != null && _dgvStock.Focused && _dgvStock.CurrentRow != null)
            {
                if (_dgvStock.CurrentRow.DataBoundItem is BomStock bs)
                    RenderStockDetail(bs);
                else if (_dgvStock.CurrentRow.DataBoundItem is Ingredient ingS)
                    RenderIngredientDetail(ingS);
            }
            else if (_dgvFiches != null && _dgvFiches.CurrentRow != null)
            {
                if (_dgvFiches.CurrentRow.DataBoundItem is BomFiche bf)
                    RenderFicheDetail(bf);
                else if (_dgvFiches.CurrentRow.DataBoundItem is Ingredient ing)
                    RenderIngredientDetail(ing);
            }

            _pnlKanbanDetailContent.ResumeLayout();
        }

        private void RenderIngredientDetail(Ingredient ing)
        {
            int y = 0;
            y = KDetailHeader(ing.Nom, "Ingrédient", y);

            y = KDetailSection("IDENTITÉ", y);
            if (!string.IsNullOrEmpty(ing.Marque))
                y = KDetailRow("Marque", ing.Marque, y);
            y = KDetailRow("Type", ing.TypePhysique, y);
            y = KDetailRow("Unité", ing.UniteMesure, y);
            if (ing.Densite.HasValue)
                y = KDetailRow("Densité", $"{ing.Densite.Value:F3} g/ml", y);
            if (!string.IsNullOrEmpty(ing.ConditionnementLabel))
                y = KDetailRow("Cond.", ing.ConditionnementLabel, y);
            y = KDetailRow("Qté/cond.", UnitConvertisseur.FormatQte(ing.QteParConditionnement, ing.UniteMesure), y);

            y = KDetailSection("STOCK & PRIX", y);
            y = KDetailRow("En stock", UnitConvertisseur.FormatQte(ing.StockActuel, ing.UniteMesure), y,
                ing.EstEnAlerte ? RED_CRIT : AppColors.Success);
            if (ing.StockPieces > 0)
                y = KDetailRow("Pièces", $"{ing.StockPieces:0} cond.", y);
            if (ing.StockCible.HasValue)
            {
                int pct = (int)(ing.StockRatio.Value * 100);
                y = KDetailRow("Cible", UnitConvertisseur.FormatQte(ing.StockCible.Value, ing.UniteMesure), y);
                y = KDetailRow("Niveau", $"{pct}%", y,
                    pct < 20 ? RED_CRIT : pct < 50 ? AppColors.OrgWarn : AppColors.Success);
            }
            if (ing.SeuilAlerteStock.HasValue)
            {
                y = KDetailRow("Seuil alerte", UnitConvertisseur.FormatQte(ing.SeuilAlerteStock.Value, ing.UniteMesure), y);
                y = KDetailRow("État", ing.EstEnAlerte ? "⚠ EN ALERTE" : "✓ OK", y,
                    ing.EstEnAlerte ? RED_CRIT : AppColors.Success);
            }
            y = KDetailRow("Prix/cond.", UnitConvertisseur.FormatPrix(ing.PrixAchatReference), y);
            if (!string.IsNullOrEmpty(ing.StockNom))
                y = KDetailRow("Emplacement", ing.StockNom, y);
            if (!string.IsNullOrEmpty(ing.NomFournisseur))
                y = KDetailRow("Fournisseur", ing.NomFournisseur, y);

            // Utilisé dans quelles fiches
            try
            {
                var recettes = BomFicheLigneDAL.GetFichesUtilisant(ing.Id);
                if (recettes.Count > 0)
                {
                    y = KDetailSection("UTILISÉ DANS", y);
                    foreach (var r in recettes)
                        y = KDetailTag(r, y);
                }
            }
            catch { }
        }

        private void RenderFicheDetail(BomFiche fiche)
        {
            int y = 0;
            y = KDetailHeader(fiche.Nom, $"Fiche recette — N{fiche.OrdreNiveau}", y);

            y = KDetailSection("IDENTITÉ", y);
            if (!string.IsNullOrEmpty(fiche.Description))
                y = KDetailRow("Description", fiche.Description, y);
            y = KDetailRow("Output", UnitConvertisseur.FormatQte(fiche.QuantiteOutput, fiche.UniteOutput), y);
            if (fiche.TempsPreparation.HasValue)
                y = KDetailRow("Temps prép.", $"{fiche.TempsPreparation.Value} min", y);
            y = KDetailRow("Créée le", fiche.DateCreation.ToString("dd/MM/yyyy"), y);
            y = KDetailRow("Statut", fiche.Actif ? "✓ Active" : "✕ Inactive", y,
                fiche.Actif ? AppColors.Success : RED_CRIT);

            // Composition (lignes de la fiche)
            try
            {
                var ficheComplete = BomFicheDAL.GetById(fiche.Id, avecLignes: true);
                if (ficheComplete?.Lignes != null && ficheComplete.Lignes.Count > 0)
                {
                    y = KDetailSection("COMPOSITION", y);
                    foreach (var ligne in ficheComplete.Lignes)
                    {
                        string qte = UnitConvertisseur.FormatQte(ligne.Quantite, ligne.UniteMesure);
                        y = KDetailRow(ligne.NomInput, qte, y);
                    }
                }
            }
            catch { }

            // Consommé par quelles fiches de niveau supérieur
            try
            {
                var consommePar = BomFicheLigneDAL.GetFichesConsommant(fiche.Id);
                if (consommePar.Count > 0)
                {
                    y = KDetailSection("CONSOMMÉ PAR", y);
                    foreach (var r in consommePar)
                        y = KDetailTag(r, y);
                }
            }
            catch { }
        }

        private void RenderStockDetail(BomStock stock)
        {
            int y = 0;
            y = KDetailHeader(stock.NomFiche, "Produit en stock", y);

            y = KDetailSection("STOCK", y);
            y = KDetailRow("Quantité", UnitConvertisseur.FormatQte(stock.QuantiteDisponible, stock.UniteOutput), y);
            y = KDetailRow("Coût unit.", UnitConvertisseur.FormatPrix(stock.CoutUnitaire), y);
            y = KDetailRow("Valeur", UnitConvertisseur.FormatPrix(stock.CoutTotal), y, CHOCO_BRAND);

            y = KDetailSection("PRODUCTION", y);
            y = KDetailRow("Produit le", stock.DateProduction.ToString("dd/MM/yyyy HH:mm"), y);
            y = KDetailRow("ID prod.", $"#{stock.IdProduction}", y);
            if (stock.DateDlc.HasValue)
            {
                bool expire = stock.DateDlc.Value < DateTime.Today;
                bool proche = !expire && (stock.DateDlc.Value - DateTime.Today).TotalDays < 7;
                y = KDetailRow("DLC", stock.DateDlc.Value.ToString("dd/MM/yyyy"), y,
                    expire ? RED_CRIT : proche ? AppColors.OrgWarn : CHOCO_MED);
            }

            y = KDetailSection("CONTEXTE", y);
            if (!string.IsNullOrEmpty(stock.NomNiveau))
                y = KDetailRow("Niveau", $"N{stock.OrdreNiveau} {stock.NomNiveau}", y);
            if (!string.IsNullOrEmpty(stock.NomContexte))
                y = KDetailRow("Contexte", stock.NomContexte, y);

            // Composition de la fiche source
            try
            {
                var fiche = BomFicheDAL.GetById(stock.IdFiche, avecLignes: true);
                if (fiche?.Lignes != null && fiche.Lignes.Count > 0)
                {
                    y = KDetailSection("COMPOSITION", y);
                    foreach (var ligne in fiche.Lignes)
                    {
                        string qte = UnitConvertisseur.FormatQte(ligne.Quantite, ligne.UniteMesure);
                        y = KDetailRow(ligne.NomInput, qte, y);
                    }
                }
            }
            catch { }
        }

        // ── Helpers rendu détail Kanban ───────────────────────────────

        private int KDetailHeader(string nom, string type, int y)
        {
            var accent = new Panel
            {
                Location = new Point(0, y), Size = new Size(300, 4),
                BackColor = CHOCO_BRAND
            };
            _pnlKanbanDetailContent.Controls.Add(accent);
            y += 8;
            _pnlKanbanDetailContent.Controls.Add(new Label
            {
                Text = nom, Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, Location = new Point(0, y),
                AutoSize = true, MaximumSize = new Size(280, 0)
            });
            y += 28;
            _pnlKanbanDetailContent.Controls.Add(new Label
            {
                Text = type, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = CHOCO_MED, BackColor = Color.FromArgb(240, 235, 225),
                AutoSize = true, Padding = new Padding(6, 2, 6, 2),
                Location = new Point(0, y)
            });
            return y + 24;
        }

        private int KDetailSection(string title, int y)
        {
            y += 6;
            _pnlKanbanDetailContent.Controls.Add(new Label
            {
                Text = title, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, Location = new Point(0, y), AutoSize = true
            });
            y += 18;
            _pnlKanbanDetailContent.Controls.Add(new Panel
            {
                Location = new Point(0, y - 2), Size = new Size(280, 1),
                BackColor = BORDER_CLR
            });
            return y;
        }

        private int KDetailRow(string label, string value, int y, Color? valColor = null)
        {
            _pnlKanbanDetailContent.Controls.Add(new Label
            {
                Text = label, Font = new Font("Segoe UI", 8.5F),
                ForeColor = CHOCO_MED, Location = new Point(0, y), AutoSize = true
            });
            _pnlKanbanDetailContent.Controls.Add(new Label
            {
                Text = value ?? "—", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = valColor ?? CHOCO_BRAND,
                Location = new Point(110, y), AutoSize = true, MaximumSize = new Size(180, 0)
            });
            return y + 20;
        }

        private int KDetailTag(string text, int y)
        {
            var tag = new Label
            {
                Text = text, Font = new Font("Segoe UI", 8F),
                ForeColor = CHOCO_BRAND, BackColor = Color.FromArgb(245, 240, 232),
                AutoSize = true, Padding = new Padding(6, 2, 6, 2),
                Location = new Point(0, y)
            };
            tag.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, tag.Width - 1, tag.Height - 1);
            };
            _pnlKanbanDetailContent.Controls.Add(tag);
            return y + 24;
        }

        private void DgvFiches_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || _dgvFiches == null) return;
            var col = _dgvFiches.Columns[e.ColumnIndex];
            var row = _dgvFiches.Rows[e.RowIndex];

            if (col.Name == "QuantiteOutput" && row.DataBoundItem is BomFiche f)
                e.Value = UnitConvertisseur.FormatQte(f.QuantiteOutput, f.UniteOutput);
            else if (col.Name == "StockActuel" && row.DataBoundItem is Ingredient ing)
                e.Value = UnitConvertisseur.FormatQte(ing.StockActuel, ing.UniteMesure);
            else if (col.Name == "PrixAchatReference" && row.DataBoundItem is Ingredient ing3)
                e.Value = UnitConvertisseur.FormatPrix(ing3.PrixAchatReference);
        }

        private void DgvStock_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || _dgvStock == null) return;
            var col = _dgvStock.Columns[e.ColumnIndex];
            var row = _dgvStock.Rows[e.RowIndex];

            if (col.Name == "QuantiteDisponible" && row.DataBoundItem is BomStock bs)
                e.Value = UnitConvertisseur.FormatQte(bs.QuantiteDisponible, bs.UniteOutput);
            else if (col.Name == "StockActuel" && row.DataBoundItem is Ingredient ing2)
                e.Value = UnitConvertisseur.FormatQte(ing2.StockActuel, ing2.UniteMesure);
            else if (col.Name == "CoutUnitaire" && row.DataBoundItem is BomStock bs2)
                e.Value = UnitConvertisseur.FormatPrix(bs2.CoutUnitaire);
            else if (col.Name == "CoutTotal" && row.DataBoundItem is BomStock bs3)
                e.Value = UnitConvertisseur.FormatPrix(bs3.CoutTotal);
            else if (col.Name == "PrixAchatReference" && row.DataBoundItem is Ingredient ing4)
                e.Value = UnitConvertisseur.FormatPrix(ing4.PrixAchatReference);
            else if (col.Name == "StockPieces" && row.DataBoundItem is Ingredient ing5)
                e.Value = ing5.StockPieces == 0 ? "—" : $"{ing5.StockPieces:0}";
        }

        /// <summary>Peint la barre jauge dans la colonne "Jauge" du DGV Stock.</summary>
        private void DgvStock_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || _dgvStock == null) return;
            if (_dgvStock.Columns[e.ColumnIndex].Name != "Jauge") return;
            if (!(_dgvStock.Rows[e.RowIndex].DataBoundItem is Ingredient ing)) return;

            e.Handled = true;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fond de la cellule
            bool sel = (e.State & DataGridViewElementStates.Selected) != 0;
            Color bgColor = sel ? _dgvStock.DefaultCellStyle.SelectionBackColor
                : (e.RowIndex % 2 == 1 ? Color.FromArgb(250, 246, 238) : Color.White);
            using (var br = new SolidBrush(bgColor))
                g.FillRectangle(br, e.CellBounds);

            double? ratio = ing.StockRatio;
            if (!ratio.HasValue)
            {
                // Pas de cible → tiret centré
                using (var f = new Font("Segoe UI", 8F))
                using (var br = new SolidBrush(CHOCO_MED))
                {
                    var sz = g.MeasureString("—", f);
                    g.DrawString("—", f, br,
                        e.CellBounds.X + (e.CellBounds.Width - sz.Width) / 2,
                        e.CellBounds.Y + (e.CellBounds.Height - sz.Height) / 2);
                }
                return;
            }

            // Dimensions de la barre
            int pad = 6, barH = 14;
            int barX = e.CellBounds.X + pad;
            int barW = e.CellBounds.Width - pad * 2;
            int barY = e.CellBounds.Y + (e.CellBounds.Height - barH) / 2;
            int fillW = (int)(Math.Min(ratio.Value, 1.0) * barW);

            // Couleur selon le ratio (seuil alerte pris en compte)
            double r = ratio.Value;
            Color barColor;
            if (r < 0.20)      barColor = Color.FromArgb(220, 60, 50);   // rouge
            else if (r < 0.50) barColor = Color.FromArgb(230, 160, 40);  // orange
            else if (r <= 1.0) barColor = Color.FromArgb(80, 165, 80);   // vert
            else               barColor = Color.FromArgb(50, 130, 200);  // bleu (surplus)

            // Fond gris de la barre
            using (var path = RoundedRect(new Rectangle(barX, barY, barW, barH), 3))
            using (var br = new SolidBrush(Color.FromArgb(230, 225, 218)))
                g.FillPath(br, path);

            // Remplissage
            if (fillW > 2)
            {
                using (var path = RoundedRect(new Rectangle(barX, barY, fillW, barH), 3))
                using (var br = new SolidBrush(barColor))
                    g.FillPath(br, path);
            }

            // Marque du seuil d'alerte
            if (ing.SeuilAlerteStock.HasValue && ing.StockCible.HasValue && ing.StockCible.Value > 0)
            {
                double seuilRatio = (double)(ing.SeuilAlerteStock.Value / ing.StockCible.Value);
                if (seuilRatio > 0 && seuilRatio < 1)
                {
                    int sx = barX + (int)(seuilRatio * barW);
                    using (var pen = new Pen(Color.FromArgb(180, 220, 60, 50), 1.5f))
                        g.DrawLine(pen, sx, barY - 1, sx, barY + barH + 1);
                }
            }

            // Label pourcentage
            string pct = r > 9.99 ? ">999%" : $"{(int)(r * 100)}%";
            using (var f = new Font("Segoe UI", 7F, FontStyle.Bold))
            using (var br = new SolidBrush(sel ? Color.White : CHOCO_BRAND))
            {
                var sz = g.MeasureString(pct, f);
                float tx = barX + barW + 2;
                if (tx + sz.Width > e.CellBounds.Right - 2)
                    tx = barX + fillW / 2f - sz.Width / 2f; // inside bar
                g.DrawString(pct, f, br, tx, e.CellBounds.Y + (e.CellBounds.Height - sz.Height) / 2);
            }

            // Bordure cellule
            e.PaintContent(e.CellBounds);
        }

        private void ChargerFiches(BomNiveau niv)
        {
            if (_dgvFiches == null) return;
            List<Ingredient> ingsCache = null;
            try
            {
                _dgvFiches.SuspendLayout();
                _dgvFiches.DataSource = null;
                _dgvFiches.Columns.Clear();

                if (niv.Ordre == 1)
                {
                    if (_lblFichesHeader != null)
                        _lblFichesHeader.Text = "Fiches Ingrédients — " +
                            (_state.ActiveActivite?.Nom ?? _state.ActiveContexte?.ActiviteNom ?? "");

                    // Cache partagé : évite un 2e appel identique dans ChargerStockNiveau
                    ingsCache = IngredientDAL.GetAll(idActivite: _state.ActiveActivite?.Id ?? 0);
                    _dgvFiches.DataSource = ingsCache;

                    if (_dgvFiches.Columns.Count > 0)
                    {
                        foreach (DataGridViewColumn col in _dgvFiches.Columns)
                            col.Visible = false;
                        int di = 0;
                        void ShowCol(string name, string header, int width)
                        {
                            var col = _dgvFiches.Columns[name];
                            if (col == null) return;
                            col.Visible      = true;
                            col.HeaderText   = header;
                            col.Width        = width;
                            col.DisplayIndex = di++;
                        }
                        ShowCol("Nom",                  "Ingrédient",    180);
                        ShowCol("ConditionnementLabel", "Conditionnement", 120);
                        ShowCol("TypePhysique",         "Type physique",  90);
                        ShowCol("NomFournisseur",       "Fournisseur",   130);
                    }

                    if (_btnNouvFiche != null) _btnNouvFiche.Enabled = false;
                    if (_btnModFiche  != null) _btnModFiche.Enabled  = false;
                    if (_btnSupFiche  != null) _btnSupFiche.Enabled  = false;
                    if (_btnDupFiche  != null) _btnDupFiche.Enabled  = false;
                }
                else
                {
                    _dgvFiches.DataSource = BomFicheDAL.GetByNiveau(niv.Id);
                    if (_dgvFiches.Columns.Count > 0)
                    {
                        foreach (DataGridViewColumn col in _dgvFiches.Columns)
                            col.Visible = false;
                        int di = 0;
                        void ShowCol(string name, string header, int width)
                        {
                            var col = _dgvFiches.Columns[name];
                            if (col == null) return;
                            col.Visible      = true;
                            col.HeaderText   = header;
                            col.Width        = width;
                            col.DisplayIndex = di++;
                        }
                        ShowCol("Nom",              "Fiche",        180);
                        ShowCol("QuantiteOutput",   "Output / lot", 100);
                        ShowCol("TempsPreparation", "Tps (min)",     70);
                        ShowCol("DateCreation",     "Créée le",      90);
                    }

                    if (_btnNouvFiche != null) _btnNouvFiche.Enabled = true;
                    if (_btnModFiche  != null) _btnModFiche.Enabled  = true;
                    if (_btnSupFiche  != null) _btnSupFiche.Enabled  = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement fiches : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _dgvFiches.ResumeLayout();
            }
            ChargerStockNiveau(niv, ingsCache);
        }

        private void ChargerStockNiveau(BomNiveau niv, List<Ingredient> ingsCache = null)
        {
            if (_dgvStock == null) return;
            try
            {
                _dgvStock.SuspendLayout();
                _dgvStock.DataSource = null;
                _dgvStock.Columns.Clear();

                if (niv.Ordre == 1)
                {
                    // Réutilise le cache chargé par ChargerFiches — zéro requête supplémentaire
                    var ings = ingsCache ?? IngredientDAL.GetAll(idActivite: _state.ActiveActivite?.Id ?? 0);
                    _dgvStock.DataSource = ings.Where(i => i.StockActuel > 0).ToList();
                    if (_dgvStock.Columns.Count > 0)
                    {
                        foreach (DataGridViewColumn col in _dgvStock.Columns)
                            col.Visible = false;
                        int di = 0;
                        void ShowCol(string name, string header, int width)
                        {
                            var col = _dgvStock.Columns[name];
                            if (col == null) return;
                            col.Visible      = true;
                            col.HeaderText   = header;
                            col.Width        = width;
                            col.DisplayIndex = di++;
                        }
                        ShowCol("Nom",                 "Ingrédient",     140);
                        ShowCol("StockActuel",         "Dispo",           80);
                        ShowCol("StockPieces",         "Pièces",          50);
                        ShowCol("PrixAchatReference",  "€/cond.",         65);
                        ShowCol("StockNom",            "Lieu",            90);

                        // Colonne jauge custom-drawn
                        var colJauge = new DataGridViewTextBoxColumn
                        {
                            Name = "Jauge", HeaderText = "Niveau", Width = 80,
                            ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable
                        };
                        colJauge.DefaultCellStyle.NullValue = "";
                        _dgvStock.Columns.Add(colJauge);
                        colJauge.DisplayIndex = di++;
                    }
                }
                else
                {
                    _dgvStock.DataSource = BomStockDAL.GetByNiveau(niv.Id);
                    if (_dgvStock.Columns.Count > 0)
                    {
                        foreach (DataGridViewColumn col in _dgvStock.Columns)
                            col.Visible = false;
                        int di = 0;
                        void ShowCol(string name, string header, int width)
                        {
                            var col = _dgvStock.Columns[name];
                            if (col == null) return;
                            col.Visible      = true;
                            col.HeaderText   = header;
                            col.Width        = width;
                            col.DisplayIndex = di++;
                        }
                        ShowCol("NomFiche",           "Fiche",        160);
                        ShowCol("QuantiteDisponible", "Qté dispo",    100);
                        ShowCol("DateProduction",     "Produit le",    90);
                        ShowCol("DateDlc",            "DLC",           90);
                        ShowCol("CoutUnitaire",       "Coût/u",        70);
                        ShowCol("CoutTotal",          "Coût/prod",     80);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement stock niveau : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _dgvStock.ResumeLayout();
            }
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
                    UpdateStatusBar();
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

        /// <summary>Carte niveau pour la colonne Kanban gauche.</summary>
        private Panel MakeNiveauCard(BomNiveau niv, int ficheCount, int ordreMax)
        {
            bool locked = niv.Ordre == 0;
            bool estTop = niv.Ordre == ordreMax && ordreMax > 0;
            string subtitle = locked ? "Stock global partagé"
                : $"{ficheCount} fiche{(ficheCount != 1 ? "s" : "")}";

            var card = new Panel
            {
                Size   = new Size(196, 64),
                Margin = new Padding(0, 0, 0, 4),
                Cursor = Cursors.Hand
            };
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, card, new object[] { true });

            card.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                bool sel     = _state.ActiveNiveau?.Id == niv.Id;
                Color accent = locked ? Color.FromArgb(195, 165, 135) : sel ? OR : CHOCO_MED;
                Color bg     = sel ? Color.FromArgb(255, 250, 229) : Color.White;
                Color brd    = sel ? OR : BORDER_CLR;

                using (var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 6))
                {
                    using (var br = new SolidBrush(bg))
                        g.FillPath(br, path);
                    using (var pen = new Pen(brd, sel ? 2f : 1f))
                        g.DrawPath(pen, path);
                }

                // Barre accent gauche
                using (var br = new SolidBrush(accent))
                    g.FillRectangle(br, 0, 8, 4, card.Height - 16);

                // Cercle numéro
                int cx = 22, cy = card.Height / 2, r = 13;
                using (var br = new SolidBrush(accent))
                    g.FillEllipse(br, cx - r, cy - r, r * 2, r * 2);
                using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
                using (var br = new SolidBrush(Color.White))
                {
                    string num = niv.Ordre.ToString();
                    var sz = g.MeasureString(num, f);
                    g.DrawString(num, f, br, cx - sz.Width / 2, cy - sz.Height / 2);
                }

                // Nom
                using (var f = new Font("Segoe UI", 9.5F, FontStyle.Bold))
                using (var br = new SolidBrush(CHOCO_BRAND))
                    g.DrawString(niv.Nom, f, br, 42, 10);

                // Sous-titre
                using (var f = new Font("Segoe UI", 8F))
                using (var br = new SolidBrush(CHOCO_MED))
                    g.DrawString(subtitle, f, br, 42, 32);

                // Badge produit final
                if (estTop)
                {
                    string badge = "★ Final";
                    using (var f = new Font("Segoe UI", 7F, FontStyle.Bold))
                    {
                        var sz = g.MeasureString(badge, f);
                        int bx = card.Width - (int)sz.Width - 10, by = 8;
                        using (var br = new SolidBrush(Color.FromArgb(240, 248, 240)))
                            g.FillRectangle(br, bx - 2, by - 1, sz.Width + 4, sz.Height + 2);
                        using (var br = new SolidBrush(AppColors.Success))
                            g.DrawString(badge, f, br, bx, by);
                    }
                }
            };

            card.Click += (s, ev) => SelectNiveauRow(niv);

            // Menu contextuel
            if (!locked)
            {
                var menu = new ContextMenuStrip();
                menu.Items.Add("📋  Gérer les fiches").Click += (s2, ev2) =>
                {
                    SelectNiveauRow(niv);
                    if (niv.Ordre == 1)
                    { using (var frm = new FrmIngredients(new Activite { Id = niv.IdActivite })) frm.ShowDialog(this); }
                    else
                    { using (var frm = new FrmBomFiches(niv)) frm.ShowDialog(this); ChargerFiches(niv); }
                };
                var miProd = menu.Items.Add("▶  Lancer une production");
                miProd.Enabled = niv.Ordre > 1;
                miProd.Click += (s2, ev2) => { SelectNiveauRow(niv); NavigateTo(ScreenId.Production); };
                menu.Items.Add(new ToolStripSeparator());
                var miDel = menu.Items.Add("✕  Supprimer ce niveau");
                miDel.Enabled = estTop;
                if (!estTop) miDel.ToolTipText = "Seul le niveau le plus haut est supprimable";
                miDel.Click += (s2, ev2) => SupprimerNiveau(niv);
                card.ContextMenuStrip = menu;
            }

            _niveauPanels[niv.Id] = card;
            return card;
        }

        /// <summary>Crée un en-tête de colonne Kanban (32px) avec accent en bas.</summary>
        private static Panel MakeKanbanHeader(string title, Color accentColor)
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = AppColors.CremeWarm };
            pnl.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                // Ligne accent fine (2px) en bas
                using (var br = new SolidBrush(accentColor))
                    g.FillRectangle(br, 0, pnl.Height - 2, pnl.Width, 2);
            };
            var lbl = new Label
            {
                Text = title, Font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(120, 100, 80), Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 2),
                BackColor = Color.Transparent
            };
            pnl.Controls.Add(lbl);
            return pnl;
        }

        /// <summary>Crée un panneau séparateur vertical 3px entre colonnes Kanban.</summary>
        private static Panel MakeColumnSeparator()
        {
            var sep = new Panel { Dock = DockStyle.Left, Width = 3, BackColor = Color.Transparent };
            sep.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                int h = sep.Height;
                // Ombre gauche (foncée) → trait central → reflet droit (clair)
                using (var pen = new Pen(Color.FromArgb(40, 80, 60, 40)))
                    g.DrawLine(pen, 0, 0, 0, h);
                using (var pen = new Pen(Color.FromArgb(200, 180, 160)))
                    g.DrawLine(pen, 1, 0, 1, h);
                using (var pen = new Pen(Color.FromArgb(20, 255, 255, 255)))
                    g.DrawLine(pen, 2, 0, 2, h);
            };
            return sep;
        }

        /// <summary>Petit bouton pour la barre d'actions Kanban.</summary>
        private static Button MakeSmallButton(string text, Color bg, Color fg)
        {
            var btn = new Button
            {
                Text = text, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = fg,
                Size = new Size(text.Length * 7 + 20, 28), Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 4, 0)
            };
            btn.FlatAppearance.BorderColor = bg;
            return btn;
        }

        /// <summary>Crée un GraphicsPath rectangulaire à coins arrondis.</summary>
        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var gp = new GraphicsPath();
            gp.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            gp.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            gp.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            gp.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
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
                default:                         return;
            }
            EmbedForm(frm);
        }

        // ShowProductionScreen → déplacé dans FrmPrincipal.Production.cs (partial class)

        /// <summary>
        /// Intègre un formulaire dans le panneau droit sans TopLevel (SFA).
        /// FormClosed déclenche le retour automatique à l'écran précédent.
        /// </summary>
        private void EmbedForm(Form frm)
        {
            _pnlDroit.SuspendLayout();
            ClearAndDisposePanel();
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

        /// <summary>
        /// Vide _pnlDroit ET dispose les contrôles enfants (Forms embarquées, DGVs, Panels).
        /// Sans Dispose, les Forms précédentes resteraient en mémoire avec leurs handlers DAL
        /// et seraient disposées en cascade à la fermeture de l'app (latence visible).
        /// </summary>
        private void ClearAndDisposePanel()
        {
            while (_pnlDroit.Controls.Count > 0)
            {
                var c = _pnlDroit.Controls[0];
                _pnlDroit.Controls.RemoveAt(0);
                c.Dispose();
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Placeholder — module en développement
        // ════════════════════════════════════════════════════════════════

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

        // ════════════════════════════════════════════════════════════════
        //  Menu / Session
        // ════════════════════════════════════════════════════════════════

        // Modules du catalogue Web (Catégories, Parfums, Produits, Commandes) — à venir
        // Connectés à une future intégration avec le site Laravel.
        private void menuCatCategories_Click(object sender, EventArgs e) => PlaceholderWeb();
        private void menuCatParfums_Click(object sender, EventArgs e)    => PlaceholderWeb();
        private void menuCatProduits_Click(object sender, EventArgs e)   => PlaceholderWeb();
        private void menuCommandes_Click(object sender, EventArgs e)     => PlaceholderWeb();

        private void menuFournisseurs_Click(object sender, EventArgs e) =>
            NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Fournisseurs));

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
