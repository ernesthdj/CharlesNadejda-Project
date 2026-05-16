# BomNiveauDAL
> Source: `CharlesNadejda/DAL/BomNiveauDAL.cs`
> Type: code

## Description
CRUD des niveaux BOM dans un contexte. Ordre sequentiel (1,2,3...). Delete conditionnel (dernier niveau seulement). Insert avec MAX(ordre)+1 automatique.

## Methodes
- `GetByContexte(idContexte) -> List<BomNiveau>`
- `GetById(id) -> BomNiveau`
- `GetByContexteEtOrdre(idCtx,ordre) -> BomNiveau`
- `GetOrdreMax(idContexte) -> int`
- `Insert(niveau) -> int`
- `Update(niveau)`
- `Delete(id)`

## Relations
Appele par: BomProductionDAL, BomFicheDAL, FrmPrincipal, FrmBomNiveaux, FrmBomContexteEdit

### Connexions sortantes
- --references--> string
- --method--> .GetByContexte()
- --method--> .GetById()
- --method--> .GetOrdreMax()
- --method--> .Insert()
- --method--> .Update()
- --method--> .Delete()
- --method--> .GetByContexteEtOrdre()
- --method--> .Bind()
- --method--> .Map()

### Connexions entrantes
- BomNiveauDAL.cs --contains-->
- [[MODULE_Fiches_&_Niveaux_BOM|MODULE: Fiches & Niveaux BOM]] --documents-->
