# MODULE: Production BOM
> Source: `app-csharp/CharlesNadejda/CharlesNadejda/DAL/BomProductionDAL.cs`
> Type: rationale

## Description
Flux: UI selectionne Contexte>Niveau>Fiche > Simuler (dry-run) > Lancer (TX atomique). Executer: verif TOCTOU > INSERT production > ConsumeStock FIFO par ligne > couts > INSERT bom_stocks. Multi-niveaux SEQUENTIEL (pas recursif). Couts RECURSIFS via BomCoutDAL. FIFO strict (date ASC).

## Relations
### Connexions sortantes
- --documents--> [[BomProductionDAL|BomProductionDAL]]
- --documents--> [[BomStockDAL|BomStockDAL]]
- --documents--> [[BomCoutDAL|BomCoutDAL]]
- --documents--> [[SimulationService|SimulationService]]
