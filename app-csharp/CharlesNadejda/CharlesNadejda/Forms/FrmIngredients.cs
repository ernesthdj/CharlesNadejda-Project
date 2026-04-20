using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmIngredients : Form
    {
        private readonly Activite _activite;            // null = vue globale
        private readonly bool     _filtreAlertes;       // US-08 : afficher seulement les alertes
        private Stock             _stockFiltre;         // null = Tous

        private FlowLayoutPanel _pnlChips;
        private Button          _btnChipTous;

        private static readonly Color CHIP_ACTIF   = Color.FromArgb(111, 78, 55);
        private static readonly Color CHIP_INACTIF = Color.FromArgb(220, 210, 200);

        /// <param name="activite">Activité contextuelle (affichage titre). null = vue globale.</param>
        /// <param name="filtreAlertesSeulement">US-08 : si true, affiche uniquement les ingrédients en alerte.</param>
        public FrmIngredients(Activite activite = null, bool filtreAlertesSeulement = false)
        {
            InitializeComponent();
            _activite      = activite;
            _filtreAlertes = filtreAlertesSeulement;
            lblTitre.Text  = filtreAlertesSeulement
                ? "Fiches ingrédients en alerte"
                : (activite != null ? $"Fiches ingrédients — {activite.Nom}" : "Fiches ingrédients");
        }

        private void FrmIngredients_Load(object sender, EventArgs e)
        {
            BuildChipPanel();
            Charger();
        }

        // ── Chips de filtre par stock ────────────────────────────────────

        private void BuildChipPanel()
        {
            _pnlChips = new FlowLayoutPanel
            {
                Location      = new Point(12, 46),
                Height        = 32,
                Width         = dgv.Width,
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = Color.White
            };

            _btnChipTous = CreerChip("Tous", null, true);
            _pnlChips.Controls.Add(_btnChipTous);

            // US-01 : tous les stocks physiques actifs — pas de filtre par activité
            var stocks = StockDAL.GetAll();
            foreach (var s in stocks)
                _pnlChips.Controls.Add(CreerChip(s.Nom, s, false));

            // Décaler le DGV vers le bas pour faire de la place
            dgv.Location = new Point(dgv.Location.X, 84);
            dgv.Height   = dgv.Height - 32;

            this.Controls.Add(_pnlChips);
            _pnlChips.BringToFront();
        }

        private Button CreerChip(string texte, Stock stock, bool actif)
        {
            var btn = new Button
            {
                Text      = texte,
                Tag       = stock,
                Font      = new Font("Segoe UI", 8.5F, actif ? FontStyle.Bold : FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                BackColor = actif ? CHIP_ACTIF   : CHIP_INACTIF,
                ForeColor = actif ? Color.White   : Color.FromArgb(60, 45, 30),
                Height    = 26,
                Width     = texte.Length * 8 + 20,
                Margin    = new Padding(0, 0, 6, 0),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => AppliquerFiltre(stock, btn);
            return btn;
        }

        private void AppliquerFiltre(Stock stock, Button chipClique)
        {
            _stockFiltre = stock;
            foreach (Control c in _pnlChips.Controls)
            {
                if (c is Button b)
                {
                    bool actif = b == chipClique;
                    b.BackColor = actif ? CHIP_ACTIF   : CHIP_INACTIF;
                    b.ForeColor = actif ? Color.White   : Color.FromArgb(60, 45, 30);
                    b.Font      = new Font("Segoe UI", 8.5F, actif ? FontStyle.Bold : FontStyle.Regular);
                }
            }
            Charger();
        }

        // ── Chargement DGV ────────────────────────────────────────────────

        private void Charger()
        {
            try
            {
                List<Ingredient> liste;
                if (_stockFiltre != null)
                    liste = IngredientDAL.GetAll(idStock: _stockFiltre.Id);
                else
                    liste = IngredientDAL.GetAll(); // US-01 : tous les ingrédients — pas de filtre activité

                // US-08 : filtre alertes-seulement si activé depuis les StatCards du Hub
                if (_filtreAlertes)
                    liste = liste.Where(i => i.EstEnAlerte).ToList();

                dgv.DataSource = null;
                dgv.DataSource = liste;

                // ── Colonnes cachées — design v10 ───────────────────────
                // Masquer tout ce qui n'est pas dans le design cible
                string[] cachées = { "Id", "IdFournisseurDefaut", "IdStock", "Actif",
                                     "Description", "EstEnAlerte",
                                     "Marque",               // absent du design v10
                                     "SeuilAlerteStock",     // info interne
                                     "NomFournisseur",       // absent du design v10
                                     "QteParConditionnement",
                                     "PrixParUniteBase",
                                     "ToString" };
                foreach (string col in cachées)
                    if (dgv.Columns[col] != null) dgv.Columns[col].Visible = false;

                // ── Colonnes visibles — ordre design v10 ─────────────────
                // Nom · Conditionnement · Unité base · Stock (lieu) · Type physique · Densité · Dispo · €/cond.
                ConfigCol("Nom",                 "Ingrédient",       180, 120);
                ConfigCol("ConditionnementLabel","Conditionnement",  140,  90);
                ConfigCol("UniteMesure",         "Unité base",        70,  55);
                ConfigCol("StockNom",            "Stock (lieu)",     110,  75);
                ConfigCol("TypePhysique",        "Type physique",     90,  65);
                ConfigCol("Densite",             "Densité",           70,  55);
                ConfigCol("StockActuel",         "Dispo",             80,  65);
                ConfigCol("PrixAchatReference",  "€/cond.",           80,  65);

                // Ordre des colonnes (DisplayIndex) — respecte le design v10
                var displayOrder = new[] { "Nom", "ConditionnementLabel", "UniteMesure",
                    "StockNom", "TypePhysique", "Densite", "StockActuel", "PrixAchatReference" }
                    .Select((name, idx) => new { name, idx })
                    .ToArray();
                foreach (var item in displayOrder)
                    if (dgv.Columns[item.name] != null) dgv.Columns[item.name].DisplayIndex = item.idx;

                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                // Coloration alertes stock
                foreach (DataGridViewRow row in dgv.Rows)
                    if (row.DataBoundItem is Ingredient ing && ing.EstEnAlerte)
                        row.DefaultCellStyle.BackColor = Color.MistyRose;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigCol(string nom, string header, int largeur, int minimum)
        {
            if (dgv.Columns[nom] == null) return;
            dgv.Columns[nom].HeaderText   = header;
            dgv.Columns[nom].Width        = largeur;
            dgv.Columns[nom].MinimumWidth = minimum;
        }

        private Ingredient Selectionne() => dgv.CurrentRow?.DataBoundItem as Ingredient;

        private void btnAjouter_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmIngredientEdit(null, _stockFiltre))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            var ing = Selectionne();
            if (ing == null) { MessageBox.Show("Sélectionnez un ingrédient."); return; }
            using (var frm = new FrmIngredientEdit(ing))
                if (frm.ShowDialog() == DialogResult.OK) Charger();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            var ing = Selectionne();
            if (ing == null) { MessageBox.Show("Sélectionnez un ingrédient."); return; }
            if (MessageBox.Show($"Supprimer « {ing.Nom} » ?\n\nCette action est irréversible.", "Confirmation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            try { IngredientDAL.Delete(ing.Id); Charger(); }
            catch (Exception ex) { MessageBox.Show("Impossible de supprimer : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnFermer_Click(object sender, EventArgs e) => Close();
    }
}
