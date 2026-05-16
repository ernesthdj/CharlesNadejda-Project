# BomFicheLigneDAL
> Source: `CharlesNadejda/DAL/BomFicheLigneDAL.cs`
> Type: code

## Description
Gestion des lignes de composition d une fiche. Chaque ligne = 1 input (ingredient OU sous-fiche). GetByFiche avec COALESCE pour NomInput. GetFichesUtilisant/Consommant pour la tracabilite inverse.

## Methodes
- `GetByFiche(idFiche) -> List<BomFicheLigne>`
- `GetFichesUtilisant(idIngredient) -> List<string>`
- `GetFichesConsommant(idFiche) -> List<string>`

## Regles (JOURNAL.md)
- #7 unite de la ligne = unite de l ingredient/fiche source, ComboBox verrouille apres selection

## Relations
Appele par: BomFicheDAL.GetById (charge les lignes), FrmBomFicheEdit, FrmVueStock (detail)

### Connexions sortantes
- --method--> .GetByFiche()
- --method--> .GetFichesUtilisant()
- --method--> .GetFichesConsommant()
- --method--> .Map()

### Connexions entrantes
- BomFicheLigneDAL.cs --contains-->
- [[MODULE_Fiches_&_Niveaux_BOM|MODULE: Fiches & Niveaux BOM]] --documents-->
