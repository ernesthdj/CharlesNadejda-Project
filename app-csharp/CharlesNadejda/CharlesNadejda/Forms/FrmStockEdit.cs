using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Formulaire de création / modification d'un stock.
    /// Migré vers FrmEditBase — errorProvider et boutons gérés par la classe de base.
    /// </summary>
    public class FrmStockEdit : FrmEditBase
    {
        private readonly Stock _stock;
        private readonly bool  _isEdit;

        private readonly TextBox txtNom;
        private readonly TextBox txtDescription;

        public FrmStockEdit(Stock stock = null)
        {
            _isEdit = stock != null;
            _stock  = stock ?? new Stock();

            var font = new Font("Segoe UI", 10F);
            ClientSize = new Size(400, 100);

            // ── Nom ───────────────────────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, 16), Text = "Nom *" });
            txtNom = new TextBox { Font = font, Location = new Point(20, 36), Size = new Size(355, 26), TabIndex = 0 };
            Controls.Add(txtNom);

            // ── Description ───────────────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, 74), Text = "Description" });
            txtDescription = new TextBox
            {
                Font = font, Location = new Point(20, 94),
                Size = new Size(355, 50), Multiline = true, TabIndex = 1
            };
            Controls.Add(txtDescription);

            PositionnerBoutons(160);

            Text = _isEdit ? "Modifier le stock" : "Nouveau stock";
            Load += (s, e) =>
            {
                if (_isEdit)
                {
                    txtNom.Text         = _stock.Nom;
                    txtDescription.Text = _stock.Description ?? "";
                }
                txtNom.Focus();
                txtNom.SelectAll();
            };
        }

        protected override bool Valider()
        {
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            { errorProvider.SetError(txtNom, "Le nom est obligatoire."); return false; }

            if (StockDAL.NomExiste(txtNom.Text.Trim(), _isEdit ? _stock.Id : 0))
            { errorProvider.SetError(txtNom, "Ce nom de stock existe déjà."); return false; }

            return true;
        }

        protected override void Sauvegarder()
        {
            _stock.Nom         = txtNom.Text.Trim();
            _stock.Description = txtDescription.Text.Trim().NullIfEmpty();

            if (_isEdit) StockDAL.Update(_stock);
            else         StockDAL.Insert(_stock);
        }
    }
}
