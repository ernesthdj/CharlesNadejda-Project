# Graph Report - app-csharp/CharlesNadejda/CharlesNadejda  (2026-05-21)

## Corpus Check
- 945 files · ~50,000 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 945 nodes · 1468 edges · 79 communities (37 shown, 42 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 54 edges (avg confidence: 0.82)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Contexte Screen & Kanban|Contexte Screen & Kanban]]
- [[_COMMUNITY_Forms Base & UI Config|Forms Base & UI Config]]
- [[_COMMUNITY_BOM Contextes & Utils|BOM Contextes & Utils]]
- [[_COMMUNITY_Production & Costing Engine|Production & Costing Engine]]
- [[_COMMUNITY_Shell & Navigation|Shell & Navigation]]
- [[_COMMUNITY_Stock & Reservations|Stock & Reservations]]
- [[_COMMUNITY_BOM Fiches & Niveaux|BOM Fiches & Niveaux]]
- [[_COMMUNITY_Production Screen|Production Screen]]
- [[_COMMUNITY_Activities & Designer|Activities & Designer]]
- [[_COMMUNITY_BOM Fiches UI & Simulation|BOM Fiches UI & Simulation]]
- [[_COMMUNITY_Edit Forms CRUD|Edit Forms CRUD]]
- [[_COMMUNITY_Vue Stock Global|Vue Stock Global]]
- [[_COMMUNITY_Catalogue & Fournisseurs DAL|Catalogue & Fournisseurs DAL]]
- [[_COMMUNITY_Architecture BOM Plan|Architecture BOM Plan]]
- [[_COMMUNITY_Boutique Web|Boutique Web]]
- [[_COMMUNITY_FrmListeBase Generic|FrmListeBase Generic]]
- [[_COMMUNITY_Fiche Edit & Lignes|Fiche Edit & Lignes]]
- [[_COMMUNITY_Achat Edit Form|Achat Edit Form]]
- [[_COMMUNITY_Activites DAL|Activites DAL]]
- [[_COMMUNITY_Ingredients List & Filters|Ingredients List & Filters]]
- [[_COMMUNITY_Community 20|Community 20]]
- [[_COMMUNITY_Community 21|Community 21]]
- [[_COMMUNITY_Community 22|Community 22]]
- [[_COMMUNITY_Community 23|Community 23]]
- [[_COMMUNITY_Community 24|Community 24]]
- [[_COMMUNITY_Community 25|Community 25]]
- [[_COMMUNITY_Community 26|Community 26]]
- [[_COMMUNITY_Community 27|Community 27]]
- [[_COMMUNITY_Community 28|Community 28]]
- [[_COMMUNITY_Community 29|Community 29]]
- [[_COMMUNITY_Community 30|Community 30]]
- [[_COMMUNITY_Community 31|Community 31]]
- [[_COMMUNITY_Community 32|Community 32]]
- [[_COMMUNITY_Community 33|Community 33]]
- [[_COMMUNITY_Community 34|Community 34]]
- [[_COMMUNITY_Community 35|Community 35]]
- [[_COMMUNITY_Community 36|Community 36]]
- [[_COMMUNITY_Community 37|Community 37]]
- [[_COMMUNITY_Community 38|Community 38]]
- [[_COMMUNITY_Community 39|Community 39]]
- [[_COMMUNITY_Community 41|Community 41]]
- [[_COMMUNITY_Community 42|Community 42]]
- [[_COMMUNITY_Community 43|Community 43]]
- [[_COMMUNITY_Community 44|Community 44]]
- [[_COMMUNITY_Community 45|Community 45]]
- [[_COMMUNITY_Community 46|Community 46]]
- [[_COMMUNITY_Community 47|Community 47]]
- [[_COMMUNITY_Community 48|Community 48]]
- [[_COMMUNITY_Community 49|Community 49]]
- [[_COMMUNITY_Community 50|Community 50]]
- [[_COMMUNITY_Community 51|Community 51]]
- [[_COMMUNITY_Community 52|Community 52]]
- [[_COMMUNITY_Community 53|Community 53]]
- [[_COMMUNITY_Community 54|Community 54]]
- [[_COMMUNITY_Community 55|Community 55]]
- [[_COMMUNITY_Community 56|Community 56]]
- [[_COMMUNITY_Community 57|Community 57]]
- [[_COMMUNITY_Community 58|Community 58]]
- [[_COMMUNITY_Community 59|Community 59]]
- [[_COMMUNITY_Community 60|Community 60]]
- [[_COMMUNITY_Community 61|Community 61]]
- [[_COMMUNITY_Community 62|Community 62]]
- [[_COMMUNITY_Community 63|Community 63]]
- [[_COMMUNITY_Community 64|Community 64]]
- [[_COMMUNITY_Community 65|Community 65]]
- [[_COMMUNITY_Community 66|Community 66]]
- [[_COMMUNITY_Community 67|Community 67]]
- [[_COMMUNITY_Community 68|Community 68]]
- [[_COMMUNITY_Community 69|Community 69]]
- [[_COMMUNITY_Community 70|Community 70]]
- [[_COMMUNITY_Community 71|Community 71]]
- [[_COMMUNITY_Community 72|Community 72]]
- [[_COMMUNITY_Community 73|Community 73]]
- [[_COMMUNITY_Community 74|Community 74]]
- [[_COMMUNITY_Community 77|Community 77]]
- [[_COMMUNITY_Community 78|Community 78]]

