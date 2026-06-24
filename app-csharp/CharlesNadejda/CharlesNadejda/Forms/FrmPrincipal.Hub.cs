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
    // ════════════════════════════════════════════════════════════════
    //  FrmPrincipal.Hub — Dashboard / tableau de bord de l'activite
    //
    //  Responsabilite : vue d'ensemble apres selection d'une activite.
    //  Affiche 4 StatCards KPI, les productions recentes (7 jours),
    //  et les alertes de stock bas avec navigation contextuelle.
    //
    //  Layout : Header (titre+actions) → Stats (4 KPI) → Corps 62/38
    //    Gauche (62%) : DGV productions recentes
    //    Droite (38%) : liste alertes stock avec clic → ecran Ingredients
    // ════════════════════════════════════════════════════════════════
    public partial class FrmPrincipal
    {
        // ShowHubScreen — construit l'écran Hub (dashboard) dans le panneau droit.
        // C'est un tableau de bord avec 3 zones :
        //   1. Header : nom de l'activité + boutons action (nouvelle prod, rapport)
        //   2. Stats : 4 cartes KPI (ingrédients, alertes, fiches BOM, productions 7j)
        //   3. Corps : productions récentes (tableau) à gauche + alertes stock à droite
        private void ShowHubScreen()
        {
            // Pas d'activité sélectionnée → on montre l'onboarding plutôt que le Hub
            if (_state.ActiveActivite == null) { ShowOnboarding(); return; }

            _pnlDroit.SuspendLayout();
            ClearAndDisposePanel();

            // ── Chargement des données depuis la DAL ──
            // Je charge tout d'un coup dans des listes locales — c'est rapide et ça évite
            // de multiplier les requêtes DB pendant la construction de l'UI.
            List<Ingredient>    ings   = new List<Ingredient>();
            List<BomFiche>      fiches = new List<BomFiche>();
            List<BomProduction> prods  = new List<BomProduction>();
            try
            {
                ings   = IngredientDAL.GetAllByActivite(_state.ActiveActivite.Id);
                fiches = BomFicheDAL.GetAll(idActivite: _state.ActiveActivite.Id);
                // Les 10 dernières productions de cette activité — pour le tableau récapitulatif
                prods  = BomProductionDAL.GetRecentByActivite(_state.ActiveActivite.Id, 10);
            }
            catch (Exception ex)
            {
                // TICKET-10 : on loggue l'erreur mais on affiche le Hub quand même (dégradé gracieux).
                // Mieux vaut un Hub avec des stats à zéro qu'un crash.
                Trace.TraceError("ShowHubScreen — chargement DAL : {0}", ex);
            }

            // ── Calcul des indicateurs ──
            // Alertes = ingrédients dont le stock est sous le seuil configuré
            var alertes    = ings.Where(i => i.EstEnAlerte).ToList();
            // Productions des 7 derniers jours — pour la stat card "Productions 7j"
            int prods7j    = prods.Count(p => p.DateProduction >= DateTime.Now.AddDays(-7));
            // Coût total des 7 derniers jours — somme des coûts ingrédients
            decimal cout7j = prods.Where(p => p.DateProduction >= DateTime.Now.AddDays(-7))
                                  .Sum(p => p.CoutIngredients);

            // US-02 : vérifier si l'activité a des contextes pour afficher le message d'onboarding
            // Si aucun contexte, j'affiche un petit message dans le header pour guider l'utilisateur
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

            // ════════════════════════════════════════════════════════════
            //  SECTION 1 — Header : nom activité + boutons d'action
            // ════════════════════════════════════════════════════════════

            // Le header est plus grand s'il y a le message "aucun contexte" (76px vs 56px)
            var pnlHdr = new Panel
            {
                Dock = DockStyle.Top, Height = aucunContexte ? 76 : 56,
                BackColor = CREME_WARM, Padding = new Padding(20, 0, 20, 0)
            };

            // Ligne de séparation en bas du header — subtile mais nécessaire pour la hiérarchie visuelle
            pnlHdr.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, pnlHdr.Height - 1, pnlHdr.Width, pnlHdr.Height - 1);
            };

            // Nom de l'activité — gros titre en haut à gauche
            pnlHdr.Controls.Add(new Label
            {
                Text = _state.ActiveActivite.Nom, Location = new Point(20, 8),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, AutoSize = true
            });

            // Sous-titre descriptif — pour que l'utilisateur comprenne qu'il est sur le Hub
            pnlHdr.Controls.Add(new Label
            {
                Text = "Vue d'ensemble · Alertes, productions récentes, contextes actifs",
                Location = new Point(20, 32), Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = CHOCO_MED, AutoSize = true
            });

            // US-02 : message d'onboarding si aucun contexte de production
            // Guide l'utilisateur vers la création de son premier contexte
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

            // Bouton "Nouvelle production" — raccourci vers l'écran Production
            var btnNouvProd = new Button
            {
                Text = "▶  Nouvelle production", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, BackColor = CHOCO_BRAND, ForeColor = Color.White,
                Size = new Size(148, 26), Cursor = Cursors.Hand
            };
            btnNouvProd.FlatAppearance.BorderColor = CHOCO_ABYSS;
            btnNouvProd.Click += (s, ev) => NavigateTo(ScreenId.Production);

            // US-09 : bouton "Rapport du jour" — visible seulement si productions > 0
            // Pas de sens d'afficher un rapport vide
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

            // Les boutons sont calés à droite — je les repositionne dynamiquement au Resize
            // pour qu'ils restent collés au bord droit quelle que soit la largeur de la fenêtre
            pnlHdr.Resize += (s, ev) =>
            {
                btnNouvProd.Location = new Point(pnlHdr.Width - 156, 14);
                btnRapport.Location  = new Point(pnlHdr.Width - 156 - 144, 14);
            };
            // Position initiale (avant le premier Resize)
            btnNouvProd.Location = new Point(900, 14);
            btnRapport.Location  = new Point(748, 14);
            pnlHdr.Controls.Add(btnNouvProd);
            pnlHdr.Controls.Add(btnRapport);

            // ════════════════════════════════════════════════════════════
            //  SECTION 2 — Stat Cards : 4 KPI cliquables
            // ════════════════════════════════════════════════════════════

            var pnlStats = new Panel
            {
                Dock = DockStyle.Top, Height = 92,
                BackColor = CREME_WARM, Padding = new Padding(16, 12, 16, 8)
            };

            // US-08 : StatCards avec navigation contextuelle au clic
            // Chaque carte a un ton (couleur) selon son état : danger (rouge), gold (doré), success (vert)
            var cardIngredients = MakeStatCard("🥣", "Ingrédients",      ings.Count.ToString(),    "",                                          "");
            var cardAlertes     = MakeStatCard("⚠",  "En alerte",        alertes.Count.ToString(), alertes.Count > 0
                                                                                                     ? string.Join(", ", alertes.Take(2).Select(i => i.Nom))
                                                                                                     : "Aucune alerte",              alertes.Count > 0 ? "danger" : "ok");
            // Nombre de niveaux BOM du contexte actif — info affichée sous "Fiches BOM"
            int niveauxCount = _state.ActiveContexte != null ? BomNiveauDAL.GetByContexte(_state.ActiveContexte.Id).Count : 0;
            var cardFiches      = MakeStatCard("🧪", "Fiches BOM",       fiches.Count.ToString(),  $"{niveauxCount} niveaux",             "gold");
            var cardProds       = MakeStatCard("▶",  "Productions · 7j", prods7j.ToString(),       $"Coût total {cout7j:F2} €",                  "success");

            // H3 — Ingrédients : au clic, navigation vers la liste complète des ingrédients
            cardIngredients.Cursor = Cursors.Hand;
            cardIngredients.Click += (s, ev) => {
                _state.SetFiltreAlertes(false);
                NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Ingredients));
            };

            // H4 — En alerte : au clic, navigation vers la liste FILTRÉE sur les alertes seulement
            // Le flag SetFiltreAlertes(true) est lu par FrmIngredients pour pré-filtrer
            if (alertes.Count > 0)
            {
                cardAlertes.Cursor = Cursors.Hand;
                cardAlertes.Click += (s, ev) => {
                    _state.SetFiltreAlertes(true);
                    NavigateTo(ScreenId.Ressources, () => _state.SetRessource(RessourceType.Ingredients));
                };
            }

            // H5 — Fiches BOM : au clic, navigation vers l'écran Production du premier contexte
            if (fiches.Count > 0 && contextesDispo.Count > 0)
            {
                cardFiches.Cursor = Cursors.Hand;
                cardFiches.Click += (s, ev) => {
                    _state.SetContexte(contextesDispo[0]);
                    NavigateTo(ScreenId.Production);
                };
            }

            // H6 — Productions 7j : au clic, navigation vers l'écran de production
            if (prods7j > 0)
            {
                cardProds.Cursor = Cursors.Hand;
                cardProds.Click += (s, ev) => NavigateTo(ScreenId.Production);
            }

            // Les 4 cartes sont réparties équitablement en largeur au Resize
            // Formule : largeur dispo / 4, avec 12px d'espace entre chaque carte
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

            // ════════════════════════════════════════════════════════════
            //  SECTION 3 — Corps : productions récentes + alertes stock
            // ════════════════════════════════════════════════════════════

            // Layout principal du corps — split horizontal : prods à gauche, alertes à droite
            var pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = CREME_WARM };

            // ── Panneau alertes (droite) ─────────────────────────────
            // Panel fixe de 288px à droite — liste les ingrédients en alerte (max 6)
            var pnlAlerts = new Panel
            {
                Dock = DockStyle.Right, Width = 288,
                BackColor = CREME_WARM, Padding = new Padding(12, 8, 16, 8)
            };
            // Bordure gauche — séparateur visuel avec le panneau productions
            pnlAlerts.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, 0, 0, pnlAlerts.Height);
            };
            // Titre de section
            pnlAlerts.Controls.Add(new Label
            {
                Text = "ALERTES STOCK", Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, Location = new Point(12, 12), AutoSize = true
            });

            // Construction des lignes d'alerte — position Y incrémentale
            int ay = 34;
            if (alertes.Count == 0)
            {
                // Pas d'alerte → message positif en vert pour rassurer l'utilisateur
                pnlAlerts.Controls.Add(new Label
                {
                    Text = "✓  Aucune alerte de stock",
                    Font = new Font("Segoe UI", 9F, FontStyle.Italic), ForeColor = GREEN_OK,
                    Location = new Point(12, ay), AutoSize = true
                });
            }
            else
            {
                // Afficher max 6 alertes pour ne pas surcharger le panneau
                foreach (var ing in alertes.Take(6))
                {
                    // "crit" si stock à 0 ou < 50% du seuil, "warn" sinon
                    bool crit = ing.StockActuel <= 0 || (ing.SeuilAlerteStock.HasValue && ing.StockActuel <= ing.SeuilAlerteStock.Value * 0.5m);
                    var row = MakeAlertRow(
                        crit ? "crit" : "warn",
                        crit ? "⚠" : "🕑",
                        $"{ing.Nom} — {ing.StockActuel:F0} {ing.UniteMesure}",
                        ing.SeuilAlerteStock.HasValue ? $"Seuil : {ing.SeuilAlerteStock.Value:F0} {ing.UniteMesure}" : "");
                    row.Location = new Point(12, ay);
                    row.Width    = pnlAlerts.ClientSize.Width - 24;
                    // Adapter la largeur au Resize du panneau parent
                    pnlAlerts.Resize += (s, ev) => row.Width = pnlAlerts.ClientSize.Width - 24;
                    pnlAlerts.Controls.Add(row);
                    ay += row.Height + 6;
                }
            }

            // ── Panneau productions récentes (gauche, Fill) ──────────
            var pnlProds = new Panel { Dock = DockStyle.Fill, BackColor = CREME_WARM, Padding = new Padding(16, 8, 12, 8) };
            pnlProds.Controls.Add(new Label
            {
                Text = "DERNIÈRES PRODUCTIONS", Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, Location = new Point(16, 12), AutoSize = true
            });

            // DataGridView des 10 dernières productions — lecture seule, pas d'ajout/suppression
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
            // Style headers — palette chocolat/crème cohérente avec le reste de l'app
            dgvProds.ColumnHeadersDefaultCellStyle.BackColor = CREME;
            dgvProds.ColumnHeadersDefaultCellStyle.ForeColor = CHOCO_BRAND;
            dgvProds.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dgvProds.DefaultCellStyle.SelectionBackColor      = CHOCO_BRAND;
            dgvProds.DefaultCellStyle.SelectionForeColor      = Color.White;
            pnlProds.Controls.Add(dgvProds);

            // Le DGV s'adapte à la taille du panneau parent
            pnlProds.Resize += (s, ev) =>
            {
                dgvProds.Size = new Size(
                    pnlProds.ClientSize.Width - 28,
                    Math.Max(100, pnlProds.ClientSize.Height - 44));
            };

            if (prods.Count > 0)
            {
                // Projection anonyme pour le DataSource du DGV —
                // je transforme les objets BomProduction en lignes lisibles
                dgvProds.DataSource = prods.Select(p =>
                {
                    // Nombre d'unités produites (ex: 2500g ÷ 250g = 10 baguettes)
                    int nbUnites = p.QuantiteOutputBatch > 0
                        ? (int)(p.QuantiteProduite / p.QuantiteOutputBatch) : 0;
                    string qteAffichee = $"{nbUnites} unité{(nbUnites != 1 ? "s" : "")}";

                    return new
                    {
                        Date    = p.DateProduction.ToString("dd/MM"),
                        Fiche   = p.NomFiche,
                        Niveau  = $"N{p.OrdreNiveau}",
                        Produit = qteAffichee,
                        Cout    = $"{p.CoutIngredients:F2} \u20ac",
                    };
                }).ToList();
            }
            else
            {
                // Pas de production → message informatif au lieu d'un DGV vide
                pnlProds.Controls.Add(new Label
                {
                    Text = "Aucune production enregistrée pour cette activité.",
                    Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                    ForeColor = CHOCO_MED, Location = new Point(16, 40), AutoSize = true
                });
            }

            // Assemblage final — l'ordre d'ajout détermine le layout (Dock)
            // Fill en premier (pnlProds), puis Right (pnlAlerts)
            pnlMain.Controls.Add(pnlProds);
            pnlMain.Controls.Add(pnlAlerts);

            // Ajout au panneau droit — ordre WinForms Dock : Fill, Top, Top
            _pnlDroit.Controls.Add(pnlMain);   // Fill — le corps principal
            _pnlDroit.Controls.Add(pnlStats);   // Top  — les stat cards
            _pnlDroit.Controls.Add(pnlHdr);      // Top  — le header

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
        // GenererRapport — crée un document imprimable avec les données du jour.
        // J'utilise PrintDocument (GDI+) plutôt qu'un PDF externe → zéro dépendance.
        // Le PrintPreviewDialog permet de voir le rapport avant d'imprimer.
        // C'est du drawing "à la main" — je positionne chaque texte avec des coordonnées Y.
        private void GenererRapport()
        {
            if (_state.ActiveActivite == null) return;

            // Charger les données du jour seulement (pas les 10 dernières comme dans le Hub)
            List<BomProduction> prodsJour;
            List<Ingredient>    alertes;
            try
            {
                prodsJour = BomProductionDAL.GetDuJourByActivite(_state.ActiveActivite.Id);
                alertes   = IngredientDAL.GetAllByActivite(_state.ActiveActivite.Id)
                                         .Where(i => i.EstEnAlerte).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la génération du rapport : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Somme des coûts du jour — affiché en pied de page du rapport
            decimal coutJour = prodsJour.Sum(p => p.CoutIngredients);

            var doc = new System.Drawing.Printing.PrintDocument();
            doc.DocumentName = $"Rapport_{_state.ActiveActivite.Nom}_{DateTime.Today:yyyy-MM-dd}";

            // PrintPage — c'est ici que je dessine le rapport page par page.
            // Tout le rendu est fait via Graphics.DrawString avec des positions Y manuelles.
            // C'est pas sexy mais c'est fiable et sans dépendance externe.
            doc.PrintPage += (s, ev) =>
            {
                var g      = ev.Graphics;

                // Polices et pinceaux — je les crée ici et les dispose en fin de handler
                var fontTitre    = new Font("Segoe UI", 13F, FontStyle.Bold);
                var fontSection  = new Font("Segoe UI", 9F,  FontStyle.Bold);
                var fontCorps    = new Font("Segoe UI", 10F, FontStyle.Regular);
                var fontMeta     = new Font("Segoe UI", 8F,  FontStyle.Italic);
                var brushNoir    = new SolidBrush(Color.FromArgb(44, 24, 16));
                var brushAccent  = new SolidBrush(AppColors.ChocoBrand);
                var brushMed     = new SolidBrush(AppColors.ChocoMed);

                // Marges et position de départ — fournis par le système d'impression
                int margin = ev.MarginBounds.Left;
                int y      = ev.MarginBounds.Top;
                int width  = ev.MarginBounds.Width;

                // ── En-tête : titre du rapport + date + activité ──
                g.DrawString($"ARTISASTOCK — Rapport du jour", fontTitre, brushAccent,
                    new RectangleF(margin, y, width, 24));
                y += 28;
                g.DrawString($"Activité : {_state.ActiveActivite.Nom}   ·   {DateTime.Now:dd/MM/yyyy HH:mm}",
                    fontMeta, brushMed, new RectangleF(margin, y, width, 18));
                y += 24;

                // Ligne de séparation horizontale
                using (var pen = new Pen(Color.FromArgb(195, 185, 168), 1))
                    g.DrawLine(pen, margin, y, margin + width, y);
                y += 10;

                // ── Section 1 : Productions du jour ──
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
                    // Une ligne par production : heure, fiche, quantité, coût
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

                // ── Section 2 : Alertes stock ──
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
                    // Liste des ingrédients en alerte avec leur stock actuel et seuil
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

                // ── Pied de page : coût total du jour ──
                g.DrawString($"Coût total du jour : {coutJour:F2} €", fontSection, brushAccent,
                    new RectangleF(margin, y, width, 20));

                // Libérer les ressources GDI — obligatoire sinon fuite mémoire
                // (les fonts et brushes GDI+ ne sont pas garbage-collectés automatiquement)
                fontTitre.Dispose(); fontSection.Dispose(); fontCorps.Dispose(); fontMeta.Dispose();
                brushNoir.Dispose(); brushAccent.Dispose(); brushMed.Dispose();
            };

            // Ouvrir la prévisualisation — l'utilisateur peut imprimer ou fermer
            using (var preview = new System.Windows.Forms.PrintPreviewDialog { Document = doc, Width = 900, Height = 700 })
                preview.ShowDialog(this);
        }

        // MakeStatCard — fabrique une carte KPI (Key Performance Indicator) pour le Hub.
        // Chaque carte affiche : icône + label en haut, grosse valeur au centre, delta en bas.
        // Le "tone" détermine la couleur d'accent : danger (rouge), gold (doré), success (vert).
        // La bande de 3px à gauche (accent bar) donne un repère visuel rapide.
        private static Panel MakeStatCard(string icon, string label, string value, string delta, string tone)
        {
            // Choix de la couleur selon le ton — par défaut blanc/chocolat
            Color bg     = Color.White;
            Color accent = CHOCO_BRAND;
            if (tone == "danger")  { bg = Color.FromArgb(255, 242, 242); accent = RED_CRIT; }
            if (tone == "gold")    { bg = Color.FromArgb(255, 252, 236); accent = OR;       }
            if (tone == "success") { bg = Color.FromArgb(242, 253, 242); accent = GREEN_OK; }

            var card = new Panel { BackColor = bg, Height = 68 };

            // Bordure + accent bar à gauche (3px de largeur) — dessinés en custom Paint
            card.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                // Accent bar — la petite bande colorée à gauche, signature visuelle de la carte
                using (var br = new SolidBrush(accent))
                    ev.Graphics.FillRectangle(br, 0, 0, 3, card.Height);
            };

            // Label en haut (ex: "🥣 Ingrédients")
            card.Controls.Add(new Label { Text = $"{icon} {label}", Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(120, 100, 80), Location = new Point(10, 8), AutoSize = true });
            // Valeur principale en gros (ex: "42")
            card.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 22F, FontStyle.Bold), ForeColor = accent == OR ? Color.FromArgb(160, 130, 0) : accent, Location = new Point(10, 20), AutoSize = true });
            // Delta/description en bas (ex: "Coût total 125.50 €") — optionnel
            if (!string.IsNullOrEmpty(delta))
                card.Controls.Add(new Label { Text = delta, Font = new Font("Segoe UI", 7.5F), ForeColor = Color.FromArgb(120, 100, 80), Location = new Point(10, 50), AutoSize = true, MaximumSize = new Size(200, 0) });
            return card;
        }

        // MakeAlertRow — fabrique une ligne d'alerte pour le panneau alertes du Hub.
        // Deux niveaux de gravité : "crit" (rouge, stock critique/épuisé) et "warn" (orange, stock bas).
        // Chaque ligne a un accent bar à gauche (4px) + icône + titre + description optionnelle.
        private static Panel MakeAlertRow(string tone, string icon, string title, string desc)
        {
            // Fond et bordure adaptés au niveau de gravité
            Color bg     = tone == "crit" ? Color.FromArgb(255, 242, 242) : Color.FromArgb(255, 248, 236);
            Color border = tone == "crit" ? RED_CRIT : ORG_WARN;

            var row = new Panel { Height = 52, BackColor = bg, Padding = new Padding(8, 6, 8, 6) };

            // Bordure + accent bar gauche — même pattern que les stat cards
            row.Paint += (s, ev) =>
            {
                using (var pen = new Pen(border, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1);
                using (var br = new SolidBrush(border))
                    ev.Graphics.FillRectangle(br, 0, 0, 4, row.Height);
            };

            // Icône d'alerte (⚠ ou 🕑)
            row.Controls.Add(new Label { Text = icon, Font = new Font("Segoe UI", 13F), ForeColor = border, Location = new Point(10, 14), AutoSize = true });
            // Titre de l'alerte (ex: "Beurre — 200 g")
            row.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = border, Location = new Point(30, 8), Size = new Size(row.Width - 38, 18) });
            // Description optionnelle (ex: "Seuil : 500 g")
            if (!string.IsNullOrEmpty(desc))
                row.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 7.5F), ForeColor = Color.FromArgb(100, 80, 60), Location = new Point(30, 26), Size = new Size(row.Width - 38, 16) });
            return row;
        }
    }
}
