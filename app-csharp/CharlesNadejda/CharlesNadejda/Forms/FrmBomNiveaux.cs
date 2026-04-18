using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmBomNiveaux : Form
    {
        private readonly BomContexte _contexte;

        public FrmBomNiveaux(BomContexte contexte)
        {
            InitializeComponent();
            _contexte     = contexte;
            lblTitre.Text = $"Niveaux de transformation — {contexte.Nom}";
        }

        private void FrmBomNiveaux_Load(object sender, EventArgs e) => Charger();

        private void Charger()
        {
            try
            {
                dgv.DataSource = null;
                dgv.DataSource = BomNiveauDAL.GetByContexte(_contexte.Id);

                foreach (string col in new[] { "Id", "IdContexte", "NomContexte", "Activite", "DateCreation" })
                    if (dgv.Columns[col] != null) dgv.Columns[col].Visible = false;

                if (dgv.Columns["Ordre"]       != null) { dgv.Columns["Ordre"].HeaderText = "Ordre"; dgv.Columns["Ordre"].Width = 60; }
                if (dgv.Columns["Nom"]         != null)   dgv.Columns["Nom"].HeaderText   = "Nom du niveau";
                if (dgv.Columns["Description"] != null)   dgv.Columns["Description"].HeaderText = "Description";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private BomNiveau Sélectionné()
        {
            if (dgv.CurrentRow == null) return null;
            return dgv.CurrentRow.DataBoundItem as BomNiveau;
        }

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            int ordreMax = BomNiveauDAL.GetOrdreMax(_contexte.Id);
            var nouveau  = new BomNiveau { IdContexte = _contexte.Id, Ordre = ordreMax + 1 };

            using (var frm = new FrmBomNiveauEdit(nouveau, false))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            var niv = Sélectionné();
            if (niv == null) { MessageBox.Show("Sélectionnez un niveau."); return; }

            using (var frm = new FrmBomNiveauEdit(niv, true))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            var niv = Sélectionné();
            if (niv == null) { MessageBox.Show("Sélectionnez un niveau."); return; }

            var confirm = MessageBox.Show(
                $"Supprimer le niveau « {niv.Nom} » (ordre {niv.Ordre}) ?\n\nLe stock et les productions de ce niveau seront supprimés.",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes) return;

            try
            {
                BomNiveauDAL.Delete(niv.Id);
                Charger();
            }
            catch (InvalidOperationException ex)
            {
                // Règle métier : suppression intermédiaire interdite
                MessageBox.Show(ex.Message, "Suppression impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de supprimer : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFermer_Click(object sender, EventArgs e) => Close();
    }
}
