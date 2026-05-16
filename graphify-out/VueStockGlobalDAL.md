# VueStockGlobalDAL
> Source: `CharlesNadejda/DAL/VueStockGlobalDAL.cs`
> Type: code

## Description
Vue unifiee de TOUS les stocks (ingredients via lots_ingredients + intermediaires/finals via bom_stocks). Basee sur une VIEW MySQL. 3 sections : Ingredients, Intermediaires, Finals.

## Methodes
- `GetAll(idActivite) -> List<VueStockGlobal>`

## Regles (JOURNAL.md)
- #14 date_peremption vs date_dlc (aliaser dans VIEW)
- #17 JOIN pour label FK dans VIEW

## Relations
Appele par: FrmVueStock. Consomme la VIEW vue_stock_global (migration v11).

### Connexions sortantes
- --references--> string
- --method--> .GetAll()
- --method--> .GetByActivite()
- --method--> .GetByContexte()
- --method--> .GetByNiveau()
- --method--> .Execute()
- --method--> .Map()

### Connexions entrantes
- VueStockGlobalDAL.cs --contains-->
- [[MODULE_Stock_&_Achats|MODULE: Stock & Achats]] --documents-->
