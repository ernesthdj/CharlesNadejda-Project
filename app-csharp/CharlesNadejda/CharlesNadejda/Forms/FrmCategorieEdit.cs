using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Formulaire de création / modification d'une catégorie.
    /// Migré vers FrmEditBase — errorProvider et boutons gérés par la classe de base.
    /// </summary>
    public class FrmCategorieEdit : FrmEditBase
    {
        private readonly Categorie _cat;
        private readonly bool      _isEdit;

        private readonly TextBox       txtNom;
        private readonly TextBox       txtDescription;
        private readonly NumericUpDown nudOrdre;

        public FrmCategorieEdit(Categorie cat)
        {
            _isEdit = cat != null;
            _cat    = cat ?? new Categorie();

            var font = new Font("Segoe UI", 10F);
            ClientSize = new Size(390, 100); // hauteur provisoire — ajustée par PositionnerBoutons

            // ── Nom ───────────────────────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, 20), Text = "Nom *" });
            txtNom = new TextBox { Font = font, Location = new Point(20, 42), Size = new Size(340, 26), TabIndex = 0 };
            Controls.Add(txtNom);

            // ── Description ───────────────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, 80), Text = "Description" });
            txtDescription = new TextBox
            {
                Font = font, Location = new Point(20, 102),
                Multiline = true, Size = new Size(340, 70), TabIndex = 1
            };
            Controls.Add(txtDescription);

            // ── Ordre d'affichage ─────────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, 186), Text = "Ordre d'affichage" });
            nudOrdre = new NumericUpDown
            {
                Font = font, Location = new Point(20, 208),
                Minimum = 0, Maximum = 999,
                Size = new Size(80, 26), TabIndex = 2
            };
            Controls.Add(nudOrdre);

            // Positionner les boutons sous le dernier champ et ajuster la hauteur
            PositionnerBoutons(250);

            // ── Titre + pré-remplissage ───────────────────────────────────
            Text = _isEdit ? "Modifier la catégorie" : "Nouvelle catégorie";
            if (_isEdit)
            {
                txtNom.Text         = _cat.Nom;
                txtDescription.Text = _cat.Description ?? "";
                nudOrdre.Value      = _cat.OrdreAffichage;
            }
        }

        protected override bool Valider()
        {
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            { errorProvider.SetError(txtNom, "Le nom est obligatoire."); return false; }

            if (CategorieDAL.NomExiste(txtNom.Text.Trim(), _isEdit ? _cat.Id : 0))
            { errorProvider.SetError(txtNom, "Ce nom existe déjà."); return false; }

            return true;
        }

        protected override void Sauvegarder()
        {
            _cat.Nom            = txtNom.Text.Trim();
            _cat.Description    = txtDescription.Text.Trim().NullIfEmpty();
            _cat.OrdreAffichage = (int)nudOrdre.Value;

            if (_isEdit) CategorieDAL.Update(_cat);
            else         CategorieDAL.Insert(_cat);
        }
    }
}
