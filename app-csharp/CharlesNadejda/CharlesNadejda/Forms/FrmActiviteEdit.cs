using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Création / modification d'une activité artisanale.
    /// Migré vers FrmEditBase — errorProvider et boutons gérés par la classe de base.
    /// </summary>
    public class FrmActiviteEdit : FrmEditBase
    {
        private readonly Activite _activite;
        private readonly bool     _isEdit;

        private readonly TextBox txtNom;
        private readonly TextBox txtDescription;

        public FrmActiviteEdit(Activite activite = null)
        {
            _isEdit   = activite != null;
            _activite = activite ?? new Activite { Actif = true };

            var font = new Font("Segoe UI", 10F);
            ClientSize = new Size(400, 100);

            // ── Nom ───────────────────────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, 16), Text = "Nom *" });
            txtNom = new TextBox
            {
                Font = font, Location = new Point(20, 38),
                Size = new Size(355, 26), MaxLength = 100, TabIndex = 0
            };
            Controls.Add(txtNom);

            // ── Description ───────────────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, 74), Text = "Description" });
            txtDescription = new TextBox
            {
                Font = font, Location = new Point(20, 96),
                Size = new Size(355, 64),
                Multiline = true, ScrollBars = ScrollBars.Vertical, TabIndex = 1
            };
            Controls.Add(txtDescription);

            PositionnerBoutons(180);

            Text = _isEdit ? "Modifier l'activité" : "Nouvelle activité";
            Load += (s, e) =>
            {
                if (_isEdit)
                {
                    txtNom.Text         = _activite.Nom;
                    txtDescription.Text = _activite.Description ?? "";
                }
                txtNom.Focus();
            };
        }

        protected override bool Valider()
        {
            string nom = txtNom.Text.Trim();
            if (string.IsNullOrEmpty(nom))
            { errorProvider.SetError(txtNom, "Le nom est obligatoire."); return false; }

            if (ActiviteDAL.NomExiste(nom, _isEdit ? _activite.Id : 0))
            { errorProvider.SetError(txtNom, "Une activité avec ce nom existe déjà."); return false; }

            return true;
        }

        protected override void Sauvegarder()
        {
            _activite.Nom         = txtNom.Text.Trim();
            _activite.Description = txtDescription.Text.Trim().NullIfEmpty();

            if (_isEdit) ActiviteDAL.Update(_activite);
            else         ActiviteDAL.Insert(_activite);
        }
    }
}
