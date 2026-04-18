using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Liste des fiches d'un niveau de transformation donné.
    /// Appelé depuis FrmArtisaStock avec le niveau sélectionné.
    /// </summary>
    public partial class FrmBomFiches : Form
    {
        private readonly BomNiveau _niveau;

        public FrmBomFiches(BomNiveau niveau)
        {
            InitializeComponent();
            _niveau       = niveau;
            lblTitre.Text = $"{niveau.NomContexte}  ›  {niveau.Nom}  (N{niveau.Ordre})";
        }

        private void FrmBomFiches_Load(object sender, EventArgs e) => Charger();

        private void Charger()
        {
            try
            {
                dgv.DataSource = null;
                dgv.DataSource = BomFicheDAL.GetByNiveau(_niveau.Id);

                string[] cachées = { "Id", "IdNiveau", "Actif", "DateCreation", "Lignes",
                                     "NomNiveau", "OrdreNiveau", "IdContexte", "NomContexte" };
                foreach (string col in cachées)
                    if (dgv.Columns[col] != null) dgv.Columns[col].Visible = false;

                if (dgv.Columns["Nom"]              != null)   dgv.Columns["Nom"].HeaderText              = "Nom de la fiche";
                if (dgv.Columns["Activite"]         != null) { dgv.Columns["Activite"].HeaderText         = "Activité";     dgv.Columns["Activite"].Width = 110; }
                if (dgv.Columns["UniteOutput"]      != null) { dgv.Columns["UniteOutput"].HeaderText      = "Unité";        dgv.Columns["UniteOutput"].Width = 70; }
                if (dgv.Columns["QuantiteOutput"]   != null) { dgv.Columns["QuantiteOutput"].HeaderText   = "Qté/exec.";   dgv.Columns["QuantiteOutput"].Width = 80; }
                if (dgv.Columns["TempsPreparation"] != null) { dgv.Columns["TempsPreparation"].HeaderText = "Temps (min)"; dgv.Columns["TempsPreparation"].Width = 90; }
                if (dgv.Columns["Description"]      != null)   dgv.Columns["Description"].HeaderText      = "Description";
                if (dgv.Columns["CoutBatch"]        != null)   dgv.Columns["CoutBatch"].Visible           = false;
                if (dgv.Columns["CoutUnitaire"]     != null)   dgv.Columns["CoutUnitaire"].Visible        = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private BomFiche Sélectionnée()
        {
            if (dgv.CurrentRow == null) return null;
            return dgv.CurrentRow.DataBoundItem as BomFiche;
        }

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmBomFicheEdit(null, _niveau))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            var fiche = Sélectionnée();
            if (fiche == null) { MessageBox.Show("Sélectionnez une fiche."); return; }

            fiche = BomFicheDAL.GetById(fiche.Id);
            using (var frm = new FrmBomFicheEdit(fiche, _niveau))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            var fiche = Sélectionnée();
            if (fiche == null) { MessageBox.Show("Sélectionnez une fiche."); return; }

            var confirm = MessageBox.Show(
                $"Supprimer la fiche « {fiche.Nom} » ?\n\nCette action est irréversible.",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes) return;

            try
            {
                BomFicheDAL.Delete(fiche.Id);
                Charger();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de supprimer : cette fiche est utilisée dans des productions.\n\n" + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFermer_Click(object sender, EventArgs e) => Close();
    }
}
