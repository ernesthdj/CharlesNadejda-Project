# BomStockDAL
> Source: `CharlesNadejda/DAL/BomStockDAL.cs`
> Type: code

## Description
Acces aux stocks BOM intermediaires. FIFO Engine : trie par date_production ASC pour consommer le plus ancien en premier. Gere aussi la disponibilite nette (stock - reservations).

## Methodes
- `GetBomStocksFIFO(idNiveau,idFiche) -> List<(id,dispo,coutUnit)>`
- `GetLotsDispoFIFO(idIngredient) -> List<(id,dispo,prixUnit)>`
- `GetByNiveau(idNiveau) -> List<BomStock>`
- `GetDisponible(idNiveau,idFiche) -> decimal`
- `GetDisponibleIngredient(idIngredient) -> decimal`

## Relations
Appele par: BomProductionDAL.ConsumeStock, BomProductionDAL.VerifierDisponibilite. Source FIFO pour lots_ingredients ET bom_stocks.

### Connexions sortantes
- --method--> .GetByNiveau()
- --method--> .GetDisponible()
- --method--> .GetDisponibleIngredient()
- --method--> .GetLotsDispoFIFO()
- --method--> .GetBomStocksFIFO()
- --method--> .Map()

### Connexions entrantes
- BomStockDAL.cs --contains-->
- [[MODULE_Production_BOM|MODULE: Production BOM]] --documents-->
