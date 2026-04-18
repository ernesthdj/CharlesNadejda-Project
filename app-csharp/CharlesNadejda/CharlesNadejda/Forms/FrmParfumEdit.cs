using System;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmParfumEdit : Form
    {
        private readonly Parfum _parfum;
        private readonly bool   _isEdit;

        public FrmParfumEdit(Parfum parfum)
        {
            InitializeComponent();
            _isEdit = parfum != null;
            _parfum = parfum ?? new Parfum { CouleurHex = "#6F4E37", Disponible = true };
        }

        private void FrmParfumEdit_Load(object sender, EventArgs e)
        {
            Text = _isEdit ? "Modifier le parfum" : "Nouveau parfum";

            try
            {
                cmbRecette.Items.Add("— Aucune —");
                foreach (var r in RecetteDAL.GetAll()) cmbRecette.Items.Add(r);
                cmbRecette.DisplayMember  = "Nom";
                cmbRecette.SelectedIndex  = 0;
            }
            catch { }

            if (_isEdit)
            {
                txtNom.Text           = _parfum.Nom;
                txtDescription.Text   = _parfum.Description;
                txtType.Text          = _parfum.TypeParfum;
                txtCouleur.Text       = _parfum.CouleurHex;
                chkDisponible.Checked = _parfum.Disponible;
                MajApercu();

                if (_parfum.IdRecette.HasValue)
                {
                    foreach (var item in cmbRecette.Items)
                    {
                        if (item is Recette r && r.Id == _parfum.IdRecette.Value)
                        { cmbRecette.SelectedItem = r; break; }
                    }
                }
            }
        }

        private void txtCouleur_Leave(object sender, EventArgs e) => MajApercu();

        private void MajApercu()
        {
            try
            {
                var hex = txtCouleur.Text.Trim();
                if (!hex.StartsWith("#")) hex = "#" + hex;
                picCouleur.BackColor = ColorTranslator.FromHtml(hex);
                txtCouleur.Text = hex;
            }
            catch { picCouleur.BackColor = Color.Gray; }
        }

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            bool ok = true;

            if (string.IsNullOrWhiteSpace(txtNom.Text))
            { errorProvider.SetError(txtNom, "Obligatoire."); ok = false; }
            else if (ParfumDAL.NomExiste(txtNom.Text.Trim(), _isEdit ? _parfum.Id : 0))
            { errorProvider.SetError(txtNom, "Ce nom existe déjà."); ok = false; }

            if (!ok) return;

            try
            {
                _parfum.Nom         = txtNom.Text.Trim();
                _parfum.Description = txtDescription.Text.Trim().NullIfEmpty();
                _parfum.TypeParfum  = txtType.Text.Trim().NullIfEmpty();
                _parfum.CouleurHex  = txtCouleur.Text.Trim();
                _parfum.Disponible  = chkDisponible.Checked;
                _parfum.IdRecette   = cmbRecette.SelectedItem is Recette r ? (int?)r.Id : null;

                if (_isEdit) ParfumDAL.Update(_parfum);
                else         ParfumDAL.Insert(_parfum);

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
}
