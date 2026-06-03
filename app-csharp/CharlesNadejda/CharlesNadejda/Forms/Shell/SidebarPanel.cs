using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CharlesNadejda.Models;
using CharlesNadejda.Navigation;

namespace CharlesNadejda.Forms.Shell
{
    /// <summary>
    /// Sidebar de navigation ERP (224px, style Odoo).
    /// Contient : ActivitySwitcher en haut, 3 groupes de navigation, version en bas.
    /// </summary>
    internal sealed class SidebarPanel : Panel
    {
        // ── Événements ──────────────────────────────────────────────
        public event Action<NavItemId>    NavigationRequested;
        public event Action<Activite>    ActivityChanged;
        public event Action              ManageActivitiesRequested;
        public event Action              NewContextRequested;
        public event Action<BomContexte> ContextChanged;
        public event Action<BomContexte> EditContextRequested;
        public event Action<BomContexte> DeleteContextRequested;

        // ── Contrôles ───────────────────────────────────────────────
        private readonly ComboBox _cboActivite;
        private readonly ComboBox _cboContexte;
        private readonly Button   _btnNewCtx;
        private readonly Button   _btnEditCtx;
        private readonly Button   _btnDelCtx;
        private readonly Label    _lblCtxLabel;
        private readonly Panel    _pnlNav;
        private readonly Label    _lblVersion;

        private NavItemId _activeItem = NavItemId.Hub;
        private readonly Dictionary<NavItemId, Panel> _navPanels = new Dictionary<NavItemId, Panel>();
        private readonly Dictionary<NavItemId, Label> _badgeLabels = new Dictionary<NavItemId, Label>();

        private const int SIDEBAR_WIDTH = 224;
        private const int ITEM_HEIGHT   = 36;

        private static readonly Color BG_COLOR       = AppColors.ChocoDark;     // #2C1810
        private static readonly Color BG_ACTIVE      = AppColors.SidebarActive; // #43301A
        private static readonly Color BG_HOVER       = AppColors.SidebarHover;  // #372616
        private static readonly Color FG_NORMAL      = Color.FromArgb(215, 245, 230, 211);
        private static readonly Color FG_ACTIVE      = Color.White;
        private static readonly Color FG_SECTION     = Color.FromArgb(100, 245, 230, 211);
        private static readonly Color ACCENT         = AppColors.Or;
        private static readonly Color ACCENT_DIM     = Color.FromArgb(140, AppColors.Or);
        private static readonly Color SEP_COLOR      = Color.FromArgb(20, 245, 230, 211);

        public SidebarPanel()
        {
            Width     = SIDEBAR_WIDTH;
            Dock      = DockStyle.Left;
            BackColor = BG_COLOR;
            AutoScroll = false;
            Padding   = new Padding(0);

            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer, true);

            // ── Container scrollable pour le contenu nav ────────────
            _pnlNav = new Panel
            {
                Dock      = DockStyle.Fill,
                AutoScroll = true,
                BackColor = BG_COLOR
            };

            // ── Bandeau ATELIER + ActivitySwitcher + ContextSwitcher ─
            var pnlTop = new Panel
            {
                Dock      = DockStyle.Top,
                BackColor = BG_COLOR,
                Padding   = new Padding(12, 10, 12, 8)
            };

            int topY = 10;

            // ── Label ATELIER ─────────────────────────────────────
            var lblAtelier = new Label
            {
                Text      = "ATELIER",
                Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = FG_SECTION,
                AutoSize  = true,
                Location  = new Point(12, topY),
                BackColor = Color.Transparent
            };
            pnlTop.Controls.Add(lblAtelier);
            topY += 20;

