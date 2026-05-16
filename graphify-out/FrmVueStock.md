# FrmVueStock
> Source: `CharlesNadejda/Forms/FrmVueStock.cs`
> Type: code

## Description
Vue stock globale avec 3 sections groupees (Ingredients/Intermediaires/Finals). DGV avec tri desactive (sections manuelles). Volet detail contextuel a droite : clic ingredient -> identite+stock+fournisseur+fiches utilisatrices. Clic produit -> composition+cout+DLC.

## Methodes
- `ChargerDonnees() AfficherDetail(type,id) AddDetailHeader() AddDetailSection() AddDetailRow() AddDetailTag() RenderIngredientDetail() RenderStockDetail()`

## Regles (JOURNAL.md)
- #25 tri desactive quand sections header manuelles
- #6 DGV Sizable+AllCells+MinimumWidth+Anchor4bords

## Relations
Appelle: VueStockGlobalDAL.GetAll, BomFicheLigneDAL.GetFichesUtilisant, IngredientDAL.GetById. God node 30 edges.

### Connexions sortantes
- --references--> int
- --references--> Label
- --inherits--> Form
- --references--> DataGridView
- --references--> List
- --references--> FlowLayoutPanel
- --references--> Panel
- --references--> Dictionary
- --method--> .BuildUI()
- --method--> .CreerLegende()
- --method--> .Charger()
- --method--> .BuildChips()
- --method--> .AjouterChip()
- --method--> .AppliquerFiltre()
- --method--> .RemplirGrille()
- ... +14 autres

### Connexions entrantes
- FrmVueStock.cs --contains-->
