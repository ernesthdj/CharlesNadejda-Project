# CharlesNadejda.Forms
> Source: `CharlesNadejda/Forms/FrmStocks.cs`
> Type: code

## Description
Liste des stocks avec SplitContainer : DGV a gauche (tous les stocks) + CheckedListBox a droite (activites liees M:N). Synchronisation differentielle des liaisons via ItemCheck. Boutons Nouveau/Modifier/Supprimer. Verification cascade avant suppression (StockContientDonnees).

## Methodes
- `BuildUI() Charger() ChargerLiaisons(idStock) ActualiserBoutonSupprimer(idStock) StockContientDonnees(idStock)->bool ClbActivites_ItemCheck() Nouveau() Modifier() Supprimer()`

## Relations
Appelle: StockDAL.GetAll/Insert/Update/Delete/LierActivite/DelierActivite, ActiviteDAL.GetAll. Appele par: FrmPrincipal via navigation.

### Connexions entrantes
- FrmStocks.cs --contains-->
