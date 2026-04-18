using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmProduitEdit : Form
    {
        private readonly Produit _produit;
        private readonly bool    _isEdit;

        public FrmProduitEdit(Produit produit)
        {
            InitializeComponent();
            _isEdit  = produit != null;
            _produit = produit ?? new Produit { Disponible = true };
        }

        private void FrmProduitEdit_Load(object sender, EventArgs e)
        {
            this.Text = _isEdit ? "Modifier le produit" : "Nouveau produit";

            // Charger les catégories dans le ComboBox
            try
            {
                var cats = CategorieDAL.GetAll();
                cmbCategorie.DataSource    = cats;
                cmbCategorie.DisplayMember = "Nom";
                cmbCategorie.ValueMember   = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement catégories : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            if (_isEdit)
            {
                txtNom.Text           = _produit.Nom;
                txtDescription.Text   = _produit.Description;
                txtPrixTTC.Text       = _produit.PrixTTC.ToString("F2");
                txtPrixPromo.Text     = _produit.PrixPromo.HasValue ? _produit.PrixPromo.Value.ToString("F2") : "";
                nudStock.Value        = _produit.Stock;
                chkDisponible.Checked = _produit.Disponible;
                txtImageUrl.Text      = _produit.ImageUrl;
                cmbCategorie.SelectedValue = _produit.IdCategorie;
            }
        }

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            bool valid = true;

            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                errorProvider.SetError(txtNom, "Le nom est obligatoire.");
                valid = false;
            }
            else if (ProduitDAL.NomExiste(txtNom.Text.Trim(), _isEdit ? _produit.Id : 0))
            {
                errorProvider.SetError(txtNom, "Ce nom existe déjà.");
                valid = false;
            }

            decimal prixTTC = 0;
            if (!decimal.TryParse(txtPrixTTC.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out prixTTC) || prixTTC < 0)
            {
                errorProvider.SetError(txtPrixTTC, "Prix invalide (ex: 12.50).");
                valid = false;
            }

            decimal? prixPromo = null;
            if (!string.IsNullOrWhiteSpace(txtPrixPromo.Text))
            {
                decimal tmp;
                if (!decimal.TryParse(txtPrixPromo.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out tmp) || tmp < 0)
                {
                    errorProvider.SetError(txtPrixPromo, "Prix promo invalide.");
                    valid = false;
                }
                else
                {
                    prixPromo = tmp;
                }
            }

            if (cmbCategorie.SelectedValue == null)
            {
                errorProvider.SetError(cmbCategorie, "Sélectionnez une catégorie.");
                valid = false;
            }

            if (!valid) return;

            try
            {
                _produit.Nom          = txtNom.Text.Trim();
                _produit.Description  = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
                _produit.PrixTTC      = prixTTC;
                _produit.PrixPromo    = prixPromo;
                _produit.Stock        = (int)nudStock.Value;
                _produit.Disponible   = chkDisponible.Checked;
                _produit.ImageUrl     = string.IsNullOrWhiteSpace(txtImageUrl.Text) ? null : txtImageUrl.Text.Trim();
                _produit.IdCategorie  = (int)cmbCategorie.SelectedValue;

                if (_isEdit)
                    ProduitDAL.Update(_produit);
                else
                    ProduitDAL.Insert(_produit);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