## God Nodes (most connected - your core abstractions)
1. `FrmPrincipal` - 78 edges
2. `FrmPrincipal` - 36 edges
3. `FrmVueStock` - 31 edges
4. `FrmPrincipal` - 24 edges
5. `FrmAchatEdit` - 22 edges
6. `FrmBomFicheEdit` - 22 edges
7. `FrmStocks` - 22 edges
8. `FrmListeBase` - 21 edges
9. `FrmBomProductionSimulation` - 20 edges
10. `FrmBomContexteEdit` - 18 edges

## Surprising Connections (you probably didn't know these)
- `FrmPrincipal — Main Shell Form` --implements--> `ArtisaStock Design System — Chocolate Palette`  [INFERRED]
  CharlesNadejda/Forms/FrmPrincipal.cs → app-csharp/CharlesNadejda/docs/PO_AUDIT_FONCTIONNALITES_DESIGN.md
- `Single-Form Application Pattern (SFA)` --rationale_for--> `FrmPrincipal — Main Shell Form`  [EXTRACTED]
  app-csharp/CharlesNadejda/docs/ARCHITECT_PLAN_TECHNIQUE.md → CharlesNadejda/Forms/FrmPrincipal.cs
- `FrmBomProductionSimulation — Production Simulation` --references--> `Architect Plan Technique — Refactoring`  [EXTRACTED]
  CharlesNadejda/Forms/FrmBomProductionSimulation.cs → app-csharp/CharlesNadejda/docs/ARCHITECT_PLAN_TECHNIQUE.md
- `IngredientDAL — Ingredient Data Access Layer` --rationale_for--> `OWASP Security Constraints`  [INFERRED]
  CharlesNadejda/DAL/IngredientDAL.cs → app-csharp/CharlesNadejda/docs/PO_USER_STORIES_REFACTORING.md
- `StockDAL — Stock Data Access Layer` --implements--> `M:N Relationship Stock-Activity Liaison`  [EXTRACTED]
  CharlesNadejda/DAL/StockDAL.cs → app-csharp/CharlesNadejda/docs/ARCHITECT_PLAN_TECHNIQUE.md

## Hyperedges (group relationships)
- **SFA Navigation Architecture (ScreenRouter + AppState + FrmPrincipal)** — sfa_pattern, screenrouter, appstate, frmprincipal, navigationparams [EXTRACTED 1.00]
- **Multi-Agent Workflow Pipeline (PO Audit -> User Stories -> Architect Plan -> Implementation)** — po_audit_design, po_user_stories, architect_plan, journal [EXTRACTED 1.00]
- **BOM Production Stack (FIFO Engine + Multi-Level + DALs)** — fifo_transaction, bom_multilevel, bomproductiondal, bomfichedal, bomniveaudal, bomstockdal [INFERRED 0.85]

## Communities (79 total, 42 thin omitted)

### Community 0 - "Contexte Screen & Kanban"
Cohesion: 0.05
Nodes (9): AppStatusBar, CharlesNadejda.Forms, FrmPrincipal, FrmPrincipal, CharlesNadejda.Forms, FrmPrincipal, SidebarPanel, TitleBarPanel (+1 more)

### Community 1 - "Forms Base & UI Config"
Cohesion: 0.06
Nodes (17): CheckedListBox, Color, ErrorProvider, Form, AppColors, CharlesNadejda.Forms, CharlesNadejda.Forms, FrmActiviteStocks (+9 more)

