using System;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public class FrmStockEdit : Form
    {
        private readonly Stock _stock;
        private readonly bool  _isEdit;

        private TextBox     txtNom;
        private TextBox     txtDescription;
        private ErrorProvider errorProvider;

        public FrmStockEdit(Stock stock = null)
        {
            _isEdit = stock != null;
            _stock  = stock ?? new Stock();
            BuildUI();
            Load += FrmStockEdit_Load;
        }

        private void BuildUI()
        {
            this.Text            = _isEdit ? "Modifier le stock" : "Nouveau stock";
            this.ClientSize      = new Size(400, 220);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;

            errorProvider = new ErrorProvider { ContainerControl = this };
            var font = new Font("Segoe UI", 10F);
            int lx = 20, w = 355;

            void Lbl(string text, int y) => this.Controls.Add(new Label
                { AutoSize = true, Font = font, Location = new Point(lx, y), Text = text });

            Lbl("Nom *", 16);
            txtNom = new TextBox { Font = font, Location = new Point(lx, 36), Size = new Size(w, 26) };
            this.Controls.Add(txtNom);

            Lbl("Description", 74);
            txtDescription = new TextBox
            {
                Font      = font,
                Location  = new Point(lx, 94),
                Size      = new Size(w, 50),
                Multiline = true
            };
            this.Controls.Add(txtDescription);

            var btnOK = new Button
            {
                Text      = "Enregistrer",
                Font      = font,
                Location  = new Point(lx, 160),
                Size      = new Size(160, 36),
                BackColor = Color.FromArgb(61, 40, 23),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Click += BtnOK_Click;

            var btnAnnuler = new Button
            {
                Text      = "Annuler",
                Font      = font,
                Location  = new Point(200, 160),
                Size      = new Size(155, 36),
                BackColor = Color.FromArgb(220, 215, 210),
                ForeColor = Color.FromArgb(61, 40, 23),
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnAnnuler.FlatAppearance.BorderSize = 0;
            btnAnnuler.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(btnOK);
            this.Controls.Add(btnAnnuler);
        }

        private void FrmStockEdit_Load(object sender, EventArgs e)
        {
            if (_isEdit)
            {
                txtNom.Text         = _stock.Nom;
                txtDescription.Text = _stock.Description ?? "";
            }
            txtNom.Focus();
            txtNom.SelectAll();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();

            if (string.IsNullOrWhiteSpace(txtNom.Text))
            { errorProvider.SetError(txtNom, "Le nom est obligatoire."); return; }

            if (StockDAL.NomExiste(txtNom.Text.Trim(), _isEdit ? _stock.Id : 0))
            { errorProvider.SetError(txtNom, "Ce nom de stock existe déjà."); return; }

            try
            {
                _stock.Nom         = txtNom.Text.Trim();
                _stock.Description = txtDescription.Text.Trim().NullIfEmpty();

                if (_isEdit) StockDAL.Update(_stock);
                else         StockDAL.Insert(_stock);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
