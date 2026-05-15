# Graph Report - app-csharp/CharlesNadejda  (2026-05-15)

## Corpus Check
- 127 files · ~99,758 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 771 nodes · 1143 edges · 78 communities (28 shown, 50 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 28 edges (avg confidence: 0.83)
- Token cost: 180,589 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Shell ERP Principal|Shell ERP Principal]]
- [[_COMMUNITY_Forms Base & Login|Forms Base & Login]]
- [[_COMMUNITY_Shell Navigation & Utils|Shell Navigation & Utils]]
- [[_COMMUNITY_BOM Contextes & Liste Base|BOM Contextes & Liste Base]]
- [[_COMMUNITY_Production Workflow|Production Workflow]]
- [[_COMMUNITY_BOM Fiches & Simulation|BOM Fiches & Simulation]]
- [[_COMMUNITY_Vue Stock Global|Vue Stock Global]]
- [[_COMMUNITY_Architecture & DAL BOM|Architecture & DAL BOM]]
- [[_COMMUNITY_Forms Edit CRUD|Forms Edit CRUD]]
- [[_COMMUNITY_BOM Fiche Editor|BOM Fiche Editor]]
- [[_COMMUNITY_Stock DAL|Stock DAL]]
- [[_COMMUNITY_Achat Editor|Achat Editor]]
- [[_COMMUNITY_BOM Fiche DAL|BOM Fiche DAL]]
- [[_COMMUNITY_Production DAL|Production DAL]]
- [[_COMMUNITY_Ingrédients Liste|Ingrédients Liste]]
- [[_COMMUNITY_Login Designer & UI|Login Designer & UI]]
- [[_COMMUNITY_Activité DAL|Activité DAL]]
- [[_COMMUNITY_BOM Contexte DAL|BOM Contexte DAL]]
- [[_COMMUNITY_BOM Niveau DAL|BOM Niveau DAL]]
- [[_COMMUNITY_Activités Forms|Activités Forms]]
- [[_COMMUNITY_NuGet Dependencies|NuGet Dependencies]]
- [[_COMMUNITY_Réservation DAL|Réservation DAL]]
- [[_COMMUNITY_Fournisseurs Liste|Fournisseurs Liste]]
- [[_COMMUNITY_VS Project Config|VS Project Config]]
- [[_COMMUNITY_Fournisseur DAL|Fournisseur DAL]]
- [[_COMMUNITY_Ingrédient DAL|Ingrédient DAL]]
- [[_COMMUNITY_Lot DAL|Lot DAL]]
- [[_COMMUNITY_BOM Contexte Editor|BOM Contexte Editor]]
- [[_COMMUNITY_Ingrédient Editor|Ingrédient Editor]]
- [[_COMMUNITY_BOM Stock DAL|BOM Stock DAL]]
- [[_COMMUNITY_Vue Stock Global DAL|Vue Stock Global DAL]]
- [[_COMMUNITY_BOM Niveaux Forms|BOM Niveaux Forms]]
- [[_COMMUNITY_AppState Navigation|AppState Navigation]]
- [[_COMMUNITY_Achats Liste|Achats Liste]]
- [[_COMMUNITY_FrmPrincipal Designer|FrmPrincipal Designer]]
- [[_COMMUNITY_Dark Menu Renderer|Dark Menu Renderer]]
- [[_COMMUNITY_BOM Coût DAL|BOM Coût DAL]]
- [[_COMMUNITY_BOM Fiche Ligne DAL|BOM Fiche Ligne DAL]]
- [[_COMMUNITY_Simulation Designer|Simulation Designer]]
- [[_COMMUNITY_Fournisseurs Designer|Fournisseurs Designer]]
- [[_COMMUNITY_Stock Editor|Stock Editor]]
- [[_COMMUNITY_Form Helper Utils|Form Helper Utils]]
- [[_COMMUNITY_Resources Designer|Resources Designer]]
- [[_COMMUNITY_Simulation Service|Simulation Service]]
- [[_COMMUNITY_VS Layout Backup|VS Layout Backup]]
- [[_COMMUNITY_VS Layout Active|VS Layout Active]]
- [[_COMMUNITY_Model BomFiche|Model BomFiche]]
- [[_COMMUNITY_Model BomManque|Model BomManque]]
- [[_COMMUNITY_Model BomReservation|Model BomReservation]]
- [[_COMMUNITY_Model Fournisseur|Model Fournisseur]]
- [[_COMMUNITY_Model RapportCout|Model RapportCout]]
- [[_COMMUNITY_Program Entry Point|Program Entry Point]]
- [[_COMMUNITY_DbHelper Connection|DbHelper Connection]]
- [[_COMMUNITY_Utilisateur Auth DAL|Utilisateur Auth DAL]]
- [[_COMMUNITY_String Extensions|String Extensions]]
- [[_COMMUNITY_Model Activite|Model Activite]]
- [[_COMMUNITY_Model BomContexte|Model BomContexte]]
- [[_COMMUNITY_Model BomFicheLigne|Model BomFicheLigne]]
- [[_COMMUNITY_Model BomNiveau|Model BomNiveau]]
- [[_COMMUNITY_Model BomProduction|Model BomProduction]]
- [[_COMMUNITY_Model BomProductionLigne|Model BomProductionLigne]]
- [[_COMMUNITY_Model BomStock|Model BomStock]]
- [[_COMMUNITY_Model Ingredient|Model Ingredient]]
- [[_COMMUNITY_Model Stock|Model Stock]]
- [[_COMMUNITY_Model Utilisateur|Model Utilisateur]]
- [[_COMMUNITY_Model Lot|Model Lot]]
- [[_COMMUNITY_Navigation Params|Navigation Params]]
- [[_COMMUNITY_Settings Designer|Settings Designer]]
- [[_COMMUNITY_Simulation Resultat|Simulation Resultat]]
- [[_COMMUNITY_Model VueStockGlobal|Model VueStockGlobal]]
- [[_COMMUNITY_RessourceType Enum|RessourceType Enum]]
- [[_COMMUNITY_NavItemId Enum|NavItemId Enum]]
- [[_COMMUNITY_ScreenId Enum|ScreenId Enum]]
- [[_COMMUNITY_Stock Vue Bridge|Stock Vue Bridge]]
- [[_COMMUNITY_ArtiStock Project|ArtiStock Project]]
- [[_COMMUNITY_Copilot Instructions|Copilot Instructions]]