            // ── ComboBox Activité ─────────────────────────────────
            _cboActivite = new ComboBox
            {
                Location      = new Point(12, topY),
                Size          = new Size(SIDEBAR_WIDTH - 24, 50),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DrawMode      = DrawMode.OwnerDrawFixed,
                ItemHeight    = 40,
                Font          = new Font("Segoe UI", 10F),
                FlatStyle     = FlatStyle.Flat,
                BackColor     = Color.FromArgb(45, 30, 18),
                ForeColor     = AppColors.SidebarTxt
            };
            _cboActivite.DrawItem += CboActivite_DrawItem;
            _cboActivite.SelectedIndexChanged += (s, e) =>
            {
                if (_cboActivite.SelectedItem is Activite a)
                    ActivityChanged?.Invoke(a);
            };
            pnlTop.Controls.Add(_cboActivite);
            topY += 46;

            // ── Bouton gestion activités ──────────────────────────
            var btnGererAct = new Button
            {
                Text = "⚙ Gérer les activités", Font = new Font("Segoe UI", 7.5F),
                FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent,
                ForeColor = ACCENT_DIM, Size = new Size(130, 20),
                Location = new Point(12, topY), Cursor = Cursors.Hand
            };
            btnGererAct.FlatAppearance.BorderSize = 0;
            btnGererAct.FlatAppearance.MouseOverBackColor = BG_HOVER;
            btnGererAct.Click += (s, e) => ManageActivitiesRequested?.Invoke();
            pnlTop.Controls.Add(btnGererAct);
            topY += 28;

            // ── Séparateur ────────────────────────────────────────
            var sep = new Panel
            {
                Location = new Point(12, topY),
                Size     = new Size(SIDEBAR_WIDTH - 24, 1),
                BackColor = Color.FromArgb(40, 245, 230, 211)
            };
            pnlTop.Controls.Add(sep);
            topY += 8;

            // ── Label CONTEXTE ────────────────────────────────────
            _lblCtxLabel = new Label
            {
                Text      = "CONTEXTE",
                Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = FG_SECTION,
                AutoSize  = true,
                Location  = new Point(12, topY),
                BackColor = Color.Transparent
            };
            pnlTop.Controls.Add(_lblCtxLabel);
            topY += 18;

            // ── ComboBox Contexte ─────────────────────────────────
            _cboContexte = new ComboBox
            {
                Location      = new Point(12, topY),
                Size          = new Size(SIDEBAR_WIDTH - 24, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 9F),
                FlatStyle     = FlatStyle.Flat,
                BackColor     = Color.FromArgb(45, 30, 18),
                ForeColor     = AppColors.SidebarTxt
            };
            _cboContexte.SelectedIndexChanged += (s, e) =>
            {
                if (_cboContexte.SelectedItem is BomContexte ctx)
                    ContextChanged?.Invoke(ctx);
            };
            pnlTop.Controls.Add(_cboContexte);
            topY += 28;

            // ── Boutons contexte : + ✎ ✕ ──────────────────────────
            int btnX = 12;
            _btnNewCtx = MakeSidebarMiniButton("+", ACCENT, btnX);
            _btnNewCtx.Click += (s, e) => NewContextRequested?.Invoke();
            pnlTop.Controls.Add(_btnNewCtx);
            _btnNewCtx.Location = new Point(btnX, topY);
            btnX += 30;

            _btnEditCtx = MakeSidebarMiniButton("✎", ACCENT_DIM, btnX);
            _btnEditCtx.Click += (s, e) =>
            {
                if (_cboContexte.SelectedItem is BomContexte ctx)
                    EditContextRequested?.Invoke(ctx);
            };
            pnlTop.Controls.Add(_btnEditCtx);
            _btnEditCtx.Location = new Point(btnX, topY);
            btnX += 30;

            _btnDelCtx = MakeSidebarMiniButton("✕", Color.FromArgb(180, AppColors.RedCrit), btnX);
            _btnDelCtx.Click += (s, e) =>
            {
                if (_cboContexte.SelectedItem is BomContexte ctx)
                    DeleteContextRequested?.Invoke(ctx);
            };
            pnlTop.Controls.Add(_btnDelCtx);
            _btnDelCtx.Location = new Point(btnX, topY);
            topY += 26;

