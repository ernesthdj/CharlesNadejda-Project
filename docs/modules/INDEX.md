# Index — Documentation modulaire ArtisaStock
> Generee depuis le graphe graphify (771 nodes, 1143 edges, 78 communautes)
> Derniere mise a jour : 2026-05-16

## Carte des modules

| # | Module | Fichiers cles | Communautes graphify |
|---|--------|---------------|---------------------|
| 01 | [Shell & Navigation](01_SHELL_NAVIGATION.md) | FrmPrincipal, Shell/*, Navigation/* | C0, C2, C32, C35 |
| 02 | [Production BOM](02_BOM_PRODUCTION.md) | BomProductionDAL, BomStockDAL, BomCoutDAL, SimulationService | C4, C13, C29, C36, C43 |
| 03 | [Fiches & Niveaux](03_BOM_FICHES_NIVEAUX.md) | BomFicheDAL, BomFicheLigneDAL, BomNiveauDAL | C5, C9, C12, C18, C31, C37 |
| 04 | [Contextes BOM](04_BOM_CONTEXTES.md) | BomContexteDAL, FrmBomContextes | C3, C7, C17, C27 |
| 05 | [Stock & Achats](05_STOCK_ACHATS.md) | StockDAL, VueStockGlobalDAL, LotDAL, BomReservationDAL | C6, C10, C11, C21, C26, C30, C33 |
| 06 | [Catalogue](06_CATALOGUE.md) | IngredientDAL, FournisseurDAL | C14, C22, C24, C25, C28 |
| 07 | [Activites](07_ACTIVITES.md) | ActiviteDAL, FrmActivites | C16, C19, C8 |
| 08 | [Forms Base & Utils](08_FORMS_BASE.md) | FrmEditBase, AppColors, UnitConvertisseur | C1, C15, C41, C54 |
| 09 | [Dependances](09_DEPENDENCIES.md) | packages/ | C20 |

## God Nodes (noeuds les plus connectes)

| Noeud | Aretes | Module |
|-------|--------|--------|
| FrmPrincipal | 76 | 01 Shell |
| FrmPrincipal.Production | 36 | 02 Production |
| FrmVueStock | 30 | 05 Stock |
| FrmAchatEdit | 21 | 05 Stock |
| FrmListeBase | 21 | 08 Forms Base |

## Maintenance

Quand du code est modifie :
1. Identifier le module concerne via la table ci-dessus
2. Mettre a jour UNIQUEMENT le fichier doc du module
3. Mettre a jour la date dans l'en-tete du fichier
4. Si un nouveau fichier .cs est ajoute -> l'ajouter dans le tableau "Fichiers source" du module approprie

Pour reconstruire le graphe apres modifications majeures :
```
/graphify app-csharp/CharlesNadejda --update
```