## God Nodes (most connected - your core abstractions)
1. `FrmPrincipal` - 76 edges
2. `FrmPrincipal` - 36 edges
3. `FrmVueStock` - 30 edges
4. `FrmAchatEdit` - 21 edges
5. `FrmListeBase` - 21 edges
6. `FrmStocks` - 21 edges
7. `FrmBomFicheEdit` - 20 edges
8. `FrmBomProductionSimulation` - 20 edges
9. `FrmBomContexteEdit` - 17 edges
10. `SidebarPanel` - 17 edges

## Surprising Connections (you probably didn't know these)
- `FrmPrincipal — Main Shell Form` --implements--> `ArtisaStock Design System — Chocolate Palette`  [INFERRED]
  CharlesNadejda/Forms/FrmPrincipal.cs → app-csharp/CharlesNadejda/docs/PO_AUDIT_FONCTIONNALITES_DESIGN.md
- `Single-Form Application Pattern (SFA)` --rationale_for--> `FrmPrincipal — Main Shell Form`  [EXTRACTED]
  app-csharp/CharlesNadejda/docs/ARCHITECT_PLAN_TECHNIQUE.md → CharlesNadejda/Forms/FrmPrincipal.cs
- `Architect Plan Technique — Refactoring` --references--> `FrmBomProductionSimulation — Production Simulation`  [EXTRACTED]
  app-csharp/CharlesNadejda/docs/ARCHITECT_PLAN_TECHNIQUE.md → CharlesNadejda/Forms/FrmBomProductionSimulation.cs
- `OWASP Security Constraints` --rationale_for--> `IngredientDAL — Ingredient Data Access Layer`  [INFERRED]
  app-csharp/CharlesNadejda/docs/PO_USER_STORIES_REFACTORING.md → CharlesNadejda/DAL/IngredientDAL.cs
- `StockDAL — Stock Data Access Layer` --implements--> `M:N Relationship Stock-Activity Liaison`  [EXTRACTED]
  CharlesNadejda/DAL/StockDAL.cs → app-csharp/CharlesNadejda/docs/ARCHITECT_PLAN_TECHNIQUE.md

