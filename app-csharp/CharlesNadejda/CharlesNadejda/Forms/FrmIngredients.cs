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
    public class FrmIngredients : FrmListeBaseIngredient
    {
        private readonly Activite _activite;              // null = vue globale
        private readonly bool     _filtreAlertes;         // US-08 : alertes seulement
        private Stock             _stockFiltre;           // null = Tous
        private bool              _stockReelSeulement;   // false = Fiches, true = Stock réel

        private FlowLayoutPanel _pnlChips;
        private Button          _btnModefiches;
        private Button          _btnModeStockReel;

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

        /// <summary>Charge la liste filtrée — tient compte du stock-chip, du mode Fiches/Stock réel et du filtre alertes.</summary>
        protected override List<Ingredient> ChargerDonnees()
        {
            List<Ingredient> liste = IngredientDAL.GetAll(
                idStock:           _stockFiltre?.Id ?? 0,
                stockReelSeulement: _stockReelSeulement);

            if (_filtreAlertes)
                liste = liste.Where(i => i.EstEnAlerte).ToList();
            return liste;
        }

        protected override void ConfigurerColonnes()
        {
            // Colonnes techniques toujours masquées
            CacherColonnes("Id", "IdFournisseurDefaut", "IdStockDefaut", "Actif",
                           "EstEnAlerte", "Marque", "SeuilAlerteStock",
                           "QteParConditionnement", "PrixParUniteBase",
                           "PrixAchatReference", "UniteMesure",
                           "StockCible",          // masqué : valeur en unités de base, sans sens pour l'utilisateur
                           "StockRatio",          // masqué : ratio interne
                           "NbParLot",            // masqué : non pertinent dans la vue liste
                           "DlcJoursReference",   // masqué : non pertinent dans la vue liste
                           "QualiteLabel");        // masqué : non pertinent dans la vue liste

            // StockActuel / StockPieces : visibles uniquement en mode Stock réel
            // StockCiblePieces : visible uniquement en mode Fiches (objectif en nombre de conditionnements)
            if (dgv.Columns["StockActuel"] != null)
                dgv.Columns["StockActuel"].Visible    = _stockReelSeulement;
            if (dgv.Columns["StockPieces"] != null)
                dgv.Columns["StockPieces"].Visible    = _stockReelSeulement;
            if (dgv.Columns["StockCiblePieces"] != null)
                dgv.Columns["StockCiblePieces"].Visible = !_stockReelSeulement;

            ConfigCol("Nom",                  "Ingrédient",          180, 120);
            ConfigCol("ConditionnementLabel", "Conditionnement",     130,  80);
            ConfigCol("QteCondLabel",         "Qté / cond.",          90,  65);
            ConfigCol("TypePhysique",         "Type physique",        90,  65);
            ConfigCol("Densite",              "Densité",              70,  55);
            ConfigCol("NomFournisseur",       "Fournisseur",         130,  90);
            ConfigCol("StockCiblePieces",     "Stock cible (pièces)", 110,  70);
            ConfigCol("Description",          "Description",         180, 100);

            if (_stockReelSeulement)
            {
                ConfigCol("StockPieces", "Pièces en stock", 100, 70);
                ConfigCol("StockActuel", "Poids / Volume",  110, 80);
            }

            string[] ordre = _stockReelSeulement
                ? new[] { "Nom", "ConditionnementLabel", "QteCondLabel", "TypePhysique", "Densite", "NomFournisseur", "StockPieces", "StockActuel", "Description" }
                : new[] { "Nom", "ConditionnementLabel", "QteCondLabel", "TypePhysique", "Densite", "NomFournisseur", "StockCiblePieces", "Description" };

            for (int i = 0; i < ordre.Length; i++)
                if (dgv.Columns[ordre[i]] != null)
                    dgv.Columns[ordre[i]].DisplayIndex = i;
        }

        protected override Form OuvrirFormulaire(Ingredient element)
        {
            if (!_stockReelSeulement)
                return new FrmIngredientEdit(element);

            // Mode Stock réel :
            //   Ajouter (element == null) → FrmAchatEdit vierge (nouveau lot)
            //   Modifier (element != null) → FrmAchats filtré par activité (consultation/gestion des lots)
            if (element == null)
                return new FrmAchatEdit(null, _activite?.Id ?? 0);

            return new FrmAchats(_activite);
        }

        protected override void Supprimer(Ingredient element)
            => IngredientDAL.Delete(element.Id);

        /// <summary>
        /// Appelé quand FrmAchatEdit retourne DialogResult.Retry ("+ Nouvelle Fiche")
        /// depuis le mode Stock réel. On ouvre FrmIngredientEdit directement —
        /// FrmAchatEdit est déjà fermé à ce stade.
        /// </summary>
        protected override void OnFormRetry()
        {
            using (var frm = new FrmIngredientEdit(null))
                if (frm.ShowDialog(this) == DialogResult.OK)
                    Charger();
        }

        protected override string NomElement(Ingredient element) => element?.Nom ?? "?";

        /// <summary>
        /// Coloration rouge pâle pour les ingrédients en alerte de stock.
        /// Non applicable en mode Fiches : la fiche n'a pas de quantité, EstEnAlerte n'a pas de sens.
        /// </summary>
        protected override void AppliquerStylesLignes()
        {
            if (!_stockReelSeulement) return;

            foreach (DataGridViewRow row in dgv.Rows)
                if (row.DataBoundItem is Ingredient ing && ing.EstEnAlerte)
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
        }

        // ── Chip panel de filtre par stock (fonctionnalité spécifique) ────

        protected override void OnLoad(EventArgs e)
        {
            BuildChipPanel();
            SouscrireCellFormatting();
            base.OnLoad(e);   // → FrmListeBase.OnLoad : Titre, Charger() = ChargerDonnees() + ConfigurerColonnes() + AppliquerStylesLignes()
        }

        private void SouscrireCellFormatting()
        {
            // Souscription unique au chargement — évite les doublons si Charger() est rappelé.
            // Actif uniquement en mode Stock réel.
            dgv.CellFormatting += (s, ev) =>
            {
                if (ev.RowIndex < 0 || !_stockReelSeulement) return;
                var col  = dgv.Columns[ev.ColumnIndex];
                var item = dgv.Rows[ev.RowIndex].DataBoundItem as Ingredient;
                if (col == null || item == null) return;

                if (col.Name == "StockPieces")
                    // Nombre de conditionnements complets en stock
                    ev.Value = item.StockPieces > 0 ? $"{item.StockPieces:0} pièces" : "—";
                else if (col.Name == "StockActuel")
                    // Quantité réelle convertie à l'unité la plus lisible (ex: 1875 cl → 18,75 l)
                    ev.Value = item.StockActuel > 0
                        ? UnitConvertisseur.FormatQte(item.StockActuel, item.UniteMesure)
                        : "—";
            };
        }

        private void BuildChipPanel()
        {
            // Les deux boutons de mode occupent ~180px sur la droite de la barre
            const int toggleW = 86;
            const int toggleG = 6;   // gap entre les deux boutons
            const int toggleTotal = toggleW * 2 + toggleG;

            _pnlChips = new FlowLayoutPanel
            {
                Location      = new Point(12, 46),
                Height        = 32,
                Width         = dgv.Width - toggleTotal - 8,
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = Color.White
            };

            _pnlChips.Controls.Add(CreerChip("Tous", null, true));

            // US-01 : tous les stocks physiques actifs
            foreach (var s in StockDAL.GetAll())
                _pnlChips.Controls.Add(CreerChip(s.Nom, s, false));

            // Bouton "Fiches" (mode catalogue, actif par défaut)
            _btnModefiches = new Button
            {
                Text      = "Fiches",
                Location  = new Point(dgv.Right - toggleTotal, 46),
                Size      = new Size(toggleW, 26),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                BackColor = AppColors.ChocoMed,
                ForeColor = Color.White,
                Cursor    = Cursors.Hand
            };
            _btnModefiches.FlatAppearance.BorderSize = 0;
            _btnModefiches.Click += (s, e) => AppliquerMode(false);

            // Bouton "Stock réel" (mode inventaire physique)
            _btnModeStockReel = new Button
            {
                Text      = "Stock réel",
                Location  = new Point(dgv.Right - toggleW, 46),
                Size      = new Size(toggleW, 26),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                BackColor = Color.FromArgb(220, 210, 200),
                ForeColor = Color.FromArgb(60, 45, 30),
                Cursor    = Cursors.Hand
            };
            _btnModeStockReel.FlatAppearance.BorderSize = 0;
            _btnModeStockReel.Click += (s, e) => AppliquerMode(true);

            // Décaler le DGV vers le bas pour laisser la place aux chips (loi de Proximité)
            dgv.Location = new Point(dgv.Location.X, 84);
            dgv.Height  -= 32;

            this.Controls.Add(_pnlChips);
            this.Controls.Add(_btnModefiches);
            this.Controls.Add(_btnModeStockReel);
            _pnlChips.BringToFront();
            _btnModefiches.BringToFront();
            _btnModeStockReel.BringToFront();
        }

        private void AppliquerMode(bool stockReelSeulement)
        {
            _stockReelSeulement = stockReelSeulement;

            // ── Toggle visuel ──────────────────────────────────────────────
            bool fiches = !stockReelSeulement;
            _btnModefiches.BackColor    = fiches  ? AppColors.ChocoMed : Color.FromArgb(220, 210, 200);
            _btnModefiches.ForeColor    = fiches  ? Color.White        : Color.FromArgb(60, 45, 30);
            _btnModefiches.Font         = new Font("Segoe UI", 8.5F, fiches  ? FontStyle.Bold : FontStyle.Regular);
            _btnModeStockReel.BackColor = !fiches ? AppColors.ChocoMed : Color.FromArgb(220, 210, 200);
            _btnModeStockReel.ForeColor = !fiches ? Color.White        : Color.FromArgb(60, 45, 30);
            _btnModeStockReel.Font      = new Font("Segoe UI", 8.5F, !fiches ? FontStyle.Bold : FontStyle.Regular);

            // ── Boutons CRUD — changent de rôle selon le mode ─────────────
            // Mode Fiches   : Ajouter / Modifier / Supprimer une fiche ingrédient
            // Mode Stock réel : Nouveau achat (FrmAchatEdit) / Voir les achats (FrmAchats) / Supprimer masqué
            if (stockReelSeulement)
            {
                btnAjouter.Text      = "＋  Nouveau achat";
                btnModifier.Text     = "☰  Voir les achats";
                btnSupprimer.Visible = false;
            }
            else
            {
                btnAjouter.Text      = "＋  Ajouter";
                btnModifier.Text     = "✎  Modifier";
                btnSupprimer.Visible = true;
            }

            Charger();
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
