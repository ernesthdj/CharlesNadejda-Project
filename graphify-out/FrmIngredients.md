# FrmIngredients
> Source: `CharlesNadejda/Forms/FrmIngredients.cs`
> Type: code

## Description
Liste ingredients (herite FrmListeBase<Ingredient>). Filtrage par chips (stock lie) via FlowLayoutPanel en haut. Filtre alertes seulement (US-08). Styles lignes : rouge si stock < seuil alerte. Colonnes avec FormatQte/FormatPrix via CellFormatting.

## Methodes
- `ConfigurerColonnes() Supprimer(Ingredient) AppliquerStylesLignes() BuildChipPanel() CreerChip(texte,stock,actif) AppliquerFiltre(stock,chipClique)`

## Regles (JOURNAL.md)
- #24 CellFormatting pour unites fusionnees
- #6 DGV sizing
- #28 StockPieces = conditionnements entiers

## Relations
Appelle: IngredientDAL.GetAll, FrmIngredientEdit (OuvrirFormulaire). Herite: FrmListeBase<Ingredient>.

### Connexions sortantes
- --references--> bool
- --inherits--> FrmListeBase
- --references--> Activite
- --references--> Stock
- --references--> FlowLayoutPanel
- --method--> .ChargerDonnees()
- --method--> .ConfigurerColonnes()
- --method--> .OuvrirFormulaire()
- --method--> .Supprimer()
- --method--> .NomElement()
- --method--> .AppliquerStylesLignes()
- --method--> .OnLoad()
- --method--> .BuildChipPanel()
- --method--> .CreerChip()
- --method--> .AppliquerFiltre()

### Connexions entrantes
- FrmIngredients.cs --contains-->
