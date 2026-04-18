using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmBomContextes : Form
    {
        private readonly Activite _activite;

        public FrmBomContextes(Activite activite = null)
        {
            InitializeComponent();
            _activite     = activite;
            lblTitre.Text = activite != null
                ? $"Contextes de production — {activite.Nom}"
                : "Contextes de production";
        }

        private void FrmBomContextes_Load(object sender, EventArgs e) => Charger();

        private void Charger()
        {
            try
            {
                dgv.DataSource = null;
                dgv.DataSource = BomContexteDAL.GetAll(_activite?.Id ?? 0);

                foreach (string col in new[] { "Actif", "DateCreation", "Niveaux" })
                    if (dgv.Columns[col] != null) dgv.Columns[col].Visible = false;

                if (dgv.Columns["Nom"]      != null) dgv.Columns["Nom"].HeaderText      = "Nom du contexte";
                if (dgv.Columns["ActiviteNom"] != null) dgv.Columns["ActiviteNom"].HeaderText = "Activité";
                if (dgv.Columns["Description"] != null) dgv.Columns["Description"].HeaderText = "Description";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private BomContexte Sélectionné()
        {
            if (dgv.CurrentRow == null) return null;
            return dgv.CurrentRow.DataBoundItem as BomContexte;
        }

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmBomContexteEdit(null, _activite))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            var ctx = Sélectionné();
            if (ctx == null) { MessageBox.Show("Sélectionnez un contexte."); return; }

            using (var frm = new FrmBomContexteEdit(ctx, _activite))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnNiveaux_Click(object sender, EventArgs e)
        {
            var ctx = Sélectionné();
            if (ctx == null) { MessageBox.Show("Sélectionnez un contexte."); return; }

            using (var frm = new FrmBomNiveaux(ctx))
                frm.ShowDialog();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            var ctx = Sélectionné();
            if (ctx == null) { MessageBox.Show("Sélectionnez un contexte."); return; }

            var confirm = MessageBox.Show(
                $"Supprimer le contexte « {ctx.Nom} » ?\n\nTous les niveaux, stocks et productions associés seront supprimés.",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                BomContexteDAL.Delete(ctx.Id);
                Charger();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de supprimer : des données liées existent.\n\n" + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFermer_Click(object sender, EventArgs e) => Close();
    }
}
