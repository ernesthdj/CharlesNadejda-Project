using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmIngredientEdit : Form
    {
        private readonly Ingredient _ing;
        private readonly bool       _isEdit;

        public FrmIngredientEdit(Ingredient ing, Stock stockDefaut = null)
        {
            InitializeComponent();
            _isEdit = ing != null;
            _ing    = ing ?? new Ingredient
            {
                IdStock  = stockDefaut?.Id  ?? 0,
                StockNom = stockDefaut?.Nom ?? "",
                Actif    = true
            };
        }

        private void FrmIngredientEdit_Load(object sender, EventArgs e)
        {
            Text = _isEdit ? "Modifier l'ingrédient" : "Nouvel ingrédient";

            cmbTypePhysique.Items.AddRange(new object[] { "solide", "liquide", "poudre", "piece" });
            // Unités standard ERP : masse (mg/g/kg) + volume (ml/cl/dl/l) + pièce
            cmbUnite.Items.AddRange(new object[] { "mg", "g", "kg", "ml", "cl", "dl", "l", "piece" });

            cmbTypePhysique.SelectedIndexChanged += cmbTypePhysique_SelectedIndexChanged;
            cmbUnite.SelectedIndexChanged        += (s, ev) => MajLabelUniteQteCond();

            // Chargement dynamique des stocks depuis la DB
            var stocks = StockDAL.GetAll();
            cmbStock.Items.Clear();
            foreach (var s in stocks) cmbStock.Items.Add(s);
            cmbStock.DisplayMember = "Nom";

            try
            {
                var fournisseurs = FournisseurDAL.GetAll();
                cmbFournisseur.Items.Add("— Aucun —");
                foreach (var f in fournisseurs) cmbFournisseur.Items.Add(f);
                cmbFournisseur.DisplayMember = "Nom";
            }
            catch { }

            if (_isEdit)
            {
                txtNom.Text                    = _ing.Nom;
                txtMarque.Text                 = _ing.Marque ?? "";
                cmbUnite.SelectedItem          = _ing.UniteMesure;
                cmbTypePhysique.SelectedItem   = _ing.TypePhysique ?? "solide";
                nudPrix.Value                  = _ing.PrixAchatReference;
                nudQteConditionnement.Value    = _ing.QteParConditionnement > 0
                                                 ? _ing.QteParConditionnement : 1m;
                txtSeuil.Text                  = _ing.SeuilAlerteStock.HasValue
                                                 ? _ing.SeuilAlerteStock.Value.ToString("F4") : "";

                // Sélectionner le stock par Id
                foreach (var item in cmbStock.Items)
                    if (((Stock)item).Id == _ing.IdStock) { cmbStock.SelectedItem = item; break; }

                if (_ing.Densite.HasValue) nudDensite.Value = _ing.Densite.Value;

                if (_ing.IdFournisseurDefaut.HasValue)
                    foreach (var item in cmbFournisseur.Items)
                        if (item is Fournisseur f && f.Id == _ing.IdFournisseurDefaut.Value)
                        { cmbFournisseur.SelectedItem = f; break; }
                else cmbFournisseur.SelectedIndex = 0;
            }
            else
            {
                cmbTypePhysique.SelectedIndex    = 0; // "solide"
                cmbUnite.SelectedIndex           = 0; // "g"
                cmbFournisseur.SelectedIndex     = 0;
                nudQteConditionnement.Value      = 1m;

                // Pré-sélectionner le stock par défaut
                if (_ing.IdStock > 0)
                    foreach (var item in cmbStock.Items)
                        if (((Stock)item).Id == _ing.IdStock) { cmbStock.SelectedItem = item; break; }
                else if (cmbStock.Items.Count > 0)
                    cmbStock.SelectedIndex = 0;
            }

            MettreAJourVisibiliteDensite();
            MajLabelUniteQteCond();
        }

        private void cmbTypePhysique_SelectedIndexChanged(object sender, EventArgs e)
            => MettreAJourVisibiliteDensite();

        private void MettreAJourVisibiliteDensite()
        {
            string type = cmbTypePhysique.SelectedItem?.ToString() ?? "solide";
            bool necessite = type == "liquide" || type == "poudre";
            lblDensite.Visible = necessite;
            nudDensite.Visible = necessite;
        }

        private void MajLabelUniteQteCond()
        {
            lblUniteQteCond.Text = cmbUnite.SelectedItem?.ToString() ?? "g";
        }

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            bool ok = true;

            if (string.IsNullOrWhiteSpace(txtNom.Text))
            { errorProvider.SetError(txtNom, "Obligatoire."); ok = false; }
            else if (IngredientDAL.NomExiste(txtNom.Text.Trim(), _isEdit ? _ing.Id : 0))
            { errorProvider.SetError(txtNom, "Ce nom existe déjà."); ok = false; }

            if (cmbUnite.SelectedItem == null)
            { errorProvider.SetError(cmbUnite, "Choisissez une unité de base."); ok = false; }

            if (nudQteConditionnement.Value <= 0)
            { errorProvider.SetError(nudQteConditionnement, "Doit être > 0."); ok = false; }

            if (cmbTypePhysique.SelectedItem == null)
            { errorProvider.SetError(cmbTypePhysique, "Choisissez un type physique."); ok = false; }

            if (cmbStock.SelectedItem == null)
            { errorProvider.SetError(cmbStock, "Choisissez un stock."); ok = false; }

            string typeSelectionne = cmbTypePhysique.SelectedItem?.ToString() ?? "solide";
            if ((typeSelectionne == "liquide" || typeSelectionne == "poudre") && nudDensite.Value <= 0)
            { errorProvider.SetError(nudDensite, "La densité est obligatoire pour ce type."); ok = false; }

            if (!ok) return;

            decimal? seuil = null;
            if (!string.IsNullOrWhiteSpace(txtSeuil.Text))
            {
                if (decimal.TryParse(txtSeuil.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal s))
                    seuil = s;
            }

            try
            {
                var stockSelectionne = (Stock)cmbStock.SelectedItem;

                _ing.Nom                    = txtNom.Text.Trim();
                _ing.Marque                 = txtMarque.Text.Trim().NullIfEmpty();
                _ing.UniteMesure            = cmbUnite.SelectedItem.ToString();
                _ing.TypePhysique           = typeSelectionne;
                _ing.Densite                = (typeSelectionne == "liquide" || typeSelectionne == "poudre")
                                              ? (decimal?)nudDensite.Value : null;
                _ing.ConditionnementLabel   = null;
                _ing.QteParConditionnement  = nudQteConditionnement.Value;
                _ing.PrixAchatReference     = nudPrix.Value;
                _ing.SeuilAlerteStock       = seuil;
                _ing.IdStock            = stockSelectionne.Id;
                _ing.StockNom           = stockSelectionne.Nom;
                _ing.IdFournisseurDefaut = cmbFournisseur.SelectedItem is Fournisseur f2
                    ? (int?)f2.Id : null;

                if (_isEdit) IngredientDAL.Update(_ing);
                else         IngredientDAL.Insert(_ing);

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
