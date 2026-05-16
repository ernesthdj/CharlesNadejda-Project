# BomReservationDAL
> Source: `CharlesNadejda/DAL/BomReservationDAL.cs`
> Type: code

## Description
Gestion des reservations de stock (soft-delete avec actif=0/1). Reserve du stock pendant la planification, libere a l execution ou expiration. GetTotalReservePourLot pour calcul disponibilite nette.

## Methodes
- `Insert(reservation) -> int`
- `GetByContexte(idContexte)`
- `GetTotalReservePourLot(idLot) -> decimal`
- `Liberer(id)`
- `LibererToutContexte(idContexte)`

## Relations
Appele par: BomProductionDAL.ConsumeStock (liberation), FrmBomProductionSimulation (creation)

### Connexions sortantes
- --method--> .GetByContexte()
- --method--> .GetTotalReservePourLot()
- --method--> .Insert()
- --method--> .Update()
- --method--> .Liberer()
- --method--> .LibererToutContexte()
- --method--> .Bind()
- --method--> .Map()

### Connexions entrantes
- BomReservationDAL.cs --contains-->
- [[MODULE_Stock_&_Achats|MODULE: Stock & Achats]] --documents-->
