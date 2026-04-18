using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmBomNiveauEdit : Form
    {
        private readonly BomNiveau _niveau;
        private readonly bool      _isEdit;

        public FrmBomNiveauEdit(BomNiveau niveau, bool isEdit)
        {
            InitializeComponent();
            _niveau = niveau;
            _isEdit = isEdit;
        }

        private void FrmBomNiveauEdit_Load(object sender, EventArgs e)
        {
            this.Text    = _isEdit ? "Modifier le niveau" : $"Nouveau niveau (ordre {_niveau.Ordre})";
            lblOrdre.Text = $"Ordre dans le contexte : {_niveau.Ordre}";

            if (_isEdit)
            {
                txtNom.Text         = _niveau.Nom;
                txtDescription.Text = _niveau.Description;
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

            try
            {
                _niveau.Nom         = txtNom.Text.Trim();
                _niveau.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();

                if (_isEdit)
                    BomNiveauDAL.Update(_niveau);
                else
                    BomNiveauDAL.Insert(_niveau);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
