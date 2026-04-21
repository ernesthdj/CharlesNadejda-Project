using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Tableau de bord ArtisaStock — point d'entrée unique du module BOM.
    /// Affiche les contextes de production à gauche et leurs niveaux à droite
    /// sous forme de cards cliquables. L'utilisateur switche entre contextes
    /// pour charger l'arbre de niveaux correspondant.
    /// </summary>
    public partial class FrmArtisaStock : Form
    {
        private readonly Activite _activite;

        // ── Couleurs projet — voir AppColors (TICKET-12) ─────────────

        // Accent par numéro de niveau
        private static readonly Color[] NIVEAU_ACCENTS = new[]
        {
            Color.FromArgb(74,  144, 217),   // N1 — bleu (stock de base)
            Color.FromArgb(92,  184,  92),   // N2 — vert (1ère transformation)
            Color.FromArgb(240, 173,  78),   // N3 — orange (2e transformation)
            Color.FromArgb(155,  89, 182),   // N4 — violet
            Color.FromArgb(52,  152, 219),   // N5+
        };

        // ── Contrôles principaux (créés programmatiquement) ─────────────
        private SplitContainer   _split;
        private ListBox          _lstContextes;
        private FlowLayoutPanel  _flowNiveaux;
        private Label            _lblContexteNom;
        private Label            _lblContexteDesc;
        private Button           _btnAjouterNiveau;

        public FrmArtisaStock(Activite activite)
        {
            InitializeComponent();
            _activite = activite;
            this.Text = $"ArtisaStock — {activite?.Nom ?? "Production"}";
        }

        private void FrmArtisaStock_Load(object sender, EventArgs e)
        {
            BuildUI();
            ChargerContextes();
        }

        // ── Construction UI ──────────────────────────────────────────────

        private void BuildUI()
        {
            this.BackColor = Color.White;

            // ── Bandeau titre ────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 56,
                BackColor = AppColors.ChocoBrand
            };

            var lblTitre = new Label
            {
                Text      = this.Text,
                Font      = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(18, 0, 0, 0)
            };

            var btnFermer = new Button
            {
                Text      = "✕  Fermer",
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(80, 80, 80),
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(100, 30),
                Dock      = DockStyle.Right,
                Cursor    = Cursors.Hand
            };
            btnFermer.FlatAppearance.BorderSize = 0;
            btnFermer.Click += (s, e) => Close();

            pnlHeader.Controls.Add(lblTitre);
            pnlHeader.Controls.Add(btnFermer);
            this.Controls.Add(pnlHeader);

            // ── SplitContainer ───────────────────────────────────────────
            _split = new SplitContainer
            {
                Dock             = DockStyle.Fill,
                SplitterDistance = 280,
                BorderStyle      = BorderStyle.None,
                Panel1MinSize    = 220,
                Panel2MinSize    = 400,
                BackColor        = Color.FromArgb(240, 240, 240)
            };
            this.Controls.Add(_split);
            pnlHeader.BringToFront();

            BuildLeftPanel();
            BuildRightPanel();
        }

        private void BuildLeftPanel()
        {
            _split.Panel1.BackColor = Color.FromArgb(248, 244, 240);

            // Header gauche
            var pnlLeftHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = AppColors.ChocoMed
            };
            var lblContextesTitre = new Label
            {
                Text      = "CONTEXTES",
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(12, 0, 0, 0)
            };
            pnlLeftHeader.Controls.Add(lblContextesTitre);
            _split.Panel1.Controls.Add(pnlLeftHeader);

            // ListBox contextes
            _lstContextes = new ListBox
            {
                Dock          = DockStyle.Fill,
                Font          = new Font("Segoe UI", 10F),
                BorderStyle   = BorderStyle.None,
                BackColor     = Color.FromArgb(248, 244, 240),
                ItemHeight    = 32,
                DrawMode      = DrawMode.OwnerDrawFixed
            };
            _lstContextes.DrawItem        += LstContextes_DrawItem;
            _lstContextes.SelectedIndexChanged += LstContextes_SelectedIndexChanged;
            _split.Panel1.Controls.Add(_lstContextes);

            // Boutons bas gauche
            var pnlLeftButtons = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 44,
                BackColor = Color.FromArgb(235, 228, 220),
                Padding   = new Padding(8, 6, 8, 6)
            };

            var btnNouveauCtx = new Button
            {
                Text      = "+ Nouveau",
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = AppColors.ChocoBrand,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(90, 30),
                Location  = new Point(8, 6),
                Cursor    = Cursors.Hand
            };
            btnNouveauCtx.FlatAppearance.BorderSize = 0;
            btnNouveauCtx.Click += BtnNouveauContexte_Click;

            var btnModifCtx = new Button
            {
                Text      = "Modifier",
                Font      = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(200, 190, 178),
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(80, 30),
                Location  = new Point(106, 6),
                Cursor    = Cursors.Hand
            };
            btnModifCtx.FlatAppearance.BorderSize = 0;
            btnModifCtx.Click += BtnModifierContexte_Click;

            var btnSuppCtx = new Button
            {
                Text      = "Supprimer",
                Font      = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(200, 190, 178),
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(90, 30),
                Location  = new Point(194, 6),
                Cursor    = Cursors.Hand
            };
            btnSuppCtx.FlatAppearance.BorderSize = 0;
            btnSuppCtx.Click += BtnSupprimerContexte_Click;

            pnlLeftButtons.Controls.AddRange(new Control[] { btnNouveauCtx, btnModifCtx, btnSuppCtx });
            _split.Panel1.Controls.Add(pnlLeftButtons);
        }

        private void BuildRightPanel()
        {
            _split.Panel2.BackColor = Color.White;

            // En-tête du contexte sélectionné
            var pnlContexteInfo = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 80,
                BackColor = AppColors.Creme,
                Padding   = new Padding(20, 12, 20, 12)
            };

            _lblContexteNom = new Label
            {
                Text      = "Sélectionnez un contexte",
                Font      = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AppColors.ChocoBrand,
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 30
            };

            _lblContexteDesc = new Label
            {
                Text      = "ou créez-en un nouveau via le bouton « + Nouveau »",
                Font      = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = AppColors.ChocoMed,
                AutoSize  = false,
                Dock      = DockStyle.Fill
            };

            pnlContexteInfo.Controls.Add(_lblContexteDesc);
            pnlContexteInfo.Controls.Add(_lblContexteNom);
            _split.Panel2.Controls.Add(pnlContexteInfo);

            // Titre section niveaux
            var pnlNiveauxHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 36,
                BackColor = Color.White,
                Padding   = new Padding(20, 0, 20, 0)
            };

            var lblNiveauxTitre = new Label
            {
                Text      = "NIVEAUX DE TRANSFORMATION",
                Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(150, 130, 110),
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var sep = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 1,
                BackColor = Color.FromArgb(220, 210, 200)
            };

            pnlNiveauxHeader.Controls.Add(lblNiveauxTitre);
            pnlNiveauxHeader.Controls.Add(sep);
            _split.Panel2.Controls.Add(pnlNiveauxHeader);

            // Zone scrollable des cards de niveaux
            var scroll = new Panel
            {
                Dock      = DockStyle.Fill,
                AutoScroll= true,
                BackColor = Color.White,
                Padding   = new Padding(20, 10, 20, 10)
            };

            _flowNiveaux = new FlowLayoutPanel
            {
                Dock         = DockStyle.Fill,
                FlowDirection= FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll   = true,
                BackColor    = Color.White
            };

            scroll.Controls.Add(_flowNiveaux);
            _split.Panel2.Controls.Add(scroll);

            // Bouton ajouter niveau
            var pnlBottom = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 50,
                BackColor = Color.White,
                Padding   = new Padding(20, 8, 20, 8)
            };

            _btnAjouterNiveau = new Button
            {
                Text      = "+ Ajouter un niveau de transformation",
                Font      = new Font("Segoe UI", 9.5F),
                ForeColor = AppColors.ChocoMed,
                BackColor = Color.FromArgb(245, 240, 234),
                FlatStyle = FlatStyle.Flat,
                Height    = 34,
                Dock      = DockStyle.Left,
                Width     = 280,
                Cursor    = Cursors.Hand,
                Enabled   = false
            };
            _btnAjouterNiveau.FlatAppearance.BorderColor = AppColors.ChocoMed;
            _btnAjouterNiveau.Click += BtnAjouterNiveau_Click;

            pnlBottom.Controls.Add(_btnAjouterNiveau);
            _split.Panel2.Controls.Add(pnlBottom);

            // Ordre d'empilement (DockStyle.Top se construit de bas en haut)
            pnlNiveauxHeader.BringToFront();
            pnlContexteInfo.BringToFront();
        }

        // ── Dessin custom de la ListBox ──────────────────────────────────

        private void LstContextes_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var  ctx      = (BomContexte)_lstContextes.Items[e.Index];

            using (var br = new SolidBrush(selected ? AppColors.ChocoMed : Color.FromArgb(248, 244, 240)))
                e.Graphics.FillRectangle(br, e.Bounds);

            if (selected)
            {
                using (var br = new SolidBrush(AppColors.Or))
                    e.Graphics.FillRectangle(br, new Rectangle(e.Bounds.X, e.Bounds.Y, 4, e.Bounds.Height));
            }

            using (var br = new SolidBrush(selected ? Color.White : AppColors.ChocoBrand))
            using (var font = new Font("Segoe UI", 10F, selected ? FontStyle.Bold : FontStyle.Regular))
            {
                var textRect = new Rectangle(e.Bounds.X + 14, e.Bounds.Y, e.Bounds.Width - 14, e.Bounds.Height);
                e.Graphics.DrawString(ctx.Nom, font, br, textRect, new StringFormat { LineAlignment = StringAlignment.Center });
            }
        }

        // ── Chargement des données ───────────────────────────────────────

        private void ChargerContextes()
        {
            var contexteActuel = _lstContextes.SelectedItem as BomContexte;

            _lstContextes.Items.Clear();
            foreach (var ctx in BomContexteDAL.GetAll(_activite?.Id ?? 0))
                _lstContextes.Items.Add(ctx);

            // Restaurer la sélection si possible
            if (contexteActuel != null)
            {
                for (int i = 0; i < _lstContextes.Items.Count; i++)
                {
                    if (((BomContexte)_lstContextes.Items[i]).Id == contexteActuel.Id)
                    {
                        _lstContextes.SelectedIndex = i;
                        return;
                    }
                }
            }

            if (_lstContextes.Items.Count > 0)
                _lstContextes.SelectedIndex = 0;
            else
                AfficherRightVide();
        }

        private void LstContextes_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ctx = _lstContextes.SelectedItem as BomContexte;
            if (ctx == null) { AfficherRightVide(); return; }
            AfficherContexte(ctx);
        }

        private void AfficherRightVide()
        {
            _lblContexteNom.Text  = "Aucun contexte";
            _lblContexteDesc.Text = "Créez un contexte via « + Nouveau »";
            _flowNiveaux.Controls.Clear();
            _btnAjouterNiveau.Enabled = false;
        }

        private void AfficherContexte(BomContexte ctx)
        {
            _lblContexteNom.Text  = ctx.Nom;
            _lblContexteDesc.Text = string.IsNullOrWhiteSpace(ctx.Description)
                ? "(aucune description)"
                : ctx.Description;

            _btnAjouterNiveau.Enabled = true;
            _btnAjouterNiveau.Tag     = ctx;

            _flowNiveaux.Controls.Clear();

            var niveaux = BomNiveauDAL.GetByContexte(ctx.Id);
            foreach (var niv in niveaux)
                _flowNiveaux.Controls.Add(BuildNiveauCard(niv, ctx));
        }

        // ── Card d'un niveau ─────────────────────────────────────────────

        private Panel BuildNiveauCard(BomNiveau niv, BomContexte ctx)
        {
            int accentIndex = Math.Min(niv.Ordre - 1, NIVEAU_ACCENTS.Length - 1);
            var accentColor = NIVEAU_ACCENTS[accentIndex];

            int cardWidth = _split.Panel2.ClientSize.Width - 60;

            var card = new Panel
            {
                Width     = Math.Max(cardWidth, 400),
                Height    = niv.Ordre == 1 ? 100 : 110,
                BackColor = Color.White,
                Margin    = new Padding(0, 0, 0, 10),
                Cursor    = Cursors.Default
            };

            // Bordure gauche colorée
            var accent = new Panel
            {
                Width     = 6,
                Height    = card.Height,
                Location  = new Point(0, 0),
                BackColor = accentColor
            };
            card.Controls.Add(accent);

            // Ombre portée simulée (bordure bas droite)
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 210, 200), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            // Titre du niveau
            var lblTitre = new Label
            {
                Text      = $"N{niv.Ordre}  ·  {niv.Nom}",
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.ChocoBrand,
                AutoSize  = false,
                Location  = new Point(20, 12),
                Size      = new Size(cardWidth - 140, 24)
            };
            card.Controls.Add(lblTitre);

            // Description
            string desc = niv.Ordre == 1
                ? "Stock de base — matières premières et ingrédients bruts"
                : string.IsNullOrWhiteSpace(niv.Description) ? "" : niv.Description;

            if (!string.IsNullOrEmpty(desc))
            {
                var lblDesc = new Label
                {
                    Text      = desc,
                    Font      = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(130, 110, 90),
                    AutoSize  = false,
                    Location  = new Point(20, 36),
                    Size      = new Size(cardWidth - 140, 20)
                };
                card.Controls.Add(lblDesc);
            }

            // Badge niveau
            var badge = new Label
            {
                Text      = $"N{niv.Ordre}",
                Font      = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize  = false,
                Location  = new Point(cardWidth - 120, 10),
                Size      = new Size(50, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(badge);

            // Boutons d'action
            int btnTop = niv.Ordre == 1 ? 58 : 65;
            int btnX   = 20;

            if (niv.Ordre == 1)
            {
                // N1 : Ingrédients + Achats
                btnX = AjouterBoutonCard(card, "Ingrédients", btnX, btnTop, Color.FromArgb(74, 144, 217),
                    () => new FrmIngredients(new Activite { Id = ctx.IdActivite, Nom = ctx.ActiviteNom }).ShowDialog());
                btnX = AjouterBoutonCard(card, "Achats & Lots", btnX, btnTop, Color.FromArgb(74, 144, 217),
                    () => new FrmAchats(new Activite { Id = ctx.IdActivite, Nom = ctx.ActiviteNom }).ShowDialog());
            }
            else
            {
                // N2+ : Fiches + Produire + Simuler
                var niv2 = niv; // capture
                btnX = AjouterBoutonCard(card, "Fiches", btnX, btnTop, accentColor,
                    () => OuvrirFiches(niv2));
                btnX = AjouterBoutonCard(card, "Produire", btnX, btnTop, Color.FromArgb(92, 160, 70),
                    () => OuvrirProduction(ctx, niv2));
                btnX = AjouterBoutonCard(card, "Simuler", btnX, btnTop, Color.FromArgb(150, 130, 110),
                    () => OuvrirSimulation(ctx));

                // Bouton supprimer niveau (coin droit)
                var btnSuppNiv = new Button
                {
                    Text      = "✕",
                    Font      = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(160, 120, 100),
                    BackColor = Color.Transparent,
                    FlatStyle = FlatStyle.Flat,
                    Size      = new Size(36, 28),
                    Location  = new Point(cardWidth - 46, btnTop),
                    Cursor    = Cursors.Hand
                };
                btnSuppNiv.FlatAppearance.BorderSize = 0;
                btnSuppNiv.Click += (s, e) => SupprimerNiveau(niv2);
                card.Controls.Add(btnSuppNiv);
            }

            return card;
        }

        private int AjouterBoutonCard(Panel card, string texte, int x, int y, Color couleur, Action action)
        {
            var btn = new Button
            {
                Text      = texte,
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                BackColor = couleur,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(x, y),
                Height    = 28,
                Width     = texte.Length * 8 + 24,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => action();
            card.Controls.Add(btn);
            return x + btn.Width + 8;
        }

        // ── Actions contexte ─────────────────────────────────────────────

        private void BtnNouveauContexte_Click(object sender, EventArgs e)
        {
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

            var r = MessageBox.Show(
                $"Supprimer le contexte « {ctx.Nom} » ?\n\nTous ses niveaux, fiches, stocks et productions seront supprimés.",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (r != DialogResult.Yes) return;

            try
            {
                BomContexteDAL.Delete(ctx.Id);
                ChargerContextes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de supprimer : des données liées existent.\n\n" + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Actions niveau ───────────────────────────────────────────────

        private void BtnAjouterNiveau_Click(object sender, EventArgs e)
        {
            var ctx = _lstContextes.SelectedItem as BomContexte;
            if (ctx == null) return;

            int ordreMax = BomNiveauDAL.GetOrdreMax(ctx.Id);
            var nouveau  = new BomNiveau { IdContexte = ctx.Id, Ordre = ordreMax + 1 };

            using (var frm = new FrmBomNiveauEdit(nouveau, false))
                if (frm.ShowDialog() == DialogResult.OK) AfficherContexte(ctx);
        }

        private void SupprimerNiveau(BomNiveau niv)
        {
            var ctx = _lstContextes.SelectedItem as BomContexte;

            var r = MessageBox.Show(
                $"Supprimer le niveau « {niv.Nom} » (N{niv.Ordre}) ?\n\nLes fiches et stocks de ce niveau seront supprimés.",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (r != DialogResult.Yes) return;

            try
            {
                BomNiveauDAL.Delete(niv.Id);
                if (ctx != null) AfficherContexte(ctx);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Suppression impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de supprimer : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Navigation vers les sous-écrans ─────────────────────────────

        private void OuvrirFiches(BomNiveau niv)
        {
            using (var frm = new FrmBomFiches(niv))
                frm.ShowDialog();

            // Rafraîchir les cards après fermeture (le stock peut avoir changé)
            var ctx = _lstContextes.SelectedItem as BomContexte;
            if (ctx != null) AfficherContexte(ctx);
        }

        private void OuvrirProduction(BomContexte ctx, BomNiveau niv)
        {
            using (var frm = new FrmBomProductionSimulation(ctx, niv))
                frm.ShowDialog();

            var ctxActuel = _lstContextes.SelectedItem as BomContexte;
            if (ctxActuel != null) AfficherContexte(ctxActuel);
        }

        private void OuvrirSimulation(BomContexte ctx)
        {
            using (var frm = new FrmBomProductionSimulation(ctx.IdActivite))
                frm.ShowDialog();
        }
    }
}
