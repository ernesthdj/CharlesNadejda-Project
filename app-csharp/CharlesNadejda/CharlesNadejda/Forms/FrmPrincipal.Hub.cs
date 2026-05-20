using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;
using CharlesNadejda.Navigation;

namespace CharlesNadejda.Forms
{
    public partial class FrmPrincipal
    {
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
                var brushAccent  = new SolidBrush(AppColors.ChocoBrand);
                var brushMed     = new SolidBrush(AppColors.ChocoMed);

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
    }
}