### Community 2 - "BOM Contextes & Utils"
Cohesion: 0.07
Nodes (11): MODULE: Forms Base & Utilitaires, BomContexteDAL, CharlesNadejda.DAL, CharlesNadejda.DAL, CommandeWebDAL, Dictionary, CharlesNadejda, UnitConvertisseur (+3 more)

### Community 3 - "Production & Costing Engine"
Cohesion: 0.07
Nodes (10): MODULE: Production BOM, BomCoutDAL, CharlesNadejda.DAL, BomProductionDAL, CharlesNadejda.DAL, BomStockDAL, CharlesNadejda.DAL, decimal (+2 more)

### Community 4 - "Shell & Navigation"
Cohesion: 0.06
Nodes (15): AppState, int, CharlesNadejda.Navigation, ScreenRouter, NavigationParams — Screen Navigation Parameters, NavItemId, Panel, RessourceType (+7 more)

### Community 5 - "Stock & Reservations"
Cohesion: 0.07
Nodes (7): MODULE: Stock & Achats, BomReservationDAL, CharlesNadejda.DAL, CharlesNadejda.DAL, StockDAL, CharlesNadejda.DAL, VueStockGlobalDAL

### Community 6 - "BOM Fiches & Niveaux"
Cohesion: 0.08
Nodes (7): MODULE: Fiches & Niveaux BOM, BomFicheDAL, CharlesNadejda.DAL, BomFicheLigneDAL, CharlesNadejda.DAL, BomNiveauDAL, CharlesNadejda.DAL

### Community 8 - "Activities & Designer"
Cohesion: 0.11
Nodes (10): DataGridView, CharlesNadejda.Forms, FrmActivites, CharlesNadejda.Forms, FrmFournisseurs, CharlesNadejda.Forms, FrmPrincipal, IContainer (+2 more)

### Community 9 - "BOM Fiches UI & Simulation"
Cohesion: 0.11
Nodes (5): BomNiveau, CharlesNadejda.Forms, FrmBomFiches, CharlesNadejda.Forms, FrmBomProductionSimulation

### Community 10 - "Edit Forms CRUD"
Cohesion: 0.1
Nodes (14): Activite, bool, CharlesNadejda.Forms, FrmActiviteEdit, CharlesNadejda.Forms, FrmBomNiveauEdit, CharlesNadejda.Forms, FrmFournisseurEdit (+6 more)

### Community 12 - "Catalogue & Fournisseurs DAL"
Cohesion: 0.12
Nodes (5): MODULE: Catalogue (Ingredients & Fournisseurs), CharlesNadejda.DAL, FournisseurDAL, CharlesNadejda.DAL, IngredientDAL

### Community 13 - "Architecture BOM Plan"
Cohesion: 0.13
Nodes (24): M:N Relationship Stock-Activity Liaison, Architect Plan Technique — Refactoring, BOM Multi-Level Production System, BomFicheDAL — BOM Recipe Data Access Layer, BomNiveauDAL — BOM Level Data Access Layer, BomProductionDAL — Production Execution DAL, BomStockDAL — BOM Stock Data Access Layer, ArtisaStock Design System — Chocolate Palette (+16 more)

### Community 14 - "Boutique Web"
Cohesion: 0.2
Nodes (3): CharlesNadejda.Forms, FrmPrincipal, TabControl

### Community 16 - "Fiche Edit & Lignes"
Cohesion: 0.21
Nodes (4): BomFiche, CharlesNadejda.Forms, FrmBomFicheEdit, InputItem

### Community 17 - "Achat Edit Form"
Cohesion: 0.21
Nodes (5): DateTimePicker, CharlesNadejda.Forms, FrmAchatEdit, Lot, RadioButton

### Community 18 - "Activites DAL"
Cohesion: 0.2
Nodes (3): MODULE: Activites, ActiviteDAL, CharlesNadejda.DAL

### Community 19 - "Ingredients List & Filters"
Cohesion: 0.19
Nodes (3): FlowLayoutPanel, CharlesNadejda.Forms, FrmIngredients

### Community 21 - "Community 21"
Cohesion: 0.3
Nodes (12): BCrypt.Net-Next 4.1.0, BouncyCastle.Cryptography 2.6.2, CharlesNadejda C# Application, MySQL Database Connectivity via Connector/NET, MySql.Data 9.6.0 (MySQL Connector/NET), Newtonsoft.Json 13.0.4 (Json.NET), BCrypt Password Hashing Strategy, System.Buffers 4.6.1 (+4 more)

