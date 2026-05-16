# FrmBomFiches
> Source: `CharlesNadejda/Forms/FrmBomFiches.cs`
> Type: code

## Description
Liste des fiches BOM pour un niveau donne (herite FrmListeBase<BomFiche>). Message etat vide si aucune fiche. Styles lignes personnalises. Ouvre FrmBomFicheEdit en mode creation/edition.

## Methodes
- `ConfigurerColonnes() ChargerDonnees()->List<BomFiche> Supprimer(BomFiche) AppliquerStylesLignes() OuvrirFormulaire(BomFiche)->Form`

## Relations
Appelle: BomFicheDAL.GetByNiveau/Delete, FrmBomFicheEdit. Herite: FrmListeBase<BomFiche>.

### Connexions sortantes
- --references--> Label
- --inherits--> FrmListeBase
- --references--> BomNiveau
- --method--> .ChargerDonnees()
- --method--> .ConfigurerColonnes()
- --method--> .OuvrirFormulaire()
- --method--> .Supprimer()
- --method--> .NomElement()
- --method--> .AppliquerStylesLignes()
- --method--> .OnLoad()

### Connexions entrantes
- FrmBomFiches.cs --contains-->
