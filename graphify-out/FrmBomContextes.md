# FrmBomContextes
> Source: `CharlesNadejda/Forms/FrmBomContextes.cs`
> Type: code

## Description
Liste des contextes BOM pour une activite (herite FrmListeBase<BomContexte>). Bouton supplementaire Niveaux pour ouvrir FrmBomNiveaux sur le contexte selectionne.

## Methodes
- `ConfigurerColonnes() ChargerDonnees()->List<BomContexte> Supprimer(BomContexte) OuvrirNiveaux() OuvrirFormulaire(BomContexte)->Form`

## Relations
Appelle: BomContexteDAL.GetAll/Delete, FrmBomContexteEdit, FrmBomNiveaux. Herite: FrmListeBase<BomContexte>.

### Connexions sortantes
- --inherits--> FrmListeBase
- --references--> Activite
- --references--> Button
- --method--> .ChargerDonnees()
- --method--> .ConfigurerColonnes()
- --method--> .OuvrirFormulaire()
- --method--> .Supprimer()
- --method--> .NomElement()
- --method--> .OnLoad()
- --method--> .OuvrirNiveaux()

### Connexions entrantes
- FrmBomContextes.cs --contains-->
