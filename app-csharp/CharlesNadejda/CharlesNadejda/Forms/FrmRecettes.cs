using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmRecettes : Form
    {
        public FrmRecettes() { InitializeComponent(); }

        private void FrmRecettes_Load(object sender, EventArgs e) => Charger();

        private void Charger()
        {
            try
            {
                dgv.DataSource = null;
                dgv.DataSource = RecetteDAL.GetAll();

                foreach (string col in new[] { "Id", "Description", "TypeRendement", "Actif", "Ingredients" })
                    if (dgv.Columns[col] != null) dgv.Columns[col].Visible = false;

                dgv.Columns["Nom"].HeaderText               = "Recette";
                dgv.Columns["RendementQuantite"].HeaderText = "Rendement";  dgv.Columns["RendementQuantite"].Width = 80;
                dgv.Columns["TempsPreparation"].HeaderText  = "Temps (min)";dgv.Columns["TempsPreparation"].Width = 90;
                dgv.Columns["CoutBatch"].HeaderText         = "Coût batch"; dgv.Columns["CoutBatch"].Width = 90;
                dgv.Columns["CoutUnitaire"].HeaderText      = "Coût/pièce"; dgv.Columns["CoutUnitaire"].Width = 90;

                dgv.Columns["CoutBatch"].DefaultCellStyle.Format    = "F3";
                dgv.Columns["CoutUnitaire"].DefaultCellStyle.Format = "F4";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Recette Selectionnee() => dgv.CurrentRow?.DataBoundItem as Recette;

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmRecetteEdit(null))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            var r = Selectionnee();
            if (r == null) { MessageBox.Show("Sélectionnez une recette."); return; }
            var recetteFull = RecetteDAL.GetById(r.Id);
            using (var frm = new FrmRecetteEdit(recetteFull))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            var r = Selectionnee();
            if (r == null) { MessageBox.Show("Sélectionnez une recette."); return; }
            if (MessageBox.Show($"Supprimer « {r.Nom} » ?", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try { RecetteDAL.Delete(r.Id); Charger(); }
            catch (Exception ex) { MessageBox.Show("Impossible : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnFermer_Click(object sender, EventArgs e) => Close();
    }
}
