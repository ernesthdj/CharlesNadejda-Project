using System.Collections.Generic;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Liste CRUD des fournisseurs de matières premières.
    /// Hérite de FrmListeBase&lt;Fournisseur&gt; — le layout et le workflow CRUD
    /// sont entièrement gérés par la classe de base.
    ///
    /// Migration depuis l'ancien FrmFournisseurs partial + Designer.cs :
    /// tout le code de construction UI (DGV, boutons, styles) est désormais
    /// dans FrmListeBase. Ici on ne fournit que la logique métier.
    /// </summary>
    public class FrmFournisseurs : FrmListeBaseFournisseur
    {
        // ── Membres abstraits — logique métier spécifique ───────────

        protected override string Titre => "Fournisseurs";

        protected override List<Fournisseur> ChargerDonnees()
            => FournisseurDAL.GetAll();

        protected override void ConfigurerColonnes()
        {
            CacherColonnes("Id", "Notes");
            ConfigCol("Nom",       "Nom",       180, 120);
            ConfigCol("Contact",   "Contact",   140, 80);
            ConfigCol("Email",     "Email",     180, 100);
            ConfigCol("Telephone", "Téléphone", 120, 80);
            ConfigCol("Adresse",   "Adresse",   180, 100);
        }

        protected override System.Windows.Forms.Form OuvrirFormulaire(Fournisseur element)
            => new FrmFournisseurEdit(element);

        protected override void Supprimer(Fournisseur element)
            => FournisseurDAL.Delete(element.Id);

        protected override string NomElement(Fournisseur element)
            => element?.Nom ?? "?";
    }
}
