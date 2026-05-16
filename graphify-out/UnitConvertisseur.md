# UnitConvertisseur
> Source: `CharlesNadejda/Forms/UnitConvertisseur.cs`
> Type: code

## Description
Convertisseur d unites intelligent. 3 groupes : masse (mg/g/kg), volume (ml/cl/l), piece. FormatQte auto-convertit (652cl->6.52l). FormatPrix toujours F2+euro. Convertir() pour calculs inter-unites.

## Methodes
- `Convertir(qte,uniteSource,uniteCible) -> decimal`
- `FormatQte(valeur,unite) -> string`
- `FormatPrix(valeur) -> string`
- `GetGroupeUnite(unite) -> string`
- `EstConvertible(u1,u2) -> bool`

## Regles (JOURNAL.md)
- #16 Convertir OBLIGATOIRE avant comparaison stock
- #24 FormatQte/FormatPrix dans CellFormatting DGV

## Relations
Appele par: TOUS les DAL de production + TOUS les formulaires avec DGV. Utilitaire transversal.

### Connexions sortantes
- --references--> string
- --references--> Dictionary
- --method--> .UnitesCompatibles()
- --method--> .SontCompatibles()
- --method--> .UniteBase()
- --method--> .VersUniteBase()
- --method--> .FormatQte()
- --method--> .FormatPrix()
- --method--> .Convertir()

### Connexions entrantes
- UnitConvertisseur.cs --contains-->
- [[MODULE_Forms_Base_&_Utilitaires|MODULE: Forms Base & Utilitaires]] --documents-->
