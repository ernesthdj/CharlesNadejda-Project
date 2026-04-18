using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmRecetteEdit : Form
    {
        private readonly Recette _recette;
        private readonly bool    _isEdit;
        private readonly List<RecetteIngredient> _ingredients = new List<RecetteIngredient>();
        private List<Ingredient> _tousIngredients;

        public FrmRecetteEdit(Recette recette)
        {
            InitializeComponent();
            _isEdit  = recette != null;
            _recette = recette ?? new Recette { TypeRendement = "par_lot", RendementQuantite = 20, Actif = true };
        }

        private void FrmRecetteEdit_Load(object sender, EventArgs e)
        {
            Text = _isEdit ? "Modifier la recette" : "Nouvelle recette";

            try { _tousIngredients = IngredientDAL.GetAll(); }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); Close(); return; }

            cmbIngredient.DataSource    = _tousIngredients;
            cmbIngredient.DisplayMember = "Nom";
            cmbIngredient.ValueMember   = "Id";

            if (_isEdit)
            {
                txtNom.Text            = _recette.Nom;
                txtDescription.Text    = _recette.Description;
                nudRendement.Value     = (decimal)_recette.RendementQuantite;
                nudTemps.Value         = _recette.TempsPreparation ?? 0;
                _ingredients.AddRange(_recette.Ingredients);
                RefreshGrille();
            }

            MajCout();
        }

        // ── Gestion de la liste d'ingrédients ────────────────────────

        private void btnAjouterIngredient_Click(object sender, EventArgs e)
        {
            var ing = cmbIngredient.SelectedItem as Ingredient;
            if (ing == null) return;

            decimal qte;
            if (!decimal.TryParse(txtQteIngredient.Text.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out qte) || qte <= 0)
            {
                errorProvider.SetError(txtQteIngredient, "Quantité invalide.");
                return;
            }
            errorProvider.Clear();

            // Remplacer si déjà présent
            _ingredients.RemoveAll(i => i.IdIngredient == ing.Id);
            _ingredients.Add(new RecetteIngredient
            {
                IdIngredient  = ing.Id,
                NomIngredient = ing.Nom,
                UniteMesure   = ing.UniteMesure,
                Quantite      = qte,
                PrixUnitaire  = ing.PrixAchatReference
            });

            txtQteIngredient.Clear();
            RefreshGrille();
            MajCout();
        }

        private void dgvIngredients_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && dgvIngredients.CurrentRow != null)
            {
                var item = dgvIngredients.CurrentRow.DataBoundItem as RecetteIngredient;
                if (item != null) { _ingredients.Remove(item); RefreshGrille(); MajCout(); }
            }
        }

        private void RefreshGrille()
        {
            dgvIngredients.DataSource = null;
            dgvIngredients.DataSource = new System.ComponentModel.BindingList<RecetteIngredient>(_ingredients);

            if (dgvIngredients.Columns["IdRecette"] != null)   dgvIngredients.Columns["IdRecette"].Visible   = false;
            if (dgvIngredients.Columns["IdIngredient"] != null) dgvIngredients.Columns["IdIngredient"].Visible = false;

            if (dgvIngredients.Columns["NomIngredient"] != null) { dgvIngredients.Columns["NomIngredient"].HeaderText = "Ingrédient"; dgvIngredients.Columns["NomIngredient"].FillWeight = 120; }
            if (dgvIngredients.Columns["UniteMesure"] != null)   { dgvIngredients.Columns["UniteMesure"].HeaderText   = "Unité";      dgvIngredients.Columns["UniteMesure"].Width = 60; }
            if (dgvIngredients.Columns["Quantite"] != null)      { dgvIngredients.Columns["Quantite"].HeaderText      = "Quantité";   dgvIngredients.Columns["Quantite"].Width = 80; dgvIngredients.Columns["Quantite"].DefaultCellStyle.Format = "F4"; }
            if (dgvIngredients.Columns["PrixUnitaire"] != null)  { dgvIngredients.Columns["PrixUnitaire"].HeaderText  = "Prix/u (€)"; dgvIngredients.Columns["PrixUnitaire"].Width = 80; dgvIngredients.Columns["PrixUnitaire"].DefaultCellStyle.Format = "F4"; }
            if (dgvIngredients.Columns["SousTotal"] != null)     { dgvIngredients.Columns["SousTotal"].HeaderText     = "Sous-total"; dgvIngredients.Columns["SousTotal"].Width = 80; dgvIngredients.Columns["SousTotal"].DefaultCellStyle.Format = "F4"; }
        }

        private void MajCout()
        {
            decimal coutBatch = 0;
            foreach (var i in _ingredients) coutBatch += i.SousTotal;
            decimal rendement = nudRendement.Value > 0 ? nudRendement.Value : 1;
            decimal coutUnit  = coutBatch / rendement;

            lblCout.Text = $"Coût batch : {coutBatch:F3} €   |   Coût/pièce : {coutUnit:F4} €";
            lblCout.ForeColor = coutBatch > 0 ? Color.DarkGreen : Color.Gray;
        }

        private void nudRendement_ValueChanged(object sender, EventArgs e) => MajCout();

        // ── Enregistrement ───────────────────────────────────────────

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            bool ok = true;

            if (string.IsNullOrWhiteSpace(txtNom.Text))
            { errorProvider.SetError(txtNom, "Le nom est obligatoire."); ok = false; }
            else if (RecetteDAL.NomExiste(txtNom.Text.Trim(), _isEdit ? _recette.Id : 0))
            { errorProvider.SetError(txtNom, "Ce nom existe déjà."); ok = false; }

            if (_ingredients.Count == 0)
            { MessageBox.Show("Ajoutez au moins un ingrédient.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); ok = false; }

            if (!ok) return;

            try
            {
                _recette.Nom               = txtNom.Text.Trim();
                _recette.Description       = txtDescription.Text.Trim().NullIfEmpty();
                _recette.RendementQuantite = nudRendement.Value;
                _recette.TempsPreparation  = nudTemps.Value > 0 ? (int?)nudTemps.Value : null;
                _recette.TypeRendement     = "par_lot";

                int id;
                if (_isEdit) { RecetteDAL.Update(_recette); id = _recette.Id; }
                else           id = RecetteDAL.Insert(_recette);

                RecetteDAL.SetIngredients(id, _ingredients);

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
