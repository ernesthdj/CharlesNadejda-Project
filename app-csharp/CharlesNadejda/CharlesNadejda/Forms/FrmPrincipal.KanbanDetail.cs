using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    // ════════════════════════════════════════════════════════════════
    //  FrmPrincipal.KanbanDetail — Factories de cards & helpers UI
    //
    //  Ce fichier partial regroupe toutes les méthodes qui fabriquent
    //  des éléments visuels pour le Kanban de production :
    //    — Cards niveaux, fiches, stock compact, historique
    //    — DGV simulation stylisée
    //    — Helpers statiques : header, séparateur, boutons, path arrondi
    //
    //  Toutes les méthodes ici sont pures "factories" : elles reçoivent
    //  des données, construisent un contrôle et le retournent.
    //  Aucune logique métier — uniquement du rendu.
    // ════════════════════════════════════════════════════════════════
    partial class FrmPrincipal
    {
        // ════════════════════════════════════════════════════════════════
        //  Cards niveaux — colonne gauche
        // ════════════════════════════════════════════════════════════════

        // Crée une card visuelle pour un niveau BOM (N0, N1, N2...)
        // Tout le rendu est en custom paint : rectangle arrondi, cercle numéro,
        // barre accent gauche, badge "★ Final" si c'est le produit final.
        private Panel MakeProdNiveauCard(BomNiveau niv, int ficheCount, int ordreMax)
        {
            // N0 est le stock global partagé, il est "locked" (pas modifiable)
            bool locked = niv.Ordre == 0;
            // estTop = c'est le produit final (dernier niveau de la chaîne BOM)
            bool estTop = niv.Ordre == ordreMax && ordreMax > 0;
            string subtitle = locked ? "Stock global partagé"
                : niv.Ordre == 1 ? "Ingrédients de base"
                : $"{ficheCount} fiche{(ficheCount != 1 ? "s" : "")}";

            var card = new Panel
            {
                Size   = new Size(PROD_CARD_W, 64),
                Margin = new Padding(0, 0, 0, 4),
                Cursor = Cursors.Hand
            };
            // J'active le DoubleBuffered par reflection pour éviter le scintillement lors du repaint
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, card, new object[] { true });

            // Tout le dessin de la card se fait en custom painting (pas de contrôles enfants)
            // Ça donne un rendu plus propre et plus performant
            card.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Je détermine les couleurs selon l'état : sélectionné (doré) ou non
                bool sel     = _prodSelectedNiveau?.Id == niv.Id;
                Color accent = locked ? Color.FromArgb(195, 165, 135) : sel ? OR : CHOCO_MED;
                Color bg     = sel ? Color.FromArgb(255, 250, 229) : Color.White;
                Color brd    = sel ? OR : BORDER_CLR;

                // Rectangle arrondi pour le fond de la card
                using (var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 6))
                {
                    using (var br = new SolidBrush(bg))
                        g.FillPath(br, path);
                    using (var pen = new Pen(brd, sel ? 2f : 1f))
                        g.DrawPath(pen, path);
                }

                // Barre accent gauche — petite bande colorée pour identifier visuellement
                using (var br = new SolidBrush(accent))
                    g.FillRectangle(br, 0, 8, 4, card.Height - 16);

                // Cercle numéro — rond coloré avec le numéro d'ordre du niveau
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

                // Nom du niveau
                using (var f = new Font("Segoe UI", 9.5F, FontStyle.Bold))
                using (var br = new SolidBrush(CHOCO_BRAND))
                    g.DrawString(niv.Nom, f, br, 42, 10);

                // Sous-titre (nb fiches ou description du rôle du niveau)
                using (var f = new Font("Segoe UI", 8F))
                using (var br = new SolidBrush(CHOCO_MED))
                    g.DrawString(subtitle, f, br, 42, 32);

                // Badge "★ Final" en haut à droite si c'est le produit final
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

            // Au clic sur la card, je sélectionne ce niveau (highlight doré + cascade)
            card.Click += (s, ev) => ProdSelectNiveau(niv);

            // Menu contextuel (clic droit) — seulement pour les niveaux non-locked (pas N0)
            if (!locked)
            {
                var menu = new ContextMenuStrip();

                // Modifier
                menu.Items.Add("✎  Modifier ce niveau").Click += (s2, ev2) =>
                {
                    ProdModifierNiveau(niv);
                };

                menu.Items.Add(new ToolStripSeparator());

                // Supprimer (top-only) — seul le niveau le plus haut peut être supprimé
                var miDel = menu.Items.Add("✕  Supprimer ce niveau");
                miDel.Enabled = estTop;
                if (!estTop) miDel.ToolTipText = "Seul le niveau le plus haut est supprimable";
                miDel.Click += (s2, ev2) => ProdSupprimerNiveau(niv);

                card.ContextMenuStrip = menu;
            }

            // J'enregistre la card dans le dictionnaire pour pouvoir la retrouver et l'Invalidate
            _prodNiveauPanels[niv.Id] = card;
            return card;
        }

        // ════════════════════════════════════════════════════════════════
        //  Cards fiches — colonne gauche (N2+)
        // ════════════════════════════════════════════════════════════════

        // Crée une card visuelle pour une fiche recette
        // Affiche le nom de la fiche et les infos batch (quantité/unité par batch)
        // Tout en custom paint, même pattern que les cards niveaux
        private Panel MakeProdFicheCard(BomFiche fiche)
        {
            var card = new Panel
            {
                Size   = new Size(PROD_CARD_W, 56),
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
                // Même logique de couleurs que les niveaux : doré si sélectionné
                bool sel    = _prodSelectedFiche?.Id == fiche.Id;
                Color bg    = sel ? Color.FromArgb(255, 250, 229) : Color.White;
                Color brd   = sel ? OR : BORDER_CLR;
                Color accent = sel ? OR : CHOCO_MED;

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

                // Nom de la fiche en gras
                using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
                using (var br = new SolidBrush(CHOCO_BRAND))
                    g.DrawString(fiche.Nom, f, br, 14, 8);

                // Info batch : ex "500 g/batch"
                string info = $"{fiche.QuantiteOutput} {fiche.UniteOutput}/batch";
                using (var f = new Font("Segoe UI", 7.5F))
                using (var br = new SolidBrush(CHOCO_MED))
                    g.DrawString(info, f, br, 14, 30);
            };

            // Au clic, je sélectionne cette fiche
            card.Click += (s, ev) => ProdSelectFiche(fiche);
            return card;
        }

        // ════════════════════════════════════════════════════════════════
        //  Cards stock compact — colonne centrale
        // ════════════════════════════════════════════════════════════════

        // Crée une card compacte pour afficher une ligne de stock dans la colonne centrale
        // Fond rosé + icône ⚠ si alerte, fond vert pâle si stock OK
        private Panel MakeProdStockCompactCard(string nom, string qte, string pieces, bool alerte)
        {
            // Largeur adaptée au parent (FlowLayoutPanel TopDown ne gère pas Anchor)
            int cardW = _prodFlowStock != null ? _prodFlowStock.ClientSize.Width - 20 : 400;
            if (cardW < 200) cardW = 400;

            var card = new Panel
            {
                Size   = new Size(cardW, 28),
                Margin = new Padding(0, 0, 0, 1),
                Cursor = Cursors.Default
            };

            // Tout le rendu en custom paint pour performance et cohérence visuelle
            card.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                // Fond légèrement coloré : rosé si alerte, vert pâle sinon
                Color bgColor = alerte ? Color.FromArgb(255, 245, 240) : Color.FromArgb(248, 252, 248);
                using (var br = new SolidBrush(bgColor))
                    g.FillRectangle(br, 0, 0, card.Width, card.Height);

                // Barre accent gauche — rouge si alerte, vert sinon
                Color accentColor = alerte ? RED_CRIT : AppColors.Success;
                using (var br = new SolidBrush(accentColor))
                    g.FillRectangle(br, 0, 2, 3, card.Height - 4);

                // Nom de l'ingrédient ou de la fiche
                using (var f = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var br = new SolidBrush(CHOCO_BRAND))
                    g.DrawString(nom ?? "—", f, br, 10, 5);

                // Quantité en stock
                using (var f = new Font("Segoe UI", 8F))
                using (var br = new SolidBrush(CHOCO_MED))
                    g.DrawString(qte, f, br, 180, 5);

                // Pièces (optionnel, ex: "3 pce" pour les conditionnements)
                if (!string.IsNullOrEmpty(pieces))
                {
                    using (var f = new Font("Segoe UI", 7.5F))
                    using (var br = new SolidBrush(Color.Gray))
                        g.DrawString(pieces, f, br, 310, 6);
                }

                // Icône alerte en bout de ligne si stock bas
                if (alerte)
                {
                    using (var f = new Font("Segoe UI", 8F))
                    using (var br = new SolidBrush(RED_CRIT))
                        g.DrawString("⚠", f, br, card.Width - 22, 4);
                }
            };

            return card;
        }

        // ════════════════════════════════════════════════════════════════
        //  Cards historique — colonne droite
        // ════════════════════════════════════════════════════════════════

        // Crée une card d'historique pour une production passée
        // Affiche la date, le nom de la fiche, la quantité produite, le coût et le contexte
        // Tout en custom paint avec un rectangle arrondi
        private Panel MakeProdHistoriqueCard(BomProduction prod)
        {
            var card = new Panel
            {
                Size   = new Size(232, 52),
                Margin = new Padding(0, 0, 0, 3),
                Cursor = Cursors.Default
            };
            // DoubleBuffered par reflection pour éviter le scintillement
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, card, new object[] { true });

            card.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Rectangle arrondi blanc avec bordure fine
                using (var path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 5))
                {
                    using (var br = new SolidBrush(Color.White))
                        g.FillPath(br, path);
                    using (var pen = new Pen(BORDER_CLR, 1f))
                        g.DrawPath(pen, path);
                }

                // Barre accent dorée à gauche
                using (var br = new SolidBrush(OR))
                    g.FillRectangle(br, 0, 6, 3, card.Height - 12);

                // Date de production
                string dateTxt = prod.DateProduction.ToString("dd/MM HH:mm");
                using (var f = new Font("Segoe UI", 7F))
                using (var br = new SolidBrush(CHOCO_MED))
                    g.DrawString(dateTxt, f, br, 10, 5);

                // Nom de la fiche recette
                using (var f = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var br = new SolidBrush(CHOCO_BRAND))
                    g.DrawString(prod.NomFiche ?? "—", f, br, 80, 4);

                // Quantité produite convertie en unités (ex: 2500g ÷ 250g = 10 baguettes)
                int nbUnites = prod.QuantiteOutputBatch > 0
                    ? (int)(prod.QuantiteProduite / prod.QuantiteOutputBatch) : 0;
                string qte = $"{nbUnites} unité{(nbUnites != 1 ? "s" : "")}";
                string cout = UnitConvertisseur.FormatPrix(prod.CoutIngredients);
                using (var f = new Font("Segoe UI", 7.5F))
                using (var br = new SolidBrush(CHOCO_MED))
                    g.DrawString(qte, f, br, 10, 22);

                using (var f = new Font("Segoe UI", 7.5F, FontStyle.Bold))
                using (var br = new SolidBrush(AppColors.Success))
                    g.DrawString(cout, f, br, 130, 22);

                // Contexte et niveau (ex: "N3 · Pâtisserie fine")
                string ctx = $"N{prod.OrdreNiveau} · {prod.NomContexte}";
                using (var f = new Font("Segoe UI", 7F, FontStyle.Italic))
                using (var br = new SolidBrush(Color.Gray))
                    g.DrawString(ctx, f, br, 10, 37);
            };

            return card;
        }

        // ════════════════════════════════════════════════════════════════
        //  DGV Factory
        //  Fabrique une DataGridView pré-configurée aux couleurs de l'app
        //  Utilisée pour la grille de simulation
        // ════════════════════════════════════════════════════════════════

        // Crée un DGV stylisé avec les couleurs crème/chocolat de l'app
        // Lecture seule, sélection par ligne, pas de headers de ligne
        private DataGridView BuildProdDgv()
        {
            var dgv = new DataGridView
            {
                Font = new Font("Segoe UI", 9F), BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, GridColor = BORDER_CLR,
                RowHeadersVisible = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false, MultiSelect = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 28,
                AutoGenerateColumns = false
            };
            // Style des headers : fond crème, texte chocolat, pas de changement au survol
            dgv.ColumnHeadersDefaultCellStyle.BackColor = CREME;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = CHOCO_BRAND;
            dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = CREME;
            dgv.DefaultCellStyle.SelectionBackColor = CHOCO_BRAND;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            // Lignes alternées légèrement teintées pour faciliter la lecture
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 247, 244);
            return dgv;
        }

        // ════════════════════════════════════════════════════════════════
        //  UI Helpers statiques — Kanban layout
        //  Méthodes utilitaires partagées pour créer les éléments visuels
        //  récurrents : headers Kanban, séparateurs, boutons, rectangles arrondis
        // ════════════════════════════════════════════════════════════════

        // Crée un header de colonne Kanban avec un titre et une barre d'accent colorée en bas
        private static Panel MakeKanbanHeader(string title, Color accentColor)
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = AppColors.CremeWarm };
            // Barre colorée de 2px en bas pour identifier visuellement la colonne
            pnl.Paint += (s, ev) =>
            {
                using (var br = new SolidBrush(accentColor))
                    ev.Graphics.FillRectangle(br, 0, pnl.Height - 2, pnl.Width, 2);
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

        // Crée un séparateur vertical de 3px entre les colonnes Kanban
        // Effet subtil avec 3 lignes de teintes différentes pour un look gravé
        private static Panel MakeColumnSeparator()
        {
            var sep = new Panel { Dock = DockStyle.Left, Width = 3, BackColor = Color.Transparent };
            sep.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                int h = sep.Height;
                // 3 lignes avec opacités différentes pour un effet de profondeur
                using (var pen = new Pen(Color.FromArgb(40, 80, 60, 40)))
                    g.DrawLine(pen, 0, 0, 0, h);
                using (var pen = new Pen(Color.FromArgb(200, 180, 160)))
                    g.DrawLine(pen, 1, 0, 1, h);
                using (var pen = new Pen(Color.FromArgb(20, 255, 255, 255)))
                    g.DrawLine(pen, 2, 0, 2, h);
            };
            return sep;
        }

        /// <summary>Petit bouton icône pour la barre d'actions niveaux (26×26).</summary>
        // Crée un bouton carré avec une icône (ex: ＋, ⚙, 🗑) pour les actions CRUD
        // Utilisé dans les headers niveaux et fiches
        private static Button MakeNivActionBtn(string icon, string tooltip, int index)
        {
            var btn = new Button
            {
                Text = icon, Font = new Font("Segoe UI", 10F),
                FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = CHOCO_MED,
                Size = new Size(30, 26), Location = new Point(index * 34, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = BORDER_CLR;
            if (!string.IsNullOrEmpty(tooltip))
            {
                var tt = new ToolTip();
                tt.SetToolTip(btn, tooltip);
            }
            return btn;
        }

        // Crée un petit bouton coloré avec texte (ex: "🛒 Acheter")
        // Largeur auto-calculée selon la longueur du texte
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

        // Crée un GraphicsPath pour dessiner un rectangle aux coins arrondis
        // Utilisé partout pour les cards (niveaux, fiches, historique)
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

        // Crée un bouton d'action standard (102×28) avec fond coloré et bordure invisible
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
