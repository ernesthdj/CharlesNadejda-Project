using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Liste des fiches ingrédients — hérite de FrmListeBase&lt;Ingredient&gt;.
    ///
    /// TICKET-13 : migration depuis partial class Form vers FrmListeBase&lt;T&gt;.
    /// Tout le boilerplate (DGV, boutons, CRUD, confirmation suppression) est
    /// délégué à la classe de base. FrmIngredients n'implémente que sa logique
    /// métier spécifique : filtre chip par stock + coloration des alertes.
    /// </summary>
    public class FrmIngredients : FrmListeBase<Ingredient>
    {
        private readonly Activite _activite;       // null = vue globale
        private readonly bool     _filtreAlertes;  // US-08 : alertes seulement
        private Stock             _stockFiltre;    // null = Tous

        private FlowLayoutPanel _pnlChips;

        /// <param name="activite">Activité contextuelle. null = vue globale.</param>
        /// <param name="filtreAlertesSeulement">US-08 : true = uniquement les ingrédients en alerte.</param>
        public FrmIngredients(Activite activite = null, bool filtreAlertesSeulement = false)
        {
            _activite      = activite;
            _filtreAlertes = filtreAlertesSeulement;
        }

        // ── Membres abstraits FrmListeBase<Ingredient> ────────────────────

        protected override string Titre => _filtreAlertes
            ? "Fiches ingrédients en alerte"
            : (_activite != null ? $"Fiches ingrédients — {_activite.Nom}" : "Fiches ingrédients");

        /// <summary>Charge la liste filtrée — tient compte du stock-chip et du filtre alertes.</summary>
        protected override List<Ingredient> ChargerDonnees()
        {
            List<Ingredient> liste = _stockFiltre != null
                ? IngredientDAL.GetAll(idStock: _stockFiltre.Id)
                : IngredientDAL.GetAll();

            if (_filtreAlertes)
                liste = liste.Where(i => i.EstEnAlerte).ToList();
            return liste;
        }

        protected override void ConfigurerColonnes()
        {
            CacherColonnes("Id", "IdFournisseurDefaut", "IdStock", "Actif",
                           "Description", "EstEnAlerte", "Marque",
                           "SeuilAlerteStock", "NomFournisseur",
                           "QteParConditionnement", "PrixParUniteBase");

            ConfigCol("Nom",                  "Ingrédient",       180, 120);
            ConfigCol("ConditionnementLabel", "Conditionnement",  140,  90);
            ConfigCol("UniteMesure",          "Unité base",        70,  55);
            ConfigCol("StockNom",             "Stock (lieu)",     110,  75);
            ConfigCol("TypePhysique",         "Type physique",     90,  65);
            ConfigCol("Densite",              "Densité",           70,  55);
            ConfigCol("StockActuel",          "Dispo",             80,  65);
            ConfigCol("PrixAchatReference",   "€/cond.",           80,  65);

            // Ordre d'affichage des colonnes
            string[] ordre = { "Nom", "StockActuel", "UniteMesure",
                                "PrixAchatReference", "StockNom", "ConditionnementLabel",
                                "TypePhysique", "Densite" };
            for (int i = 0; i < ordre.Length; i++)
                if (dgv.Columns[ordre[i]] != null)
                    dgv.Columns[ordre[i]].DisplayIndex = i;
        }

        protected override Form OuvrirFormulaire(Ingredient element)
            => element == null
                ? new FrmIngredientEdit(null, _stockFiltre)
                : new FrmIngredientEdit(element);

        protected override void Supprimer(Ingredient element)
            => IngredientDAL.Delete(element.Id);

        protected override string NomElement(Ingredient element) => element?.Nom ?? "?";

        /// <summary>Coloration rouge pâle pour les ingrédients en alerte de stock.</summary>
        protected override void AppliquerStylesLignes()
        {
            foreach (DataGridViewRow row in dgv.Rows)
                if (row.DataBoundItem is Ingredient ing && ing.EstEnAlerte)
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
        }

        // ── Chip panel de filtre par stock (fonctionnalité spécifique) ────

        protected override void OnLoad(EventArgs e)
        {
            BuildChipPanel();
            base.OnLoad(e);   // → FrmListeBase.OnLoad : Titre, Charger() = ChargerDonnees() + ConfigurerColonnes() + AppliquerStylesLignes()
        }

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

            _pnlChips.Controls.Add(CreerChip("Tous", null, true));

            // US-01 : tous les stocks physiques actifs
            foreach (var s in StockDAL.GetAll())
                _pnlChips.Controls.Add(CreerChip(s.Nom, s, false));

            // Décaler le DGV vers le bas pour laisser la place aux chips (loi de Proximité)
            dgv.Location = new Point(dgv.Location.X, 84);
            dgv.Height  -= 32;

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
                BackColor = actif ? AppColors.ChocoMed : Color.FromArgb(220, 210, 200),
                ForeColor = actif ? Color.White         : Color.FromArgb(60, 45, 30),
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
                    b.BackColor = actif ? AppColors.ChocoMed : Color.FromArgb(220, 210, 200);
                    b.ForeColor = actif ? Color.White         : Color.FromArgb(60, 45, 30);
                    b.Font      = new Font("Segoe UI", 8.5F, actif ? FontStyle.Bold : FontStyle.Regular);
                }
            }
            Charger();   // Recharge via FrmListeBase.Charger() → ChargerDonnees() avec le nouveau filtre
        }
    }
}
