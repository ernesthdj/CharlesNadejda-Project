using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmFournisseurs : Form
    {
        public FrmFournisseurs() { InitializeComponent(); }

        private void FrmFournisseurs_Load(object sender, EventArgs e) => Charger();

        private void Charger()
        {
            try
            {
                dgv.DataSource = null;
                dgv.DataSource = FournisseurDAL.GetAll();
                if (dgv.Columns["Id"] != null) dgv.Columns["Id"].Visible = false;
                dgv.Columns["Nom"].HeaderText       = "Nom";
                dgv.Columns["Contact"].HeaderText   = "Contact";
                dgv.Columns["Email"].HeaderText      = "Email";
                dgv.Columns["Telephone"].HeaderText  = "Téléphone";
                dgv.Columns["Adresse"].HeaderText    = "Adresse";
                dgv.Columns["Notes"].Visible         = false;
            }
            catch (Exception ex) { Erreur(ex.Message); }
        }

        private Fournisseur Selectionne() => dgv.CurrentRow?.DataBoundItem as Fournisseur;

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmFournisseurEdit(null))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            var f = Selectionne();
            if (f == null) { MessageBox.Show("Sélectionnez un fournisseur."); return; }
            using (var frm = new FrmFournisseurEdit(f))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            var f = Selectionne();
            if (f == null) { MessageBox.Show("Sélectionnez un fournisseur."); return; }
            if (MessageBox.Show($"Supprimer « {f.Nom} » ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            try { FournisseurDAL.Delete(f.Id); Charger(); }
            catch (Exception ex) { Erreur("Impossible de supprimer : " + ex.Message); }
        }

        private void btnFermer_Click(object sender, EventArgs e) => Close();

        private void Erreur(string msg) =>
            MessageBox.Show(msg, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