### Community 23 - "Community 23"
Cohesion: 0.18
Nodes (4): CharlesNadejda.Forms, FrmBomContexteEdit, List, ListBox

### Community 26 - "Community 26"
Cohesion: 0.2
Nodes (3): Button, CharlesNadejda.Forms, FrmBomContextes

### Community 28 - "Community 28"
Cohesion: 0.29
Nodes (3): MODULE: Shell & Navigation, AppState, CharlesNadejda.Navigation

### Community 29 - "Community 29"
Cohesion: 0.2
Nodes (9): Analyzers, Configuration, CopyToOutputEntries, FrameworkPath, Outputs, ProjectFileName, References, RootPath (+1 more)

### Community 30 - "Community 30"
Cohesion: 0.24
Nodes (4): CharlesNadejda.Forms, FrmIngredientEdit, Ingredient, NumericUpDown

### Community 31 - "Community 31"
Cohesion: 0.22
Nodes (3): BomContexte, CharlesNadejda.Forms, FrmBomNiveaux

### Community 32 - "Community 32"
Cohesion: 0.22
Nodes (4): CharlesNadejda.Forms, FrmProduitWebEdit, PictureBox, ProduitWeb

### Community 33 - "Community 33"
Cohesion: 0.28
Nodes (5): CharlesNadejda.Forms, DarkColorTable, DarkMenuRenderer, ProfessionalColorTable, ToolStripProfessionalRenderer

### Community 34 - "Community 34"
Cohesion: 0.25
Nodes (3): CharlesNadejda.Forms, FrmAchats, FrmListeBase

### Community 35 - "Community 35"
Cohesion: 0.29
Nodes (4): ComboBox, CharlesNadejda.Forms, FrmBomProductionSimulation, GroupBox

### Community 36 - "Community 36"
Cohesion: 0.25
Nodes (4): CategorieWeb, CheckBox, CharlesNadejda.Forms, FrmCategorieWebEdit

### Community 37 - "Community 37"
Cohesion: 0.33
Nodes (3): CharlesNadejda.Forms, FrmLogin, Label

### Community 38 - "Community 38"
Cohesion: 0.4
Nodes (4): CultureInfo, CharlesNadejda.Properties, Resources, ResourceManager

### Community 43 - "Community 43"
Cohesion: 0.4
Nodes (4): DocumentGroupContainers, Documents, Version, WorkspaceRootPath

### Community 44 - "Community 44"
Cohesion: 0.4
Nodes (4): DocumentGroupContainers, Documents, Version, WorkspaceRootPath

### Community 63 - "Community 63"
Cohesion: 0.5
Nodes (3): CharlesNadejda.Models, LigneCout, RapportCout

## Knowledge Gaps
- **129 isolated node(s):** `Version`, `WorkspaceRootPath`, `Documents`, `DocumentGroupContainers`, `Version` (+124 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **42 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `FrmPrincipal` connect `Contexte Screen & Kanban` to `Forms Base & UI Config`, `Community 33`, `BOM Contextes & Utils`, `Shell & Navigation`, `Community 37`, `Production Screen`, `Activities & Designer`, `Architecture BOM Plan`, `Community 23`, `Community 26`, `Community 28`?**
  _High betweenness centrality (0.199) - this node is a cross-community bridge._
- **Why does `string` connect `BOM Contextes & Utils` to `Community 32`, `Shell & Navigation`, `Stock & Reservations`, `BOM Fiches & Niveaux`, `Community 20`, `Community 22`?**
  _High betweenness centrality (0.174) - this node is a cross-community bridge._
- **Why does `FrmProduitWebEdit` connect `Community 32` to `BOM Contextes & Utils`, `Community 35`, `Community 36`, `Community 37`, `Edit Forms CRUD`, `Community 26`, `Community 30`?**
  _High betweenness centrality (0.116) - this node is a cross-community bridge._
- **What connects `Version`, `WorkspaceRootPath`, `Documents` to the rest of the system?**
  _129 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Contexte Screen & Kanban` be split into smaller, more focused modules?**
  _Cohesion score 0.05 - nodes in this community are weakly interconnected._
- **Should `Forms Base & UI Config` be split into smaller, more focused modules?**
  _Cohesion score 0.06 - nodes in this community are weakly interconnected._
- **Should `BOM Contextes & Utils` be split into smaller, more focused modules?**
  _Cohesion score 0.07 - nodes in this community are weakly interconnected._