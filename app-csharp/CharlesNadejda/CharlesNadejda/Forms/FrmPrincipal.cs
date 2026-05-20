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
        private DataGridView           _dgvStock;
        private Label                  _lblStockHeader;
        private Button                 _btnNouveauNiveau;
        private Button                 _btnGererFiches;
        private Button                 _btnAchatRapide;
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

        // ── Contexte screen, niveau selection → voir FrmPrincipal.Contexte.cs ──

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

            // Résoudre le ratio selon le type d'item (Ingredient ou BomStock)
            double? ratio = null;
            decimal? seuilAlerte = null;
            decimal? stockCible = null;
            var item = _dgvStock.Rows[e.RowIndex].DataBoundItem;
            if (item is Ingredient ing)
            {
                ratio = ing.StockRatio;
                seuilAlerte = ing.SeuilAlerteStock;
                stockCible = ing.StockCible;
            }
            else if (item is BomStock bs)
            {
                ratio = bs.StockRatio;
                stockCible = bs.StockCible;
            }
            else return;

            e.Handled = true;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fond de la cellule
            bool sel = (e.State & DataGridViewElementStates.Selected) != 0;
            Color bgColor = sel ? _dgvStock.DefaultCellStyle.SelectionBackColor
                : (e.RowIndex % 2 == 1 ? Color.FromArgb(250, 246, 238) : Color.White);
            using (var br = new SolidBrush(bgColor))
                g.FillRectangle(br, e.CellBounds);

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

            // Couleur selon le ratio
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

            // Marque du seuil d'alerte (ingrédients uniquement)
            if (seuilAlerte.HasValue && stockCible.HasValue && stockCible.Value > 0)
            {
                double seuilRatio = (double)(seuilAlerte.Value / stockCible.Value);
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
                    // Charge les ingrédients pour le stock N1
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

                        // Colonne jauge custom-drawn (identique aux ingrédients)
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

        // ── Actions contextes/niveaux, Kanban helpers → voir FrmPrincipal.Contexte.cs ──

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
                    ? AppColors.Or : AppColors.ChocoBrand))
                    e.Graphics.FillRectangle(br, new Rectangle(Point.Empty, e.Item.Size));
            }
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = (e.Item.Selected || e.Item.Pressed) ? AppColors.ChocoBrand : Color.White;
                base.OnRenderItemText(e);
            }
        }

        private class DarkColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected              => AppColors.Or;
            public override Color MenuItemBorder                => Color.Transparent;
            public override Color MenuBorder                    => Color.FromArgb(80, 55, 30);
            public override Color ToolStripDropDownBackground   => Color.FromArgb(50, 32, 18);
            public override Color ImageMarginGradientBegin      => Color.FromArgb(50, 32, 18);
            public override Color ImageMarginGradientMiddle     => Color.FromArgb(50, 32, 18);
            public override Color ImageMarginGradientEnd        => Color.FromArgb(50, 32, 18);
            public override Color MenuItemSelectedGradientBegin => AppColors.Or;
            public override Color MenuItemSelectedGradientEnd   => AppColors.Or;
        }
    }
}
