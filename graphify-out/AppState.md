# AppState
> Source: `CharlesNadejda/Navigation/AppState.cs`
> Type: code

## Description
Singleton observable centralisant l etat global : activite courante, contexte selectionne, niveau actif, utilisateur connecte. Publie des evenements Changed pour que le shell reagisse.

## Methodes
- `SetActivite(Activite) SetContexte(BomContexte) SetNiveau(BomNiveau) RaiseChanged() [events: ActiviteChanged, ContexteChanged, NiveauChanged]`

## Relations
Consomme par: FrmPrincipal, SidebarPanel, StatusBar. Source de verite navigation.

### Connexions sortantes
- --method--> .SetActivite()
- --method--> .SetContexte()
- --method--> .SetNiveau()
- --method--> .SetRessource()
- --method--> .SetFiltreAlertes()
- --method--> .RaiseChanged()

### Connexions entrantes
- AppState.cs --contains-->
- [[MODULE_Shell_&_Navigation|MODULE: Shell & Navigation]] --documents-->
