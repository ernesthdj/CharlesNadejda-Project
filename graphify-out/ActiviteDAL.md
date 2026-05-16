# ActiviteDAL
> Source: `CharlesNadejda/DAL/ActiviteDAL.cs`
> Type: code

## Description
CRUD des activites (domaines metier dynamiques). Desactiver verifie les cascades (stocks lies, productions). Jamais coder en dur un nom d activite dans le C#.

## Methodes
- `GetAll() -> List<Activite>`
- `GetById(id) -> Activite`
- `Insert(act) -> int`
- `Update(act)`
- `Delete(id)`
- `Desactiver(id)`
- `Bind(act,cmd)`

## Regles (JOURNAL.md)
- #9 jamais hardcoder chocolaterie — toujours passer par objet Activite
- #10 grepper tous call sites apres refactor signature

## Relations
Appele par: FrmActivites, FrmActiviteEdit, SidebarPanel.ActivitySwitcher, AppState

### Connexions sortantes
- --method--> .GetAll()
- --method--> .GetById()
- --method--> .NomExiste()
- --method--> .Insert()
- --method--> .Update()
- --method--> .Delete()
- --method--> .Desactiver()
- --method--> .Bind()
- --method--> .Map()

### Connexions entrantes
- ActiviteDAL.cs --contains-->
- [[MODULE_Activites|MODULE: Activites]] --documents-->
