using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmProduits : Form
    {
        public FrmProduits()
        {
            InitializeComponent();
        }

        private void FrmProduits_Load(object sender, EventArgs e)
        {
            ChargerGrille();
        }

        private void ChargerGrille()
        {
            try
            {
                var list = ProduitDAL.GetAll();
                dgvProduits.DataSource = null;
                dgvProduits.DataSource = list;

                // Colonnes masquées
                foreach (string col in new[] { "Id", "IdCategorie", "ImageUrl" })
                    if (dgvProduits.Columns[col] != null)
                        dgvProduits.Columns[col].Visible = false;

                dgvProduits.Columns["NomCategorie"].HeaderText = "Catégorie";
                dgvProduits.Columns["NomCategorie"].Width      = 130;
                dgvProduits.Columns["Nom"].HeaderText          = "Produit";
                dgvProduits.Columns["Description"].HeaderText  = "Description";
                dgvProduits.Columns["PrixTTC"].HeaderText      = "Prix TTC";
                dgvProduits.Columns["PrixTTC"].Width           = 80;
                dgvProduits.Columns["PrixPromo"].HeaderText    = "Prix promo";
                dgvProduits.Columns["PrixPromo"].Width         = 85;
                dgvProduits.Columns["Stock"].HeaderText        = "Stock";
                dgvProduits.Columns["Stock"].Width             = 55;
                dgvProduits.Columns["Disponible"].HeaderText   = "Dispo";
                dgvProduits.Columns["Disponible"].Width        = 50;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Produit ProduitSélectionné()
        {
            if (dgvProduits.CurrentRow == null) return null;
            return dgvProduits.CurrentRow.DataBoundItem as Produit;
        }

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmProduitEdit(null))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                    ChargerGrille();
            }
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            var prod = ProduitSélectionné();
            if (prod == null) { MessageBox.Show("Sélectionnez un produit."); return; }

            using (var frm = new FrmProduitEdit(prod))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                    ChargerGrille();
            }
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            var prod = ProduitSélectionné();
            if (prod == null) { MessageBox.Show("Sélectionnez un produit."); return; }

            var confirm = MessageBox.Show(
                $"Supprimer le produit « {prod.Nom} » ?\n\nCette action est irréversible.",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                ProduitDAL.Delete(prod.Id);
                ChargerGrille();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de supprimer ce produit.\n\n" + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFermer_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
