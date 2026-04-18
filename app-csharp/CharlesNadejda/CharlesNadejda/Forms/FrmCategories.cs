using System.Collections.Generic;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Liste et gestion des catégories de produits.
    /// Migré vers FrmListeBase&lt;Categorie&gt; — tout le boilerplate est dans la classe de base.
    /// </summary>
    public class FrmCategories : FrmListeBase<Categorie>
    {
        protected override string Titre => "Catégories de produits";

        protected override List<Categorie> ChargerDonnees()
            => CategorieDAL.GetAll();

        protected override void ConfigurerColonnes()
        {
            CacherColonnes("Id");
            ConfigCol("Nom",            "Catégorie",   200, 120);
            ConfigCol("Description",    "Description", 300,  80);
            ConfigCol("OrdreAffichage", "Ordre",        60,  50);
        }

        protected override Form OuvrirFormulaire(Categorie cat)
            => new FrmCategorieEdit(cat);

        protected override void Supprimer(Categorie cat)
            => CategorieDAL.Delete(cat.Id);

        protected override string NomElement(Categorie cat) => cat.Nom;
    }
}
