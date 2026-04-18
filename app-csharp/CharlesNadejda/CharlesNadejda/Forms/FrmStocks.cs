using System;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Gestion des stocks (contenants physiques/logiques).
    /// CRUD complet — accessible depuis le bouton 📦 dans FrmPrincipal.
    /// </summary>
    public class FrmStocks : Form
    {
        private DataGridView _dgv;
        private Button _btnNouveau, _btnModifier, _btnSupprimer;

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
            this.ClientSize      = new Size(620, 420);
            this.MinimumSize     = new Size(500, 360);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = Color.White;

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = CHOCOLAT_FONCE, Padding = new Padding(16, 0, 16, 0) };
            pnlHeader.Controls.Add(new Label { Text = "STOCKS", Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = OR, Dock = DockStyle.Left, AutoSize = false, Width = 160, TextAlign = ContentAlignment.MiddleLeft });
            pnlHeader.Controls.Add(new Label { Text = "Contenants physiques ou logiques — indépendants des activités", Font = new Font("Segoe UI", 8F, FontStyle.Italic), ForeColor = Color.FromArgb(200, 175, 140), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft });

            _dgv = new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 32, Font = new Font("Segoe UI", 9.5F),
                GridColor = Color.FromArgb(230, 220, 210)
            };
            _dgv.ColumnHeadersDefaultCellStyle.BackColor            = CREME;
            _dgv.ColumnHeadersDefaultCellStyle.ForeColor            = CHOCOLAT_FONCE;
            _dgv.ColumnHeadersDefaultCellStyle.Font                 = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor   = CREME;
            _dgv.DefaultCellStyle.SelectionBackColor                = Color.FromArgb(111, 78, 55);
            _dgv.DefaultCellStyle.SelectionForeColor                = Color.White;
            _dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Modifier(); };

            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",          HeaderText = "ID",          Visible = false });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nom",         HeaderText = "Nom",         MinimumWidth = 180 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description",  HeaderText = "Description", MinimumWidth = 240 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "DateCreation", HeaderText = "Créé le",     MinimumWidth = 100 });

            var pnlBas = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = Color.FromArgb(245, 240, 235), Padding = new Padding(16, 10, 16, 10) };
            _btnNouveau   = CreerBtn("+ Nouveau stock",  CHOCOLAT_FONCE,              Color.White,  0);
            _btnModifier  = CreerBtn("✎  Modifier",      Color.FromArgb(90, 130, 80), Color.White, 152);
            _btnSupprimer = CreerBtn("🗑  Supprimer",     Color.FromArgb(180, 50,  40),Color.White, 288);

            _btnNouveau.Click   += (s, e) => Nouveau();
            _btnModifier.Click  += (s, e) => Modifier();
            _btnSupprimer.Click += (s, e) => Supprimer();
            pnlBas.Controls.AddRange(new Control[] { _btnNouveau, _btnModifier, _btnSupprimer });

            this.Controls.Add(_dgv);
            this.Controls.Add(pnlBas);
            this.Controls.Add(pnlHeader);
        }

        private Button CreerBtn(string text, Color bg, Color fg, int x) =>
            new Button
            {
                Text = text, Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = bg, ForeColor = fg, FlatStyle = FlatStyle.Flat,
                Location = new Point(x, 0), Size = new Size(128, 32), Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };

        private void Charger()
        {
            _dgv.Rows.Clear();
            foreach (var s in StockDAL.GetAll())
                _dgv.Rows.Add(s.Id, s.Nom, s.Description ?? "", s.DateCreation.ToString("dd/MM/yyyy"));
        }

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

        private Stock StockSelectionne()
        {
            if (_dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un stock.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            return StockDAL.GetById((int)_dgv.SelectedRows[0].Cells["Id"].Value);
        }
    }
}
