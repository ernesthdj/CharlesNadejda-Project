using System;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public class FrmCategorieWebEdit : FrmEditBase
    {
        private readonly bool _isEdit;
        private readonly CategorieWeb _cat;

        private readonly TextBox    txtNom;
        private readonly TextBox    txtDescription;
        private readonly NumericUpDown nudOrdre;
        private readonly CheckBox   chkActif;

        public FrmCategorieWebEdit(CategorieWeb cat)
        {
            _isEdit = cat != null;
            _cat    = cat ?? new CategorieWeb { Actif = true, OrdreAffichage = 0 };

            Text = _isEdit ? "Modifier la catégorie" : "Nouvelle catégorie";
            Size = new Size(420, 300);

            txtNom         = MakeRow("Nom *", 20);
            txtDescription = MakeRow("Description", 65);
            txtDescription.Height = 60;
            txtDescription.Multiline = true;

            // Ordre
            var lblOrdre = new Label
            {
                Text = "Ordre d'affichage", Font = new Font("Segoe UI", 9F),
                Location = new Point(16, 140), AutoSize = true
            };
            nudOrdre = new NumericUpDown
            {
                Location = new Point(160, 138), Width = 80,
                Minimum = 0, Maximum = 999, Value = _cat.OrdreAffichage,
                Font = new Font("Segoe UI", 9.5F)
            };
            Controls.Add(lblOrdre);
            Controls.Add(nudOrdre);

            // Actif
            chkActif = new CheckBox
            {
                Text = "Catégorie active (visible sur le site)",
                Font = new Font("Segoe UI", 9F),
                Location = new Point(16, 175), AutoSize = true,
                Checked = _cat.Actif
            };
            Controls.Add(chkActif);

            PositionnerBoutons(210);

            // Pré-remplir si édition
            if (_isEdit)
            {
                txtNom.Text         = _cat.Nom;
                txtDescription.Text = _cat.Description;
            }
        }

        protected override bool Valider()
        {
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                errorProvider.SetError(txtNom, "Le nom est obligatoire.");
                return false;
            }
            if (CategorieWebDAL.NomExiste(txtNom.Text.Trim(), _isEdit ? _cat.Id : 0))
            {
                errorProvider.SetError(txtNom, "Ce nom existe déjà.");
                return false;
            }
            return true;
        }

        protected override void Sauvegarder()
        {
            _cat.Nom            = txtNom.Text.Trim();
            _cat.Description    = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
            _cat.OrdreAffichage = (int)nudOrdre.Value;
            _cat.Actif          = chkActif.Checked;

            if (_isEdit) CategorieWebDAL.Update(_cat);
            else         CategorieWebDAL.Insert(_cat);
        }

        private TextBox MakeRow(string label, int y)
        {
            Controls.Add(new Label
            {
                Text = label, Font = new Font("Segoe UI", 9F),
                Location = new Point(16, y + 2), AutoSize = true
            });
            var txt = new TextBox
            {
                Location = new Point(160, y), Width = 220,
                Font = new Font("Segoe UI", 9.5F)
            };
            Controls.Add(txt);
            return txt;
        }
    }
}
