using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmAchats : Form
    {
        private readonly Activite _activite;   // null = tous

        public FrmAchats(Activite activite = null)
        {
            InitializeComponent();
            _activite     = activite;
            lblTitre.Text = activite != null
                ? $"Achats — {activite.Nom}"
                : "Tous les achats";
        }

        private void FrmAchats_Load(object sender, EventArgs e) => Charger();

        private void Charger()
        {
            try
            {
                dgv.DataSource = null;
                dgv.DataSource = LotDAL.GetAll(_activite?.Id ?? 0);
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                foreach (string col in new[] { "Id", "IdFicheIngredient", "IdFournisseur",
                                               "QuantiteDisponible", "Notes", "TvaPct" })
                    if (dgv.Columns[col] != null) dgv.Columns[col].Visible = false;

                void Col(string name, string header, int w, int min = 60)
                {
                    if (dgv.Columns[name] == null) return;
                    dgv.Columns[name].HeaderText  = header;
                    dgv.Columns[name].Width        = w;
                    dgv.Columns[name].MinimumWidth = min;
                }

                Col("NomIngredient",    "Ingrédient",      180, 120);
                Col("UniteMesure",      "Unité",            55,  45);
                Col("NumeroLot",        "N° lot",           90,  70);
                Col("NomFournisseur",   "Fournisseur",      140,  90);
                Col("DateAchat",        "Date achat",        95,  80);
                Col("DatePeremption",   "Péremption",        95,  80);
                Col("QuantiteInitiale", "Qté achetée",       90,  70);
                Col("PrixUnitaire",     "Prix unit. HTVA",  100,  80);
                Col("PrixAchatReel",    "Total HTVA (€)",    95,  75);
                Col("ReferenceFacture", "Réf. facture",     100,  80);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Lot Selectionne() => dgv.CurrentRow?.DataBoundItem as Lot;

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmAchatEdit(null, _activite?.Id ?? 0))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            var lot = Selectionne();
            if (lot == null)
            { MessageBox.Show("Sélectionnez un achat.", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            using (var frm = new FrmAchatEdit(lot, _activite?.Id ?? 0))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            var lot = Selectionne();
            if (lot == null)
            { MessageBox.Show("Sélectionnez un achat.", "Aucune sélection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            if (MessageBox.Show(
                    $"Supprimer le lot « {lot.NomIngredient} » du {lot.DateAchat:dd/MM/yyyy} ?\n\nOpération irréversible.",
                    "Confirmer la suppression",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;

            try { LotDAL.Delete(lot.Id); Charger(); }
            catch (Exception ex)
            { MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnFermer_Click(object sender, EventArgs e) => Close();
    }
}
