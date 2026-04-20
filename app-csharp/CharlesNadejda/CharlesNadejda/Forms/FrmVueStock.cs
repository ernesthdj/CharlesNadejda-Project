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
        // ── Couleurs ─────────────────────────────────────────────────
        private static readonly Color CHOCOLAT_FONCE = Color.FromArgb(61,  40, 23);
        private static readonly Color CHOCOLAT_MOYEN = Color.FromArgb(111, 78, 55);
        private static readonly Color CREME          = Color.FromArgb(245, 230, 211);
        private static readonly Color OR             = Color.FromArgb(212, 175, 55);
        private static readonly Color VERT_DISPO     = Color.FromArgb(224, 243, 224);
        private static readonly Color ORANGE_RESERV  = Color.FromArgb(255, 237, 204);
        private static readonly Color ROUGE_PENUR    = Color.FromArgb(255, 218, 218);

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
                BackColor = CHOCOLAT_FONCE,
                Padding   = new Padding(16, 0, 16, 0)
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "VUE STOCK GLOBAL",
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = OR,
                Dock      = DockStyle.Left,
                AutoSize  = false,
                Width     = 220,
                TextAlign = ContentAlignment.MiddleLeft
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = "Lots ingrédients + produits fabriqués — lecture seule",
                Font      = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(200, 175, 140),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            });

            // ── Chips de filtre par activité ──────────────────────────
            var pnlChips = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = CREME,
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
            _dgv.ColumnHeadersDefaultCellStyle.BackColor           = CREME;
            _dgv.ColumnHeadersDefaultCellStyle.ForeColor           = CHOCOLAT_FONCE;
            _dgv.ColumnHeadersDefaultCellStyle.Font                = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor  = CREME;
            _dgv.DefaultCellStyle.SelectionBackColor               = Color.FromArgb(111, 78, 55);
            _dgv.DefaultCellStyle.SelectionForeColor               = Color.White;

            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "TypeStock",   HeaderText = "Type",          MinimumWidth = 80  });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nom",         HeaderText = "Nom",           MinimumWidth = 180 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Dispo",       HeaderText = "Disponible",    MinimumWidth = 90, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reservee",    HeaderText = "Réservé",       MinimumWidth = 80, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total",       HeaderText = "Total",         MinimumWidth = 80, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Unite",       HeaderText = "Unité",         MinimumWidth = 60  });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "DLC",         HeaderText = "DLC",           MinimumWidth = 90  });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "StockOuAct",  HeaderText = "Stock / Activité", MinimumWidth = 140 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "CoutUnit",    HeaderText = "Coût unit.",    MinimumWidth = 90, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight } });

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
                BackColor = CHOCOLAT_MOYEN,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(120, 32),
                Location  = new Point(0, 10),
                Cursor    = Cursors.Hand
            };
            btnCsv.FlatAppearance.BorderSize = 0;
            btnCsv.Click += (s, e) => ExporterCsv();
            pnlBas.Controls.Add(btnCsv);

            CreerLegende(pnlBas, VERT_DISPO,   "Disponible",    130);
            CreerLegende(pnlBas, ORANGE_RESERV, "Réservé",      260);
            CreerLegende(pnlBas, ROUGE_PENUR,   "Pénurie / DLC", 390);

            _lblTotal = new Label
            {
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = CHOCOLAT_MOYEN,
                AutoSize  = true,
                Location  = new Point(420, 16)
            };
            pnlBas.Controls.Add(_lblTotal);

            var btnFermer = new Button
            {
                Text      = "Fermer",
                Font      = new Font("Segoe UI", 9F),
                BackColor = CHOCOLAT_FONCE,
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
                ForeColor = CHOCOLAT_FONCE,
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
                BackColor = selected ? CHOCOLAT_FONCE : Color.FromArgb(220, 208, 192),
                ForeColor = selected ? Color.White     : CHOCOLAT_FONCE,
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

            var today = DateTime.Today;

            foreach (var l in _lignes)
            {
                string typeLabel  = l.EstLot ? "Ingrédient" : "Produit BOM";
                string dlcText    = l.DateDlc.HasValue ? l.DateDlc.Value.ToString("dd/MM/yyyy") : "—";
                string lieuText   = l.EstLot ? (l.StockNom ?? "—") : (l.NomActivite ?? "—");
                string coutText   = l.CoutUnitaire > 0
                    ? (l.CoutUnitaire < 0.01m
                        ? l.CoutUnitaire.ToString("F4")
                        : l.CoutUnitaire.ToString("F2")) + " €"
                    : "—";

                int idx = _dgv.Rows.Add(
                    typeLabel,
                    l.Nom,
                    l.QuantiteDispoReelle.ToString("F3"),
                    l.QuantiteReservee > 0 ? l.QuantiteReservee.ToString("F3") : "—",
                    l.QuantiteTotale.ToString("F3"),
                    l.Unite,
                    dlcText,
                    lieuText,
                    coutText);

                // Tag pour la coloration
                _dgv.Rows[idx].Tag = l;
            }

            _lblTotal.Text = $"{_lignes.Count} entrée{(_lignes.Count > 1 ? "s" : "")} " +
                             $"· {_lignes.Count(x => x.EstEnAlerte)} pénurie{(_lignes.Count(x => x.EstEnAlerte) > 1 ? "s" : "")}";
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
                    sw.WriteLine("Type;Nom;Disponible;Réservé;Total;Unité;DLC;Stock / Activité;Coût unit.");

                    foreach (var l in _lignes)
                    {
                        string dlcText   = l.DateDlc.HasValue ? l.DateDlc.Value.ToString("dd/MM/yyyy") : "";
                        string lieuText  = l.EstLot ? (l.StockNom ?? "") : (l.NomActivite ?? "");
                        string typeLabel = l.EstLot ? "Ingrédient" : "Produit BOM";

                        sw.WriteLine(string.Join(";",
                            Escape(typeLabel),
                            Escape(l.Nom),
                            l.QuantiteDispoReelle.ToString("F3"),
                            l.QuantiteReservee.ToString("F3"),
                            l.QuantiteTotale.ToString("F3"),
                            Escape(l.Unite),
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
            if (!(row.Tag is VueStockGlobal l)) return;

            bool selected = (row.State & DataGridViewElementStates.Selected) != 0;
            if (selected) return;   // laisser la sélection gérer la couleur

            // Couleur de fond de la ligne entière
            Color fond;
            if (l.EstEnAlerte)
                fond = ROUGE_PENUR;
            else if (l.QuantiteReservee > 0)
                fond = ORANGE_RESERV;
            else
                fond = VERT_DISPO;

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
