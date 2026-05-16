# BomContexteDAL
> Source: `CharlesNadejda/DAL/BomContexteDAL.cs`
> Type: code

## Description
CRUD des contextes BOM. Un contexte = une ligne de production (ex: Chocolaterie Noel 2026). InsertAvecNiveaux cree le contexte + ses niveaux dans une seule TX. Lie a une activite.

## Methodes
- `GetAll()`
- `GetById(id) -> BomContexte`
- `Insert(ctx) -> int`
- `InsertAvecNiveaux(ctx,niveaux) -> int`
- `Update(ctx)`
- `Delete(id)`

## Regles (JOURNAL.md)
- #8 migration ENUM vers FK = supprimer ancienne colonne ET ajouter nouvelle dans meme ALTER TABLE

## Relations
Appele par: FrmBomContextes, FrmBomContexteEdit, FrmPrincipal, BomProductionDAL

### Connexions sortantes
- --references--> string
- --method--> .GetAll()
- --method--> .GetById()
- --method--> .NomExiste()
- --method--> .InsertAvecNiveaux()
- --method--> .Insert()
- --method--> .Update()
- --method--> .Delete()
- --method--> .Bind()
- --method--> .Map()

### Connexions entrantes
- BomContexteDAL.cs --contains-->