## Hyperedges (group relationships)
- **SFA Navigation Architecture (ScreenRouter + AppState + FrmPrincipal)** — sfa_pattern, screenrouter, appstate, frmprincipal, navigationparams [EXTRACTED 1.00]
- **Multi-Agent Workflow Pipeline (PO Audit -> User Stories -> Architect Plan -> Implementation)** — po_audit_design, po_user_stories, architect_plan, journal [EXTRACTED 1.00]
- **BOM Production Stack (FIFO Engine + Multi-Level + DALs)** — fifo_transaction, bom_multilevel, bomproductiondal, bomfichedal, bomniveaudal, bomstockdal [INFERRED 0.85]

## Communities (78 total, 50 thin omitted)

### Community 0 - "Shell ERP Principal"
Cohesion: 0.07
Nodes (5): AppStatusBar, FrmPrincipal, SidebarPanel, TitleBarPanel, Utilisateur

### Community 1 - "Forms Base & Login"
Cohesion: 0.07
Nodes (17): CheckedListBox, Color, ErrorProvider, Form, AppColors, CharlesNadejda.Forms, CharlesNadejda.Forms, FrmActiviteStocks (+9 more)

### Community 2 - "Shell Navigation & Utils"
Cohesion: 0.06
Nodes (16): AppState, Dictionary, CharlesNadejda, UnitConvertisseur, int, CharlesNadejda.Navigation, ScreenRouter, NavigationParams — Screen Navigation Parameters (+8 more)

### Community 3 - "BOM Contextes & Liste Base"
Cohesion: 0.1
Nodes (4): CharlesNadejda.Forms, FrmBomContextes, CharlesNadejda.Forms, FrmListeBase

### Community 5 - "BOM Fiches & Simulation"
Cohesion: 0.11
Nodes (5): BomNiveau, CharlesNadejda.Forms, FrmBomFiches, CharlesNadejda.Forms, FrmBomProductionSimulation

### Community 7 - "Architecture & DAL BOM"
Cohesion: 0.13
Nodes (24): M:N Relationship Stock-Activity Liaison, Architect Plan Technique — Refactoring, BOM Multi-Level Production System, BomFicheDAL — BOM Recipe Data Access Layer, BomNiveauDAL — BOM Level Data Access Layer, BomProductionDAL — Production Execution DAL, BomStockDAL — BOM Stock Data Access Layer, ArtisaStock Design System — Chocolate Palette (+16 more)

### Community 8 - "Forms Edit CRUD"
Cohesion: 0.13
Nodes (10): bool, CharlesNadejda.Forms, FrmActiviteEdit, CharlesNadejda.Forms, FrmBomNiveauEdit, CharlesNadejda.Forms, FrmFournisseurEdit, Fournisseur (+2 more)

### Community 9 - "BOM Fiche Editor"
Cohesion: 0.19
Nodes (5): BomFiche, CharlesNadejda.Forms, FrmBomFicheEdit, InputItem, List

### Community 11 - "Achat Editor"
Cohesion: 0.2
Nodes (6): CheckBox, DateTimePicker, CharlesNadejda.Forms, FrmAchatEdit, Lot, RadioButton

### Community 14 - "Ingrédients Liste"
Cohesion: 0.19
Nodes (3): FlowLayoutPanel, CharlesNadejda.Forms, FrmIngredients

### Community 15 - "Login Designer & UI"
Cohesion: 0.17
Nodes (6): CharlesNadejda.Forms, FrmLogin, Label, Panel, AppStatusBar, CharlesNadejda.Forms.Shell

### Community 20 - "NuGet Dependencies"
Cohesion: 0.3
Nodes (12): BCrypt.Net-Next 4.1.0, BouncyCastle.Cryptography 2.6.2, CharlesNadejda C# Application, MySQL Database Connectivity via Connector/NET, MySql.Data 9.6.0 (MySQL Connector/NET), Newtonsoft.Json 13.0.4 (Json.NET), BCrypt Password Hashing Strategy, System.Buffers 4.6.1 (+4 more)

### Community 23 - "VS Project Config"
Cohesion: 0.2
Nodes (9): Analyzers, Configuration, CopyToOutputEntries, FrameworkPath, Outputs, ProjectFileName, References, RootPath (+1 more)

