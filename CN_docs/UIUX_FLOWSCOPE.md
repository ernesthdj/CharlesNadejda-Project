# UI/UX Design — FlowScope

> **Agent #3 — UI/UX Designer**
> **Date :** 2026-05-14
> **Version :** 1.0 — MVP
> **Documents sources :** `docs/PO_FLOWSCOPE.md` v1.0, `docs/ARCH_FLOWSCOPE.md` v1.0
> **Destinataires :** Agent #4 (Parsers), Agent #5 (Frontend)

---

## Table des matieres

1. [Design System](#1-design-system)
2. [Layout & Wireframes](#2-layout--wireframes)
3. [Composants UI — Specifications detaillees](#3-composants-ui--specifications-detaillees)
4. [Interactions & Micro-interactions](#4-interactions--micro-interactions)
5. [Responsive & Contraintes](#5-responsive--contraintes)
6. [Checklist Accessibilite](#6-checklist-accessibilite)

---

## 1. Design System

### 1.1 Palette de couleurs

Theme dark neutre, inspire de GitHub Dark / VS Code. Toutes les couleurs sont definies comme tokens Tailwind avec le prefixe `fs-` (FlowScope).

#### Couleurs de surface

| Token Tailwind | Hex | Usage |
|---------------|-----|-------|
| `fs-bg` | `#0d1117` | Background principal (canvas, page) |
| `fs-surface` | `#161b22` | Sidebar, panels, cards, inspector |
| `fs-surface-hover` | `#1c2129` | Hover sur elements de surface |
| `fs-surface-active` | `#282e36` | Element actif/selectionne dans sidebar |
| `fs-border` | `#30363d` | Bordures, separateurs |
| `fs-border-focus` | `#58a6ff` | Bordure de focus visible (a11y) |

#### Couleurs de texte

| Token Tailwind | Hex | Ratio vs `fs-bg` | Usage |
|---------------|-----|-------------------|-------|
| `fs-text` | `#e6edf3` | 13.2:1 | Texte principal, labels, titres |
| `fs-text-secondary` | `#8b949e` | 5.3:1 | Texte secondaire, descriptions, hints |
| `fs-text-muted` | `#6e7681` | 3.6:1 | Texte desactive, placeholders (elements larges uniquement) |

#### Couleurs d'interaction

| Token Tailwind | Hex | Usage |
|---------------|-----|-------|
| `fs-accent` | `#58a6ff` | Liens, boutons actifs, selection, focus ring |
| `fs-accent-hover` | `#79c0ff` | Hover sur elements accent |
| `fs-accent-subtle` | `#58a6ff1a` | Background subtle accent (10% opacite) |
| `fs-danger` | `#f85149` | Erreurs, actions destructrices |
| `fs-success` | `#3fb950` | Confirmations, statuts OK |

#### Couleurs par NodeType

| NodeType | Token | Hex | Reference visuelle |
|----------|-------|-----|-------------------|
| `table` | `fs-node-table` | `#58a6ff` | Bleu |
| `form` | `fs-node-form` | `#bc8cff` | Violet |
| `dal` | `fs-node-dal` | `#f0883e` | Orange |
| `model` | `fs-node-model` | `#3fb950` | Vert |
| `controller` | `fs-node-controller` | `#f85149` | Rouge |
| `route` | `fs-node-route` | `#79c0ff` | Cyan |
| `view` | `fs-node-view` | `#f778ba` | Rose |
| `process` | `fs-node-process` | `#d29922` | Jaune |
| `stock` | `fs-node-stock` | `#2ea043` | Emeraude |
| `custom` | `fs-node-custom` | `#8b949e` | Gris |

**Utilisation dans les composants :** chaque NodeType utilise sa couleur a 20% d'opacite pour le fond du badge (`bg-fs-node-table/20`) et a 100% pour le texte et la bordure gauche du node.

#### Couleurs par EdgeType

| EdgeType | Token | Hex | Style visuel |
|----------|-------|-----|-------------|
| `fk` | `fs-edge-fk` | `#58a6ff` | Trait plein, fleche fermee |
| `inheritance` | `fs-edge-inheritance` | `#bc8cff` | Pointilles, fleche ouverte |
| `dependency` | `fs-edge-dependency` | `#8b949e` | Trait plein, fleche fermee |
| `flow` | `fs-edge-flow` | `#3fb950` | Trait plein anime, fleche fermee |
| `data` | `fs-edge-data` | `#f0883e` | Pointilles espaces, fleche fermee |

#### CSS Variables (fallback)

```css
/* src/index.css — en complement des tokens Tailwind */
:root {
  --fs-bg: #0d1117;
  --fs-surface: #161b22;
  --fs-surface-hover: #1c2129;
  --fs-surface-active: #282e36;
  --fs-border: #30363d;
  --fs-border-focus: #58a6ff;
  --fs-text: #e6edf3;
  --fs-text-secondary: #8b949e;
  --fs-text-muted: #6e7681;
  --fs-accent: #58a6ff;
  --fs-accent-hover: #79c0ff;
  --fs-danger: #f85149;
  --fs-success: #3fb950;

  /* Canvas React Flow */
  --fs-canvas-dot: #30363d;
  --fs-canvas-dot-size: 1px;
  --fs-canvas-dot-gap: 20px;
}
```

### 1.2 Typographie

**Famille principale :** `Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif`
**Famille monospace :** `"JetBrains Mono", "Fira Code", "Cascadia Code", Consolas, monospace`

| Role | Taille | Poids | Famille | Token Tailwind |
|------|--------|-------|---------|----------------|
| Titre projet (sidebar) | 16px | 600 (semibold) | Inter | `text-base font-semibold` |
| Titre section (sidebar) | 11px | 600 (semibold) | Inter | `text-[11px] font-semibold uppercase tracking-wider` |
| Label systeme (sidebar) | 14px | 400 (regular) | Inter | `text-sm font-normal` |
| Label node | 13px | 600 (semibold) | Inter | `text-[13px] font-semibold` |
| Badge type | 10px | 600 (semibold) | Inter | `text-[10px] font-semibold uppercase tracking-wide` |
| Metadata node | 11px | 400 (regular) | Inter | `text-[11px] font-normal` |
| Breadcrumb segment | 13px | 400 (regular) | Inter | `text-[13px] font-normal` |
| Breadcrumb actif | 13px | 600 (semibold) | Inter | `text-[13px] font-semibold` |
| Inspecteur titre | 16px | 600 (semibold) | Inter | `text-base font-semibold` |
| Inspecteur label | 12px | 500 (medium) | Inter | `text-xs font-medium` |
| Inspecteur valeur | 13px | 400 (regular) | Inter | `text-[13px] font-normal` |
| Code / chemin fichier | 12px | 400 (regular) | JetBrains Mono | `text-xs font-normal font-mono` |
| Recherche input | 16px | 400 (regular) | Inter | `text-base font-normal` |
| Tooltip | 12px | 400 (regular) | Inter | `text-xs font-normal` |

**Interlignes :** `leading-tight` (1.25) pour les labels compacts, `leading-normal` (1.5) pour les descriptions.

### 1.3 Grille et espacements (systeme 8px)

Toutes les dimensions sont des multiples de 8px. Les micro-espacements utilisent 4px.

| Token | Valeur | Usage |
|-------|--------|-------|
| `space-1` | 4px | Ecart entre icone et texte inline, padding badge |
| `space-2` | 8px | Padding interne composants compacts, gap entre items |
| `space-3` | 12px | Padding vertical sidebar items |
| `space-4` | 16px | Padding horizontal sidebar, padding cards |
| `space-5` | 20px | Marge entre sections inspecteur |
| `space-6` | 24px | Padding horizontal inspecteur |
| `space-8` | 32px | Hauteur toolbar, espacement entre groupes |

**Dimensions fixes :**

| Element | Largeur | Hauteur |
|---------|---------|---------|
| Sidebar | 240px | 100vh |
| Inspector | 320px | 100vh |
| Toolbar button | 32px | 32px |
| Sidebar item | 240px (full width) | 40px |
| Node min width | 180px | auto |
| Node max width | 280px | auto |
| Overview node | 240px | 128px |
| MiniMap | 180px | 120px |
| Search modal | 560px | auto (max 480px) |
| Breadcrumb bar | 100% canvas width | 40px |

### 1.4 Rayons de bordure

| Usage | Rayon | Token Tailwind |
|-------|-------|----------------|
| Nodes | 8px | `rounded-lg` |
| Cards, panels | 8px | `rounded-lg` |
| Badges | 4px | `rounded` |
| Boutons toolbar | 6px | `rounded-md` |
| Input recherche | 8px | `rounded-lg` |
| Search modal | 12px | `rounded-xl` |
| Tooltip | 6px | `rounded-md` |

### 1.5 Ombres

| Usage | Valeur CSS | Token Tailwind |
|-------|-----------|----------------|
| Node default | `0 1px 3px rgba(0,0,0,0.3)` | `shadow-md` (custom) |
| Node hover | `0 4px 12px rgba(0,0,0,0.4)` | `shadow-lg` (custom) |
| Node selected (glow) | `0 0 0 2px {nodeColor}40, 0 0 16px {nodeColor}20` | Inline style dynamique |
| Inspector panel | `-4px 0 16px rgba(0,0,0,0.3)` | Custom |
| Search modal | `0 16px 48px rgba(0,0,0,0.5)` | `shadow-2xl` (custom) |
| Toolbar | `0 2px 8px rgba(0,0,0,0.3)` | `shadow-lg` (custom) |

### 1.6 Opacites

| Usage | Opacite |
|-------|---------|
| Background badge NodeType | 20% (`/20`) |
| Overlay fond modal recherche | 60% (`bg-black/60`) |
| Edge default | 60% |
| Edge hover | 100% |
| MiniMap mask | 80% (`bg-fs-bg/80`) |
| Element disabled | 40% (`opacity-40`) |
| Tooltip background | 95% |

### 1.7 Iconographie — Mapping NodeType vers Lucide

| NodeType | Icone Lucide | Nom exact import | Justification |
|----------|-------------|-----------------|---------------|
| `table` | Base de donnees | `Database` | Standard pour les tables SQL |
| `form` | Ecran moniteur | `Monitor` | Formulaire = interface utilisateur |
| `dal` | Couches empilees | `Layers` | Couche d'abstraction = layers |
| `model` | Boite | `Box` | Entite = boite contenant des donnees |
| `controller` | Globe | `Globe` | Controleur web = globe |
| `route` | Route | `Route` | Route URL = chemin |
| `view` | Layout | `Layout` | Vue = mise en page |
| `process` | Branche Git | `GitBranch` | Processus = flux |
| `stock` | Paquet | `Package` | Stock = paquet de marchandises |
| `custom` | Cercle aide | `HelpCircle` | Type inconnu |

**Icones supplementaires utilisees dans l'UI :**

| Usage | Icone Lucide | Import |
|-------|-------------|--------|
| Recherche | Loupe | `Search` |
| Fermer panel/modal | Croix | `X` |
| Zoom in | Plus dans loupe | `ZoomIn` |
| Zoom out | Moins dans loupe | `ZoomOut` |
| Fit view | Maximiser | `Maximize2` |
| Reorganiser layout | Grille | `LayoutGrid` |
| Copier chemin fichier | Copier | `Copy` |
| Retour arriere | Fleche gauche | `ArrowLeft` |
| Raccourcis clavier | Clavier | `Keyboard` |
| Cle primaire | Cle | `Key` |
| Cle etrangere | Lien | `Link` |
| Overview systeme | Tableau de bord | `LayoutDashboard` |
| Chevron breadcrumb | Chevron droit | `ChevronRight` |
| Section inspecteur | Chevron bas | `ChevronDown` |
| Connexion entrante | Fleche entrant | `ArrowDownLeft` |
| Connexion sortante | Fleche sortant | `ArrowUpRight` |

**Taille des icones :**

| Contexte | Taille | Classe Tailwind |
|----------|--------|-----------------|
| Dans un node | 16px | `h-4 w-4` |
| Dans la sidebar | 16px | `h-4 w-4` |
| Dans le toolbar | 18px | `h-[18px] w-[18px]` |
| Dans l'inspecteur (titre) | 20px | `h-5 w-5` |
| Dans un badge | 12px | `h-3 w-3` |
| Overview node | 24px | `h-6 w-6` |
| Search modal icon | 20px | `h-5 w-5` |

---

## 2. Layout & Wireframes

### 2.1 Layout principal — Sidebar + Canvas + Inspector

```
|<---- 240px ---->|<------------ flex-grow ------------>|<-- 320px -->|
|                 |                                     |  (conditi-  |
|    SIDEBAR      |            CANVAS AREA              |   onnel)    |
|                 |                                     |             |
| +-------------+ | +--------------------------------+  | +---------+ |
| | FlowScope   | | | Breadcrumb     [Toolbar icons] |  | |Inspector| |
| | projet name | | +--------------------------------+  | |         | |
| +-------------+ | |                                |  | | Titre   | |
| | [Search]    | | |                                |  | | Badge   | |
| +-------------+ | |                                |  | | Desc    | |
| | SYSTEMS     | | |      React Flow Canvas         |  | |         | |
| |  > Overview | | |                                |  | | Meta    | |
| |    DB       | | |      (nodes + edges)           |  | | data    | |
| |    C#       | | |                                |  | |         | |
| |    BOM      | | |                                |  | | Tags    | |
| |    Laravel  | | |                                |  | |         | |
| |             | | |                                |  | | Conn.   | |
| |             | | |                  +--------+    |  | | list    | |
| |             | | |                  |MiniMap |    |  | |         | |
| |             | | |                  +--------+    |  | |         | |
| +-------------+ | +--------------------------------+  | +---------+ |
|                 |                                     |             |
+--------100vw (min 1280px)----------------------------+
```

**Comportement :**
- Sidebar : position fixe a gauche, `width: 240px`, `height: 100vh`, `bg-fs-surface`
- Canvas Area : `flex-grow`, remplit tout l'espace entre sidebar et inspector
- Inspector : conditionnel (absent par defaut), `width: 320px`, `height: 100vh`, animation `slide-in` depuis la droite
- Quand l'inspector s'ouvre, le canvas **se reduit** (pas de superposition)

### 2.2 Sidebar — Detail

```
+---------------------------+
|  p-4                      |
|  [Diamond icon] FlowScope |   <- 16px semibold, text-fs-text
|  Charles & Nadejda        |   <- 12px regular, text-fs-text-secondary
|                           |
+---------------------------+
|  px-4 py-2                |
|  +---------------------+  |
|  | [Search] Rechercher  |  |   <- input-like button, rounded-lg
|  |           Ctrl+K     |  |   <- right-aligned shortcut hint
|  +---------------------+  |
+---------------------------+
|  px-4 pt-4 pb-2           |
|  SYSTEMES                 |   <- 11px semibold uppercase, text-fs-text-muted
+---------------------------+
|  +-----------------------+ |
|  | [LayoutDash] Overview | |   <- 40px height, px-4, rounded-md
|  +-----------------------+ |   <- ACTIF: bg-fs-surface-active, text-fs-text
|  | [Database]   DB MySQL | |      border-left 2px fs-accent
|  +-----------------------+ |
|  | [Monitor]    C# App   | |   <- INACTIF: bg-transparent, text-fs-text-secondary
|  +-----------------------+ |      hover: bg-fs-surface-hover
|  | [GitBranch]  BOM      | |
|  +-----------------------+ |
|  | [Globe]      Laravel  | |
|  +-----------------------+ |
|                            |
|                            |   <- espace vide (flex-grow)
|                            |
+----------------------------+
```

### 2.3 Vue Overview — Blocs systeme

```
+------------------------------------------------------------------------+
|  Overview                                              [Toolbar icons]  |
+------------------------------------------------------------------------+
|                                                                        |
|     +---------------------+          +---------------------+           |
|     |  [Database] 24px    |          |  [Monitor]  24px    |           |
|     |                     |          |                     |           |
|     |  Base de donnees    |          |  Application C#     |           |
|     |  MySQL              |          |  WinForms           |           |
|     |                     |          |                     |           |
|     |  25 tables          |          |  42 classes         |           |
|     |  -- description --  |          |  -- description --  |           |
|     +---------------------+          +---------------------+           |
|            240 x 128px                     240 x 128px                 |
|                                                                        |
|     +---------------------+          +---------------------+           |
|     |  [GitBranch] 24px   |          |  [Globe]   24px     |           |
|     |                     |          |                     |           |
|     |  Pipeline BOM       |          |  Laravel             |           |
|     |                     |          |  (placeholder)       |           |
|     |  6 etapes           |          |                     |           |
|     |  -- description --  |          |  0 elements         |           |
|     +---------------------+          +---------------------+           |
|                                                                        |
|                                          +--------+                    |
|                                          |MiniMap |                    |
|                                          |180x120 |                    |
|                                          +--------+                    |
+------------------------------------------------------------------------+
```

**Layout :** Auto-layout Dagre (direction TB), espacement 80px horizontal, 64px vertical entre nodes.

### 2.4 Vue detaillee (DB, C#) — Canvas avec nodes

```
+------------------------------------------------------------------------+
|  Overview > Base de donnees MySQL                  [ZI][ZO][FV][RL][S] |
+------------------------------------------------------------------------+
|                                                                        |
|  +------------------+      +------------------+                        |
|  | [DB] ingredients |----->| [DB] unites      |                        |
|  | TABLE            |  fk  | TABLE            |                        |
|  | 5 cols           |      | 3 cols           |                        |
|  +------------------+      +------------------+                        |
|          |                                                             |
|          | fk                                                          |
|          v                                                             |
|  +------------------+      +------------------+                        |
|  | [DB] recettes    |      | [DB] categories  |                        |
|  | TABLE            |----->| TABLE            |                        |
|  | 4 cols           |  fk  | 3 cols           |                        |
|  +------------------+      +------------------+                        |
|                                                                        |
|                                                   +--------+           |
|                                                   |MiniMap |           |
|                                                   +--------+           |
+------------------------------------------------------------------------+
```

### 2.5 Vue BOM Pipeline — Layout horizontal

```
+------------------------------------------------------------------------+
|  Overview > Pipeline BOM                           [ZI][ZO][FV][RL][S] |
+------------------------------------------------------------------------+
|                                                                        |
|  +-----------+    +----------+    +-----------+    +----------+        |
|  | [Package] |--->| [GitBr.] |--->| [GitBr.]  |--->| [GitBr.] |       |
|  | Ingre-    | => | Niveaux  | => | Fiches    | => | Produc-  |       |
|  | dients    |    | BOM      |    | Recettes  |    | tions    |       |
|  |           |    |          |    |           |    |          |        |
|  | Point     |    | Structure|    | Definition|    | Execution|        |
|  | d'entree  |    | nomen.   |    | produit   |    | prod.    |        |
|  +-----------+    +----------+    +-----------+    +----------+        |
|                                                         |              |
|                                                         | =>           |
|                                                         v              |
|                                   +-----------+    +----------+        |
|                                   | [Package] |<---| [Package]|        |
|                                   | Reserva-  | <= | Stocks   |        |
|                                   | tions     |    |          |        |
|                                   +-----------+    +----------+        |
|                                                                        |
|                                                   +--------+           |
|                                                   |MiniMap |           |
|                                                   +--------+           |
+------------------------------------------------------------------------+
```

**Les fleches `=>` representent des edges animes (pointilles mouvants verts).**

### 2.6 Panneau Inspecteur ouvert

```
                                     +-----------------------------------+
                                     |  px-6 py-4                    [X] |
                                     +-----------------------------------+
                                     |  [Database] 20px                  |
                                     |  ingredients                      |  <- 16px semibold
                                     |  +-------+                        |
                                     |  | TABLE |                        |  <- badge type
                                     |  +-------+                        |
                                     +-----------------------------------+
                                     |  Description                      |  <- 12px medium, text-fs-text-secondary
                                     |  Table centrale du module stock   |  <- 13px regular, text-fs-text
                                     +-----------------------------------+
                                     |  Fichier source                   |
                                     |  Models/Ingredient.cs     [Copy] |  <- font-mono 12px
                                     +-----------------------------------+
                                     |  Metadonnees                      |
                                     |                                   |
                                     |  Colonnes (5)                     |
                                     |    id: INT                        |  <- font-mono 12px, text-fs-text-secondary
                                     |    nom: VARCHAR(100)              |
                                     |    unite_id: INT           [Link] |
                                     |    prix_unitaire: DECIMAL         |
                                     |    stock_actuel: DECIMAL          |
                                     |                                   |
                                     |  Cles primaires                   |
                                     |    [Key] id                       |
                                     +-----------------------------------+
                                     |  Tags                             |
                                     |  +-----+ +------+ +-------+      |
                                     |  | bom | | stock| |critique|     |
                                     |  +-----+ +------+ +-------+      |
                                     +-----------------------------------+
                                     |  Connexions                       |
                                     |                                   |
                                     |  [ArrowUpRight] Sortantes (1)     |
                                     |    -> unites (FK: unite_id)       |  <- cliquable
                                     |                                   |
                                     |  [ArrowDownLeft] Entrantes (1)    |
                                     |    <- recettes (FK: via rec_ing)  |  <- cliquable
                                     +-----------------------------------+
```

### 2.7 Modale de recherche (Command Palette)

```
                     +--------------------------------------------+
                     |                                            |
  bg-black/60        |   +------------------------------------+   |
  (overlay plein     |   |  [Search] Rechercher un node...    |   |  <- 16px, rounded-xl top
  ecran)             |   +------------------------------------+   |
                     |   |                                    |   |
                     |   |  [Database] ingredients            |   |  <- resultat 1, bg-fs-surface-hover si focus
                     |   |    Base de donnees MySQL           |   |  <- text-fs-text-secondary 12px
                     |   |                                    |   |
                     |   |  [Monitor] FrmIngredients          |   |  <- resultat 2
                     |   |    Application C# WinForms         |   |
                     |   |                                    |   |
                     |   |  [Package] Ingredients             |   |  <- resultat 3
                     |   |    Pipeline BOM                    |   |
                     |   |                                    |   |
                     |   +------------------------------------+   |
                     |           560px wide, max 480px tall       |
                     |                                            |
                     +--------------------------------------------+
```

### 2.8 Overlay raccourcis clavier (?)

```
                +-----------------------------------------------+
                |                                               |
                |   Raccourcis clavier                    [X]   |  <- 16px semibold
                |                                               |
                |   Navigation                                  |  <- section title, 12px uppercase
                |   +-------------------------------------------+
                |   | 1-9         Naviguer vers une vue         |
                |   | Backspace   Retour a la vue precedente    |
                |   | F           Ajuster le zoom (fit view)    |
                |   +-------------------------------------------+
                |                                               |
                |   Recherche                                   |
                |   +-------------------------------------------+
                |   | Ctrl+K      Ouvrir la recherche           |
                |   | Escape      Fermer panneau / modale       |
                |   +-------------------------------------------+
                |                                               |
                |   Aide                                        |
                |   +-------------------------------------------+
                |   | ?           Afficher ce panneau           |
                |   +-------------------------------------------+
                |                                               |
                +-----------------------------------------------+
                        480px wide, centree, rounded-xl
```

---

## 3. Composants UI — Specifications detaillees

### 3.1 CustomNode (node React Flow)

Le composant principal du canvas. Utilise en type `"custom"` React Flow, il lit `data.type` (NodeType metier) pour adapter son apparence.

#### Dimensions

| Propriete | Valeur |
|-----------|--------|
| Min width | 180px |
| Max width | 280px |
| Padding | 12px (horizontal), 8px (vertical) |
| Border-left width | 3px |
| Border radius | 8px (`rounded-lg`) |
| Gap interne (icone-texte) | 8px |
| Gap vertical (sections) | 4px |

#### Structure interne

```
+--- 3px border-left color = nodeColor ----+
| px-3 py-2                                |
|                                           |
|  [Icon 16px]  label-du-node    <- row 1  |  <- 13px semibold, text-fs-text
|               +-------+                  |
|               | TYPE  |         <- row 2 |  <- badge 10px uppercase
|               +-------+                  |
|  8 cols | 3 methods     <- row 3 (opt.)  |  <- 11px, text-fs-text-secondary
+-------------------------------------------+
```

#### Etats visuels

| Etat | Background | Bordure | Ombre | Autres |
|------|-----------|---------|-------|--------|
| Default | `fs-surface` | 1px `fs-border` + 3px left `nodeColor` | `shadow-md` | -- |
| Hover | `fs-surface-hover` | 1px `fs-border` + 3px left `nodeColor` | `shadow-lg` | Cursor pointer |
| Selected | `fs-surface` | 1px `nodeColor` + 3px left `nodeColor` | Glow: `0 0 0 2px {nodeColor}40, 0 0 16px {nodeColor}20` | -- |
| Dragging | `fs-surface-hover` | Same as selected | `shadow-lg` | `opacity-90`, cursor grabbing |

#### Transitions

| Propriete | Duree | Easing |
|-----------|-------|--------|
| Background | 150ms | `ease-out` |
| Box-shadow | 200ms | `ease-out` |
| Border-color | 150ms | `ease-out` |
| Transform (drag) | 0ms | -- (instant, gere par React Flow) |

#### Variantes par NodeType

Le composant ne change **pas** de structure entre les types. Seuls ces elements varient :

| Element variable | Source |
|-----------------|--------|
| Icone | `NODE_ICONS[type]` (cf. section 1.7) |
| Couleur bordure gauche | `NODE_BORDER_COLORS[type]` |
| Couleur texte badge | `nodeColor` (100%) |
| Fond badge | `nodeColor` (20% opacite) |
| Couleur glow (selected) | `nodeColor` (25% et 12% opacite) |

#### Metadata affichee (row 3)

| NodeType | Metadata affichee | Format |
|----------|-------------------|--------|
| `table` | `metadata.columns.length` | "N cols" |
| `form` | `metadata.methods.length` | "N methods" |
| `dal` | `metadata.methods.length` | "N methods" |
| `model` | `metadata.properties.length` | "N props" |
| `process` | `metadata.role` (tronque 30 char) | Texte direct |
| `stock` | `metadata.role` (tronque 30 char) | Texte direct |
| Autres | Nombre total de cles dans `metadata` | "N attrs" |

#### Handles (points de connexion React Flow)

- **Source handle :** bas du node (`Position.Bottom`) pour TB, droite (`Position.Right`) pour LR
- **Target handle :** haut du node (`Position.Top`) pour TB, gauche (`Position.Left`) pour LR
- Handles invisibles (`opacity: 0`, `width: 8px`, `height: 8px`) — pas de cercle visible au repos
- Handles visibles au hover du node (`opacity: 1`, `background: nodeColor`)

#### Accessibilite

- `role="button"` sur le container
- `aria-label="{label} — {type}"` (ex: "ingredients — table")
- `tabIndex={0}` pour la navigation clavier
- Focus ring visible : `outline: 2px solid fs-accent` + `outline-offset: 2px`

---

### 3.2 OverviewNode (node vue Overview)

Node specifique pour la vue d'ensemble, affichant les systemes comme de gros blocs.

#### Dimensions

| Propriete | Valeur |
|-----------|--------|
| Width | 240px |
| Height | 128px |
| Padding | 20px |
| Border radius | 12px (`rounded-xl`) |
| Border | 1px `fs-border` |

#### Structure interne

```
+----------------------------------------------+
|  p-5                                          |
|                                               |
|  [Icon 24px]  text-fs-accent                  |
|                                               |
|  Nom du systeme                               |  <- 16px semibold, text-fs-text
|  Description courte du systeme                |  <- 12px regular, text-fs-text-secondary
|                                               |
|  25 tables                                    |  <- 13px medium, text-fs-accent
+----------------------------------------------+
```

#### Etats visuels

| Etat | Background | Bordure | Ombre |
|------|-----------|---------|-------|
| Default | `fs-surface` | 1px `fs-border` | `shadow-md` |
| Hover | `fs-surface-hover` | 1px `fs-accent/40` | `shadow-lg` |
| Active (clic) | `fs-surface-active` | 1px `fs-accent` | `shadow-lg` |

#### Transitions

- Background, border, box-shadow : 200ms ease-out
- Transform on hover : `scale(1.02)` sur 200ms ease-out (effet de "levee")

#### Accessibilite

- `role="button"` + `aria-label="Ouvrir {systemLabel} — {nodeCount} elements"`
- `tabIndex={0}`
- Focus ring : `outline: 2px solid fs-accent` + `outline-offset: 2px`
- Double-clic OU Enter pour naviguer

---

### 3.3 Sidebar

#### Dimensions et positionnement

| Propriete | Valeur |
|-----------|--------|
| Width | 240px |
| Height | 100vh |
| Position | Fixed, left: 0, top: 0 |
| Background | `fs-surface` |
| Border right | 1px `fs-border` |
| Z-index | 20 |

#### Sections

**1. Header (projet)**
- Padding : 16px
- Icone projet : `Diamond` Lucide, 16px, `text-fs-accent`
- Titre "FlowScope" : 16px semibold, `text-fs-text`
- Sous-titre (nom projet) : 12px regular, `text-fs-text-secondary`
- Separateur en bas : 1px `fs-border`

**2. Bouton recherche**
- Padding conteneur : 16px horizontal, 8px vertical
- Apparence : simule un input, `bg-fs-bg`, bordure 1px `fs-border`, `rounded-lg`
- Hauteur : 36px
- Contenu : icone `Search` 16px `text-fs-text-muted` + "Rechercher" `text-fs-text-muted` + badge "Ctrl+K" align-right
- Hover : `border-color: fs-accent`
- Clic : ouvre la modale SearchModal

**3. Liste des systemes**
- Label section : "SYSTEMES" — 11px semibold uppercase `text-fs-text-muted`, px-4, pt-4, pb-2
- Chaque item : `SystemItem`

#### SystemItem — Detail

| Propriete | Valeur |
|-----------|--------|
| Height | 40px |
| Padding | 12px horizontal, 0 vertical |
| Border radius | 6px (`rounded-md`) |
| Margin | 2px horizontal (pour laisser le rounded respirer) |

| Etat | Background | Texte | Bordure gauche | Icone |
|------|-----------|-------|----------------|-------|
| Default | transparent | `fs-text-secondary` | none | `fs-text-muted` |
| Hover | `fs-surface-hover` | `fs-text` | none | `fs-text-secondary` |
| Active (selectionne) | `fs-surface-active` | `fs-text` | 2px `fs-accent` | `fs-accent` |

Transition : background 150ms ease-out.

---

### 3.4 Breadcrumb

#### Dimensions et positionnement

| Propriete | Valeur |
|-----------|--------|
| Height | 40px |
| Position | En haut du canvas area, fixe (pas scrollable avec le graphe) |
| Background | `fs-bg` (meme que le canvas, pas de fond distinct) |
| Padding | 0 horizontal (aligne au bord du canvas), centrage vertical |
| Border bottom | 1px `fs-border` |
| Z-index | 10 (au-dessus du canvas React Flow) |

#### Segments

```
[Overview]  >  [Base de donnees MySQL]
```

- Separateur : icone `ChevronRight`, 14px, `text-fs-text-muted`
- Segment cliquable (pas le dernier) : `text-fs-accent`, hover `text-fs-accent-hover`, cursor pointer
- Segment actif (dernier) : `text-fs-text`, `font-semibold`, non cliquable
- Taille texte : 13px
- Gap entre segments : 4px
- Transition couleur : 150ms ease-out
- Focus ring sur segments cliquables : `outline: 2px solid fs-accent`

---

### 3.5 InspectorPanel

#### Dimensions et positionnement

| Propriete | Valeur |
|-----------|--------|
| Width | 320px |
| Height | 100vh |
| Position | Fixed, right: 0, top: 0 |
| Background | `fs-surface` |
| Border left | 1px `fs-border` |
| Z-index | 20 |
| Overflow-y | auto (scroll si contenu depasse) |

#### Structure des sections

Chaque section a :
- Padding horizontal : 24px
- Padding vertical entre sections : 20px
- Separateur entre sections : 1px `fs-border` + 20px margin top/bottom

**Section 1 — Header**
- Bouton fermer `[X]` : position absolue top-right, 32x32px, hover `bg-fs-surface-hover`
- Icone type : 20px, couleur `nodeColor`
- Titre (label du node) : 16px semibold, `text-fs-text`, margin-top 8px
- Badge type : cf. section 3.1 (meme style que dans le node)

**Section 2 — Description** (conditionnelle, si `description` existe)
- Label : "Description" — 12px medium, `text-fs-text-secondary`
- Contenu : 13px regular, `text-fs-text`, `leading-normal`

**Section 3 — Fichier source** (conditionnel, si `filePath` existe)
- Label : "Fichier source" — 12px medium, `text-fs-text-secondary`
- Chemin : `font-mono` 12px, `text-fs-text`, `bg-fs-bg`, padding 8px, `rounded-md`
- Bouton copier : icone `Copy` 14px, position droite du bloc, hover `text-fs-accent`
- Clic sur copier : `navigator.clipboard.writeText(filePath)`, toast ou changement icone en `Check` pendant 2s

**Section 4 — Metadonnees**
- Label : "Metadonnees" — 12px medium, `text-fs-text-secondary`
- Sous-section par cle de metadata (ex: "Colonnes (5)", "Methodes (3)")
  - Sous-label : 12px medium, `text-fs-text-secondary`
  - Items : 12px `font-mono`, `text-fs-text`, bullet point ou padding-left 16px
  - Si array > 10 items : afficher les 10 premiers + lien "... +N de plus" cliquable pour expand

**Section 5 — Tags** (conditionnel, si `tags` existe et non vide)
- Label : "Tags" — 12px medium, `text-fs-text-secondary`
- Badges en flex-wrap, gap 4px

**Section 6 — Connexions**
- Label : "Connexions" — 12px medium, `text-fs-text-secondary`
- Sous-section "Sortantes" : icone `ArrowUpRight`, count
  - Items : label du node cible + label de l'edge entre parentheses
  - Chaque item est cliquable (hover `text-fs-accent`, cursor pointer)
  - Clic : centre et selectionne le node cible dans le canvas
- Sous-section "Entrantes" : icone `ArrowDownLeft`, count
  - Meme format

#### Animation ouverture/fermeture

| Animation | Duree | Easing | Propriete |
|-----------|-------|--------|-----------|
| Ouverture (slide-in) | 250ms | `ease-out` | `transform: translateX(100%) -> translateX(0)` |
| Fermeture (slide-out) | 200ms | `ease-in` | `transform: translateX(0) -> translateX(100%)` |

#### Scrollbar custom

```css
.inspector-panel::-webkit-scrollbar {
  width: 6px;
}
.inspector-panel::-webkit-scrollbar-track {
  background: transparent;
}
.inspector-panel::-webkit-scrollbar-thumb {
  background: var(--fs-border);
  border-radius: 3px;
}
.inspector-panel::-webkit-scrollbar-thumb:hover {
  background: var(--fs-text-muted);
}
```

---

### 3.6 SearchModal (Command Palette)

#### Dimensions et positionnement

| Propriete | Valeur |
|-----------|--------|
| Width | 560px |
| Max height | 480px |
| Position | Centre ecran (position fixed, top 20vh, left 50%, transform translateX(-50%)) |
| Background | `fs-surface` |
| Border | 1px `fs-border` |
| Border radius | 12px (`rounded-xl`) |
| Z-index | 50 |
| Shadow | `0 16px 48px rgba(0,0,0,0.5)` |

#### Overlay

- Background : `bg-black/60` (noir 60% opacite)
- Position : fixed, inset-0
- Z-index : 49
- Clic sur overlay : ferme la modale
- Animation : fade-in 150ms ease-out

#### Input de recherche

| Propriete | Valeur |
|-----------|--------|
| Height | 56px |
| Padding | 16px horizontal |
| Background | transparent |
| Border bottom | 1px `fs-border` |
| Font size | 16px |
| Placeholder | "Rechercher un node..." |
| Icone gauche | `Search` 20px, `text-fs-text-muted` |
| Raccourci droite | Badge "ESC" — `text-fs-text-muted`, 11px, bg-fs-bg, rounded, px-2 |

- Focus : pas de outline visible (le fond modal est suffisant)
- `autoFocus` : true (focus immediat a l'ouverture)

#### SearchResult (ligne de resultat)

| Propriete | Valeur |
|-----------|--------|
| Height | 48px |
| Padding | 12px horizontal |

Structure :
```
[Icon 16px nodeColor]  Label du node       <- 14px medium, text-fs-text
                       Systeme parent      <- 12px regular, text-fs-text-secondary
```

| Etat | Background |
|------|-----------|
| Default | transparent |
| Focused (clavier) | `fs-surface-hover` |
| Hover (souris) | `fs-surface-hover` |

- Navigation clavier : `ArrowUp` / `ArrowDown` pour naviguer, `Enter` pour selectionner
- L'item focused a une bordure gauche 2px `fs-accent`
- Max 10 resultats affiches

#### Etat vide

Si aucun resultat : texte centre "Aucun resultat pour '{query}'" — 13px, `text-fs-text-secondary`, padding 32px vertical.

---

### 3.7 Toolbar

#### Dimensions et positionnement

| Propriete | Valeur |
|-----------|--------|
| Position | Absolute, top-right du canvas (top: 56px pour eviter breadcrumb, right: 16px) |
| Background | `fs-surface/90` (90% opacite, backdrop-blur 8px) |
| Border | 1px `fs-border` |
| Border radius | 8px (`rounded-lg`) |
| Padding | 4px |
| Z-index | 10 |
| Layout | Flex horizontal, gap 2px |

#### Boutons

Chaque bouton est un `IconButton` avec tooltip :

| Icone | Action | Tooltip | Raccourci |
|-------|--------|---------|-----------|
| `ZoomIn` | Zoom avant | "Zoom avant" | -- |
| `ZoomOut` | Zoom arriere | "Zoom arriere" | -- |
| `Maximize2` | Fit view | "Ajuster la vue" | F |
| `LayoutGrid` | Reorganiser layout | "Reorganiser" | -- |
| `Search` | Ouvrir recherche | "Rechercher (Ctrl+K)" | Ctrl+K |

#### IconButton — Detail

| Propriete | Valeur |
|-----------|--------|
| Width | 32px |
| Height | 32px |
| Border radius | 6px (`rounded-md`) |
| Icone size | 18px |

| Etat | Background | Icone color |
|------|-----------|-------------|
| Default | transparent | `fs-text-secondary` |
| Hover | `fs-surface-hover` | `fs-text` |
| Active (clic) | `fs-surface-active` | `fs-accent` |
| Disabled | transparent | `fs-text-muted`, `opacity-40` |
| Focused | transparent + ring | `fs-text-secondary` |

- Focus ring : `outline: 2px solid fs-accent`, `outline-offset: 1px`
- Transition : background 150ms ease-out
- Tooltip : apparait apres 500ms hover, position bottom, 12px `text-fs-text`, `bg-fs-bg/95`, padding 4px 8px, `rounded-md`, max-width 200px

---

### 3.8 Badge (composant atomique)

#### Variante "type badge" (dans les nodes et l'inspecteur)

| Propriete | Valeur |
|-----------|--------|
| Padding | 2px 6px |
| Border radius | 4px (`rounded`) |
| Font | 10px semibold uppercase, `tracking-wide` |
| Background | `nodeColor` a 20% opacite |
| Texte | `nodeColor` a 100% |

#### Variante "tag badge" (dans l'inspecteur)

| Propriete | Valeur |
|-----------|--------|
| Padding | 2px 8px |
| Border radius | 4px (`rounded`) |
| Font | 11px regular |
| Background | `fs-bg` |
| Border | 1px `fs-border` |
| Texte | `fs-text-secondary` |

---

### 3.9 EdgeStyles — Rendu visuel par type

Chaque edge est rendu via le composant `CustomEdge`. Le type metier (`FlowEdge.type`) determine le style React Flow.

| EdgeType | Couleur | Stroke width | Style | Animated | Marker end | Label visible |
|----------|---------|-------------|-------|----------|------------|--------------|
| `fk` | `#58a6ff` | 1.5px | Plein (`solid`) | Non | Fleche fermee (arrowclosed) | Oui |
| `inheritance` | `#bc8cff` | 1.5px | Pointilles (`strokeDasharray: "6 3"`) | Non | Triangle vide (arrow) | Non |
| `dependency` | `#8b949e` | 1.5px | Plein | Non | Fleche fermee (arrowclosed) | Non |
| `flow` | `#3fb950` | 2px | Plein | Oui (pointilles mouvants) | Fleche fermee (arrowclosed) | Non |
| `data` | `#f0883e` | 1.5px | Pointilles espaces (`strokeDasharray: "4 6"`) | Non | Fleche fermee (arrowclosed) | Oui |

#### Etats des edges

| Etat | Opacite | Stroke width | Autres |
|------|---------|-------------|--------|
| Default | 0.6 | Selon type | -- |
| Hover | 1.0 | +1px (2.5 ou 3px) | Label affiche si cache par defaut |
| Selected | 1.0 | +1px | Halo subtil autour de l'edge |

#### Label d'edge

- Background : `fs-surface` avec 90% opacite
- Padding : 2px 6px
- Border radius : 4px
- Font : 11px regular, `text-fs-text-secondary`
- Le label n'est visible par defaut que pour les types `fk` et `data`
- Au hover, le label apparait pour tous les types (si un label existe)

---

## 4. Interactions & Micro-interactions

### 4.1 Clic sur un node

| Etape | Action | Duree |
|-------|--------|-------|
| 1 | Node clique recoit l'etat `selected` (glow + bordure) | Instantane |
| 2 | Tous les autres nodes perdent l'etat `selected` | Instantane |
| 3 | InspectorPanel s'ouvre (slide-in depuis la droite) | 250ms ease-out |
| 4 | Canvas area se reduit en largeur (flex-grow ajuste) | 250ms ease-out (synchronise avec le panel) |
| 5 | InspectorPanel affiche les donnees du node | Instantane |

**Clic sur un node deja selectionne :** pas d'effet supplementaire (inspecteur reste ouvert).

### 4.2 Double-clic sur un OverviewNode

| Etape | Action | Duree |
|-------|--------|-------|
| 1 | Effet ripple/flash subtil sur le bloc | 150ms |
| 2 | `navigateTo(metadata.systemRef)` appele | Instantane |
| 3 | Breadcrumb mis a jour | Instantane |
| 4 | Sidebar met en evidence le systeme actif | 150ms (transition bg) |
| 5 | Canvas charge les nouveaux nodes/edges | Instantane (useMemo) |
| 6 | Auto-layout Dagre calcule | < 100ms |
| 7 | `fitView()` anime le zoom | 300ms ease-out |

### 4.3 Hover sur un edge

| Etape | Action | Duree |
|-------|--------|-------|
| 1 | Opacite de l'edge passe de 0.6 a 1.0 | 150ms ease-out |
| 2 | Stroke width augmente de +1px | 150ms ease-out |
| 3 | Si label existe mais cache par defaut, il apparait | 150ms fade-in |
| 4 | Les nodes source et target recoivent un highlight subtil (bordure brightened) | 150ms ease-out |

### 4.4 Ouverture/fermeture de l'inspecteur

**Ouverture :**
1. Le panel part de `translateX(100%)` et glisse vers `translateX(0)` — 250ms ease-out
2. Le canvas area reduit sa largeur de 320px — 250ms ease-out, synchronise
3. React Flow auto-ajuste le viewport (pas de fitView automatique, juste resize)

**Fermeture :**
1. Declenchee par : clic sur `[X]`, clic sur le fond du canvas (`onPaneClick`), touche `Escape`
2. Le panel glisse de `translateX(0)` a `translateX(100%)` — 200ms ease-in
3. Le canvas area reprend sa largeur complete — 200ms ease-in, synchronise
4. Le node perd l'etat `selected` (sauf si fermeture par Escape — a discuter)

### 4.5 Ouverture de la modale de recherche

| Etape | Action | Duree |
|-------|--------|-------|
| 1 | Overlay noir apparait en fade-in | 150ms ease-out |
| 2 | Modale apparait en fade-in + leger translateY(-8px -> 0) | 200ms ease-out |
| 3 | Input recupere le focus immediatement | Instantane |
| 4 | L'utilisateur tape : resultats filtres en temps reel | < 100ms par frappe |

**Fermeture :**
- `Escape` ou clic overlay : modale fade-out 150ms ease-in, overlay fade-out 150ms ease-in

### 4.6 Transition entre vues

| Etape | Action | Duree |
|-------|--------|-------|
| 1 | L'ancien graphe disparait instantanement (pas de fade) | 0ms |
| 2 | Le nouveau `activeSystemId` est set | Instantane |
| 3 | `useMemo` recalcule les nodes/edges React Flow | < 50ms |
| 4 | `useLayout` recalcule le layout Dagre | < 100ms |
| 5 | `fitView({ duration: 300 })` anime le zoom vers le nouveau graphe | 300ms ease-out |

**Pas de crossfade entre vues** — la transition est nette (swap instantane + fitView anime). Cela evite la complexite d'animer deux graphes React Flow simultanement.

### 4.7 Drag & Pan sur le canvas

Gere nativement par React Flow. Pas de customisation necessaire sauf :
- `panOnDrag={true}` — drag le fond pour pan
- `selectionOnDrag={false}` — pas de rectangle de selection (pas necessaire MVP)
- `zoomOnScroll={true}` — scroll pour zoomer
- `minZoom={0.2}` — empeche le zoom trop eloigne
- `maxZoom={2}` — empeche le zoom trop rapproche
- `defaultViewport={{ x: 0, y: 0, zoom: 1 }}` — vue initiale
- Curseur : `grab` par defaut, `grabbing` pendant le drag

---

## 5. Responsive & Contraintes

### 5.1 Largeur minimale

**Breakpoint minimum : 1280px**

L'application est concue pour un usage desktop. En dessous de 1280px :
- Afficher un message centre : "FlowScope necessite un ecran d'au moins 1280px de large."
- `min-width: 1280px` sur le body pour empecher le layout de casser

### 5.2 Comportement avec l'inspecteur ouvert

| Configuration | Sidebar | Canvas | Inspector | Total min |
|--------------|---------|--------|-----------|-----------|
| Inspector ferme | 240px | flex-grow (min 1040px) | 0px | 1280px |
| Inspector ouvert | 240px | flex-grow (min 720px) | 320px | 1280px |

Le canvas se comprime pour laisser place a l'inspecteur. React Flow gere nativement le resize du container — les nodes restent positionnes correctement.

### 5.3 MiniMap

| Propriete | Valeur |
|-----------|--------|
| Position | Bottom-right du canvas |
| Width | 180px |
| Height | 120px |
| Offset | 16px du bord droit, 16px du bord bas |
| Background | `fs-surface` (opacity 90%) |
| Border | 1px `fs-border` |
| Border radius | 8px |
| Node color | Fonction : retourne `nodeColor` selon le type |
| Mask color | `fs-bg` a 80% opacite |
| Z-index | 5 (sous la toolbar et le breadcrumb) |

---

## 6. Checklist Accessibilite

### 6.1 Focus visible

| Exigence | Implementation |
|----------|---------------|
| Tous les elements interactifs ont un focus visible | `outline: 2px solid #58a6ff`, `outline-offset: 2px` |
| Le focus ring n'est pas supprime par `outline: none` | Reset CSS preserve le focus outline |
| Focus visible uniquement au clavier (pas au clic) | Utiliser `:focus-visible` (pas `:focus`) |
| Ordre de tabulation logique | Sidebar items → Breadcrumb → Toolbar → Canvas nodes |

### 6.2 Navigation clavier complete

| Zone | Navigation | Action |
|------|-----------|--------|
| Sidebar | `Tab` entre items, `Enter`/`Space` pour selectionner | Change la vue active |
| Canvas | `Tab` entre nodes (ordre du DOM), `Enter` pour selectionner | Ouvre l'inspecteur |
| Canvas (Overview) | `Enter` sur un OverviewNode | Navigue vers le systeme |
| Toolbar | `Tab` entre boutons, `Enter`/`Space` pour activer | Declenche l'action |
| Inspecteur | `Tab` dans les connexions, `Enter` pour naviguer | Centre le node cible |
| Search modal | `ArrowUp`/`ArrowDown` pour naviguer, `Enter` pour selectionner | Navigation + selection |
| Global | `Escape` ferme le panneau/modale actif | Fermeture |
| Global | `?` affiche l'aide raccourcis | Overlay |

### 6.3 Contrastes WCAG AA

| Combinaison | Ratio | Exigence | Statut |
|-------------|-------|----------|--------|
| `fs-text` (#e6edf3) sur `fs-bg` (#0d1117) | 13.2:1 | 4.5:1 (texte normal) | CONFORME |
| `fs-text` (#e6edf3) sur `fs-surface` (#161b22) | 10.8:1 | 4.5:1 | CONFORME |
| `fs-text-secondary` (#8b949e) sur `fs-bg` (#0d1117) | 5.3:1 | 4.5:1 | CONFORME |
| `fs-text-secondary` (#8b949e) sur `fs-surface` (#161b22) | 4.5:1 | 4.5:1 | CONFORME (limite) |
| `fs-text-muted` (#6e7681) sur `fs-bg` (#0d1117) | 3.6:1 | 3:1 (elements larges) | CONFORME (grands elements seulement) |
| `fs-accent` (#58a6ff) sur `fs-bg` (#0d1117) | 6.4:1 | 4.5:1 | CONFORME |
| `fs-accent` (#58a6ff) sur `fs-surface` (#161b22) | 5.3:1 | 4.5:1 | CONFORME |
| `fs-node-process` (#d29922) sur `fs-surface` (#161b22) | 5.6:1 | 4.5:1 | CONFORME |
| `fs-node-stock` (#2ea043) sur `fs-surface` (#161b22) | 4.2:1 | 3:1 (badge = grand element) | CONFORME |

**Attention :** `fs-text-muted` ne doit etre utilise que pour des elements non essentiels et de taille >= 18px ou 14px bold (elements larges WCAG). Pour du texte informatif de taille normale, utiliser `fs-text-secondary`.

### 6.4 ARIA labels

| Composant | Attribut | Valeur |
|-----------|----------|--------|
| CustomNode | `aria-label` | `"{label} — {type}"` |
| OverviewNode | `aria-label` | `"Ouvrir {systemLabel} — {nodeCount} elements"` |
| SystemItem (sidebar) | `aria-label` | `"Naviguer vers {systemLabel}"` |
| SystemItem actif | `aria-current` | `"page"` |
| IconButton (toolbar) | `aria-label` | Texte du tooltip (ex: "Zoom avant") |
| Bouton fermer inspecteur | `aria-label` | `"Fermer l'inspecteur"` |
| Input recherche | `aria-label` | `"Rechercher un node dans tous les systemes"` |
| SearchResult | `aria-label` | `"{nodeLabel} dans {systemLabel}"` |
| Breadcrumb segment | `aria-label` | `"Retour a {segmentLabel}"` |
| Breadcrumb container | `aria-label` | `"Fil d'Ariane"` + `role="navigation"` |
| Search modal | `role` | `"dialog"` + `aria-modal="true"` + `aria-label="Recherche globale"` |
| Shortcuts overlay | `role` | `"dialog"` + `aria-modal="true"` + `aria-label="Raccourcis clavier"` |

### 6.5 Regles supplementaires

- **Pas de contenu uniquement visuel sans alternative textuelle.** Chaque icone accompagnee d'un `aria-label` ou d'un texte visible.
- **Pas de piege clavier.** `Escape` ferme toujours la modale/panneau en cours. `Tab` ne reste jamais bloque dans un composant.
- **Animations reduites.** Respecter `prefers-reduced-motion: reduce` : desactiver les transitions > 150ms, l'animation des edges `flow`, et le scale hover sur les OverviewNodes.

```css
@media (prefers-reduced-motion: reduce) {
  * {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}
```

---

## Annexe A — Tokens supplementaires pour `tailwind.config.ts`

Les tokens suivants completent ceux definis dans l'ARCH_FLOWSCOPE.md :

```typescript
// A ajouter dans theme.extend.colors.fs
"surface-active": "#282e36",
"accent-subtle": "rgba(88, 166, 255, 0.1)",
"danger": "#f85149",
"success": "#3fb950",
"text-muted": "#6e7681",
"border-focus": "#58a6ff",
```

---

## Annexe B — Resume des lois ergonomiques appliquees

| Loi | Application dans FlowScope |
|-----|---------------------------|
| **Fitts** | Sidebar large (240px) avec items de 40px. Toolbar en position fixe, boutons 32x32px. Bouton fermer inspecteur en haut a droite (coin previsible). |
| **Hick-Hyman** | Max 5 systemes dans la sidebar. Max 5 boutons dans la toolbar. Max 10 resultats dans la recherche. Pas de menus imbriques. |
| **Miller** | Les sections de l'inspecteur sont separees visuellement. Les metadata sont groupees par type (colonnes, methodes, proprietes). |
| **Tesler** | L'auto-layout Dagre absorbe la complexite de positionnement. La recherche fuzzy tolere les fautes. Le breadcrumb empeche de se perdre. |
| **Jakob** | Interface inspiree de VS Code / GitHub : sidebar a gauche, panneau a droite, command palette Ctrl+K — patterns connus des developpeurs. |
| **Gestalt — Proximite** | Les metadata d'un node sont groupees dans l'inspecteur. Les connexions sont dans une section distincte. |
| **Gestalt — Similarite** | Tous les nodes du meme type ont la meme couleur. Tous les edges du meme type ont le meme style. |
| **Gestalt — Continuite** | Le breadcrumb suit un axe horizontal continu. Les edges suivent des courbes Bezier fluides. |
| **Gestalt — Fermeture** | Les nodes ont des bordures et backgrounds qui delimitent clairement leur contenu. |
| **Gestalt — Figure/fond** | Contraste 13.2:1 pour le texte principal. Canvas sombre avec nodes plus clairs qui "emergent". |
| **Nielsen #1** | Breadcrumb affiche la position actuelle. Sidebar met en evidence le systeme actif. Node selectionne a un glow visible. |
| **Nielsen #6** | Les types de nodes sont reconnaissables par icone + couleur + badge (reconnaissance > rappel). |
| **Nielsen #7** | Raccourcis clavier pour les utilisateurs avances (1-9, Ctrl+K, F, Escape). |
| **Nielsen #8** | Interface minimaliste : pas de decoration superflue, chaque pixel sert un objectif. |

---

*Document produit par Agent #3 — UI/UX Designer. Pret pour consommation par les agents #4 (Parsers) et #5 (Frontend).*
