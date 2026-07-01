using System.Collections.Generic;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Liste des achats (lots d'ingrédients) — hérite de FrmListeBase&lt;Lot&gt;.
    /// C'est l'écran qui affiche tous les lots achetés dans un DataGridView,
    /// avec les boutons Ajouter/Modifier/Supprimer gérés par la classe de base.
    ///
    /// TICKET-14 : migration depuis partial class Form vers FrmListeBase&lt;T&gt;.
    /// Avant, tout le layout était dupliqué dans chaque formulaire liste.
    /// Maintenant j'hérite de FrmListeBase et je ne surcharge que le nécessaire.
    /// </summary>
    public class FrmAchats : FrmListeBaseLot
    {
        // L'activité courante — permet de filtrer les achats par activité
        // Si null, on affiche tous les achats (mode "vue globale")
        private readonly Activite _activite;   // null = tous

        // Constructeur — l'activité est optionnelle (default null = pas de filtre)
        public FrmAchats(Activite activite = null)
        {
            _activite = activite;
        }

        // ── Membres abstraits FrmListeBase<Lot> ───────────────────────────
        // Je dois implémenter les 5 membres abstraits de la classe de base

        // Titre affiché dans la barre de titre et le label en haut du formulaire
        // Si une activité est sélectionnée, le titre inclut son nom
        protected override string Titre => _activite != null
            ? $"Achats — {_activite.Nom}"
            : "Tous les achats";

        // ChargerDonnees() — appelle le DAL pour récupérer la liste des lots
        // Si _activite est null, je passe 0 pour récupérer tous les lots
        protected override List<Lot> ChargerDonnees()
            => LotDAL.GetAll(_activite?.Id ?? 0);

        // ConfigurerColonnes() — configure les colonnes du DataGridView après le data-binding
        // J'utilise les helpers CacherColonnes() et ConfigCol() de FrmListeBase
        protected override void ConfigurerColonnes()
        {
            // Je cache les colonnes techniques ou redondantes — l'utilisateur n'en a pas besoin
            CacherColonnes("Id", "IdFicheIngredient", "IdFournisseur",
                           "QuantiteDisponible", "Notes", "TvaPct",
                           "UniteMesure", "ConditionnementLabel",
                           "QteParConditionnement", "NbConditionnements",
                           "PrixUnitaireBase");

            // ConfigCol(nom, header, largeur, minimum) — renomme les colonnes en français lisible
            // et fixe les largeurs initiales et minimales
            ConfigCol("NomIngredient",    "Ingrédient",        180, 120);
            ConfigCol("NumeroLot",        "N° lot",             90,  70);
            ConfigCol("NomFournisseur",   "Fournisseur",        140,  90);
            ConfigCol("DateAchat",        "Date achat",          95,  80);
            ConfigCol("DatePeremption",   "Péremption",          95,  80);
            ConfigCol("QuantiteInitiale", "Qté achetée",        110,  80);
            ConfigCol("PrixUnitaire",     "Prix unit. HTVA",    100,  80);
            ConfigCol("PrixAchatReel",    "Total HTVA",          95,  75);
            ConfigCol("ReferenceFacture", "Réf. facture",       100,  80);

            // CellFormatting — je formate certaines cellules à la volée lors du rendu
            // C'est mieux que de modifier le modèle parce que ça n'affecte que l'affichage
            dgv.CellFormatting += (s, ev) =>
            {
                if (ev.RowIndex < 0) return;  // Ignorer l'en-tête
                var col = dgv.Columns[ev.ColumnIndex];
                var item = dgv.Rows[ev.RowIndex].DataBoundItem as Lot;
                if (item == null) return;

                // NomIngredient : j'ajoute la quantité par conditionnement à côté du nom
                // Ex: "Beurre de cacao 1kg" au lieu de juste "Beurre de cacao"
                if (col.Name == "NomIngredient")
                    ev.Value = $"{item.NomIngredient} {UnitConvertisseur.FormatQte(item.QteParConditionnement, item.UniteMesure)}";
                // QuantiteInitiale : formatage avec l'unité (ex: "5 kg" au lieu de "5000")
                else if (col.Name == "QuantiteInitiale")
                    ev.Value = UnitConvertisseur.FormatQte(item.QuantiteInitiale, item.UniteMesure);
                // PrixUnitaire et PrixAchatReel : formatage monétaire (ex: "12,50 €")
                else if (col.Name == "PrixUnitaire")
                    ev.Value = UnitConvertisseur.FormatPrix(item.PrixUnitaire);
                else if (col.Name == "PrixAchatReel")
                    ev.Value = UnitConvertisseur.FormatPrix(item.PrixAchatReel);
            };
        }

        // OuvrirFormulaire() — crée et retourne le formulaire d'édition d'un lot
        // Si element == null → mode création (nouveau lot)
        // Si element != null → mode modification (lot existant)
        // Je passe toujours l'id de l'activité pour le filtre dans FrmAchatEdit
        protected override Form OuvrirFormulaire(Lot element)
            => element == null
                ? new FrmAchatEdit(null, _activite?.Id ?? 0)
                : new FrmAchatEdit(element, _activite?.Id ?? 0);

        // Supprimer() — appelle le DAL pour supprimer un lot par son id
        // La confirmation utilisateur est déjà gérée par FrmListeBase.OnSupprimer()
        protected override void Supprimer(Lot element)
            => LotDAL.Delete(element.Id);

        // NomElement() — retourne un nom lisible pour la boîte de confirmation de suppression
        // Ex: "Beurre de cacao du 10/06/2026"
        protected override string NomElement(Lot element)
            => element != null ? $"{element.NomIngredient} du {element.DateAchat:dd/MM/yyyy}" : "?";

        // OnFormRetry() — mon hook pour intercepter le signal 'Nouvelle Fiche' depuis FrmAchatEdit
        // Quand FrmAchatEdit retourne DialogResult.Retry, ça veut dire que l'utilisateur
        // a cliqué sur "Créer un ingrédient" (parce qu'il n'existait pas encore).
        // J'ouvre FrmIngredientEdit pour créer l'ingrédient, puis je recharge la liste.
        protected override void OnFormRetry()
        {
            using (var frm = new FrmIngredientEdit(null))
            {
                frm.ShowDialog(this);
                // Après création de l'ingrédient, je recharge la liste des achats
                // pour que le nouvel ingrédient soit disponible dans FrmAchatEdit
                Charger();
            }
        }
    }
}
