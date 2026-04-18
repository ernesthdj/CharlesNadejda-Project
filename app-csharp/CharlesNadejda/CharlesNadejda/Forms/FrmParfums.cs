using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmParfums : Form
    {
        public FrmParfums() { InitializeComponent(); }

        private void FrmParfums_Load(object sender, EventArgs e) => Charger();

        private void Charger()
        {
            try
            {
                dgv.DataSource = null;
                dgv.DataSource = ParfumDAL.GetAll();

                foreach (string col in new[] { "Id", "IdRecette", "Description" })
                    if (dgv.Columns[col] != null) dgv.Columns[col].Visible = false;

                dgv.Columns["Nom"].HeaderText        = "Parfum";
                dgv.Columns["TypeParfum"].HeaderText = "Type";        dgv.Columns["TypeParfum"].Width = 100;
                dgv.Columns["CouleurHex"].HeaderText = "Couleur hex"; dgv.Columns["CouleurHex"].Width = 90;
                dgv.Columns["NomRecette"].HeaderText = "Recette liée";
                dgv.Columns["Disponible"].HeaderText = "Dispo";       dgv.Columns["Disponible"].Width = 55;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Parfum Selectionne() => dgv.CurrentRow?.DataBoundItem as Parfum;

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmParfumEdit(null))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            var p = Selectionne();
            if (p == null) { MessageBox.Show("Sélectionnez un parfum."); return; }
            using (var frm = new FrmParfumEdit(p))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            var p = Selectionne();
            if (p == null) { MessageBox.Show("Sélectionnez un parfum."); return; }
            if (MessageBox.Show($"Supprimer « {p.Nom} » ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try { ParfumDAL.Delete(p.Id); Charger(); }
            catch (Exception ex) { MessageBox.Show("Impossible : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnFermer_Click(object sender, EventArgs e) => Close();
    }
}
