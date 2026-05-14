using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Consultation du stock global (lecture seule).
    ///
    /// Affiche le contenu de la VIEW vue_stock_global qui unifie :
    ///   - lots_ingredients   (type_stock = 'lot_ingredient')
    ///   - bom_stocks         (type_stock = 'produit_fabrique')
    ///
    /// Chips de filtrage par activité en bandeau.
    /// Coloration des lignes :
    ///   - Rouge   : QuantiteDispoReelle &lt;= 0  (pénurie)
    ///   - Orange  : QuantiteReservee &gt; 0       (réservation active)
    ///   - Vert    : disponible, DLC OK
    ///   - DLC indépendante : fond rouge si expirée, orange si &lt; 7 jours
    /// </summary>
    public class FrmVueStock : Form
    {
        // ── Couleurs — voir AppColors (TICKET-12) ────────────────────

        // ── Contrôles ────────────────────────────────────────────────
        private DataGridView    _dgv;
        private FlowLayoutPanel _flowChips;
        private Label           _lblTotal;
        private Panel           _pnlDetail;
        private Panel           _pnlDetailContent;

        // ── Données ──────────────────────────────────────────────────
        private List<Activite>      _activites;
        private List<VueStockGlobal> _lignes;
        private int                  _idActiviteFiltre;  // 0 = Tous

        public FrmVueStock()
        {
            BuildUI();
            Load  += (s, e) => Charger();
            Shown += (s, e) => _dgv.Focus();
        }

        // ════════════════════════════════════════════════════════════
        //  Construction de l'interface
        // ════════════════════════════════════════════════════════════

        private void BuildUI()
        {
            this.Text            = "Vue stock global";
            this.ClientSize      = new Size(980, 580);
            this.MinimumSize     = new Size(760, 420);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = Color.White;

            // ── Bandeau ───────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = AppColors.ChocoBrand,
                Padding   = new Padding(16, 0, 16, 0)
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "VUE STOCK GLOBAL",
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.Or,
                Dock      = DockStyle.Left,
                AutoSize  = false,
                Width     = 220,
                TextAlign = ContentAlignment.MiddleLeft
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = "Lots ingrédients + produits fabriqués — lecture seule",
                Font      = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = AppColors.HintOnDark,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            });

            // ── Chips de filtre par activité ──────────────────────────
            var pnlChips = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = AppColors.Creme,
                Padding   = new Padding(12, 6, 12, 6)
            };
            _flowChips = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0)
            };
            pnlChips.Controls.Add(_flowChips);

            // ── DGV ───────────────────────────────────────────────────
            _dgv = new DataGridView
            {
                Dock                    = DockStyle.Fill,
                ReadOnly                = true,
                AllowUserToAddRows      = false,
                AllowUserToDeleteRows   = false,
                AllowUserToResizeColumns = false,
                AllowUserToResizeRows    = false,
                SelectionMode           = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect             = false,
                RowHeadersVisible       = false,
                AutoSizeColumnsMode     = DataGridViewAutoSizeColumnsMode.AllCells,
                BackgroundColor         = Color.White,
                BorderStyle             = BorderStyle.None,
                ColumnHeadersHeight     = 32,
                Font                    = new Font("Segoe UI", 9.5F),
                GridColor               = Color.FromArgb(230, 220, 210)
            };
            _dgv.ColumnHeadersDefaultCellStyle.BackColor           = AppColors.Creme;
            _dgv.ColumnHeadersDefaultCellStyle.ForeColor           = AppColors.ChocoBrand;
            _dgv.ColumnHeadersDefaultCellStyle.Font                = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor  = AppColors.Creme;
            _dgv.DefaultCellStyle.SelectionBackColor               = Color.FromArgb(111, 78, 55);
            _dgv.DefaultCellStyle.SelectionForeColor               = Color.White;

            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "TypeStock",   HeaderText = "Type",          MinimumWidth = 80  });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nom",         HeaderText = "Nom",           MinimumWidth = 180 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Dispo",       HeaderText = "Disponible",    MinimumWidth = 110, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reservee",    HeaderText = "Réservé",       MinimumWidth = 100, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total",       HeaderText = "Total",         MinimumWidth = 100, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "DLC",         HeaderText = "DLC",           MinimumWidth = 90  });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "StockOuAct",  HeaderText = "Stock / Activité", MinimumWidth = 140 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "CoutUnit",    HeaderText = "Coût unit.",    MinimumWidth = 90, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "CoutTotal",   HeaderText = "Valeur stock",    MinimumWidth = 100, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });

            // Désactiver le tri natif — incompatible avec les headers de section groupés
            foreach (DataGridViewColumn col in _dgv.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

            _dgv.CellFormatting += Dgv_CellFormatting;
            _dgv.SelectionChanged += Dgv_SelectionChanged;

            // ── Volet détail (droite) ─────────────────────────────────
            _pnlDetail = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = 320,
                BackColor = Color.FromArgb(252, 250, 246),
                Visible   = false,
                AutoScroll = true,
                Padding   = new Padding(0)
            };
            _pnlDetail.Paint += (s, ev) =>
            {
                using (var pen = new Pen(AppColors.Border, 1))
                    ev.Graphics.DrawLine(pen, 0, 0, 0, _pnlDetail.Height);
            };

            _pnlDetailContent = new Panel
            {
                Dock      = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding   = new Padding(16, 12, 16, 12)
            };
            _pnlDetail.Controls.Add(_pnlDetailContent);

            // ── Bas — légende + stats + fermer ────────────────────────
            var pnlBas = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 52,
                BackColor = Color.FromArgb(245, 240, 235),
                Padding   = new Padding(16, 6, 16, 6)
            };

            // US-06 : bouton Export CSV — à gauche des légendes
            var btnCsv = new Button
            {
                Text      = "🖨 Exporter CSV",
                Font      = new Font("Segoe UI", 8.5F),
                BackColor = AppColors.ChocoMed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(120, 32),
                Location  = new Point(0, 10),
                Cursor    = Cursors.Hand
            };
            btnCsv.FlatAppearance.BorderSize = 0;
            btnCsv.Click += (s, e) => ExporterCsv();
            pnlBas.Controls.Add(btnCsv);

            CreerLegende(pnlBas, AppColors.VertDispo,   "Disponible",    130);
            CreerLegende(pnlBas, AppColors.OrangeReserv, "Réservé",      260);
            CreerLegende(pnlBas, AppColors.RougePenur,   "Pénurie / DLC", 390);

            _lblTotal = new Label
            {
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = AppColors.ChocoMed,
                AutoSize  = true,
                Location  = new Point(420, 16)
            };
            pnlBas.Controls.Add(_lblTotal);

            var btnFermer = new Button
            {
                Text      = "Fermer",
                Font      = new Font("Segoe UI", 9F),
                BackColor = AppColors.ChocoBrand,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(90, 32),
                Cursor    = Cursors.Hand
            };
            btnFermer.FlatAppearance.BorderSize = 0;
            btnFermer.Click += (s, e) => this.Close();
            pnlBas.Controls.Add(btnFermer);

            pnlBas.Resize += (s, e) =>
                btnFermer.Location = new Point(pnlBas.ClientSize.Width - 106, 10);
            btnFermer.Location = new Point(880 - 16, 10);   // sera recalculé au Resize

            // Ordre d'ajout (WinForms docking) : Fill en 1er, puis Right, Bottom, Top
            this.Controls.Add(_dgv);
            this.Controls.Add(_pnlDetail);
            this.Controls.Add(pnlBas);
            this.Controls.Add(pnlChips);
            this.Controls.Add(pnlHeader);
        }

        private void CreerLegende(Panel parent, Color couleur, string texte, int x)
        {
            var carré = new Panel
            {
                BackColor = couleur,
                Size      = new Size(16, 16),
                Location  = new Point(x, 18),
                BorderStyle = BorderStyle.FixedSingle
            };
            var lbl = new Label
            {
                Text      = texte,
                Font      = new Font("Segoe UI", 8.5F),
                ForeColor = AppColors.ChocoBrand,
                AutoSize  = true,
                Location  = new Point(x + 20, 17)
            };
            parent.Controls.Add(carré);
            parent.Controls.Add(lbl);
        }

        // ════════════════════════════════════════════════════════════
        //  Chargement
        // ════════════════════════════════════════════════════════════

        private void Charger()
        {
            _activites = ActiviteDAL.GetAll();
            BuildChips();
            AppliquerFiltre();
        }

        private void BuildChips()
        {
            _flowChips.Controls.Clear();
            AjouterChip("Tous", 0, selected: _idActiviteFiltre == 0);
            foreach (var act in _activites)
                AjouterChip(act.Nom, act.Id, selected: _idActiviteFiltre == act.Id);
        }

        private void AjouterChip(string texte, int idActivite, bool selected)
        {
            var chip = new Button
            {
                Text      = texte,
                Font      = new Font("Segoe UI", 9F, selected ? FontStyle.Bold : FontStyle.Regular),
                BackColor = selected ? AppColors.ChocoBrand : Color.FromArgb(220, 208, 192),
                ForeColor = selected ? Color.White     : AppColors.ChocoBrand,
                FlatStyle = FlatStyle.Flat,
                AutoSize  = false,
                Height    = 28,
                Width     = TextRenderer.MeasureText(texte, new Font("Segoe UI", 9F)).Width + 24,
                Margin    = new Padding(0, 0, 6, 0),
                Cursor    = Cursors.Hand,
                Tag       = idActivite
            };
            chip.FlatAppearance.BorderSize = 0;
            chip.Click += (s, ev) =>
            {
                _idActiviteFiltre = (int)((Button)s).Tag;
                BuildChips();
                AppliquerFiltre();
            };
            _flowChips.Controls.Add(chip);
        }

        private void AppliquerFiltre()
        {
            _lignes = _idActiviteFiltre == 0
                ? VueStockGlobalDAL.GetAll()
                : VueStockGlobalDAL.GetByActivite(_idActiviteFiltre);

            RemplirGrille();
        }

        private void RemplirGrille()
        {
            _dgv.Rows.Clear();

            // Déterminer l'ordre max par contexte pour distinguer intermédiaire vs final
            var ordreMaxParContexte = new Dictionary<int, int>();
            foreach (var l in _lignes.Where(x => !x.EstLot && x.IdContexte.HasValue))
            {
                int ctxId = l.IdContexte.Value;
                if (!ordreMaxParContexte.ContainsKey(ctxId))
                    ordreMaxParContexte[ctxId] = BomNiveauDAL.GetOrdreMax(ctxId);
            }

            // Catégoriser chaque ligne
            var ingredients    = _lignes.Where(l => l.EstLot).ToList();
            var intermediaires = _lignes.Where(l => !l.EstLot && l.IdNiveau.HasValue && l.IdContexte.HasValue
                && ordreMaxParContexte.ContainsKey(l.IdContexte.Value)
                && GetOrdreNiveau(l.IdNiveau.Value) < ordreMaxParContexte[l.IdContexte.Value]).ToList();
            var finaux         = _lignes.Where(l => !l.EstLot && l.IdNiveau.HasValue && l.IdContexte.HasValue
                && ordreMaxParContexte.ContainsKey(l.IdContexte.Value)
                && GetOrdreNiveau(l.IdNiveau.Value) >= ordreMaxParContexte[l.IdContexte.Value]).ToList();
            // Produits sans contexte (cas rare) → intermédiaires par défaut
            var sansContexte   = _lignes.Where(l => !l.EstLot && (!l.IdNiveau.HasValue || !l.IdContexte.HasValue)).ToList();
            intermediaires.AddRange(sansContexte);

            AjouterSectionHeader($"🥣  INGRÉDIENTS  ({ingredients.Count})");
            foreach (var l in ingredients) AjouterLigne(l, "Ingrédient");

            AjouterSectionHeader($"⚙  PRODUITS INTERMÉDIAIRES  ({intermediaires.Count})");
            foreach (var l in intermediaires) AjouterLigne(l, "Intermédiaire");

            AjouterSectionHeader($"🏆  PRODUITS FINALS  ({finaux.Count})");
            foreach (var l in finaux) AjouterLigne(l, "Produit final");

            _lblTotal.Text = $"{_lignes.Count} entrée{(_lignes.Count > 1 ? "s" : "")} " +
                             $"· {_lignes.Count(x => x.EstEnAlerte)} pénurie{(_lignes.Count(x => x.EstEnAlerte) > 1 ? "s" : "")}";
        }

        private void AjouterSectionHeader(string titre)
        {
            int idx = _dgv.Rows.Add(titre, "", "", "", "", "", "", "", "");
            var row = _dgv.Rows[idx];
            row.DefaultCellStyle.BackColor = AppColors.ChocoBrand;
            row.DefaultCellStyle.ForeColor = AppColors.Or;
            row.DefaultCellStyle.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            row.DefaultCellStyle.SelectionBackColor = AppColors.ChocoBrand;
            row.DefaultCellStyle.SelectionForeColor = AppColors.Or;
            row.Height = 30;
            row.Tag    = "section"; // marqueur pour éviter la coloration automatique
        }

        private void AjouterLigne(VueStockGlobal l, string typeLabel)
        {
            string u       = l.Unite ?? "";
            string dlcText  = l.DateDlc.HasValue ? l.DateDlc.Value.ToString("dd/MM/yyyy") : "—";
            string lieuText = l.EstLot ? (l.StockNom ?? "—") : (l.NomActivite ?? "—");
            // Ingrédients : afficher le prix du conditionnement (bouteille, sachet)
            // Produits fabriqués : afficher le coût unitaire de production
            string coutText = l.EstLot && l.PrixConditionnement.HasValue
                ? UnitConvertisseur.FormatPrix(l.PrixConditionnement.Value)
                : UnitConvertisseur.FormatPrix(l.CoutUnitaire);

            // Valeur stock = qté dispo × prix par unité de base (toujours correct en €)
            decimal valeurStock  = l.CoutUnitaire * l.QuantiteTotale;
            string valeurText    = valeurStock > 0 ? UnitConvertisseur.FormatPrix(valeurStock) : "—";

            int idx = _dgv.Rows.Add(
                typeLabel, l.Nom,
                UnitConvertisseur.FormatQte(l.QuantiteDispoReelle, u),
                l.QuantiteReservee > 0 ? UnitConvertisseur.FormatQte(l.QuantiteReservee, u) : "—",
                UnitConvertisseur.FormatQte(l.QuantiteTotale, u),
                dlcText, lieuText, coutText, valeurText);

            _dgv.Rows[idx].Tag = l;
        }

        /// <summary>Cache d'ordres de niveaux pour éviter des requêtes répétées.</summary>
        private readonly Dictionary<int, int> _cacheOrdreNiveau = new Dictionary<int, int>();
        private int GetOrdreNiveau(int idNiveau)
        {
            if (!_cacheOrdreNiveau.ContainsKey(idNiveau))
            {
                var niv = BomNiveauDAL.GetById(idNiveau);
                _cacheOrdreNiveau[idNiveau] = niv?.Ordre ?? 0;
            }
            return _cacheOrdreNiveau[idNiveau];
        }

        // ════════════════════════════════════════════════════════════
        //  US-06 — Export CSV
        // ════════════════════════════════════════════════════════════

        private void ExporterCsv()
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter           = "CSV (*.csv)|*.csv";
                dlg.FileName         = $"stock_global_{DateTime.Today:yyyy-MM-dd}.csv";
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (dlg.ShowDialog() != DialogResult.OK) return;

                using (var sw = new System.IO.StreamWriter(dlg.FileName, false, new System.Text.UTF8Encoding(true)))
                {
                    // En-tête
                    sw.WriteLine("Type;Nom;Disponible;Réservé;Total;DLC;Stock / Activité;Coût unit.;Valeur stock");

                    foreach (var l in _lignes)
                    {
                        string u         = l.Unite ?? "";
                        string dlcText   = l.DateDlc.HasValue ? l.DateDlc.Value.ToString("dd/MM/yyyy") : "";
                        string lieuText  = l.EstLot ? (l.StockNom ?? "") : (l.NomActivite ?? "");
                        string typeLabel = l.EstLot ? "Ingrédient" : "Produit BOM";

                        decimal prixAffiche = l.EstLot && l.PrixConditionnement.HasValue
                            ? l.PrixConditionnement.Value : l.CoutUnitaire;
                        decimal valeurStock = l.CoutUnitaire * l.QuantiteTotale;
                        sw.WriteLine(string.Join(";",
                            Escape(typeLabel),
                            Escape(l.Nom),
                            $"{l.QuantiteDispoReelle:F3} {u}",
                            $"{l.QuantiteReservee:F3} {u}",
                            $"{l.QuantiteTotale:F3} {u}",
                            dlcText,
                            Escape(lieuText),
                            prixAffiche > 0 ? prixAffiche.ToString("F2") : "",
                            valeurStock > 0 ? valeurStock.ToString("F2") : ""));
                    }
                }
                MessageBox.Show($"Fichier exporté :\n{dlg.FileName}", "Export réussi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>Échappe un champ CSV si il contient le séparateur ';'.</summary>
        private static string Escape(string s)
            => s != null && s.Contains(';') ? $"\"{s}\"" : (s ?? "");

        // ════════════════════════════════════════════════════════════
        //  VOLET DÉTAIL — sélection DGV
        // ════════════════════════════════════════════════════════════

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (_dgv.CurrentRow == null || !(_dgv.CurrentRow.Tag is VueStockGlobal item))
            {
                _pnlDetail.Visible = false;
                return;
            }
            AfficherDetail(item);
        }

        private void AfficherDetail(VueStockGlobal item)
        {
            _pnlDetailContent.SuspendLayout();
            _pnlDetailContent.Controls.Clear();

            int y = 0;
            if (item.EstLot)
                y = RenderDetailIngredient(item, y);
            else
                y = RenderDetailProduit(item, y);

            _pnlDetailContent.ResumeLayout();
            _pnlDetail.Visible = true;
        }

        // ── Détail INGRÉDIENT (lot) ──────────────────────────────────

        private int RenderDetailIngredient(VueStockGlobal item, int y)
        {
            Lot lot = null;
            Ingredient fiche = null;
            try
            {
                lot = LotDAL.GetById(item.IdEntree);
                if (lot != null)
                {
                    var fiches = IngredientDAL.GetAll();
                    fiche = fiches.FirstOrDefault(i => i.Id == lot.IdFicheIngredient);
                }
            }
            catch (Exception ex) { Trace.TraceError("Detail lot: {0}", ex); }

            // ── En-tête ──
            y = AddDetailHeader(item.Nom, "Ingrédient", y);

            // ── Identité ──
            y = AddDetailSection("IDENTITÉ", y);
            if (fiche != null)
            {
                if (!string.IsNullOrEmpty(fiche.Marque))
                    y = AddDetailRow("Marque", fiche.Marque, y);
                y = AddDetailRow("Type physique", fiche.TypePhysique, y);
                if (fiche.Densite.HasValue)
                    y = AddDetailRow("Densité", $"{fiche.Densite.Value:F3} g/ml", y);
                y = AddDetailRow("Unité de base", fiche.UniteMesure, y);
                string condLabel = !string.IsNullOrEmpty(fiche.ConditionnementLabel)
                    ? fiche.ConditionnementLabel
                    : UnitConvertisseur.FormatQte(fiche.QteParConditionnement, fiche.UniteMesure);
                y = AddDetailRow("Conditionnement", condLabel, y);
                y = AddDetailRow("Stock de rattach.", fiche.StockNom, y);
            }

            // ── Stock ──
            y = AddDetailSection("STOCK", y);
            string u = item.Unite ?? "";
            y = AddDetailRow("Disponible", UnitConvertisseur.FormatQte(item.QuantiteDispoReelle, u), y);
            if (item.QuantiteReservee > 0)
                y = AddDetailRow("Réservé", UnitConvertisseur.FormatQte(item.QuantiteReservee, u), y, AppColors.OrgWarn);
            y = AddDetailRow("Total", UnitConvertisseur.FormatQte(item.QuantiteTotale, u), y);

            if (fiche != null && fiche.SeuilAlerteStock.HasValue)
            {
                y = AddDetailRow("Seuil alerte", UnitConvertisseur.FormatQte(fiche.SeuilAlerteStock.Value, u), y);
                bool alert = fiche.EstEnAlerte;
                y = AddDetailRow("État", alert ? "⚠ EN ALERTE" : "✓ OK",
                    y, alert ? AppColors.RedCrit : AppColors.Success);
            }

            // ── Prix ──
            y = AddDetailSection("PRIX", y);
            if (lot != null)
            {
                y = AddDetailRow("Prix cond. HTVA", UnitConvertisseur.FormatPrix(lot.PrixUnitaire), y);
                y = AddDetailRow("Prix unité base", UnitConvertisseur.FormatPrix(lot.PrixUnitaireBase), y);
                y = AddDetailRow("Total achat HTVA", UnitConvertisseur.FormatPrix(lot.PrixAchatReel), y);
            }
            decimal valeur = item.CoutUnitaire * item.QuantiteTotale;
            y = AddDetailRow("Valeur stock", UnitConvertisseur.FormatPrix(valeur), y, AppColors.ChocoBrand);

            // ── Lot ──
            if (lot != null)
            {
                y = AddDetailSection("LOT", y);
                if (!string.IsNullOrEmpty(lot.NumeroLot))
                    y = AddDetailRow("N° lot", lot.NumeroLot, y);
                y = AddDetailRow("Date achat", lot.DateAchat.ToString("dd/MM/yyyy"), y);
                if (lot.DatePeremption.HasValue)
                {
                    bool expire = lot.DatePeremption.Value < DateTime.Today;
                    bool proche = !expire && (lot.DatePeremption.Value - DateTime.Today).TotalDays < 7;
                    y = AddDetailRow("DLC", lot.DatePeremption.Value.ToString("dd/MM/yyyy"),
                        y, expire ? AppColors.RedCrit : proche ? AppColors.OrgWarn : AppColors.ChocoMed);
                }
                if (!string.IsNullOrEmpty(lot.NomFournisseur))
                    y = AddDetailRow("Fournisseur", lot.NomFournisseur, y);
                if (!string.IsNullOrEmpty(lot.ReferenceFacture))
                    y = AddDetailRow("Réf. facture", lot.ReferenceFacture, y);
                if (!string.IsNullOrEmpty(lot.Notes))
                    y = AddDetailRow("Notes", lot.Notes, y);
            }

            // ── Utilisé dans ──
            if (lot != null)
            {
                try
                {
                    var recettes = BomFicheLigneDAL.GetFichesUtilisant(lot.IdFicheIngredient);
                    if (recettes.Count > 0)
                    {
                        y = AddDetailSection("UTILISÉ DANS", y);
                        foreach (var r in recettes)
                            y = AddDetailTag(r, y);
                    }
                }
                catch (Exception ex) { Trace.TraceError("Detail recettes: {0}", ex); }
            }

            return y;
        }

        // ── Détail PRODUIT FABRIQUÉ (bom_stock) ──────────────────────

        private int RenderDetailProduit(VueStockGlobal item, int y)
        {
            BomFiche fiche = null;
            BomStock stock = null;
            try
            {
                if (item.IdFicheBom.HasValue)
                    fiche = BomFicheDAL.GetById(item.IdFicheBom.Value, avecLignes: true);

                // Charger le bom_stock pour les détails de production
                if (item.IdNiveau.HasValue)
                {
                    var stocks = BomStockDAL.GetByNiveau(item.IdNiveau.Value);
                    stock = stocks.FirstOrDefault(s => s.Id == item.IdEntree);
                }
            }
            catch (Exception ex) { Trace.TraceError("Detail produit: {0}", ex); }

            // ── En-tête ──
            string typeLabel = "Produit fabriqué";
            y = AddDetailHeader(item.Nom, typeLabel, y);

            // ── Identité ──
            y = AddDetailSection("IDENTITÉ", y);
            y = AddDetailRow("Unité", item.Unite, y);
            if (item.NomActivite != null)
                y = AddDetailRow("Activité", item.NomActivite, y);
            if (stock != null)
            {
                y = AddDetailRow("Contexte", stock.NomContexte, y);
                y = AddDetailRow("Niveau", stock.NomNiveau, y);
            }
            if (fiche != null && fiche.TempsPreparation.HasValue)
                y = AddDetailRow("Temps prép.", $"{fiche.TempsPreparation.Value} min", y);

            // ── Stock ──
            y = AddDetailSection("STOCK", y);
            string u = item.Unite ?? "";
            y = AddDetailRow("Disponible", UnitConvertisseur.FormatQte(item.QuantiteDispoReelle, u), y);
            y = AddDetailRow("Coût unitaire", UnitConvertisseur.FormatPrix(item.CoutUnitaire), y);
            decimal valeur = item.CoutUnitaire * item.QuantiteTotale;
            y = AddDetailRow("Valeur stock", UnitConvertisseur.FormatPrix(valeur), y, AppColors.ChocoBrand);

            if (item.DateDlc.HasValue)
            {
                bool expire = item.DateDlc.Value < DateTime.Today;
                y = AddDetailRow("DLC", item.DateDlc.Value.ToString("dd/MM/yyyy"),
                    y, expire ? AppColors.RedCrit : AppColors.ChocoMed);
            }

            // ── Production source ──
            if (stock != null)
            {
                y = AddDetailSection("PRODUCTION", y);
                y = AddDetailRow("Date prod.", stock.DateProduction.ToString("dd/MM/yyyy HH:mm"), y);
                y = AddDetailRow("ID production", $"#{stock.IdProduction}", y);
            }

            // ── Composition (lignes de la fiche) ──
            if (fiche != null && fiche.Lignes != null && fiche.Lignes.Count > 0)
            {
                y = AddDetailSection("COMPOSITION", y);
                foreach (var ligne in fiche.Lignes)
                {
                    string qte = UnitConvertisseur.FormatQte(ligne.Quantite, ligne.UniteMesure);
                    y = AddDetailRow(ligne.NomInput, qte, y);
                }
            }

            // ── Consommé par ──
            if (item.IdFicheBom.HasValue)
            {
                try
                {
                    var consommePar = BomFicheLigneDAL.GetFichesConsommant(item.IdFicheBom.Value);
                    if (consommePar.Count > 0)
                    {
                        y = AddDetailSection("CONSOMMÉ PAR", y);
                        foreach (var r in consommePar)
                            y = AddDetailTag(r, y);
                    }
                }
                catch (Exception ex) { Trace.TraceError("Detail consommePar: {0}", ex); }
            }

            return y;
        }

        // ── Helpers de rendu du volet ─────────────────────────────────

        private int AddDetailHeader(string nom, string type, int y)
        {
            // Barre accent
            var accent = new Panel
            {
                Location = new Point(0, y), Size = new Size(288, 4),
                BackColor = AppColors.ChocoBrand
            };
            _pnlDetailContent.Controls.Add(accent);
            y += 8;

            _pnlDetailContent.Controls.Add(new Label
            {
                Text = nom, Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = AppColors.ChocoBrand, Location = new Point(0, y),
                AutoSize = true, MaximumSize = new Size(280, 0)
            });
            y += 28;

            var badge = new Label
            {
                Text = type, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = AppColors.ChocoMed, BackColor = Color.FromArgb(240, 235, 225),
                AutoSize = true, Padding = new Padding(6, 2, 6, 2),
                Location = new Point(0, y)
            };
            _pnlDetailContent.Controls.Add(badge);
            y += 24;

            return y;
        }

        private int AddDetailSection(string title, int y)
        {
            y += 6;
            _pnlDetailContent.Controls.Add(new Label
            {
                Text = title, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = AppColors.SidebarMeta, Location = new Point(0, y),
                AutoSize = true
            });
            y += 18;

            // Ligne séparatrice
            var line = new Panel
            {
                Location = new Point(0, y - 2), Size = new Size(280, 1),
                BackColor = AppColors.Line1
            };
            _pnlDetailContent.Controls.Add(line);

            return y;
        }

        private int AddDetailRow(string label, string value, int y, Color? valueColor = null)
        {
            _pnlDetailContent.Controls.Add(new Label
            {
                Text = label, Font = new Font("Segoe UI", 8.5F),
                ForeColor = AppColors.ChocoMed, Location = new Point(0, y),
                AutoSize = true
            });
            _pnlDetailContent.Controls.Add(new Label
            {
                Text = value ?? "—", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = valueColor ?? AppColors.ChocoBrand,
                Location = new Point(120, y), AutoSize = true,
                MaximumSize = new Size(168, 0)
            });
            return y + 20;
        }

        private int AddDetailTag(string text, int y)
        {
            var tag = new Label
            {
                Text = text, Font = new Font("Segoe UI", 8F),
                ForeColor = AppColors.ChocoBrand, BackColor = Color.FromArgb(245, 240, 232),
                AutoSize = true, Padding = new Padding(6, 2, 6, 2),
                Location = new Point(0, y)
            };
            tag.Paint += (s, ev) =>
            {
                using (var pen = new Pen(AppColors.Border, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, tag.Width - 1, tag.Height - 1);
            };
            _pnlDetailContent.Controls.Add(tag);
            return y + 24;
        }

        // ── Coloration des cellules ───────────────────────────────────

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _dgv.Rows.Count) return;
            var row = _dgv.Rows[e.RowIndex];
            if (row.Tag is string s && s == "section") return; // header de section
            if (!(row.Tag is VueStockGlobal l)) return;

            bool selected = (row.State & DataGridViewElementStates.Selected) != 0;
            if (selected) return;   // laisser la sélection gérer la couleur

            // Couleur de fond de la ligne entière
            Color fond;
            if (l.EstEnAlerte)
                fond = AppColors.RougePenur;
            else if (l.QuantiteReservee > 0)
                fond = AppColors.OrangeReserv;
            else
                fond = AppColors.VertDispo;

            row.DefaultCellStyle.BackColor = fond;

            // Colonne DLC : surcharge si DLC proche ou expirée
            if (e.ColumnIndex == _dgv.Columns["DLC"].Index && l.DateDlc.HasValue)
            {
                var today = DateTime.Today;
                if (l.DateDlc.Value < today)
                    e.CellStyle.BackColor = Color.FromArgb(220, 80, 80);      // rouge vif
                else if ((l.DateDlc.Value - today).TotalDays < 7)
                    e.CellStyle.BackColor = Color.FromArgb(255, 165, 0);      // orange vif
            }
        }
    }
}
