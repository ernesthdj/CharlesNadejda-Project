# StockDAL
> Source: `CharlesNadejda/DAL/StockDAL.cs`
> Type: code

## Description
CRUD des stocks avec liaison M:N activites (table activites_stocks). LierActivite/DelierActivite gerent la jonction. GetActivitesLiees retourne les activites associees a un stock.

## Methodes
- `GetAll(idStock,idActivite)`
- `GetById(id) -> Stock`
- `Insert(stock) -> int`
- `Update(stock)`
- `Delete(id)`
- `Bind(stock,cmd)`
- `LierActivite(idStock,idActivite)`
- `DelierActivite(idStock,idActivite)`
- `GetActivitesLiees(idStock) -> List<Activite>`

## Relations
Appele par: FrmStocks, FrmActiviteStocks. Utilise la table activites_stocks (v10+v11 discriminants).

### Connexions sortantes
- --method--> .GetAll()
- --method--> .GetById()
- --method--> .GetByActivite()
- --method--> .NomExiste()
- --method--> .Insert()
- --method--> .Update()
- --method--> .Delete()
- --method--> .LierActivite()
- --method--> .DelierActivite()
- --method--> .GetActivitesLiees()
- --method--> .Bind()
- --method--> .Map()

### Connexions entrantes
- StockDAL.cs --contains-->
- [[MODULE_Stock_&_Achats|MODULE: Stock & Achats]] --documents-->
