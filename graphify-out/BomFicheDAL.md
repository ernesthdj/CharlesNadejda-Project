# BomFicheDAL
> Source: `CharlesNadejda/DAL/BomFicheDAL.cs`
> Type: code

## Description
CRUD des fiches BOM (recettes). Insert/Update transactionnels (header + lignes dans la meme TX). Duplicate pour copier une fiche. GetByNiveau filtre par niveau. GetCountsByContexte pour les badges.

## Methodes
- `GetAll()`
- `GetById(id) -> BomFiche (avec Lignes chargees)`
- `GetByNiveau(idNiveau)`
- `Insert(fiche) -> int`
- `Update(fiche)`
- `Delete(id)`
- `Duplicate(id) -> int`
- `GetCountsByContexte(idContexte) -> Dict`
- `NomExiste(nom,excludeId) -> bool`

## Regles (JOURNAL.md)
- #4 fiche liee a un niveau specifique (id_niveau)
- #5 ajout champ = verifier SELECT+INSERT+UPDATE+Map()

## Relations
Appele par: BomProductionDAL, FrmBomFiches, FrmBomFicheEdit, FrmPrincipal.ShowContexteScreen

### Connexions sortantes
- --references--> string
- --method--> .GetByNiveau()
- --method--> .GetAll()
- --method--> .GetById()
- --method--> .NomExiste()
- --method--> .Insert()
- --method--> .Update()
- --method--> .GetCountsByContexte()
- --method--> .Delete()
- --method--> .Duplicate()
- --method--> .InsertLignes()
- --method--> .BindHeader()
- --method--> .MapHeader()

### Connexions entrantes
- BomFicheDAL.cs --contains-->
- [[MODULE_Fiches_&_Niveaux_BOM|MODULE: Fiches & Niveaux BOM]] --documents-->