### Community 27 - "BOM Contexte Editor"
Cohesion: 0.2
Nodes (3): CharlesNadejda.Forms, FrmBomContexteEdit, ListBox

### Community 28 - "Ingrédient Editor"
Cohesion: 0.24
Nodes (4): ComboBox, CharlesNadejda.Forms, FrmIngredientEdit, Ingredient

### Community 31 - "BOM Niveaux Forms"
Cohesion: 0.22
Nodes (4): BomContexte, CharlesNadejda.Forms, FrmBomNiveaux, FrmListeBase

### Community 33 - "Achats Liste"
Cohesion: 0.25
Nodes (3): Activite, CharlesNadejda.Forms, FrmAchats

### Community 34 - "FrmPrincipal Designer"
Cohesion: 0.25
Nodes (5): CharlesNadejda.Forms, FrmPrincipal, IContainer, MenuStrip, ToolStripMenuItem

### Community 35 - "Dark Menu Renderer"
Cohesion: 0.25
Nodes (5): CharlesNadejda.Forms, DarkColorTable, DarkMenuRenderer, ProfessionalColorTable, ToolStripProfessionalRenderer

### Community 38 - "Simulation Designer"
Cohesion: 0.29
Nodes (4): CharlesNadejda.Forms, FrmBomProductionSimulation, GroupBox, NumericUpDown

### Community 39 - "Fournisseurs Designer"
Cohesion: 0.29
Nodes (4): Button, DataGridView, CharlesNadejda.Forms, FrmFournisseurs

### Community 40 - "Stock Editor"
Cohesion: 0.33
Nodes (3): CharlesNadejda.Forms, FrmStockEdit, Stock

### Community 42 - "Resources Designer"
Cohesion: 0.4
Nodes (4): CultureInfo, CharlesNadejda.Properties, Resources, ResourceManager

### Community 44 - "VS Layout Backup"
Cohesion: 0.4
Nodes (4): DocumentGroupContainers, Documents, Version, WorkspaceRootPath

### Community 45 - "VS Layout Active"
Cohesion: 0.4
Nodes (4): DocumentGroupContainers, Documents, Version, WorkspaceRootPath

### Community 50 - "Model RapportCout"
Cohesion: 0.5
Nodes (3): CharlesNadejda.Models, LigneCout, RapportCout

## Knowledge Gaps
- **138 isolated node(s):** `Version`, `WorkspaceRootPath`, `Documents`, `DocumentGroupContainers`, `Version` (+133 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **50 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `FrmPrincipal` connect `Shell ERP Principal` to `Forms Base & Login`, `Shell Navigation & Utils`, `Dark Menu Renderer`, `Architecture & DAL BOM`, `Fournisseurs Designer`, `BOM Fiche Editor`, `Login Designer & UI`?**
  _High betweenness centrality (0.163) - this node is a cross-community bridge._
- **Why does `string` connect `Shell Navigation & Utils` to `BOM Contexte DAL`, `BOM Niveau DAL`, `BOM Fiche DAL`, `Vue Stock Global DAL`?**
  _High betweenness centrality (0.078) - this node is a cross-community bridge._
- **Why does `Label` connect `Login Designer & UI` to `Shell ERP Principal`, `Shell Navigation & Utils`, `BOM Contextes & Liste Base`, `Production Workflow`, `BOM Fiches & Simulation`, `Simulation Designer`, `Fournisseurs Designer`, `Forms Edit CRUD`, `BOM Fiche Editor`, `Vue Stock Global`, `Achat Editor`, `Ingrédient Editor`?**
  _High betweenness centrality (0.069) - this node is a cross-community bridge._
- **What connects `Version`, `WorkspaceRootPath`, `Documents` to the rest of the system?**
  _138 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Shell ERP Principal` be split into smaller, more focused modules?**
  _Cohesion score 0.07 - nodes in this community are weakly interconnected._
- **Should `Forms Base & Login` be split into smaller, more focused modules?**
  _Cohesion score 0.07 - nodes in this community are weakly interconnected._
- **Should `Shell Navigation & Utils` be split into smaller, more focused modules?**
  _Cohesion score 0.06 - nodes in this community are weakly interconnected._