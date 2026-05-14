using System.Collections.Generic;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Liste des achats (lots d'ingrédients) — hérite de FrmListeBase&lt;Lot&gt;.
    ///
    /// TICKET-14 : migration depuis partial class Form vers FrmListeBase&lt;T&gt;.
    /// </summary>
    public class FrmAchats : FrmListeBase<Lot>
    {
        private readonly Activite _activite;   // null = tous

        public FrmAchats(Activite activite = null)
        {
            _activite = activite;
        }

        // ── Membres abstraits FrmListeBase<Lot> ───────────────────────────

        protected override string Titre => _activite != null
            ? $"Achats — {_activite.Nom}"
            : "Tous les achats";

        protected override List<Lot> ChargerDonnees()
            => LotDAL.GetAll(_activite?.Id ?? 0);

        protected override void ConfigurerColonnes()
        {
            CacherColonnes("Id", "IdFicheIngredient", "IdFournisseur",
                           "QuantiteDisponible", "Notes", "TvaPct",
                           "UniteMesure", "ConditionnementLabel",
                           "QteParConditionnement", "NbConditionnements",
                           "PrixUnitaireBase");

            ConfigCol("NomIngredient",    "Ingrédient",        180, 120);
            ConfigCol("NumeroLot",        "N° lot",             90,  70);
            ConfigCol("NomFournisseur",   "Fournisseur",        140,  90);
            ConfigCol("DateAchat",        "Date achat",          95,  80);
            ConfigCol("DatePeremption",   "Péremption",          95,  80);
            ConfigCol("QuantiteInitiale", "Qté achetée",        110,  80);
            ConfigCol("PrixUnitaire",     "Prix unit. HTVA",    100,  80);
            ConfigCol("PrixAchatReel",    "Total HTVA",          95,  75);
            ConfigCol("ReferenceFacture", "Réf. facture",       100,  80);

            dgv.CellFormatting += (s, ev) =>
            {
                if (ev.RowIndex < 0) return;
                var col = dgv.Columns[ev.ColumnIndex];
                var item = dgv.Rows[ev.RowIndex].DataBoundItem as Lot;
                if (item == null) return;
                if (col.Name == "NomIngredient")
                    ev.Value = $"{item.NomIngredient} {UnitConvertisseur.FormatQte(item.QteParConditionnement, item.UniteMesure)}";
                else if (col.Name == "QuantiteInitiale")
                    ev.Value = UnitConvertisseur.FormatQte(item.QuantiteInitiale, item.UniteMesure);
                else if (col.Name == "PrixUnitaire")
                    ev.Value = UnitConvertisseur.FormatPrix(item.PrixUnitaire);
                else if (col.Name == "PrixAchatReel")
                    ev.Value = UnitConvertisseur.FormatPrix(item.PrixAchatReel);
            };
        }

        protected override Form OuvrirFormulaire(Lot element)
            => element == null
                ? new FrmAchatEdit(null, _activite?.Id ?? 0)
                : new FrmAchatEdit(element, _activite?.Id ?? 0);

        protected override void Supprimer(Lot element)
            => LotDAL.Delete(element.Id);

        protected override string NomElement(Lot element)
            => element != null ? $"{element.NomIngredient} du {element.DateAchat:dd/MM/yyyy}" : "?";
    }
}
