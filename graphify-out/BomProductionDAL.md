# BomProductionDAL
> Source: `CharlesNadejda/DAL/BomProductionDAL.cs`
> Type: code

## Description
Moteur de production transactionnel. Flux : VerifierDisponibilite (dans TX anti-TOCTOU) > INSERT bom_productions > ConsumeStock FIFO par ligne > UPDATE couts > INSERT bom_stocks. Non-recursif : l utilisateur lance sequentiellement par niveau. Les couts se propagent vers le haut via le cout_unitaire stocke.

## Methodes
- `Executer(idNiveau,idFiche,quantiteCible,notes,delaiJours) -> int`
- `VerifierDisponibilite(idNiveau,idFiche,quantiteCible) -> List<BomManque>`
- `Simuler(idNiveau,idFiche,quantiteCible) -> List<BomManque>`
- `ConsumeStock(conn,tx,ligne,aConommer,idProd,niveau) -> decimal`
- `GetByNiveau(idNiveau)`
- `GetDuJourByActivite(idActivite)`
- `GetRecentByActivite(idActivite,limit)`
- `GetIdNiveauDeFiche(idFiche) -> int`

## Regles (JOURNAL.md)
- #15 quantiteCible = nombre de batches, qte reelle = batches * fiche.QuantiteOutput
- #16 UnitConvertisseur.Convertir() OBLIGATOIRE avant toute comparaison/decrementation de stock (g vs kg = penurie fantome)

## Relations
Appelle: BomStockDAL.GetLotsDispoFIFO/GetBomStocksFIFO/GetDisponible, BomNiveauDAL.GetById, BomFicheDAL.GetById, UnitConvertisseur.Convertir. Appele par: FrmPrincipal.Production, FrmBomProductionSimulation, SimulationService.

### Connexions sortantes
- --method--> .GetByNiveau()
- --method--> .GetRecentByActivite()
- --method--> .GetDuJourByActivite()
- --method--> .VerifierDisponibilite()
- --method--> .Simuler()
- --method--> .Executer()
- --method--> .ConsumeStock()
- --method--> .InsertLigne()
- --method--> .GetIdNiveauDeFiche()
- --method--> .GetIdNiveauPrecedent()
- --method--> .MapHeader()

### Connexions entrantes
- BomProductionDAL.cs --contains-->
- [[MODULE_Production_BOM|MODULE: Production BOM]] --documents-->
