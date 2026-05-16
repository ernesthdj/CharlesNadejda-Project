# BomCoutDAL
> Source: `CharlesNadejda/DAL/BomCoutDAL.cs`
> Type: code

## Description
Calcul RECURSIF du cout de revient multi-niveaux. CalculerCout(fiche) appelle CalculerLigneFiche qui rappelle CalculerCout (descend les niveaux). Detection de cycle (TICKET-09). GetPrixMoyenIngredient = moyenne ponderee FIFO des lots.

## Methodes
- `CalculerCout(idFiche) -> RapportCout`
- `CalculerLigneIngredient(ligne,multiplicateur) -> LigneCout`
- `CalculerLigneFiche(ligne,multiplicateur) -> LigneCout`
- `GetPrixMoyenIngredient(idIngredient) -> decimal`

## Relations
Appele par: FrmPrincipal.Production (estimation cout). Recursion: CalculerCout -> CalculerLigneFiche -> CalculerCout.

### Connexions sortantes
- --method--> .CalculerCout()
- --method--> .CalculerLigneIngredient()
- --method--> .CalculerLigneFiche()
- --method--> .GetPrixMoyenIngredient()

### Connexions entrantes
- BomCoutDAL.cs --contains-->
- [[MODULE_Production_BOM|MODULE: Production BOM]] --documents-->
