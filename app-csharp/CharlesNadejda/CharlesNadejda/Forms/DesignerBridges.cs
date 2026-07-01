using System.Collections.Generic;
using System.Windows.Forms;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    // =========================================================================
    // DesignerBridges.cs
    //
    // Problème : le Designer Visual Studio ne peut pas instancier une classe
    // abstraite ou générique (FrmListeBase<T>, FrmEditBase) pour rendre la vue.
    //
    // Solution : une classe-pont concrète, non-abstraite, non-générique par type.
    // Le Designer instancie le pont → peut afficher le formulaire fils.
    //
    // RÈGLE : ces classes ne sont JAMAIS instanciées dans le code applicatif.
    //         Elles existent uniquement pour le Designer VS.
    // =========================================================================

    // ── FrmListeBase<T> — un pont par type d'entité ──────────────────────────

    public class FrmListeBaseStock       : FrmListeBase<Stock>
    {
        protected override string         Titre              => string.Empty;
        protected override List<Stock>    ChargerDonnees     () => new List<Stock>();
        protected override void           ConfigurerColonnes () { }
        protected override Form           OuvrirFormulaire   (Stock e)      => null;
        protected override void           Supprimer          (Stock e)      { }
    }

    public class FrmListeBaseIngredient  : FrmListeBase<Ingredient>
    {
        protected override string           Titre              => string.Empty;
        protected override List<Ingredient> ChargerDonnees     () => new List<Ingredient>();
        protected override void             ConfigurerColonnes () { }
        protected override Form             OuvrirFormulaire   (Ingredient e) => null;
        protected override void             Supprimer          (Ingredient e) { }
    }

    public class FrmListeBaseLot         : FrmListeBase<Lot>
    {
        protected override string      Titre              => string.Empty;
        protected override List<Lot>   ChargerDonnees     () => new List<Lot>();
        protected override void        ConfigurerColonnes () { }
        protected override Form        OuvrirFormulaire   (Lot e)          => null;
        protected override void        Supprimer          (Lot e)          { }
    }

    public class FrmListeBaseActivite    : FrmListeBase<Activite>
    {
        protected override string          Titre              => string.Empty;
        protected override List<Activite>  ChargerDonnees     () => new List<Activite>();
        protected override void            ConfigurerColonnes () { }
        protected override Form            OuvrirFormulaire   (Activite e)  => null;
        protected override void            Supprimer          (Activite e)  { }
    }

    public class FrmListeBaseFournisseur : FrmListeBase<Fournisseur>
    {
        protected override string             Titre              => string.Empty;
        protected override List<Fournisseur>  ChargerDonnees     () => new List<Fournisseur>();
        protected override void               ConfigurerColonnes () { }
        protected override Form               OuvrirFormulaire   (Fournisseur e) => null;
        protected override void               Supprimer          (Fournisseur e) { }
    }

    public class FrmListeBaseBomContexte : FrmListeBase<BomContexte>
    {
        protected override string              Titre              => string.Empty;
        protected override List<BomContexte>   ChargerDonnees     () => new List<BomContexte>();
        protected override void                ConfigurerColonnes () { }
        protected override Form                OuvrirFormulaire   (BomContexte e) => null;
        protected override void                Supprimer          (BomContexte e) { }
    }

    public class FrmListeBaseBomFiche    : FrmListeBase<BomFiche>
    {
        protected override string           Titre              => string.Empty;
        protected override List<BomFiche>   ChargerDonnees     () => new List<BomFiche>();
        protected override void             ConfigurerColonnes () { }
        protected override Form             OuvrirFormulaire   (BomFiche e)  => null;
        protected override void             Supprimer          (BomFiche e)  { }
    }

    public class FrmListeBaseBomNiveau   : FrmListeBase<BomNiveau>
    {
        protected override string            Titre              => string.Empty;
        protected override List<BomNiveau>   ChargerDonnees     () => new List<BomNiveau>();
        protected override void              ConfigurerColonnes () { }
        protected override Form              OuvrirFormulaire   (BomNiveau e) => null;
        protected override void              Supprimer          (BomNiveau e) { }
    }

    // ── FrmEditBase — un seul pont suffit (pas de générique) ─────────────────

    public class FrmEditBaseDesigner : FrmEditBase
    {
        protected override bool Valider    () => true;
        protected override void Sauvegarder() { }
    }
}
