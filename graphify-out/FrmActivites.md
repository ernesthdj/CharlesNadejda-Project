# FrmActivites
> Source: `CharlesNadejda/Forms/FrmActivites.cs`
> Type: code

## Description
Liste des activites construite programmatiquement (pas FrmListeBase — pattern different car boutons specifiques). DGV + 5 boutons (Nouveau/Modifier/Desactiver/Supprimer/Gerer Stocks). Desactiver verifie les cascades. Gerer Stocks ouvre FrmActiviteStocks.

## Methodes
- `BuildUI() Charger() Nouveau() Modifier() Desactiver() Supprimer() GererStocks() CreerBouton() ActiviteSelectionnee()->Activite`

## Regles (JOURNAL.md)
- #9 jamais hardcoder nom activite
- #10 grepper tous call sites apres refactor

## Relations
Appelle: ActiviteDAL.GetAll/Insert/Update/Delete/Desactiver, FrmActiviteEdit, FrmActiviteStocks. Appele par: FrmPrincipal via navigation.

### Connexions sortantes
- --inherits--> Form
- --references--> DataGridView
- --references--> Button
- --method--> .BuildUI()
- --method--> .CreerBouton()
- --method--> .Charger()
- --method--> .Nouveau()
- --method--> .Modifier()
- --method--> .Desactiver()
- --method--> .Supprimer()
- --method--> .GererStocks()
- --method--> .ActiviteSelectionnee()

### Connexions entrantes
- FrmActivites.cs --contains-->
