using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmFournisseurEdit : Form
    {
        private readonly Fournisseur _f;
        private readonly bool _isEdit;

        public FrmFournisseurEdit(Fournisseur f)
        {
            InitializeComponent();
            _isEdit = f != null;
            _f = f ?? new Fournisseur();
        }

        private void FrmFournisseurEdit_Load(object sender, EventArgs e)
        {
            Text = _isEdit ? "Modifier le fournisseur" : "Nouveau fournisseur";
            if (_isEdit)
            {
                txtNom.Text       = _f.Nom;
                txtContact.Text   = _f.Contact;
                txtEmail.Text     = _f.Email;
                txtTel.Text       = _f.Telephone;
                txtAdresse.Text   = _f.Adresse;
                txtNotes.Text     = _f.Notes;
            }
        }

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                errorProvider.SetError(txtNom, "Le nom est obligatoire.");
                return;
            }
            if (FournisseurDAL.NomExiste(txtNom.Text.Trim(), _isEdit ? _f.Id : 0))
            {
                errorProvider.SetError(txtNom, "Ce nom existe déjà.");
                return;
            }
            try
            {
                _f.Nom       = txtNom.Text.Trim();
                _f.Contact   = txtContact.Text.Trim().NullIfEmpty();
                _f.Email     = txtEmail.Text.Trim().NullIfEmpty();
                _f.Telephone = txtTel.Text.Trim().NullIfEmpty();
                _f.Adresse   = txtAdresse.Text.Trim().NullIfEmpty();
                _f.Notes     = txtNotes.Text.Trim().NullIfEmpty();

                if (_isEdit) FournisseurDAL.Update(_f);
                else         FournisseurDAL.Insert(_f);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e) { DialogResult = DialogResult.Cancel; Close(); }
    }

    internal static class StringExtensions
    {
        public static string NullIfEmpty(this string s) => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
