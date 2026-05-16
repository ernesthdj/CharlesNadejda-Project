# LotDAL
> Source: `CharlesNadejda/DAL/LotDAL.cs`
> Type: code

## Description
CRUD des lots d achat (lots_ingredients). Un lot = une livraison d un ingredient avec quantite, prix, DLC. Consomme en FIFO par BomProductionDAL.

## Methodes
- `GetAll(idIngredient,idActivite)`
- `GetById(id) -> Lot`
- `Insert(lot) -> int`
- `Update(lot)`
- `Delete(id)`
- `Bind(lot,cmd)`

## Relations
Appele par: FrmAchats, FrmAchatEdit, BomStockDAL.GetLotsDispoFIFO

### Connexions sortantes
- --method--> .GetAll()
- --method--> .GetById()
- --method--> .Insert()
- --method--> .Update()
- --method--> .Delete()
- --method--> .Bind()
- --method--> .Map()

### Connexions entrantes
- LotDAL.cs --contains-->
