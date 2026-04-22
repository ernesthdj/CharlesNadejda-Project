using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Formulaire de création / modification d'un niveau BOM.
    /// Migré vers FrmEditBase — errorProvider et boutons gérés par la classe de base.
    /// </summary>
    public class FrmBomNiveauEdit : FrmEditBase
    {
        private readonly BomNiveau _niveau;
        private readonly bool      _isEdit;

        private readonly Label   lblOrdre;
        private readonly TextBox txtNom;
        private readonly TextBox txtDescription;

        public FrmBomNiveauEdit(BomNiveau niveau, bool isEdit)
        {
            _niveau = niveau;
            _isEdit = isEdit;

            var font = new Font("Segoe UI", 10F);
            ClientSize = new Size(390, 100);

            // ── Ordre (lecture seule) ─────────────────────────────────────
            lblOrdre = new Label
            {
                AutoSize = false,
                Font     = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(20, 15), Size = new Size(340, 20),
                Text     = $"Ordre dans le contexte : {_niveau.Ordre}"
            };
            Controls.Add(lblOrdre);

            // ── Nom ───────────────────────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, 45), Text = "Nom du niveau *" });
            txtNom = new TextBox { Font = font, Location = new Point(20, 67), Size = new Size(340, 26), TabIndex = 0 };
            Controls.Add(txtNom);

            // ── Description ───────────────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, 107), Text = "Description" });
            txtDescription = new TextBox
            {
                Font = font, Location = new Point(20, 129),
                Multiline = true, Size = new Size(340, 60), TabIndex = 1
            };
            Controls.Add(txtDescription);

            PositionnerBoutons(210);

            Text = _isEdit ? "Modifier le niveau" : $"Nouveau niveau (ordre {_niveau.Ordre})";
            if (_isEdit)
            {
                txtNom.Text         = _niveau.Nom;
                txtDescription.Text = _niveau.Description;
            }
        }

        protected override bool Valider()
        {
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            { errorProvider.SetError(txtNom, "Le nom est obligatoire."); return false; }
            return true;
        }

        protected override void Sauvegarder()
        {
            _niveau.Nom         = txtNom.Text.Trim();
            _niveau.Description = txtDescription.Text.Trim().NullIfEmpty();

            if (_isEdit) BomNiveauDAL.Update(_niveau);
            else         BomNiveauDAL.Insert(_niveau);
        }
    }
}
