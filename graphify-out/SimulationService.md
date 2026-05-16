# SimulationService
> Source: `CharlesNadejda/Services/SimulationService.cs`
> Type: code

## Description
Couche service async entre UI et DAL Production. Dry-run : appelle Simuler() sans effet de bord. Projette le resultat en SimulationResultat pour affichage DGV.

## Methodes
- `SimulerAsync(idNiveau,idFiche,quantiteCible) -> Task<SimulationResultat>`
- `Project(List<BomManque>) -> SimulationResultat`

## Relations
Appelle: BomProductionDAL.Simuler. Appele par: FrmPrincipal.Production.ProdBtnSimuler_Click.

### Connexions sortantes
- --method--> .SimulerAsync()
- --method--> .Project()

### Connexions entrantes
- SimulationService.cs --contains-->
- [[MODULE_Production_BOM|MODULE: Production BOM]] --documents-->
