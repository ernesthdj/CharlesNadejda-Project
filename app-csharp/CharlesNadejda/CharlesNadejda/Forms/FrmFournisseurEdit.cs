using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Formulaire de création / modification d'un fournisseur.
    /// Migré vers FrmEditBase — errorProvider et boutons gérés par la classe de base.
    /// </summary>
    public class FrmFournisseurEdit : FrmEditBase
    {
        private readonly Fournisseur _f;
        private readonly bool        _isEdit;

        private readonly TextBox txtNom;
        private readonly TextBox txtContact;
        private readonly TextBox txtEmail;
        private readonly TextBox txtTel;
        private readonly TextBox txtAdresse;
        private readonly TextBox txtNotes;

        public FrmFournisseurEdit(Fournisseur f)
        {
            _isEdit = f != null;
            _f      = f ?? new Fournisseur();

            var font = new Font("Segoe UI", 10F);
            ClientSize = new Size(405, 100);

            int tab = 0;
            TextBox MakeRow(string label, int y, bool multi = false)
            {
                Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, y), Text = label });
                var tb = new TextBox
                {
                    Font = font, Location = new Point(20, y + 22),
                    Size = new Size(360, multi ? 60 : 26), TabIndex = tab++
                };
                if (multi) tb.Multiline = true;
                Controls.Add(tb);
                return tb;
            }

            txtNom     = MakeRow("Nom *",      20);
            txtContact = MakeRow("Contact",    65);
            txtEmail   = MakeRow("Email",      110);
            txtTel     = MakeRow("Téléphone",  155);
            txtAdresse = MakeRow("Adresse",    200);
            txtNotes   = MakeRow("Notes",      245, multi: true);

            PositionnerBoutons(325);

            Text = _isEdit ? "Modifier le fournisseur" : "Nouveau fournisseur";
            if (_isEdit)
            {
                txtNom.Text     = _f.Nom;
                txtContact.Text = _f.Contact;
                txtEmail.Text   = _f.Email;
                txtTel.Text     = _f.Telephone;
                txtAdresse.Text = _f.Adresse;
                txtNotes.Text   = _f.Notes;
            }
        }

        protected override bool Valider()
        {
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            { errorProvider.SetError(txtNom, "Le nom est obligatoire."); return false; }

            if (FournisseurDAL.NomExiste(txtNom.Text.Trim(), _isEdit ? _f.Id : 0))
            { errorProvider.SetError(txtNom, "Ce nom existe déjà."); return false; }

            return true;
        }

        protected override void Sauvegarder()
        {
            _f.Nom       = txtNom.Text.Trim();
            _f.Contact   = txtContact.Text.Trim().NullIfEmpty();
            _f.Email     = txtEmail.Text.Trim().NullIfEmpty();
            _f.Telephone = txtTel.Text.Trim().NullIfEmpty();
            _f.Adresse   = txtAdresse.Text.Trim().NullIfEmpty();
            _f.Notes     = txtNotes.Text.Trim().NullIfEmpty();

            if (_isEdit) FournisseurDAL.Update(_f);
            else         FournisseurDAL.Insert(_f);
        }
    }
}
