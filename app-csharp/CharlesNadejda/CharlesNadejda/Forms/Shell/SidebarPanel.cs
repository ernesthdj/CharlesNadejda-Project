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
    /// C'est le panneau latéral gauche de l'application — il ne change jamais,
    /// seul le contenu principal (à droite) est swappé par le ScreenRouter.
    /// Tout le rendu est fait en custom paint (owner-draw) pour le look chocolat.
    /// </summary>
    internal sealed class SidebarPanel : Panel
    {
        // ── Événements ──────────────────────────────────────────────
        // Ces events sont écoutés par le MainForm qui orchestre la navigation.
        // Le SidebarPanel ne navigue pas lui-même — il se contente de signaler.

        // Déclenché quand l'utilisateur clique sur un item de navigation (Hub, Production, etc.)
        public event Action<NavItemId>    NavigationRequested;

        // Déclenché quand l'utilisateur change d'activité dans le ComboBox du haut
        public event Action<Activite>    ActivityChanged;

        // Déclenché quand l'utilisateur clique sur "Gérer les activités"
        public event Action              ManageActivitiesRequested;

        // Déclenché quand l'utilisateur clique sur le bouton "+" pour créer un nouveau contexte
        public event Action              NewContextRequested;

        // Déclenché quand l'utilisateur change de contexte dans le ComboBox contexte
        public event Action<BomContexte> ContextChanged;

        // Déclenché quand l'utilisateur clique sur le bouton "✎" pour éditer un contexte
        public event Action<BomContexte> EditContextRequested;

        // Déclenché quand l'utilisateur clique sur le bouton "✕" pour supprimer un contexte
        public event Action<BomContexte> DeleteContextRequested;

        // ── Contrôles ───────────────────────────────────────────────
        // ComboBox pour switcher d'activité (ex: "Chocolaterie", "Pâtisserie")
        private readonly ComboBox _cboActivite;

        // ComboBox pour switcher de contexte BOM (ex: "Tablettes", "Bonbons")
        private readonly ComboBox _cboContexte;

        // Boutons CRUD pour les contextes : Nouveau, Modifier, Supprimer
        private readonly Button   _btnNewCtx;
        private readonly Button   _btnEditCtx;
        private readonly Button   _btnDelCtx;

        // Label "CONTEXTE" au-dessus du ComboBox contexte
        private readonly Label    _lblCtxLabel;

        // Panel scrollable qui contient tous les items de navigation
        private readonly Panel    _pnlNav;

        // Label de version tout en bas de la sidebar ("● v1.0")
        private readonly Label    _lblVersion;

        // L'item de navigation actuellement actif (surligné en doré)
        private NavItemId _activeItem = NavItemId.Hub;

        // Dictionnaires pour retrouver rapidement un panel ou un badge par NavItemId
        // _navPanels : chaque item de nav est un Panel avec du custom paint
        private readonly Dictionary<NavItemId, Panel> _navPanels = new Dictionary<NavItemId, Panel>();

        // _badgeLabels : les petits compteurs "pill" à droite des items (ex: "3" achats en attente)
        private readonly Dictionary<NavItemId, Label> _badgeLabels = new Dictionary<NavItemId, Label>();

        // Dimensions fixes de la sidebar — 224px de large, chaque item fait 36px de haut
        private const int SIDEBAR_WIDTH = 224;
        private const int ITEM_HEIGHT   = 36;

        // ── Palette de couleurs sidebar ─────────────────────────────
        // Tout est en tons chocolat/or pour coller au branding Charles & Nadejda
        private static readonly Color BG_COLOR       = AppColors.ChocoDark;     // #2C1810 — fond principal
        private static readonly Color BG_ACTIVE      = AppColors.SidebarActive; // #43301A — fond de l'item actif
        private static readonly Color BG_HOVER       = AppColors.SidebarHover;  // #372616 — fond au survol
        private static readonly Color FG_NORMAL      = Color.FromArgb(215, 245, 230, 211); // texte normal (semi-transparent)
        private static readonly Color FG_ACTIVE      = Color.White;             // texte de l'item actif
        private static readonly Color FG_SECTION     = Color.FromArgb(100, 245, 230, 211); // titres de section (discret)
        private static readonly Color ACCENT         = AppColors.Or;            // doré — couleur d'accent principale
        private static readonly Color ACCENT_DIM     = Color.FromArgb(140, AppColors.Or);  // doré atténué
        private static readonly Color SEP_COLOR      = Color.FromArgb(20, 245, 230, 211);  // séparateurs très subtils

        // Constructeur — je construis tout le layout en code (pas de Designer)
        // L'ordre d'ajout au Controls est important pour le Dock : Fill d'abord, puis Bottom, puis Top
        public SidebarPanel()
        {
            Width     = SIDEBAR_WIDTH;
            Dock      = DockStyle.Left;
            BackColor = BG_COLOR;
            AutoScroll = false;
            Padding   = new Padding(0);

            // Double-buffering pour éviter le flickering pendant le scroll/repaint
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer, true);

            // ── Container scrollable pour le contenu nav ────────────
            // Ce panel reçoit tous les items de navigation (Hub, Production, etc.)
            // Il prend tout l'espace restant (Dock.Fill) et scroll si nécessaire
            _pnlNav = new Panel
            {
                Dock      = DockStyle.Fill,
                AutoScroll = true,
                BackColor = BG_COLOR
            };

            // ── Bandeau ATELIER + ActivitySwitcher + ContextSwitcher ─
            // Panel docké en haut, contient les deux ComboBox et les boutons contexte
            var pnlTop = new Panel
            {
                Dock      = DockStyle.Top,
                BackColor = BG_COLOR,
                Padding   = new Padding(12, 10, 12, 8)
            };

            // topY me sert de curseur vertical pour empiler les contrôles
            int topY = 10;

            // ── Label ATELIER ─────────────────────────────────────
            // Titre de section en haut de la sidebar — tout en majuscules, discret
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
            // DropDownList = pas de saisie libre, seulement sélection
            // OwnerDrawFixed = je dessine moi-même chaque item (emoji + nom + description)
            // ItemHeight = 40px pour avoir de la place pour 2 lignes de texte
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
            // Je branche mon handler de rendu custom pour les items du dropdown
            _cboActivite.DrawItem += CboActivite_DrawItem;
            // Quand l'utilisateur change d'activité, je fire l'event vers le MainForm
            _cboActivite.SelectedIndexChanged += (s, e) =>
            {
                if (_cboActivite.SelectedItem is Activite a)
                    ActivityChanged?.Invoke(a);
            };
            pnlTop.Controls.Add(_cboActivite);
            topY += 46;

            // ── Bouton gestion activités ──────────────────────────
            // Petit lien discret "⚙ Gérer les activités" sous le ComboBox
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
            // Fine ligne horizontale entre le bloc Activité et le bloc Contexte
            var sep = new Panel
            {
                Location = new Point(12, topY),
                Size     = new Size(SIDEBAR_WIDTH - 24, 1),
                BackColor = Color.FromArgb(40, 245, 230, 211)
            };
            pnlTop.Controls.Add(sep);
            topY += 8;

            // ── Label CONTEXTE ────────────────────────────────────
            // Titre de section pour le bloc contexte BOM
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
            // Sélection du contexte BOM actif (ex: "Tablettes", "Pralinés")
            // Pas d'OwnerDraw ici, le rendu par défaut suffit
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
            // Quand l'utilisateur change de contexte, je notifie le MainForm
            _cboContexte.SelectedIndexChanged += (s, e) =>
            {
                if (_cboContexte.SelectedItem is BomContexte ctx)
                    ContextChanged?.Invoke(ctx);
            };
            pnlTop.Controls.Add(_cboContexte);
            topY += 28;

            // ── Boutons contexte : + ✎ ✕ ──────────────────────────
            // Trois mini-boutons alignés horizontalement pour le CRUD contexte
            int btnX = 12;

            // Bouton "+" — créer un nouveau contexte
            _btnNewCtx = MakeSidebarMiniButton("+", ACCENT, btnX);
            _btnNewCtx.Click += (s, e) => NewContextRequested?.Invoke();
            pnlTop.Controls.Add(_btnNewCtx);
            _btnNewCtx.Location = new Point(btnX, topY);
            btnX += 30;

            // Bouton "✎" — modifier le contexte sélectionné
            _btnEditCtx = MakeSidebarMiniButton("✎", ACCENT_DIM, btnX);
            _btnEditCtx.Click += (s, e) =>
            {
                if (_cboContexte.SelectedItem is BomContexte ctx)
                    EditContextRequested?.Invoke(ctx);
            };
            pnlTop.Controls.Add(_btnEditCtx);
            _btnEditCtx.Location = new Point(btnX, topY);
            btnX += 30;

            // Bouton "✕" — supprimer le contexte sélectionné (rouge atténué)
            _btnDelCtx = MakeSidebarMiniButton("✕", Color.FromArgb(180, AppColors.RedCrit), btnX);
            _btnDelCtx.Click += (s, e) =>
            {
                if (_cboContexte.SelectedItem is BomContexte ctx)
                    DeleteContextRequested?.Invoke(ctx);
            };
            pnlTop.Controls.Add(_btnDelCtx);
            _btnDelCtx.Location = new Point(btnX, topY);
            topY += 26;

            // Je fixe la hauteur du panel top en fonction du curseur topY
            pnlTop.Height = topY + 6;

            // ── Version en bas ──────────────────────────────────────
            // Petit panel docké en bas avec le numéro de version
            var pnlBottom = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 32,
                BackColor = BG_COLOR,
                Padding   = new Padding(14, 0, 14, 0)
            };
            // Ligne séparatrice en haut du panel bottom (custom paint)
            pnlBottom.Paint += (s, e) =>
            {
                using (var pen = new Pen(SEP_COLOR))
                    e.Graphics.DrawLine(pen, 0, 0, pnlBottom.Width, 0);
            };

            // Label "● v1.0" — le point vert indique que l'app est connectée
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
            // BuildNavSections() crée tous les items cliquables (Hub, Production, Planning, etc.)
            BuildNavSections();

            // ── Assemblage ──────────────────────────────────────────
            // L'ordre est crucial pour le Dock : Fill en premier, puis Bottom, puis Top
            // WinForms dock de bas en haut dans l'ordre d'ajout
            Controls.Add(_pnlNav);       // Fill — prend tout l'espace restant
            Controls.Add(pnlBottom);     // Bottom — version en bas
            Controls.Add(pnlTop);        // Top — activité + contexte en haut

            // Bordure droite de la sidebar — fine ligne sombre pour séparer du contenu principal
            Paint += (s, e) =>
            {
                using (var pen = new Pen(AppColors.ChocoAbyss))
                    e.Graphics.DrawLine(pen, Width - 1, 0, Width - 1, Height);
            };
        }

        // ── API publique ────────────────────────────────────────────

        // SetActivities() — remplit le ComboBox activité avec la liste des activités
        // Appelé par le MainForm au démarrage et après un CRUD activité
        public void SetActivities(List<Activite> activities)
        {
            _cboActivite.Items.Clear();
            foreach (var a in activities)
                _cboActivite.Items.Add(a);
            // Sélection automatique du premier item si la liste n'est pas vide
            if (_cboActivite.Items.Count > 0)
                _cboActivite.SelectedIndex = 0;
        }

        // SetSelectedActivity() — sélectionne une activité spécifique dans le ComboBox
        // Recherche par Id pour éviter les problèmes de référence d'objet
        public void SetSelectedActivity(Activite act)
        {
            if (act == null) return;
            for (int i = 0; i < _cboActivite.Items.Count; i++)
            {
                if (_cboActivite.Items[i] is Activite a && a.Id == act.Id)
                { _cboActivite.SelectedIndex = i; return; }
            }
        }

        // SetContextes() — remplit le ComboBox contexte avec la liste des contextes BOM
        // Je détache/reattache l'event SelectedIndexChanged pour éviter de fire un faux changement
        // pendant le remplissage (sinon ça déclencherait une navigation parasite)
        public void SetContextes(List<BomContexte> contextes)
        {
            _cboContexte.SelectedIndexChanged -= CboContexte_OnChange;
            _cboContexte.Items.Clear();
            if (contextes != null)
                foreach (var c in contextes)
                    _cboContexte.Items.Add(c);
            _cboContexte.SelectedIndexChanged += CboContexte_OnChange;

            // Désactiver les boutons Edit/Delete s'il n'y a aucun contexte
            bool hasItems = _cboContexte.Items.Count > 0;
            _btnEditCtx.Enabled = hasItems;
            _btnDelCtx.Enabled  = hasItems;
        }

        // SetSelectedContext() — sélectionne un contexte spécifique sans fire l'event
        // Même pattern que SetContextes : je détache l'event pendant la manipulation
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

        // Handler intermédiaire pour le changement de contexte — fire l'event ContextChanged
        private void CboContexte_OnChange(object s, EventArgs e)
        {
            if (_cboContexte.SelectedItem is BomContexte ctx)
                ContextChanged?.Invoke(ctx);
        }

        // SetActiveItem() — met en surbrillance l'item de navigation actif
        // Invalide tous les panels nav pour forcer un repaint (la barre dorée et le fond changent)
        public void SetActiveItem(NavItemId id)
        {
            _activeItem = id;
            foreach (var kv in _navPanels)
                kv.Value.Invalidate();
        }

        // SetBadge() — affiche ou cache un badge compteur sur un item de navigation
        // Ex: SetBadge(NavItemId.AchatsLots, "3") affiche "3" à côté de "Achats & lots"
        public void SetBadge(NavItemId id, string text)
        {
            if (_badgeLabels.TryGetValue(id, out var lbl))
            {
                lbl.Text    = text ?? "";
                lbl.Visible = !string.IsNullOrEmpty(text);
            }
        }

        // ── Helpers ────────────────────────────────────────────────

        // MakeSidebarMiniButton() — fabrique un petit bouton flat 26x22px pour la sidebar
        // Utilisé pour les boutons +, ✎ et ✕ du bloc contexte
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

        // BuildNavSections() — construit tous les groupes et items de navigation
        // Chaque groupe a un titre de section (WORKFLOW, STOCK & ACHATS, etc.)
        // et chaque item a une icône, un label et un badge optionnel
        private void BuildNavSections()
        {
            int y = 0;

            // Groupe WORKFLOW — les écrans principaux de production
            y = AddSection(y, "WORKFLOW");
            y = AddNavItem(y, NavItemId.Hub,             "⊕", "Hub");
            y = AddNavItem(y, NavItemId.Production,      "▶", "Production");
            y = AddNavItem(y, NavItemId.Planning,        "◷", "Planning");
            y = AddNavItem(y, NavItemId.DevisPatisserie, "◬", "Devis pâtisserie");

            // Groupe STOCK & ACHATS — avec un bouton "＋ Contexte" en action rapide
            y = AddSection(y, "STOCK & ACHATS", "＋ Contexte", () => NewContextRequested?.Invoke());
            y = AddNavItem(y, NavItemId.StocksLiaisons,  "📦", "Stocks & liaisons");
            y = AddNavItem(y, NavItemId.VueStockGlobal,  "▦", "Vue stock global");
            y = AddNavItem(y, NavItemId.Mouvements,      "◰", "Mouvements");
            y = AddNavItem(y, NavItemId.Ingredients,     "○", "Fiches & Stock");
            y = AddNavItem(y, NavItemId.Fournisseurs,    "◐", "Fournisseurs");

            // Groupe RÉFÉRENTIELS
            y = AddSection(y, "RÉFÉRENTIELS");
            y = AddNavItem(y, NavItemId.Parametres,       "∷", "Paramètres");

            // Groupe BOUTIQUE EN LIGNE — lien vers le site e-commerce Laravel
            y = AddSection(y, "BOUTIQUE EN LIGNE");
            y = AddNavItem(y, NavItemId.BoutiqueWeb,      "\U0001f6d2", "Boutique web");
        }

        // AddSection() — ajoute un titre de section dans le panel nav
        // Peut optionnellement ajouter un bouton d'action rapide à droite du titre
        // (ex: "＋ Contexte" à côté de "STOCK & ACHATS")
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

            // Si un actionText est fourni, je crée un bouton d'action rapide à droite
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

            // Retourne la nouvelle position Y (chaque section prend 34px)
            return y + 34;
        }

        // AddNavItem() — ajoute un item de navigation cliquable dans le panel nav
        // Chaque item est un Panel avec du custom paint (icône + label + barre dorée si actif)
        // Le hover et le click sont gérés par des handlers inline
        private int AddNavItem(int y, NavItemId id, string icon, string label)
        {
            var pnl = new Panel
            {
                Location  = new Point(0, y),
                Size      = new Size(SIDEBAR_WIDTH, ITEM_HEIGHT),
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand,
                Tag       = id  // Je stocke le NavItemId dans le Tag pour le retrouver dans le Paint
            };

            // ── Badge (pill) ────────────────────────────────────────
            // Petit compteur arrondi à droite de l'item (ex: "3" pour 3 achats en attente)
            // Invisible par défaut, activé via SetBadge()
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
            // Custom paint pour dessiner la pill avec des coins arrondis
            lblBadge.Paint += (s, e) =>
            {
                var r = new Rectangle(0, 0, lblBadge.Width - 1, lblBadge.Height - 1);
                using (var path = RoundedRectPath(r, 9))
                using (var brush = new SolidBrush(lblBadge.BackColor))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    e.Graphics.FillPath(brush, path);
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    using (var f = lblBadge.Font)
                    using (var fgBrush = new SolidBrush(lblBadge.ForeColor))
                        e.Graphics.DrawString(lblBadge.Text, f, fgBrush, r, sf);
                }
            };
            pnl.Controls.Add(lblBadge);
            _badgeLabels[id] = lblBadge;

            // ── Custom paint de l'item nav ────────────────────────
            // Je dessine moi-même le fond, la barre dorée, l'icône et le texte
            // pour avoir un contrôle total sur le look (pas de contrôle WinForms standard qui fait ça)
            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.HighQuality;
                bool active = (NavItemId)pnl.Tag == _activeItem;

                // Fond coloré si l'item est actif (sinon transparent)
                if (active)
                {
                    using (var brush = new SolidBrush(BG_ACTIVE))
                        g.FillRectangle(brush, pnl.ClientRectangle);
                }

                // Barre dorée à gauche (3px) — indicateur visuel de l'item actif
                if (active)
                {
                    using (var brush = new SolidBrush(ACCENT))
                        g.FillRectangle(brush, 0, 0, 3, ITEM_HEIGHT);
                }

                // Icône Unicode — doré vif si actif, doré atténué sinon
                using (var fIcon = new Font("Segoe UI", 11F))
                {
                    var iconColor = active ? ACCENT : ACCENT_DIM;
                    using (var brush = new SolidBrush(iconColor))
                        g.DrawString(icon, fIcon, brush, 11, 8);
                }

                // Label texte — blanc si actif, crème semi-transparent sinon
                // Bold si actif pour renforcer la hiérarchie visuelle
                using (var fLabel = new Font("Segoe UI", 9.5F, active ? FontStyle.Bold : FontStyle.Regular))
                {
                    var labelColor = active ? FG_ACTIVE : FG_NORMAL;
                    using (var brush = new SolidBrush(labelColor))
                        g.DrawString(label, fLabel, brush, 34, 9);
                }
            };

            // ── Hover ───────────────────────────────────────────────
            // Fond légèrement éclairci au survol (sauf si l'item est déjà actif)
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
            // Fire l'event NavigationRequested avec le NavItemId — le MainForm fait le reste
            pnl.Click += (s, e) => NavigationRequested?.Invoke((NavItemId)pnl.Tag);
            // Propagation : si on clique sur le badge (enfant du panel), ça navigue aussi
            lblBadge.Click += (s, e) => NavigationRequested?.Invoke(id);

            _pnlNav.Controls.Add(pnl);
            _navPanels[id] = pnl;

            // Retourne la nouvelle position Y (chaque item prend ITEM_HEIGHT = 36px)
            return y + ITEM_HEIGHT;
        }

        // ── ComboBox OwnerDraw pour ActivitySwitcher ────────────────

        // CboActivite_DrawItem() — rendu custom de chaque item dans le dropdown Activité
        // Dessine un emoji chocolat, le nom de l'activité en gras, et la description en dessous
        // Le tout dans le style chocolat/or de la sidebar
        private void CboActivite_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            // Fond différent si l'item est sélectionné (hover dans le dropdown)
            bool selected = (e.State & DrawItemState.Selected) != 0;
            var bg = selected ? BG_ACTIVE : Color.FromArgb(45, 30, 18);
            using (var brush = new SolidBrush(bg))
                g.FillRectangle(brush, e.Bounds);

            var act = _cboActivite.Items[e.Index] as Activite;
            if (act == null) return;

            // Emoji chocolat à gauche — toujours le même pour l'instant
            string emoji = "🍫";
            string nom   = act.Nom;

            using (var f = new Font("Segoe UI", 12F))
                g.DrawString(emoji, f, Brushes.White, e.Bounds.X + 8, e.Bounds.Y + 10);

            // Nom de l'activité en gras (première ligne)
            using (var fNom = new Font("Segoe UI", 10F, FontStyle.Bold))
            using (var brNom = new SolidBrush(AppColors.SidebarTxt))
                g.DrawString(nom, fNom, brNom,
                    e.Bounds.X + 36, e.Bounds.Y + 6);

            // Sous-texte = description de l'activité (deuxième ligne, plus petit, semi-transparent)
            // Tronqué à 40 caractères max pour ne pas déborder
            var subText = act.Description ?? "";
            if (subText.Length > 40) subText = subText.Substring(0, 37) + "…";
            if (!string.IsNullOrWhiteSpace(subText))
            {
                using (var fSub = new Font("Segoe UI", 8F))
                using (var brSub = new SolidBrush(Color.FromArgb(130, 245, 230, 211)))
                    g.DrawString(subText, fSub, brSub,
                        e.Bounds.X + 36, e.Bounds.Y + 24);
            }

            // Bordure dorée autour de chaque item du dropdown (subtile)
            using (var pen = new Pen(Color.FromArgb(60, AppColors.Or)))
                g.DrawRectangle(pen, e.Bounds.X + 4, e.Bounds.Y + 2,
                    e.Bounds.Width - 8, e.Bounds.Height - 4);
        }

        // RoundedRectPath() — helper pour créer un GraphicsPath rectangulaire avec coins arrondis
        // Utilisé pour le badge "pill" — 4 arcs de cercle reliés par des segments
        private static GraphicsPath RoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);              // Coin haut-gauche
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);      // Coin haut-droit
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);  // Coin bas-droit
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);      // Coin bas-gauche
            path.CloseFigure();
            return path;
        }
    }
}
