# FrmPrincipal
> Source: `CharlesNadejda/Forms/FrmPrincipal.Production.cs`
> Type: code

## Description
Partial class Production du shell. Ecran inline avec : header KPI, parametres (Contexte/Niveau/Fiche cascadants), simulation DGV avec code couleur stock, historique productions, journal activite (chatter). 26 methodes dediees.

## Methodes
- `ShowProductionScreen() BuildProdHeader() BuildProdKpiBar() BuildProdParams() BuildProdSimulation() BuildProdDgv() ProdBtnSimuler_Click() ProdBtnLancer_Click() ProdContexte_Changed() ProdNiveau_Changed() ProdFiche_Changed() ProdColoriserLignes() ProdRefreshHistorique() ProdAjouterJournalEntry()`

## Regles (JOURNAL.md)
- #15 quantiteCible=batches pas quantite finale
- #16 conversion unite OBLIGATOIRE avant comparaison stock

## Relations
Appelle: BomProductionDAL.Executer/Simuler, BomFicheDAL, BomNiveauDAL, BomContexteDAL, SimulationService

### Connexions sortantes
- --references--> bool
- --references--> ComboBox
- --references--> Label
- --references--> TextBox
- --references--> NumericUpDown
- --references--> DataGridView
- --references--> Button
- --references--> List
- --references--> Panel
- --method--> .ShowProductionScreen()
- --method--> .BuildProdHeader()
- --method--> .BuildProdKpiBar()
- --method--> .BuildProdParams()
- --method--> .MakeProdLabel()
- --method--> .MakeProdCombo()
- ... +20 autres

### Connexions entrantes
- FrmPrincipal.Production.cs --contains-->
