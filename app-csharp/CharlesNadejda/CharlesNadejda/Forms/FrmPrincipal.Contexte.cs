using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;
using CharlesNadejda.Navigation;

namespace CharlesNadejda.Forms
{
    partial class FrmPrincipal
    {
        // ════════════════════════════════════════════════════════════════
        //  CONTEXTE SCREEN — Kanban 3 colonnes (Niveaux | Stock | Detail)
        // ════════════════════════════════════════════════════════════════

        private void ShowContexteScreen()
        {
            if (_state.ActiveContexte == null) return;
            _niveauPanels.Clear();
            _dgvStock               = null;
            _lblStockHeader         = null;
            _btnNouveauNiveau       = null;
            _btnGererFiches         = null;
            _btnAchatRapide         = null;
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

            // ── Colonne 2 : Stock (Dock Left, taille auto) ────────────
            var pnlStock = new Panel { Dock = DockStyle.Left, Width = 500, BackColor = CREME_WARM };

            // Header custom avec bouton "Gérer fiches"
            var pnlStockHeader = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = CREME_WARM };
            pnlStockHeader.Paint += (s, ev) =>
            {
                using (var br = new SolidBrush(AppColors.Success))
                    ev.Graphics.FillRectangle(br, 0, pnlStockHeader.Height - 2, pnlStockHeader.Width, 2);
            };
            _lblStockHeader = new Label
            {
                Text = "STOCK", Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 100, 80), Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 2),
                BackColor = Color.Transparent
            };
            _btnGererFiches = MakeSmallButton("📋 Gérer fiches", CHOCO_MED, Color.White);
            _btnGererFiches.Dock = DockStyle.Right;
            _btnGererFiches.Margin = new Padding(0, 2, 8, 2);
            _btnGererFiches.AutoSize = true;
            _btnGererFiches.Enabled = false;
            _btnGererFiches.Click += (s, ev) =>
            {
                var niv = _state.ActiveNiveau;
                if (niv == null) return;
                if (niv.Ordre == 1)
                { using (var frm = new FrmIngredients(new Activite { Id = niv.IdActivite })) frm.ShowDialog(this); }
                else
                { using (var frm = new FrmBomFiches(niv)) frm.ShowDialog(this); }
                ChargerStockNiveau(niv);
            };
            _btnAchatRapide = MakeSmallButton("🛒 Acheter", AppColors.Success, Color.White);
            _btnAchatRapide.Dock = DockStyle.Right;
            _btnAchatRapide.Margin = new Padding(0, 2, 4, 2);
            _btnAchatRapide.AutoSize = true;
            _btnAchatRapide.Enabled = false;
            _btnAchatRapide.Click += (s, ev) =>
            {
                var niv = _state.ActiveNiveau;
                if (niv == null || niv.Ordre != 1) return;
                int idIng = 0;
                if (_dgvStock?.CurrentRow?.DataBoundItem is Ingredient ing)
                    idIng = ing.Id;
                using (var frm = new FrmAchatEdit(null, _state.ActiveActivite?.Id ?? 0, idIng))
                    frm.ShowDialog(this);
                ChargerStockNiveau(niv);
            };

            pnlStockHeader.Controls.Add(_lblStockHeader);
            pnlStockHeader.Controls.Add(_btnAchatRapide);
            pnlStockHeader.Controls.Add(_btnGererFiches);
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
            pnlStock.Controls.Add(pnlStockHeader);

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
            var sep1 = MakeColumnSeparator();  // entre Niveaux et Stock
            var sep2 = MakeColumnSeparator();  // entre Stock et Détail

            // ── Assemblage (WinForms : Fill en 1er, puis Left de droite à gauche) ──
            _pnlDroit.Controls.Add(_pnlKanbanDetail);
            _pnlDroit.Controls.Add(sep2);
            _pnlDroit.Controls.Add(pnlStock);
            _pnlDroit.Controls.Add(sep1);
            _pnlDroit.Controls.Add(colNiveaux);
            _pnlDroit.Controls.Add(pnlHdr);

            // Auto-dimensionner la colonne Stock après le 1er chargement
            var pnlStockRef  = pnlStock;
            var dgvStockRef  = _dgvStock;
            dgvStockRef.DataBindingComplete += (s, ev) =>
            {
                int w = 20;
                foreach (DataGridViewColumn c in dgvStockRef.Columns)
                    if (c.Visible) w += c.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
                pnlStockRef.Width = Math.Max(380, Math.Min(w, 650));
            };

            if (_state.ActiveNiveau != null && _niveauPanels.ContainsKey(_state.ActiveNiveau.Id))
                SelectNiveauRow(_state.ActiveNiveau);
            else if (niveaux.Count > 0)
                SelectNiveauRow(niveaux[0]);

            _pnlDroit.ResumeLayout();
        }

        // ════════════════════════════════════════════════════════════════
        //  Selection niveau (synchronise sidebar <-> droite)
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

            if (_lblStockHeader != null)
                _lblStockHeader.Text = $"STOCK — N{niv.Ordre} {niv.Nom}";
            if (_btnGererFiches != null)
                _btnGererFiches.Enabled = true;
            if (_btnAchatRapide != null)
                _btnAchatRapide.Visible = niv.Ordre == 1;
            ChargerStockNiveau(niv);
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
        //  UI Helpers — Kanban cards & layout
        // ════════════════════════════════════════════════════════════════

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

                // Cercle numero
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
                    { using (var frm = new FrmBomFiches(niv)) frm.ShowDialog(this); ChargerStockNiveau(niv); }
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

        /// <summary>Cree un en-tete de colonne Kanban (32px) avec accent en bas.</summary>
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

        /// <summary>Cree un panneau separateur vertical 3px entre colonnes Kanban.</summary>
        private static Panel MakeColumnSeparator()
        {
            var sep = new Panel { Dock = DockStyle.Left, Width = 3, BackColor = Color.Transparent };
            sep.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                int h = sep.Height;
                // Ombre gauche (foncee) -> trait central -> reflet droit (clair)
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

        /// <summary>Cree un GraphicsPath rectangulaire a coins arrondis.</summary>
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
    }
}
