# Architect Plan Technique — Refactoring
> Source: `app-csharp/CharlesNadejda/docs/ARCHITECT_PLAN_TECHNIQUE.md`
> Type: document

## Description
Document d architecture technique du refactoring. Definit : SFA (Single-Form Architecture), pattern FrmListeBase/FrmEditBase, ScreenRouter, AppState, structure DAL sans ORM, transactions MySQL, palette chocolat, regles de docking WinForms, audit P1/P2. Reference centrale pour toutes les decisions architecturales.

## Relations
Reference par: tous les modules. Definit les patterns implementes dans FrmPrincipal, FrmListeBase, FrmEditBase, AppState, ScreenRouter.

### Connexions sortantes
- --references--> FrmPrincipal — Main Shell Form
- --references--> FrmLogin — Authentication Form
- --references--> FrmIngredients — Ingredients Screen
- --references--> FrmStocks — Stocks Screen
- --references--> FrmBomProductionSimulation — Production Simulation
- --references--> StockDAL — Stock Data Access Layer
- --references--> BomFicheDAL — BOM Recipe Data Access Layer
- --references--> BomProductionDAL — Production Execution DAL
- --references--> PO User Stories Refactoring
- --references--> PO Audit Fonctionnalites Design

### Connexions entrantes
- Development Journal — ArtisaStock --references-->
