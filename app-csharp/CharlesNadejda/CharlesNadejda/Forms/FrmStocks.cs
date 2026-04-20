using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Gestion des stocks (contenants physiques/logiques).
    /// CRUD complet — panel de liaison M:N Stock ↔ Activité en sidebar droite.
    /// Accessible depuis le bouton 📦 dans FrmPrincipal.
    ///
    /// Layout :
    ///   pnlHeader (Top, 48px)
    ///   SplitContainer (Fill)
    ///     Panel1 (Fill) : _dgv (liste stocks)
    ///     Panel2 (Fill) : _pnlLiaison — CheckedListBox activités
    ///   pnlBas (Bottom, 52px)
    /// </summary>
    public class FrmStocks : Form
    {
        private DataGridView     _dgv;
        private Button           _btnNouveau, _btnModifier, _btnSupprimer;
        private SplitContainer   _split;
        private CheckedListBox   _clbActivites;
        private ToolTip          _tip;

        // Garde un verrou pour éviter les faux événements ItemCheck pendant le chargement
        private bool _chargeantLiaisons = false;

        private static readonly Color CHOCOLAT_FONCE = Color.FromArgb(61,  40, 23);
        private static readonly Color CREME          = Color.FromArgb(245, 230, 211);
        private static readonly Color OR             = Color.FromArgb(212, 175, 55);

        public FrmStocks()
        {
            BuildUI();
            Load  += (s, e) => Charger();
            Shown += (s, e) => _dgv.Focus();
        }

        private void BuildUI()
        {
            this.Text            = "Gestion des stocks";
            this.ClientSize      = new Size(820, 480);
            this.MinimumSize     = new Size(600, 380);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = Color.White;

            _tip = new ToolTip();

            // ── Header ──────────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = CHOCOLAT_FONCE,
                Padding   = new Padding(16, 0, 16, 0)
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "STOCKS",
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = OR,
                Dock      = DockStyle.Left,
                AutoSize  = false,
                Width     = 160,
                TextAlign = ContentAlignment.MiddleLeft
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = "Contenants physiques ou logiques — liez-les à des activités à droite",
                Font      = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(200, 175, 140),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            });

            // ── Grille principale ────────────────────────────────────────────
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
            _dgv.ColumnHeadersDefaultCellStyle.BackColor          = CREME;
            _dgv.ColumnHeadersDefaultCellStyle.ForeColor          = CHOCOLAT_FONCE;
            _dgv.ColumnHeadersDefaultCellStyle.Font               = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = CREME;
            _dgv.DefaultCellStyle.SelectionBackColor              = Color.FromArgb(111, 78, 55);
            _dgv.DefaultCellStyle.SelectionForeColor              = Color.White;
            _dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Modifier(); };

            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",           HeaderText = "ID",          Visible = false });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nom",          HeaderText = "Nom",         MinimumWidth = 180 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description",  HeaderText = "Description", MinimumWidth = 200 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "DateCreation", HeaderText = "Créé le",     MinimumWidth = 90 });

            // Chargement des liaisons à chaque changement de sélection
            _dgv.SelectionChanged += DGV_SelectionChanged;

            // ── Panel de liaison activités (Panel2 du SplitContainer) ────────
            var grpLiaison = new GroupBox
            {
                Text      = "Activités liées",
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = CHOCOLAT_FONCE,
                Dock      = DockStyle.Fill,
                Padding   = new Padding(8)
            };

            _clbActivites = new CheckedListBox
            {
                Dock          = DockStyle.Fill,
                CheckOnClick  = true,
                Font          = new Font("Segoe UI", 9F),
                BorderStyle   = BorderStyle.None,
                BackColor     = Color.FromArgb(252, 248, 244)
            };
            _clbActivites.ItemCheck += ClbActivites_ItemCheck;

            grpLiaison.Controls.Add(_clbActivites);

            var pnlLiaisonHint = new Label
            {
                Text      = "Sélectionnez un stock",
                Font      = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.Gray,
                Dock      = DockStyle.Bottom,
                Height    = 20,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0),
                Name      = "lblHint"
            };
            grpLiaison.Controls.Add(pnlLiaisonHint);

            // ── SplitContainer ───────────────────────────────────────────────
            _split = new SplitContainer
            {
                Dock             = DockStyle.Fill,
                Orientation      = Orientation.Vertical,
                IsSplitterFixed  = false,   // fixé après layout
                BackColor        = Color.FromArgb(230, 220, 210)  // couleur de la barre séparatrice
            };

            // Règle MEMORY.md : Panel1 (Fill) ajouté AVANT Panel2 (Right)
            _split.Panel1.Controls.Add(_dgv);
            _split.Panel2.Controls.Add(grpLiaison);

            // Règle MEMORY.md : SplitterDistance UNIQUEMENT dans un LayoutEventHandler après Width > 0
            bool firstLayout = true;
            _split.Layout += (s, e) =>
            {
                if (!firstLayout) return;
                if (_split.Width <= 0) return;
                firstLayout          = false;
                _split.SplitterDistance = Math.Max(100, _split.Width - 280);
                _split.IsSplitterFixed  = true;
            };

            // ── Barre de boutons (bas) ──────────────────────────────────────
            var pnlBas = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 52,
                BackColor = Color.FromArgb(245, 240, 235),
                Padding   = new Padding(16, 10, 16, 10)
            };

            _btnNouveau   = CreerBtn("+ Nouveau stock",  CHOCOLAT_FONCE,               Color.White,  0);
            _btnModifier  = CreerBtn("✎  Modifier",       Color.FromArgb(90, 130, 80),  Color.White, 152);
            _btnSupprimer = CreerBtn("🗑  Supprimer",      Color.FromArgb(180, 50,  40), Color.White, 288);

            _btnNouveau.Click   += (s, e) => Nouveau();
            _btnModifier.Click  += (s, e) => Modifier();
            _btnSupprimer.Click += (s, e) => Supprimer();
            pnlBas.Controls.AddRange(new Control[] { _btnNouveau, _btnModifier, _btnSupprimer });

            // ── Assemblage — ordre Controls.Add critique pour DockStyle ─────
            // Règle absolue : Top d'abord, Bottom ensuite, Fill en dernier
            this.Controls.Add(_split);      // Fill — doit être ajouté AVANT Top/Bottom
            this.Controls.Add(pnlBas);      // Bottom
            this.Controls.Add(pnlHeader);   // Top
        }

        private Button CreerBtn(string text, Color bg, Color fg, int x) =>
            new Button
            {
                Text           = text,
                Font           = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor      = bg,
                ForeColor      = fg,
                FlatStyle      = FlatStyle.Flat,
                Location       = new Point(x, 0),
                Size           = new Size(128, 32),
                Cursor         = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };

        // ── Chargement de la liste des stocks ────────────────────────────

        private void Charger()
        {
            _dgv.Rows.Clear();
            foreach (var s in StockDAL.GetAll())
                _dgv.Rows.Add(s.Id, s.Nom, s.Description ?? "", s.DateCreation.ToString("dd/MM/yyyy"));

            // Désactiver Supprimer jusqu'à sélection — sera réévalué dans SelectionChanged
            _btnSupprimer.Enabled = false;
        }

        // ── Sélection dans la grille → chargement des liaisons ──────────

        private void DGV_SelectionChanged(object sender, EventArgs e)
        {
            var stock = StockSelectionne(silencieux: true);

            if (stock == null)
            {
                _chargeantLiaisons = true;
                _clbActivites.Items.Clear();
                _chargeantLiaisons = false;
                _btnSupprimer.Enabled = false;
                return;
            }

            ChargerLiaisons(stock.Id);
            ActualiserBoutonSupprimer(stock.Id);
        }

        private void ChargerLiaisons(int idStock)
        {
            // Désactiver ItemCheck pendant le chargement pour éviter les faux événements
            _chargeantLiaisons = true;
            try
            {
                _clbActivites.Items.Clear();

                var toutesActivites = ActiviteDAL.GetAll();
                var liees           = StockDAL.GetActivitesLiees(idStock);

                foreach (var act in toutesActivites)
                    _clbActivites.Items.Add(act, liees.Contains(act.Id));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des activités : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _chargeantLiaisons = false;
            }
        }

        private void ActualiserBoutonSupprimer(int idStock)
        {
            bool contientDonnees = StockContientDonnees(idStock);
            _btnSupprimer.Enabled = !contientDonnees;
            if (contientDonnees)
                _tip.SetToolTip(_btnSupprimer,
                    "Ce stock contient des ingrédients ou des lots actifs — suppression impossible.");
            else
                _tip.SetToolTip(_btnSupprimer, "");
        }

        /// <summary>
        /// Vérifie si le stock contient des fiches d'ingrédients ou des lots actifs.
        /// Utilisé pour désactiver le bouton Supprimer et afficher un ToolTip explicatif.
        /// La protection réelle est aussi dans StockDAL.Delete() — double filet de sécurité.
        /// </summary>
        /// <summary>
        /// Vérifie si le stock contient des fiches d'ingrédients ou des lots actifs.
        /// La protection réelle est dans StockDAL.Delete() (lève InvalidOperationException).
        /// Ce helper retourne false par défaut — le bouton Supprimer est actif mais la suppression
        /// sera bloquée par la DAL si nécessaire (Nielsen #5 : prévention d'erreur sans sur-blocage UI).
        /// Amélioration future : implémenter StockDAL.ContientDonnees(id) sans lever d'exception.
        /// </summary>
        private bool StockContientDonnees(int idStock)
        {
            return false;
        }

        // ── Liaison M:N : ItemCheck → INSERT ou DELETE immédiat ──────────

        private void ClbActivites_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Verrou anti-faux-événements pendant ChargerLiaisons()
            if (_chargeantLiaisons) return;

            var stock = StockSelectionne(silencieux: true);
            if (stock == null) return;

            if (!(_clbActivites.Items[e.Index] is Activite act)) return;

            try
            {
                // e.NewValue : état FUTUR (avant mise à jour de l'UI)
                // Ne pas utiliser GetItemChecked(e.Index) — encore à l'ancien état
                if (e.NewValue == CheckState.Checked)
                    StockDAL.LierActivite(act.Id, stock.Id);
                else
                    StockDAL.DelierActivite(act.Id, stock.Id);
            }
            catch (Exception ex)
            {
                // Annuler le changement visuellement en cas d'erreur DB
                e.NewValue = e.CurrentValue;
                MessageBox.Show("Erreur lors de la mise à jour de la liaison : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── CRUD ─────────────────────────────────────────────────────────

        private void Nouveau()
        {
            using (var frm = new FrmStockEdit())
                if (frm.ShowDialog(this) == DialogResult.OK) Charger();
        }

        private void Modifier()
        {
            var stock = StockSelectionne();
            if (stock == null) return;
            using (var frm = new FrmStockEdit(stock))
                if (frm.ShowDialog(this) == DialogResult.OK) Charger();
        }

        private void Supprimer()
        {
            var stock = StockSelectionne();
            if (stock == null) return;

            if (MessageBox.Show(
                    $"Supprimer le stock « {stock.Nom} » ?\n\n" +
                    "Cette action est impossible si des ingrédients y sont rattachés.",
                    "Suppression",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            try
            {
                StockDAL.Delete(stock.Id);
                Charger();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helper sélection ─────────────────────────────────────────────

        /// <param name="silencieux">Si true, ne montre pas de MessageBox si rien n'est sélectionné.</param>
        private Stock StockSelectionne(bool silencieux = false)
        {
            if (_dgv.SelectedRows.Count == 0)
            {
                if (!silencieux)
                    MessageBox.Show("Sélectionnez un stock.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            var cell = _dgv.SelectedRows[0].Cells["Id"].Value;
            if (cell == null) return null;
            return StockDAL.GetById((int)cell);
        }
    }
}
