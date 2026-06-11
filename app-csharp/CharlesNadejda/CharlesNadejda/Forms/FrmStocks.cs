using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Gestion des stocks (contenants physiques/logiques) — CRUD + liaison M:N Stock ↔ Activité.
    /// Hérite de FrmListeBase&lt;Stock&gt; pour le layout et le workflow CRUD standard.
    ///
    /// Particularité : un SplitContainer remplace le DGV simple de FrmListeBase.
    ///   - Panel1 : le DGV hérité de FrmListeBase (liste des stocks)
    ///   - Panel2 : CheckedListBox des activités liées au stock sélectionné
    ///
    /// La liaison M:N est gérée en temps réel : chaque coche/décoche fait un INSERT/DELETE
    /// immédiat via StockDAL.LierActivite / DelierActivite.
    /// </summary>
    public class FrmStocks : FrmListeBase<Stock>
    {
        // ── Contrôles spécifiques au panel de liaison ───────────────
        private readonly SplitContainer   _split;
        private readonly CheckedListBox   _clbActivites;
        private readonly ToolTip          _tip;

        // Verrou anti-faux-événements pendant le chargement des liaisons
        private bool _chargeantLiaisons;

        public FrmStocks()
        {
            _tip = new ToolTip();

            // ── SplitContainer — englobe le DGV hérité + panel liaisons ──
            // On retire le DGV du Controls de la base pour le placer dans Panel1
            Controls.Remove(dgv);

            _split = new SplitContainer
            {
                Location         = dgv.Location,
                Size             = dgv.Size,
                Anchor           = dgv.Anchor,
                Orientation      = Orientation.Vertical,
                IsSplitterFixed  = false,
                BackColor        = AppColors.GridLine
            };

            // Panel1 : le DGV hérité (liste des stocks)
            dgv.Dock = DockStyle.Fill;
            _split.Panel1.Controls.Add(dgv);

            // Panel2 : GroupBox avec CheckedListBox des activités
            var grpLiaison = new GroupBox
            {
                Text      = "Activités liées",
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.ChocoBrand,
                Dock      = DockStyle.Fill,
                Padding   = new Padding(8)
            };

            _clbActivites = new CheckedListBox
            {
                Dock         = DockStyle.Fill,
                CheckOnClick = true,
                Font         = new Font("Segoe UI", 9F),
                BorderStyle  = BorderStyle.None,
                BackColor    = Color.FromArgb(252, 248, 244)
            };
            _clbActivites.ItemCheck += ClbActivites_ItemCheck;

            var lblHint = new Label
            {
                Text      = "Sélectionnez un stock",
                Font      = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.Gray,
                Dock      = DockStyle.Bottom,
                Height    = 20,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            grpLiaison.Controls.Add(_clbActivites);
            grpLiaison.Controls.Add(lblHint);
            _split.Panel2.Controls.Add(grpLiaison);

            // SplitterDistance fixé après le premier layout (quand Width > 0)
            bool firstLayout = true;
            _split.Layout += (s, e) =>
            {
                if (!firstLayout) return;
                if (_split.Width <= 0) return;
                firstLayout            = false;
                _split.SplitterDistance = Math.Max(100, _split.Width - 250);
                _split.IsSplitterFixed  = true;
            };

            Controls.Add(_split);

            // Chargement des liaisons à chaque changement de sélection dans le DGV
            dgv.SelectionChanged += DGV_SelectionChanged;
        }

        // ── Membres abstraits — logique métier spécifique ───────────

        protected override string Titre => "Stocks";

        protected override List<Stock> ChargerDonnees()
            => StockDAL.GetAll();

        protected override void ConfigurerColonnes()
        {
            CacherColonnes("Id", "Actif");
            ConfigCol("Nom",          "Nom",         200, 140);
            ConfigCol("Description",  "Description", 220, 140);
            ConfigCol("DateCreation", "Créé le",     100,  80);
        }

        protected override Form OuvrirFormulaire(Stock element)
            => new FrmStockEdit(element);

        protected override void Supprimer(Stock element)
            => StockDAL.Delete(element.Id);

        protected override string NomElement(Stock element)
            => element?.Nom ?? "?";

        // ── Sélection dans la grille → chargement des liaisons ──────

        private void DGV_SelectionChanged(object sender, EventArgs e)
        {
            var stock = Selectionne();

            if (stock == null)
            {
                _chargeantLiaisons = true;
                _clbActivites.Items.Clear();
                _chargeantLiaisons = false;
                ActualiserBoutonSupprimer(0, estVide: true);
                return;
            }

            ChargerLiaisons(stock.Id);
            ActualiserBoutonSupprimer(stock.Id, estVide: false);
        }

        private void ChargerLiaisons(int idStock)
        {
            _chargeantLiaisons = true;
            try
            {
                _clbActivites.Items.Clear();

                var toutesActivites = ActiviteDAL.GetAll();
                var liees           = StockDAL.GetActivitesLiees(idStock);

                foreach (var act in toutesActivites)
                    _clbActivites.Items.Add(act, liees.Contains(act.Id));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des activités : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _chargeantLiaisons = false;
            }
        }

        private void ActualiserBoutonSupprimer(int idStock, bool estVide)
        {
            if (estVide)
            {
                btnSupprimer.Enabled = false;
                _tip.SetToolTip(btnSupprimer, "");
                return;
            }

            bool contientDonnees = StockDAL.ContientDonnees(idStock);
            btnSupprimer.Enabled = !contientDonnees;

            _tip.SetToolTip(btnSupprimer,
                contientDonnees
                    ? "Ce stock contient des ingrédients ou des lots actifs — suppression impossible."
                    : "");
        }

        // ── Liaison M:N : ItemCheck → INSERT ou DELETE immédiat ─────

        private void ClbActivites_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_chargeantLiaisons) return;

            var stock = Selectionne();
            if (stock == null) return;

            if (!(_clbActivites.Items[e.Index] is Activite act)) return;

            try
            {
                if (e.NewValue == CheckState.Checked)
                    StockDAL.LierActivite(act.Id, stock.Id);
                else
                    StockDAL.DelierActivite(act.Id, stock.Id);
            }
            catch (Exception ex)
            {
                e.NewValue = e.CurrentValue;
                MessageBox.Show("Erreur lors de la mise à jour de la liaison : " + ex.Message,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
