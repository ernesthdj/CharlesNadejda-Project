using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Liste des fiches d'un niveau de transformation donné — hérite de FrmListeBase&lt;BomFiche&gt;.
    ///
    /// TICKET-14 : migration depuis partial class Form vers FrmListeBase&lt;T&gt;.
    /// TICKET-19 : label état vide préservé via AppliquerStylesLignes().
    /// </summary>
    public class FrmBomFiches : FrmListeBase<BomFiche>
    {
        private readonly BomNiveau _niveau;
        private Label              _lblEtatVide;   // TICKET-19 : message quand liste vide

        public FrmBomFiches(BomNiveau niveau)
        {
            _niveau = niveau;
        }

        // ── Membres abstraits FrmListeBase<BomFiche> ──────────────────────

        protected override string Titre
            => $"{_niveau.NomContexte}  ›  {_niveau.Nom}  (N{_niveau.Ordre})";

        protected override List<BomFiche> ChargerDonnees()
            => BomFicheDAL.GetByNiveau(_niveau.Id);

        protected override void ConfigurerColonnes()
        {
            CacherColonnes("Id", "IdNiveau", "Actif", "DateCreation", "Lignes",
                           "NomNiveau", "OrdreNiveau", "IdContexte", "NomContexte",
                           "CoutBatch", "CoutUnitaire");

            ConfigCol("Nom",              "Nom de la fiche", 220, 120);
            ConfigCol("QuantiteOutput",   "Qté/exec.",        100,  75);
            ConfigCol("TempsPreparation", "Temps (min)",       90,  70);
            ConfigCol("Description",      "Description",      200,  80);

            CacherColonnes("UniteOutput");

            dgv.CellFormatting += (s, ev) =>
            {
                if (ev.RowIndex < 0) return;
                var col = dgv.Columns[ev.ColumnIndex];
                if (col.Name == "QuantiteOutput" && dgv.Rows[ev.RowIndex].DataBoundItem is BomFiche f)
                    ev.Value = UnitConvertisseur.FormatQte(f.QuantiteOutput, f.UniteOutput);
            };

            if (dgv.Columns["Activite"] != null)
            {
                dgv.Columns["Activite"].HeaderText = "Activité";
                dgv.Columns["Activite"].Width      = 110;
            }
        }

        protected override Form OuvrirFormulaire(BomFiche element)
            => element == null
                ? new FrmBomFicheEdit(null, _niveau)
                : new FrmBomFicheEdit(BomFicheDAL.GetById(element.Id), _niveau);

        protected override void Supprimer(BomFiche element)
            => BomFicheDAL.Delete(element.Id);

        protected override string NomElement(BomFiche element) => element?.Nom ?? "?";

        /// <summary>TICKET-19 : affiche le label état vide si aucune fiche dans ce niveau.</summary>
        protected override void AppliquerStylesLignes()
        {
            if (_lblEtatVide != null)
                _lblEtatVide.Visible = dgv.Rows.Count == 0;
        }

        // ── Cycle de vie ──────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            // TICKET-19 : label état vide — positionné sur le DGV, caché par défaut
            _lblEtatVide = new Label
            {
                Text      = "Aucune fiche dans ce niveau.\nCliquez « ＋ Ajouter » pour créer la première.",
                Font      = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = Color.FromArgb(140, 110, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock      = DockStyle.Fill,
                Visible   = false
            };
            dgv.Controls.Add(_lblEtatVide);

            base.OnLoad(e);   // → FrmListeBase : Titre + Charger() → AppliquerStylesLignes()
        }
    }
}
