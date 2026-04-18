using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Hub principal d'ArtisaStock.
    /// Bandeau supérieur : boutons d'activité générés dynamiquement depuis la DB + bouton ⚙.
    /// Panneau gauche : liste des contextes de l'activité sélectionnée.
    /// Panneau droit  : niveaux de transformation du contexte sélectionné.
    ///
    /// Ergonomie :
    ///  - Activité active = bouton surligné OR (Gestalt similarité)
    ///  - Bouton ⚙ fixé à l'extrémité droite du bandeau (Fitts — position prévisible)
    ///  - Split principal/secondaire φ ≈ 62%/38% selon la largeur de fenêtre
    /// </summary>
    public partial class FrmPrincipal : Form
    {
        private readonly Utilisateur _utilisateur;
        private Activite _activite;                  // activité sélectionnée (null si aucune)

        // ── Couleurs ─────────────────────────────────────────────────
        private static readonly Color CHOCOLAT_FONCE = Color.FromArgb(61,  40,  23);
        private static readonly Color CHOCOLAT_MOYEN = Color.FromArgb(111, 78,  55);
        private static readonly Color CREME          = Color.FromArgb(245, 230, 211);
        private static readonly Color OR             = Color.FromArgb(212, 175, 55);
        private static readonly Color[] NIVEAU_ACCENTS = {
            Color.FromArgb(74,  144, 217),
            Color.FromArgb(92,  184,  92),
            Color.FromArgb(240, 173,  78),
            Color.FromArgb(155,  89, 182),
            Color.FromArgb(52,  152, 219),
        };

        // ── Contrôles du hub ─────────────────────────────────────────
        private SplitContainer  _split;
        private ListBox         _lstActivites;           // liste verticale des activités
        private ListBox         _lstContextes;
        private Label           _lblContexteNom;
        private Label           _lblContexteDesc;
        private FlowLayoutPanel _flowNiveaux;
        private Button          _btnAjouterNiveau;
        private Panel           _pnlActivite;

        public FrmPrincipal(Utilisateur utilisateur)
        {
            _utilisateur = utilisateur;
            InitializeComponent();
        }

        private void FrmPrincipal_Load(object sender, EventArgs e)
        {
            lblUtilisateur.Text = $"Connecté : {_utilisateur}";
            BuildHub();
            ChargerActivites();   // charge les activités dans le rail gauche et sélectionne la 1ère
        }

        // ════════════════════════════════════════════════════════════
        //  Construction du hub (une seule fois au Load)
        // ════════════════════════════════════════════════════════════

        private void BuildHub()
        {
            // ── Bandeau — identité de l'application uniquement ───────────
            _pnlActivite = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = CHOCOLAT_FONCE,
                Padding   = new Padding(12, 8, 8, 8)
            };

            var lblAppNom = new Label
            {
                Text      = "ArtisaStock",
                Font      = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = OR,
                AutoSize  = true,
                Location  = new Point(14, 12)
            };

            // Bouton ⚙ fixé à droite — gestion globale (Fitts + Nielsen #4)
            var btnGerer = new Button
            {
                Text      = "⚙",
                Font      = new Font("Segoe UI", 11F),
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(32, 32),
                Cursor    = Cursors.Hand,
                BackColor = Color.FromArgb(80, 60, 40),
                ForeColor = Color.FromArgb(200, 180, 160)
            };
            btnGerer.FlatAppearance.BorderSize = 0;
            btnGerer.Click += (s, ev) =>
            {
                using (var frm = new FrmActivites())
                    frm.ShowDialog(this);
                ChargerActivites();
            };

            _pnlActivite.Resize += (s, ev) =>
            {
                btnGerer.Location = new Point(_pnlActivite.ClientSize.Width - 40, 8);
            };
            btnGerer.Location = new Point(_pnlActivite.ClientSize.Width - 40, 8);

            _pnlActivite.Controls.Add(lblAppNom);
            _pnlActivite.Controls.Add(btnGerer);

            this.Controls.Add(_pnlActivite);
            _pnlActivite.BringToFront();

            // ── SplitContainer ────────────────────────────────────────
            _split = new SplitContainer
            {
                Dock        = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor   = Color.FromArgb(240, 235, 228)
            };
            this.Controls.Add(_split);
            _split.BringToFront();

            // Ratio φ ≈ 38% gauche / 62% droite
            _split.Panel1MinSize    = 200;
            _split.Panel2MinSize    = 340;
            _split.SplitterDistance = 260;

            BuildLeftPanel();
            BuildRightPanel();
        }

        // ════════════════════════════════════════════════════════════
        //  Activités — liste verticale dans le rail gauche
        // ════════════════════════════════════════════════════════════

        private void ChargerActivites()
        {
            if (_lstActivites == null) return;

            _lstActivites.Items.Clear();

            var activites = ActiviteDAL.GetAll();

            if (activites.Count == 0)
            {
                _activite = null;
                AfficherPanneauVide();
                return;
            }

            foreach (var act in activites)
                _lstActivites.Items.Add(act);

            // Conserver la sélection courante ou prendre la première
            int idx = 0;
            if (_activite != null)
            {
                int found = activites.FindIndex(a => a.Id == _activite.Id);
                if (found >= 0) idx = found;
            }
            _lstActivites.SelectedIndex = idx;   // déclenche SelectedIndexChanged → ChangerActivite
        }

        private void LstActivites_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_lstActivites.SelectedItem is Activite act)
                ChangerActivite(act);
        }

        private void ChangerActivite(Activite activite)
        {
            _activite = activite;
            ChargerContextes();
        }

        // ── Panneau gauche — Ressources + Contextes ──────────────────

        private void BuildLeftPanel()
        {
            _split.Panel1.BackColor = Color.FromArgb(248, 244, 240);

            // ── Section RESSOURCES — header ───────────────────────────
            var pnlHeaderRessources = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = CHOCOLAT_MOYEN };
            pnlHeaderRessources.Controls.Add(new Label
            {
                Text      = "RESSOURCES",
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(12, 0, 0, 0)
            });

            // ── Section RESSOURCES — boutons (grille 8px : 3×32px + marges) ─
            // Hauteur : 8 top + 32 + 4 + 32 + 4 + 32 + 8 bottom = 120px
            var pnlRessources = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 160,
                BackColor = Color.FromArgb(248, 244, 240)
            };

            var btnStocksRail = new Button
            {
                Text      = "📦  Stocks",
                Font      = new Font("Segoe UI", 9.5F),
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(8, 8),
                Size      = new Size(_split.SplitterDistance - 20, 32),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(8, 0, 0, 0),
                BackColor = Color.Transparent,
                ForeColor = CHOCOLAT_FONCE,
                Cursor    = Cursors.Hand
            };
            btnStocksRail.FlatAppearance.BorderSize           = 0;
            btnStocksRail.FlatAppearance.MouseOverBackColor   = Color.FromArgb(232, 222, 208);
            btnStocksRail.Click += (s, ev) => new FrmStocks().ShowDialog(this);

            var btnIngsRail = new Button
            {
                Text      = "🧪  Ingrédients",
                Font      = new Font("Segoe UI", 9.5F),
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(8, 44),
                Size      = new Size(_split.SplitterDistance - 20, 32),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(8, 0, 0, 0),
                BackColor = Color.Transparent,
                ForeColor = CHOCOLAT_FONCE,
                Cursor    = Cursors.Hand
            };
            btnIngsRail.FlatAppearance.BorderSize         = 0;
            btnIngsRail.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 222, 208);
            btnIngsRail.Click += (s, ev) => new FrmIngredients(_activite).ShowDialog(this);

            var btnFournsRail = new Button
            {
                Text      = "🚚  Fournisseurs",
                Font      = new Font("Segoe UI", 9.5F),
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(8, 80),
                Size      = new Size(_split.SplitterDistance - 20, 32),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(8, 0, 0, 0),
                BackColor = Color.Transparent,
                ForeColor = CHOCOLAT_FONCE,
                Cursor    = Cursors.Hand
            };
            btnFournsRail.FlatAppearance.BorderSize         = 0;
            btnFournsRail.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 222, 208);
            btnFournsRail.Click += (s, ev) => new FrmFournisseurs().ShowDialog(this);

            var btnVueStockRail = new Button
            {
                Text      = "📊  Vue stock global",
                Font      = new Font("Segoe UI", 9.5F),
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(8, 120),
                Size      = new Size(_split.SplitterDistance - 20, 32),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(8, 0, 0, 0),
                BackColor = Color.Transparent,
                ForeColor = CHOCOLAT_FONCE,
                Cursor    = Cursors.Hand
            };
            btnVueStockRail.FlatAppearance.BorderSize         = 0;
            btnVueStockRail.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 222, 208);
            btnVueStockRail.Click += (s, ev) => new FrmVueStock().ShowDialog(this);

            pnlRessources.Controls.AddRange(new Control[] { btnStocksRail, btnIngsRail, btnFournsRail, btnVueStockRail });

            // ── Séparateur Ressources → Activités ────────────────────
            var pnlSep1 = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.FromArgb(248, 244, 240) };
            pnlSep1.Paint += (s, ev) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 195, 175)))
                    ev.Graphics.DrawLine(pen, 12, 0, pnlSep1.Width - 12, 0);
            };

            // ── Section ACTIVITÉS — header ────────────────────────────
            var pnlHeaderActivites = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = CHOCOLAT_MOYEN };
            pnlHeaderActivites.Controls.Add(new Label
            {
                Text      = "ACTIVITÉS",
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(12, 0, 0, 0)
            });

            // ── Section ACTIVITÉS — liste + bouton ajout ──────────────
            // Hauteur fixe : 4 items × 34px + 40px bas = 176px
            var pnlActivitesSection = new Panel { Dock = DockStyle.Top, Height = 176, BackColor = Color.FromArgb(248, 244, 240) };

            _lstActivites = new ListBox
            {
                Dock        = DockStyle.Fill,
                Font        = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.None,
                BackColor   = Color.FromArgb(248, 244, 240),
                ItemHeight  = 34,
                DrawMode    = DrawMode.OwnerDrawFixed
            };
            _lstActivites.DrawItem             += LstActivites_DrawItem;
            _lstActivites.SelectedIndexChanged += LstActivites_SelectedIndexChanged;

            var pnlBasActivites = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 40,
                BackColor = Color.FromArgb(235, 228, 220),
                Padding   = new Padding(8, 4, 8, 4)
            };
            var btnNouvelleActivite = new Button
            {
                Text      = "+ Nouvelle activité",
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = CHOCOLAT_FONCE,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height    = 32,
                Width     = 172,
                Location  = new Point(8, 4),
                Cursor    = Cursors.Hand
            };
            btnNouvelleActivite.FlatAppearance.BorderSize = 0;
            btnNouvelleActivite.Click += (s, ev) =>
            {
                using (var frm = new FrmActivites())
                    frm.ShowDialog(this);
                ChargerActivites();
            };
            pnlBasActivites.Controls.Add(btnNouvelleActivite);

            // Ordre strict dans pnlActivitesSection : Fill en premier, Bottom ensuite
            pnlActivitesSection.Controls.Add(_lstActivites);   // Fill
            pnlActivitesSection.Controls.Add(pnlBasActivites); // Bottom

            // ── Séparateur Activités → Contextes ─────────────────────
            var pnlSep2 = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Color.FromArgb(248, 244, 240) };
            pnlSep2.Paint += (s, ev) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 195, 175)))
                    ev.Graphics.DrawLine(pen, 12, 0, pnlSep2.Width - 12, 0);
            };

            // ── Section CONTEXTES — header ────────────────────────────
            var pnlHeaderContextes = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = CHOCOLAT_MOYEN };
            pnlHeaderContextes.Controls.Add(new Label
            {
                Text      = "CONTEXTES",
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(12, 0, 0, 0)
            });

            // ── Liste des contextes (Fill) ────────────────────────────
            _lstContextes = new ListBox
            {
                Dock        = DockStyle.Fill,
                Font        = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.None,
                BackColor   = Color.FromArgb(248, 244, 240),
                ItemHeight  = 34,
                DrawMode    = DrawMode.OwnerDrawFixed
            };
            _lstContextes.DrawItem             += LstContextes_DrawItem;
            _lstContextes.SelectedIndexChanged += LstContextes_SelectedIndexChanged;

            // ── Bas — actions contexte ────────────────────────────────
            var pnlBas = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 48,
                BackColor = Color.FromArgb(235, 228, 220),
                Padding   = new Padding(8, 8, 8, 8)
            };

            var btnNouv = new Button
            {
                Text      = "+ Nouveau contexte",
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = CHOCOLAT_FONCE,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height    = 32,
                Width     = 160,
                Location  = new Point(8, 8),
                Cursor    = Cursors.Hand
            };
            btnNouv.FlatAppearance.BorderSize = 0;
            btnNouv.Click += BtnNouveauContexte_Click;

            var btnModif = new Button
            {
                Text      = "✎",
                Font      = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(200, 190, 178),
                FlatStyle = FlatStyle.Flat,
                Height    = 32, Width = 34,
                Location  = new Point(174, 8),
                Cursor    = Cursors.Hand
            };
            btnModif.FlatAppearance.BorderSize = 0;
            btnModif.Click += BtnModifierContexte_Click;

            var btnSupp = new Button
            {
                Text      = "✕",
                Font      = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(200, 190, 178),
                FlatStyle = FlatStyle.Flat,
                Height    = 32, Width = 34,
                Location  = new Point(212, 8),
                Cursor    = Cursors.Hand
            };
            btnSupp.FlatAppearance.BorderSize = 0;
            btnSupp.Click += BtnSupprimerContexte_Click;

            pnlBas.Controls.AddRange(new Control[] { btnNouv, btnModif, btnSupp });

            // Ordre d'ajout strict (WinForms — traitement en ordre inverse) :
            // Fill en index 0, Bottom ensuite, Tops en ordre inverse de position visuelle
            // (dernier ajouté = premier visuellement)
            _split.Panel1.Controls.Add(_lstContextes);        // Fill
            _split.Panel1.Controls.Add(pnlBas);               // Bottom
            _split.Panel1.Controls.Add(pnlHeaderContextes);   // Top #6
            _split.Panel1.Controls.Add(pnlSep2);              // Top #5
            _split.Panel1.Controls.Add(pnlActivitesSection);  // Top #4
            _split.Panel1.Controls.Add(pnlHeaderActivites);   // Top #3
            _split.Panel1.Controls.Add(pnlSep1);              // Top #2
            _split.Panel1.Controls.Add(pnlRessources);        // Top #1b
            _split.Panel1.Controls.Add(pnlHeaderRessources);  // Top #1a — premier visuellement
        }

        // ── Panneau droit — niveaux du contexte ──────────────────────

        private void BuildRightPanel()
        {
            _split.Panel2.BackColor = Color.White;

            // Ordre d'ajout strict (WinForms docking) : Fill en 1er, Top en dernier
            var pnlScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            _flowNiveaux = new FlowLayoutPanel
            {
                Dock         = DockStyle.Top,
                AutoSize     = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection= FlowDirection.TopDown,
                WrapContents = false,
                BackColor    = Color.White,
                Padding      = new Padding(20, 14, 20, 14)
            };
            pnlScroll.Controls.Add(_flowNiveaux);
            _split.Panel2.Controls.Add(pnlScroll);

            var pnlBas = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.White, Padding = new Padding(20, 8, 20, 8) };
            _btnAjouterNiveau = new Button
            {
                Text      = "+ Ajouter un niveau de transformation",
                Font      = new Font("Segoe UI", 9.5F),
                ForeColor = CHOCOLAT_MOYEN,
                BackColor = Color.FromArgb(245, 240, 234),
                FlatStyle = FlatStyle.Flat,
                Height    = 32,
                Width     = 280,
                Enabled   = false,
                Cursor    = Cursors.Hand
            };
            _btnAjouterNiveau.FlatAppearance.BorderColor = Color.FromArgb(180, 155, 120);
            _btnAjouterNiveau.Click += BtnAjouterNiveau_Click;
            pnlBas.Controls.Add(_btnAjouterNiveau);
            _split.Panel2.Controls.Add(pnlBas);

            var pnlNivTitre = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = Color.White, Padding = new Padding(20, 0, 20, 0) };
            var lblNivTitre = new Label { Text = "NIVEAUX DE TRANSFORMATION", Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = Color.FromArgb(160, 130, 100), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            var sep = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(225, 215, 202) };
            pnlNivTitre.Controls.Add(lblNivTitre);
            pnlNivTitre.Controls.Add(sep);
            _split.Panel2.Controls.Add(pnlNivTitre);

            var pnlInfo = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = CREME, Padding = new Padding(20, 10, 20, 10) };
            _lblContexteNom = new Label { Text = "Sélectionnez ou créez un contexte", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = CHOCOLAT_FONCE, Dock = DockStyle.Top, Height = 28 };
            _lblContexteDesc = new Label { Text = "Utilisez « + Nouveau contexte » pour démarrer", Font = new Font("Segoe UI", 9F, FontStyle.Italic), ForeColor = CHOCOLAT_MOYEN, Dock = DockStyle.Fill };
            pnlInfo.Controls.Add(_lblContexteDesc);
            pnlInfo.Controls.Add(_lblContexteNom);
            _split.Panel2.Controls.Add(pnlInfo);
        }

        // ════════════════════════════════════════════════════════════
        //  Chargement et affichage
        // ════════════════════════════════════════════════════════════

        private void ChargerContextes()
        {
            var ctxActuel = _lstContextes?.SelectedItem as BomContexte;
            _lstContextes.Items.Clear();

            if (_activite == null) { AfficherPanneauVide(); return; }

            foreach (var ctx in BomContexteDAL.GetAll(_activite.Id))
                _lstContextes.Items.Add(ctx);

            // Restaurer la sélection précédente
            if (ctxActuel != null)
                for (int i = 0; i < _lstContextes.Items.Count; i++)
                    if (((BomContexte)_lstContextes.Items[i]).Id == ctxActuel.Id)
                    { _lstContextes.SelectedIndex = i; return; }

            if (_lstContextes.Items.Count > 0)
                _lstContextes.SelectedIndex = 0;
            else
                AfficherPanneauVide();
        }

        private void LstContextes_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ctx = _lstContextes.SelectedItem as BomContexte;
            if (ctx == null) AfficherPanneauVide();
            else             AfficherContexte(ctx);
        }

        private void AfficherPanneauVide()
        {
            _btnAjouterNiveau.Enabled = false;
            _flowNiveaux.Controls.Clear();

            if (_activite == null)
            {
                // ── Onboarding : aucune activité créée ───────────────────
                _lblContexteNom.Text  = "Bienvenue dans ArtisaStock";
                _lblContexteDesc.Text = "Configurez vos ressources, puis créez votre première activité";

                var pnlOnboard = new Panel
                {
                    Width     = Math.Max(_split.Panel2.ClientSize.Width - 80, 400),
                    Height    = 240,
                    BackColor = Color.White,
                    Padding   = new Padding(32, 24, 32, 24),
                    Margin    = new Padding(0, 8, 0, 0)
                };
                pnlOnboard.Paint += (s, e) =>
                {
                    using (var pen = new Pen(Color.FromArgb(210, 190, 165), 1))
                        e.Graphics.DrawRectangle(pen, 0, 0, pnlOnboard.Width - 1, pnlOnboard.Height - 1);
                };

                var lblSteps = new Label
                {
                    Text      = "Pour démarrer, suivez ces 4 étapes :\r\n\r\n" +
                                "  1.  📦  Créez vos stocks physiques  (ex : Réfrigérateur, Cave…)  — panneau gauche\r\n" +
                                "  2.  🧪  Créez vos fiches ingrédients et réceptionnez vos achats  — panneau gauche\r\n" +
                                "  3.  Créez une activité et liez-la à ses stocks  — bouton « + Activité » ci-dessus\r\n" +
                                "  4.  Sélectionnez l'activité → créez un Contexte → ajoutez vos niveaux de transformation",
                    Font      = new Font("Segoe UI", 9.5F),
                    ForeColor = CHOCOLAT_MOYEN,
                    AutoSize  = false,
                    Width     = pnlOnboard.Width - 64,
                    Height    = 136,
                    Location  = new Point(32, 16)
                };
                pnlOnboard.Controls.Add(lblSteps);

                var btnCreer = new Button
                {
                    Text      = "⚡  Créer ma première activité",
                    Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                    BackColor = OR,
                    ForeColor = CHOCOLAT_FONCE,
                    FlatStyle = FlatStyle.Flat,
                    Height    = 40,
                    Width     = 264,
                    Location  = new Point(32, 168),
                    Cursor    = Cursors.Hand
                };
                btnCreer.FlatAppearance.BorderSize = 0;
                btnCreer.Click += (s, e) =>
                {
                    using (var frm = new FrmActivites())
                        frm.ShowDialog(this);
                    ChargerActivites();
                };
                pnlOnboard.Controls.Add(btnCreer);

                _flowNiveaux.Controls.Add(pnlOnboard);
            }
            else
            {
                // ── Activité sélectionnée mais aucun contexte ─────────────
                _lblContexteNom.Text  = "Aucun contexte";
                _lblContexteDesc.Text = $"Créez le premier contexte pour « {_activite.Nom} »";
            }
        }

        private void AfficherContexte(BomContexte ctx)
        {
            _lblContexteNom.Text      = ctx.Nom;
            _lblContexteDesc.Text     = string.IsNullOrWhiteSpace(ctx.Description)
                ? $"Activité : {ctx.ActiviteNom}"
                : ctx.Description;
            _btnAjouterNiveau.Enabled = true;
            _btnAjouterNiveau.Tag     = ctx;

            _flowNiveaux.Controls.Clear();
            foreach (var niv in BomNiveauDAL.GetByContexte(ctx.Id))
                _flowNiveaux.Controls.Add(BuildNiveauCard(niv, ctx));
        }

        // ── Dessin custom ListBoxes ───────────────────────────────────

        private void LstActivites_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var  act      = (Activite)_lstActivites.Items[e.Index];
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var br = new SolidBrush(selected ? CHOCOLAT_MOYEN : Color.FromArgb(248, 244, 240)))
                e.Graphics.FillRectangle(br, e.Bounds);

            if (selected)
                using (var br = new SolidBrush(OR))
                    e.Graphics.FillRectangle(br, new Rectangle(e.Bounds.X, e.Bounds.Y, 4, e.Bounds.Height));

            var font     = new Font("Segoe UI", 10F, selected ? FontStyle.Bold : FontStyle.Regular);
            var couleur  = selected ? Color.White : CHOCOLAT_FONCE;
            var textRect = new Rectangle(e.Bounds.X + 14, e.Bounds.Y, e.Bounds.Width - 14, e.Bounds.Height);
            e.Graphics.DrawString(act.Nom, font, new SolidBrush(couleur), textRect,
                new StringFormat { LineAlignment = StringAlignment.Center });
            font.Dispose();
        }

        private void LstContextes_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var  ctx      = (BomContexte)_lstContextes.Items[e.Index];
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var br = new SolidBrush(selected ? CHOCOLAT_MOYEN : Color.FromArgb(248, 244, 240)))
                e.Graphics.FillRectangle(br, e.Bounds);

            if (selected)
                using (var br = new SolidBrush(OR))
                    e.Graphics.FillRectangle(br, new Rectangle(e.Bounds.X, e.Bounds.Y, 4, e.Bounds.Height));

            var font     = new Font("Segoe UI", 10F, selected ? FontStyle.Bold : FontStyle.Regular);
            var couleur  = selected ? Color.White : CHOCOLAT_FONCE;
            var textRect = new Rectangle(e.Bounds.X + 14, e.Bounds.Y, e.Bounds.Width - 14, e.Bounds.Height);
            e.Graphics.DrawString(ctx.Nom, font, new SolidBrush(couleur), textRect,
                new StringFormat { LineAlignment = StringAlignment.Center });
            font.Dispose();
        }

        // ── Card d'un niveau ─────────────────────────────────────────

        private Panel BuildNiveauCard(BomNiveau niv, BomContexte ctx)
        {
            var accent    = NIVEAU_ACCENTS[Math.Min(niv.Ordre - 1, NIVEAU_ACCENTS.Length - 1)];
            int panelW    = _split.Panel2.ClientSize.Width - 60;
            int cardWidth = Math.Max(panelW, 400);

            var card = new Panel { Width = cardWidth, Height = 96, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10) };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(225, 215, 202)))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using (var br = new SolidBrush(accent))
                    e.Graphics.FillRectangle(br, 0, 0, 5, card.Height);
            };

            var lblTitre = new Label { Text = $"N{niv.Ordre}  ·  {niv.Nom}", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = CHOCOLAT_FONCE, Location = new Point(18, 10), Size = new Size(cardWidth - 120, 24), AutoSize = false };
            var descText = niv.Ordre == 1 ? $"Stock de base — matières premières de {niv.ActiviteNom}" : niv.Description ?? "";
            var lblDesc  = new Label { Text = descText, Font = new Font("Segoe UI", 8.5F, FontStyle.Italic), ForeColor = Color.FromArgb(140, 115, 90), Location = new Point(18, 34), Size = new Size(cardWidth - 120, 18), AutoSize = false };
            var lblBadge = new Label { Text = $"N{niv.Ordre}", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = accent, Location = new Point(cardWidth - 64, 8), Size = new Size(48, 36), TextAlign = ContentAlignment.MiddleCenter };

            card.Controls.AddRange(new Control[] { lblTitre, lblDesc, lblBadge });

            int bx = 18, by = 58;

            if (niv.Ordre == 1)
            {
                bx = AjouterBtnCard(card, "Ingrédients", bx, by, Color.FromArgb(74, 144, 217),
                    () => new FrmIngredients(_activite).ShowDialog());
                AjouterBtnCard(card, "Achats & Lots", bx, by, Color.FromArgb(74, 144, 217),
                    () => new FrmAchats(_activite).ShowDialog());
            }
            else
            {
                var niv2 = niv;
                bx = AjouterBtnCard(card, "Fiches", bx, by, accent,
                    () => { new FrmBomFiches(niv2).ShowDialog(); AfficherContexte(ctx); });
                bx = AjouterBtnCard(card, "Produire", bx, by, Color.FromArgb(80, 160, 70),
                    () => new FrmBomProductionSimulation(ctx, niv2).ShowDialog());

                var btnDel = new Button { Text = "✕", Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(180, 120, 100), BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, Size = new Size(26, 22), Location = new Point(cardWidth - 36, by), Cursor = Cursors.Hand };
                btnDel.FlatAppearance.BorderSize = 0;
                btnDel.Click += (s, e) => SupprimerNiveau(niv2, ctx);
                card.Controls.Add(btnDel);
            }

            return card;
        }

        private int AjouterBtnCard(Panel card, string txt, int x, int y, Color couleur, Action action)
        {
            var btn = new Button
            {
                Text      = txt,
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                BackColor = couleur,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(x, y),
                Height    = 28,
                Width     = txt.Length * 8 + 20,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => action();
            card.Controls.Add(btn);
            return x + btn.Width + 6;
        }

        // ════════════════════════════════════════════════════════════
        //  Actions contextes
        // ════════════════════════════════════════════════════════════

        private void BtnNouveauContexte_Click(object sender, EventArgs e)
        {
            if (_activite == null)
            {
                MessageBox.Show("Sélectionnez d'abord une activité.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var frm = new FrmBomContexteEdit(null, _activite))
                if (frm.ShowDialog() == DialogResult.OK) ChargerContextes();
        }

        private void BtnModifierContexte_Click(object sender, EventArgs e)
        {
            var ctx = _lstContextes.SelectedItem as BomContexte;
            if (ctx == null) { MessageBox.Show("Sélectionnez un contexte.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using (var frm = new FrmBomContexteEdit(ctx, _activite))
                if (frm.ShowDialog() == DialogResult.OK) ChargerContextes();
        }

        private void BtnSupprimerContexte_Click(object sender, EventArgs e)
        {
            var ctx = _lstContextes.SelectedItem as BomContexte;
            if (ctx == null) { MessageBox.Show("Sélectionnez un contexte.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            if (MessageBox.Show($"Supprimer « {ctx.Nom} » et toutes ses données ?\n\nCette action est irréversible.", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            try
            {
                BomContexteDAL.Delete(ctx.Id);
                ChargerContextes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de supprimer : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAjouterNiveau_Click(object sender, EventArgs e)
        {
            var ctx = _lstContextes.SelectedItem as BomContexte;
            if (ctx == null) return;
            var n = new BomNiveau { IdContexte = ctx.Id, Ordre = BomNiveauDAL.GetOrdreMax(ctx.Id) + 1 };
            using (var frm = new FrmBomNiveauEdit(n, false))
                if (frm.ShowDialog() == DialogResult.OK) AfficherContexte(ctx);
        }

        private void SupprimerNiveau(BomNiveau niv, BomContexte ctx)
        {
            if (MessageBox.Show($"Supprimer le niveau « {niv.Nom} » (N{niv.Ordre}) ?\n\nCette action est irréversible.", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            try
            {
                BomNiveauDAL.Delete(niv.Id);
                AfficherContexte(ctx);
            }
            catch (InvalidOperationException ex) { MessageBox.Show(ex.Message, "Impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ── Resize ───────────────────────────────────────────────────

        private void FrmPrincipal_Resize(object sender, EventArgs e)
        {
            var ctx = _lstContextes?.SelectedItem as BomContexte;
            if (ctx != null && _flowNiveaux != null && _split != null)
                AfficherContexte(ctx);
        }

        // ════════════════════════════════════════════════════════════
        //  Menu secondaire
        // ════════════════════════════════════════════════════════════

        private void menuCatCategories_Click(object sender, EventArgs e)  => PlaceholderCatalogueWeb();
        private void menuCatParfums_Click(object sender, EventArgs e)     => PlaceholderCatalogueWeb();
        private void menuCatProduits_Click(object sender, EventArgs e)    => PlaceholderCatalogueWeb();
        private void menuFournisseurs_Click(object sender, EventArgs e)   => new FrmFournisseurs().ShowDialog();
        private void menuCommandes_Click(object sender, EventArgs e)      => PlaceholderCatalogueWeb();

        private static void PlaceholderCatalogueWeb() =>
            MessageBox.Show(
                "Module Catalogue Web — à venir.\n\n" +
                "Cette section sera développée après la finalisation du module ERP\n" +
                "(gestion des stocks, productions et consommations inter-niveaux).",
                "En développement",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

        private void menuDeconnexion_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Se déconnecter ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                new FrmLogin().Show();
                this.Close();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }

        // ════════════════════════════════════════════════════════════
        //  Renderer menu sombre
        // ════════════════════════════════════════════════════════════

        public class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer() : base(new DarkColorTable()) { }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                var rect = new Rectangle(Point.Empty, e.Item.Size);
                using (var br = new SolidBrush((e.Item.Selected || e.Item.Pressed)
                    ? Color.FromArgb(212, 175, 55) : Color.FromArgb(61, 40, 23)))
                    e.Graphics.FillRectangle(br, rect);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = (e.Item.Selected || e.Item.Pressed) ? Color.FromArgb(61, 40, 23) : Color.White;
                base.OnRenderItemText(e);
            }
        }

        private class DarkColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected             => Color.FromArgb(212, 175, 55);
            public override Color MenuItemBorder               => Color.Transparent;
            public override Color MenuBorder                   => Color.FromArgb(80, 55, 30);
            public override Color ToolStripDropDownBackground  => Color.FromArgb(50, 32, 18);
            public override Color ImageMarginGradientBegin     => Color.FromArgb(50, 32, 18);
            public override Color ImageMarginGradientMiddle    => Color.FromArgb(50, 32, 18);
            public override Color ImageMarginGradientEnd       => Color.FromArgb(50, 32, 18);
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(212, 175, 55);
            public override Color MenuItemSelectedGradientEnd   => Color.FromArgb(212, 175, 55);
        }
    }
}