            pnlTop.Height = topY + 6;

            // ── Version en bas ──────────────────────────────────────
            var pnlBottom = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 32,
                BackColor = BG_COLOR,
                Padding   = new Padding(14, 0, 14, 0)
            };
            pnlBottom.Paint += (s, e) =>
            {
                using (var pen = new Pen(SEP_COLOR))
                    e.Graphics.DrawLine(pen, 0, 0, pnlBottom.Width, 0);
            };

            _lblVersion = new Label
            {
                Text      = "● v1.0",
                Font      = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(130, 245, 230, 211),
                AutoSize  = true,
                Location  = new Point(14, 9),
                BackColor = Color.Transparent
            };
            pnlBottom.Controls.Add(_lblVersion);

            // ── Sections de navigation ──────────────────────────────
            BuildNavSections();

            // ── Assemblage ──────────────────────────────────────────
            Controls.Add(_pnlNav);       // Fill
            Controls.Add(pnlBottom);     // Bottom
            Controls.Add(pnlTop);        // Top

            // Bordure droite
            Paint += (s, e) =>
            {
                using (var pen = new Pen(AppColors.ChocoAbyss))
                    e.Graphics.DrawLine(pen, Width - 1, 0, Width - 1, Height);
            };
        }

        // ── API publique ────────────────────────────────────────────

        public void SetActivities(List<Activite> activities)
        {
            _cboActivite.Items.Clear();
            foreach (var a in activities)
                _cboActivite.Items.Add(a);
            if (_cboActivite.Items.Count > 0)
                _cboActivite.SelectedIndex = 0;
        }

        public void SetSelectedActivity(Activite act)
        {
            if (act == null) return;
            for (int i = 0; i < _cboActivite.Items.Count; i++)
            {
                if (_cboActivite.Items[i] is Activite a && a.Id == act.Id)
                { _cboActivite.SelectedIndex = i; return; }
            }
        }

        public void SetContextes(List<BomContexte> contextes)
        {
            _cboContexte.SelectedIndexChanged -= CboContexte_OnChange;
            _cboContexte.Items.Clear();
            if (contextes != null)
                foreach (var c in contextes)
                    _cboContexte.Items.Add(c);
            _cboContexte.SelectedIndexChanged += CboContexte_OnChange;

            bool hasItems = _cboContexte.Items.Count > 0;
            _btnEditCtx.Enabled = hasItems;
            _btnDelCtx.Enabled  = hasItems;
        }

        public void SetSelectedContext(BomContexte ctx)
        {
            if (ctx == null) { _cboContexte.SelectedIndex = -1; return; }
            _cboContexte.SelectedIndexChanged -= CboContexte_OnChange;
            for (int i = 0; i < _cboContexte.Items.Count; i++)
            {
                if (_cboContexte.Items[i] is BomContexte c && c.Id == ctx.Id)
                { _cboContexte.SelectedIndex = i; break; }
            }
            _cboContexte.SelectedIndexChanged += CboContexte_OnChange;
        }

        private void CboContexte_OnChange(object s, EventArgs e)
        {
            if (_cboContexte.SelectedItem is BomContexte ctx)
                ContextChanged?.Invoke(ctx);
        }

        public void SetActiveItem(NavItemId id)
        {
            _activeItem = id;
            foreach (var kv in _navPanels)
                kv.Value.Invalidate();
        }

        public void SetBadge(NavItemId id, string text)
        {
            if (_badgeLabels.TryGetValue(id, out var lbl))
            {
                lbl.Text    = text ?? "";
                lbl.Visible = !string.IsNullOrEmpty(text);
            }
        }

        // ── Helpers ────────────────────────────────────────────────

        private static Button MakeSidebarMiniButton(string text, Color fg, int x)
        {
            var btn = new Button
            {
                Text = text, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent,
                ForeColor = fg, Size = new Size(26, 22),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = BG_HOVER;
            return btn;
        }

        // ── Construction des sections nav ───────────────────────────

        private void BuildNavSections()
        {
            int y = 0;

            y = AddSection(y, "WORKFLOW");
            y = AddNavItem(y, NavItemId.Hub,             "⊕", "Hub");
            y = AddNavItem(y, NavItemId.Production,      "▶", "Production");
            y = AddNavItem(y, NavItemId.Planning,        "◷", "Planning");
            y = AddNavItem(y, NavItemId.DevisPatisserie, "◬", "Devis pâtisserie");

            y = AddSection(y, "STOCK & ACHATS", "＋ Contexte", () => NewContextRequested?.Invoke());
            y = AddNavItem(y, NavItemId.StocksLiaisons,  "📦", "Stocks & liaisons");
            y = AddNavItem(y, NavItemId.VueStockGlobal,  "▦", "Vue stock global");
            y = AddNavItem(y, NavItemId.Mouvements,      "◰", "Mouvements");
            y = AddNavItem(y, NavItemId.AchatsLots,      "◧", "Achats & lots");
            y = AddNavItem(y, NavItemId.Fournisseurs,    "◐", "Fournisseurs");

            y = AddSection(y, "RÉFÉRENTIELS");
            y = AddNavItem(y, NavItemId.Ingredients,      "○", "Ingrédients");
            y = AddNavItem(y, NavItemId.NiveauxContextes, "◫", "Niveaux & contextes");
            y = AddNavItem(y, NavItemId.Parametres,       "∷", "Paramètres");

            y = AddSection(y, "BOUTIQUE EN LIGNE");
            y = AddNavItem(y, NavItemId.BoutiqueWeb,      "\U0001f6d2", "Boutique web");
        }

        private int AddSection(int y, string title, string actionText = null, Action actionClick = null)
        {
            var lbl = new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = FG_SECTION,
                AutoSize  = false,
                Location  = new Point(14, y + 14),
                Size      = new Size(actionText != null ? SIDEBAR_WIDTH - 90 : SIDEBAR_WIDTH - 28, 16),
                BackColor = Color.Transparent
            };
            _pnlNav.Controls.Add(lbl);

            if (actionText != null && actionClick != null)
            {
                var btn = new Button
                {
                    Text      = actionText,
                    Font      = new Font("Segoe UI", 7F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent,
                    ForeColor = ACCENT,
                    Size      = new Size(76, 18),
                    Location  = new Point(SIDEBAR_WIDTH - 88, y + 12),
                    Cursor    = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = BG_HOVER;
                btn.Click += (s, e) => actionClick();
                _pnlNav.Controls.Add(btn);
            }

            return y + 34;
        }

        private int AddNavItem(int y, NavItemId id, string icon, string label)
        {
            var pnl = new Panel
            {
                Location  = new Point(0, y),
                Size      = new Size(SIDEBAR_WIDTH, ITEM_HEIGHT),
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand,
                Tag       = id
            };

            // ── Badge (pill) ────────────────────────────────────────
            var lblBadge = new Label
            {
                AutoSize  = false,
                Size      = new Size(26, 18),
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = AppColors.ChocoDark,
                BackColor = ACCENT,
                Visible   = false,
                Location  = new Point(SIDEBAR_WIDTH - 44, 9)
            };
            lblBadge.Paint += (s, e) =>
            {
                // Arrondi pill
                var r = new Rectangle(0, 0, lblBadge.Width - 1, lblBadge.Height - 1);
                using (var path = RoundedRectPath(r, 9))
                using (var brush = new SolidBrush(lblBadge.BackColor))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    e.Graphics.FillPath(brush, path);
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    using (var f = lblBadge.Font)
                        e.Graphics.DrawString(lblBadge.Text, f, new SolidBrush(lblBadge.ForeColor), r, sf);
                }
            };
            pnl.Controls.Add(lblBadge);
            _badgeLabels[id] = lblBadge;

            // ── Custom paint ────────────────────────────────────────
            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.HighQuality;
                bool active = (NavItemId)pnl.Tag == _activeItem;

                // Fond
                if (active)
                {
                    using (var brush = new SolidBrush(BG_ACTIVE))
                        g.FillRectangle(brush, pnl.ClientRectangle);
                }

                // Barre dorée à gauche
                if (active)
                {
                    using (var brush = new SolidBrush(ACCENT))
                        g.FillRectangle(brush, 0, 0, 3, ITEM_HEIGHT);
                }

                // Icône
                using (var fIcon = new Font("Segoe UI", 11F))
                {
                    var iconColor = active ? ACCENT : ACCENT_DIM;
                    using (var brush = new SolidBrush(iconColor))
                        g.DrawString(icon, fIcon, brush, 11, 8);
                }

                // Label
                using (var fLabel = new Font("Segoe UI", 9.5F, active ? FontStyle.Bold : FontStyle.Regular))
                {
                    var labelColor = active ? FG_ACTIVE : FG_NORMAL;
                    using (var brush = new SolidBrush(labelColor))
                        g.DrawString(label, fLabel, brush, 34, 9);
                }
            };

            // ── Hover ───────────────────────────────────────────────
            pnl.MouseEnter += (s, e) =>
            {
                if ((NavItemId)pnl.Tag != _activeItem)
                    pnl.BackColor = BG_HOVER;
            };
            pnl.MouseLeave += (s, e) =>
            {
                pnl.BackColor = Color.Transparent;
            };

            // ── Click ───────────────────────────────────────────────
            pnl.Click += (s, e) => NavigationRequested?.Invoke((NavItemId)pnl.Tag);
            // Propagation click depuis les enfants
            lblBadge.Click += (s, e) => NavigationRequested?.Invoke(id);

            _pnlNav.Controls.Add(pnl);
            _navPanels[id] = pnl;

            return y + ITEM_HEIGHT;
        }

        // ── ComboBox OwnerDraw pour ActivitySwitcher ────────────────

        private void CboActivite_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            bool selected = (e.State & DrawItemState.Selected) != 0;
            var bg = selected ? BG_ACTIVE : Color.FromArgb(45, 30, 18);
            using (var brush = new SolidBrush(bg))
                g.FillRectangle(brush, e.Bounds);

            var act = _cboActivite.Items[e.Index] as Activite;
            if (act == null) return;

            string emoji = "🍫";
            string nom   = act.Nom;

            // Emoji
            using (var f = new Font("Segoe UI", 12F))
                g.DrawString(emoji, f, Brushes.White, e.Bounds.X + 8, e.Bounds.Y + 10);

            // Nom activité
            using (var fNom = new Font("Segoe UI", 10F, FontStyle.Bold))
                g.DrawString(nom, fNom,
                    new SolidBrush(AppColors.SidebarTxt),
                    e.Bounds.X + 36, e.Bounds.Y + 6);

            // Sous-texte — description de l'activité (ou rien)
            var subText = act.Description ?? "";
            if (subText.Length > 40) subText = subText.Substring(0, 37) + "…";
            if (!string.IsNullOrWhiteSpace(subText))
            {
                using (var fSub = new Font("Segoe UI", 8F))
                    g.DrawString(subText, fSub,
                        new SolidBrush(Color.FromArgb(130, 245, 230, 211)),
                        e.Bounds.X + 36, e.Bounds.Y + 24);
            }

            // Bordure
            using (var pen = new Pen(Color.FromArgb(60, AppColors.Or)))
                g.DrawRectangle(pen, e.Bounds.X + 4, e.Bounds.Y + 2,
                    e.Bounds.Width - 8, e.Bounds.Height - 4);
        }

        private static GraphicsPath RoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
