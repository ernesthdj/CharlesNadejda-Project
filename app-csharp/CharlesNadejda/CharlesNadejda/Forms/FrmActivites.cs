using System;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Gestion des activités artisanales (CRUD).
    /// Accessible depuis le bouton ⚙ du bandeau dans FrmPrincipal.
    /// Pattern : classe non-partial, UI construite programmatiquement.
    /// </summary>
    public class FrmActivites : Form
    {
        private DataGridView _dgv;
        private Button _btnNouveau;
        private Button _btnModifier;
        private Button _btnDesactiver;
        private Button _btnSupprimer;
        private Button _btnStocks;

        private static readonly Color CHOCOLAT_FONCE = Color.FromArgb(61, 40, 23);
        private static readonly Color CHOCOLAT_MOYEN = Color.FromArgb(111, 78, 55);
        private static readonly Color CREME          = Color.FromArgb(245, 230, 211);
        private static readonly Color OR             = Color.FromArgb(212, 175, 55);

        public FrmActivites()
        {
            BuildUI();
            Load    += (s, e) => Charger();
            Shown   += (s, e) => _dgv.Focus();
        }

        private void BuildUI()
        {
            this.Text            = "Gestion des activités";
            this.ClientSize      = new Size(620, 420);
            this.MinimumSize     = new Size(540, 360);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = Color.White;

            // ── En-tête ──────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = CHOCOLAT_FONCE,
                Padding   = new Padding(16, 0, 16, 0)
            };
            var lblTitre = new Label
            {
                Text      = "ACTIVITÉS",
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = OR,
                Dock      = DockStyle.Left,
                AutoSize  = false,
                Width     = 200,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblNote = new Label
            {
                Text      = "Chaque activité possède son propre stock d'ingrédients",
                Font      = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(200, 175, 140),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblNote);
            pnlHeader.Controls.Add(lblTitre);   // Gestalt proximité : titre d'abord

            // ── DataGridView ─────────────────────────────────────────────
            _dgv = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                RowHeadersVisible     = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.AllCells,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                ColumnHeadersHeight   = 32,
                Font                  = new Font("Segoe UI", 9.5F),
                GridColor             = Color.FromArgb(230, 220, 210)
            };
            _dgv.ColumnHeadersDefaultCellStyle.BackColor   = CREME;
            _dgv.ColumnHeadersDefaultCellStyle.ForeColor   = CHOCOLAT_FONCE;
            _dgv.ColumnHeadersDefaultCellStyle.Font        = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = CREME;
            _dgv.DefaultCellStyle.SelectionBackColor        = CHOCOLAT_MOYEN;
            _dgv.DefaultCellStyle.SelectionForeColor        = Color.White;
            _dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) Modifier(); };

            // Colonnes explicites
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",          HeaderText = "ID",          MinimumWidth = 40,  Visible = false });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nom",         HeaderText = "Nom",         MinimumWidth = 160 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description",  HeaderText = "Description", MinimumWidth = 200 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "DateCreation", HeaderText = "Créée le",    MinimumWidth = 100 });

            // ── Barre d'actions (Bottom) ──────────────────────────────────
            var pnlBas = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 52,
                BackColor = Color.FromArgb(245, 240, 235),
                Padding   = new Padding(16, 10, 16, 10)
            };

            // Fitts : action principale (Nouveau) à gauche, actions secondaires à droite
            // 5 boutons × 104px + 4 espaces × 14px = 576px = largeur utile (620 - 2×22 padding)
            _btnNouveau    = CreerBouton("+ Nouvelle activité", CHOCOLAT_FONCE,                   Color.White,   0);
            _btnModifier   = CreerBouton("✎  Modifier",         Color.FromArgb(90, 130, 80),      Color.White, 118);
            _btnDesactiver = CreerBouton("✕  Désactiver",        Color.FromArgb(160, 120, 60),     Color.White, 236);
            _btnSupprimer  = CreerBouton("🗑  Supprimer",         Color.FromArgb(180, 50,  40),     Color.White, 354);
            _btnStocks     = CreerBouton("📦 Stocks liés",        Color.FromArgb(60,  110, 160),    Color.White, 472);

            _btnNouveau.Click    += (s, e) => Nouveau();
            _btnModifier.Click   += (s, e) => Modifier();
            _btnDesactiver.Click += (s, e) => Desactiver();
            _btnSupprimer.Click  += (s, e) => Supprimer();
            _btnStocks.Click     += (s, e) => GererStocks();

            pnlBas.Controls.AddRange(new Control[] { _btnNouveau, _btnModifier, _btnDesactiver, _btnSupprimer, _btnStocks });

            // Ordre d'ajout critique pour le docking WinForms :
            // Fill en index 0 → traité en dernier → prend l'espace restant après Top et Bottom
            this.Controls.Add(_dgv);       // index 0 — Fill
            this.Controls.Add(pnlBas);     // index 1 — Bottom
            this.Controls.Add(pnlHeader);  // index 2 — Top (traité en 1er, pousse le Fill vers le bas)
        }

        private Button CreerBouton(string text, Color bg, Color fg, int x)
        {
            var btn = new Button
            {
                Text      = text,
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(x, 0),
                Size      = new Size(104, 32),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void Charger()
        {
            _dgv.Rows.Clear();
            foreach (var a in ActiviteDAL.GetAll())
                _dgv.Rows.Add(a.Id, a.Nom, a.Description ?? "", a.DateCreation.ToString("dd/MM/yyyy"));
        }

        private void Nouveau()
        {
            using (var frm = new FrmActiviteEdit())
                if (frm.ShowDialog(this) == DialogResult.OK) Charger();
        }

        private void Modifier()
        {
            var activite = ActiviteSelectionnee();
            if (activite == null) return;
            using (var frm = new FrmActiviteEdit(activite))
                if (frm.ShowDialog(this) == DialogResult.OK) Charger();
        }

        private void Desactiver()
        {
            var activite = ActiviteSelectionnee();
            if (activite == null) return;

            if (MessageBox.Show(
                    $"Désactiver l'activité « {activite.Nom} » ?\n\n" +
                    "Elle sera masquée mais ses données historiques seront conservées.",
                    "Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;

            try
            {
                ActiviteDAL.Desactiver(activite.Id);
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

        private void Supprimer()
        {
            var activite = ActiviteSelectionnee();
            if (activite == null) return;

            if (MessageBox.Show(
                    $"Supprimer définitivement l'activité « {activite.Nom} » ?\n\n" +
                    "Cette action supprimera en cascade :\n" +
                    "  - Tous les ingrédients liés (et leurs lots d'achat)\n" +
                    "  - Tous les contextes BOM liés (et leurs niveaux)\n\n" +
                    "Cette action est irréversible.",
                    "Suppression définitive",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;

            try
            {
                ActiviteDAL.Delete(activite.Id);
                Charger();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Suppression impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GererStocks()
        {
            var activite = ActiviteSelectionnee();
            if (activite == null) return;
            using (var frm = new FrmActiviteStocks(activite))
                frm.ShowDialog(this);
        }

        private Activite ActiviteSelectionnee()
        {
            if (_dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez une activité.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            int id = (int)_dgv.SelectedRows[0].Cells["Id"].Value;
            return ActiviteDAL.GetById(id);
        }
    }
}
