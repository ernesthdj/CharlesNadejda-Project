using System;
using System.Collections.Generic;
using System.Drawing;
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

            // Désactiver le tri natif — incompatible avec les headers de section groupés
            foreach (DataGridViewColumn col in _dgv.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

            _dgv.CellFormatting += Dgv_CellFormatting;

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

            // Ordre d'ajout (WinForms docking) : Fill en 1er
            this.Controls.Add(_dgv);
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
            int idx = _dgv.Rows.Add(titre, "", "", "", "", "", "", "");
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
            string coutText = UnitConvertisseur.FormatPrix(l.CoutUnitaire);

            int idx = _dgv.Rows.Add(
                typeLabel, l.Nom,
                UnitConvertisseur.FormatQte(l.QuantiteDispoReelle, u),
                l.QuantiteReservee > 0 ? UnitConvertisseur.FormatQte(l.QuantiteReservee, u) : "—",
                UnitConvertisseur.FormatQte(l.QuantiteTotale, u),
                dlcText, lieuText, coutText);

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
                    sw.WriteLine("Type;Nom;Disponible;Réservé;Total;DLC;Stock / Activité;Coût unit.");

                    foreach (var l in _lignes)
                    {
                        string u         = l.Unite ?? "";
                        string dlcText   = l.DateDlc.HasValue ? l.DateDlc.Value.ToString("dd/MM/yyyy") : "";
                        string lieuText  = l.EstLot ? (l.StockNom ?? "") : (l.NomActivite ?? "");
                        string typeLabel = l.EstLot ? "Ingrédient" : "Produit BOM";

                        sw.WriteLine(string.Join(";",
                            Escape(typeLabel),
                            Escape(l.Nom),
                            $"{l.QuantiteDispoReelle:F3} {u}",
                            $"{l.QuantiteReservee:F3} {u}",
                            $"{l.QuantiteTotale:F3} {u}",
                            dlcText,
                            Escape(lieuText),
                            l.CoutUnitaire > 0 ? l.CoutUnitaire.ToString("F4") : ""));
                    }
                }
                MessageBox.Show($"Fichier exporté :\n{dlg.FileName}", "Export réussi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>Échappe un champ CSV si il contient le séparateur ';'.</summary>
        private static string Escape(string s)
            => s != null && s.Contains(';') ? $"\"{s}\"" : (s ?? "");

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
