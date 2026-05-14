# Frontend — FlowScope

> **Agent #5 — Frontend Developer**
> **Date :** 2026-05-14
> **Version :** 1.0 — MVP
> **Documents sources :** `docs/PO_FLOWSCOPE.md` v1.0, `docs/ARCH_FLOWSCOPE.md` v1.0, `docs/UIUX_FLOWSCOPE.md` v1.0
> **Destinataire :** mentalyas (implementation)

---

## Table des matieres

1. [Setup & Configuration](#1-setup--configuration)
2. [Composants — Specifications d'implementation](#2-composants--specifications-dimplementation)
3. [Hooks Custom](#3-hooks-custom)
4. [State Management](#4-state-management)
5. [Integration React Flow](#5-integration-react-flow)
6. [Styles & Theme](#6-styles--theme)
7. [Sprint-by-Sprint Implementation Guide](#7-sprint-by-sprint-implementation-guide)

---

## 1. Setup & Configuration

### 1.1 Initialisation du projet

```bash
# 1. Creer le projet Vite + React + TypeScript
npm create vite@latest flowscope -- --template react-ts

# 2. Entrer dans le dossier
cd flowscope

# 3. Installer les dependances principales
npm install @xyflow/react @dagrejs/dagre lucide-react fuse.js

# 4. Installer Tailwind CSS v3
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p --ts

# 5. Installer les outils de parsing (dev)
npm install -D tsx @types/node

# 6. Installer les types Dagre (si necessaire — voir note ci-dessous)
# NOTE : @dagrejs/dagre v1+ inclut ses propres types TypeScript.
# Verifier a l'installation si @types/dagre est necessaire.
# Si @dagrejs/dagre expose ses types nativement, ne pas installer @types/dagre.
npm install -D @types/dagre  # Conditionnel — tester d'abord sans
```

### 1.2 Dependances exactes

| Package | Version | Categorie | Role |
|---------|---------|-----------|------|
| `react` | `^18.3` | prod | Framework UI |
| `react-dom` | `^18.3` | prod | Rendu DOM |
| `@xyflow/react` | `^12` | prod | Moteur de graphe interactif |
| `lucide-react` | `^0.400` | prod | Icones SVG |
| `fuse.js` | `^7` | prod | Recherche fuzzy client-side |
| `@dagrejs/dagre` | `^1` | prod | Auto-layout de graphes diriges |
| `tailwindcss` | `^3.4` | dev | Framework CSS utility-first |
| `postcss` | `^8` | dev | Plugin pipeline CSS |
| `autoprefixer` | `^10` | dev | Prefixes CSS automatiques |
| `typescript` | `^5.5` | dev | Typage statique |
| `vite` | `^5` | dev | Bundler + dev server |
| `tsx` | `^4` | dev | Execution TypeScript CLI (parsers) |
| `@types/react` | `^18` | dev | Types React |
| `@types/react-dom` | `^18` | dev | Types React DOM |
| `@types/node` | `^20` | dev | Types Node.js (parsers) |

### 1.3 Configuration `tsconfig.json`

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "useDefineForClassFields": true,
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,

    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "isolatedModules": true,
    "moduleDetection": "force",
    "noEmit": true,
    "jsx": "react-jsx",

    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,

    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"]
    },
    "resolveJsonModule": true
  },
  "include": ["src"]
}
```

Points cles :
- `strict: true` — pas de `any`, pas de type implicite
- `paths` avec alias `@/` vers `src/`
- `resolveJsonModule: true` — import statique des fichiers JSON

### 1.4 Configuration `tsconfig.node.json`

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "module": "ESNext",
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "strict": true,
    "noEmit": true,
    "resolveJsonModule": true
  },
  "include": ["vite.config.ts", "tailwind.config.ts"]
}
```

### 1.5 Configuration `vite.config.ts`

```typescript
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { resolve } from "path";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": resolve(__dirname, "src"),
    },
  },
});
```

### 1.6 Configuration `tailwind.config.ts`

Configuration complete integrant tous les tokens du Design System (UIUX doc) :

```typescript
import type { Config } from "tailwindcss";

const config: Config = {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      fontFamily: {
        sans: [
          "Inter",
          "-apple-system",
          "BlinkMacSystemFont",
          '"Segoe UI"',
          "sans-serif",
        ],
        mono: [
          '"JetBrains Mono"',
          '"Fira Code"',
          '"Cascadia Code"',
          "Consolas",
          "monospace",
        ],
      },
      colors: {
        fs: {
          // Surfaces
          bg: "#0d1117",
          surface: "#161b22",
          "surface-hover": "#1c2129",
          "surface-active": "#282e36",
          // Bordures
          border: "#30363d",
          "border-focus": "#58a6ff",
          // Texte
          text: "#e6edf3",
          "text-secondary": "#8b949e",
          "text-muted": "#6e7681",
          // Interactions
          accent: "#58a6ff",
          "accent-hover": "#79c0ff",
          "accent-subtle": "rgba(88, 166, 255, 0.1)",
          danger: "#f85149",
          success: "#3fb950",
          // Couleurs par NodeType
          "node-table": "#58a6ff",
          "node-form": "#bc8cff",
          "node-dal": "#f0883e",
          "node-model": "#3fb950",
          "node-controller": "#f85149",
          "node-route": "#79c0ff",
          "node-view": "#f778ba",
          "node-process": "#d29922",
          "node-stock": "#2ea043",
          "node-custom": "#8b949e",
          // Couleurs par EdgeType
          "edge-fk": "#58a6ff",
          "edge-inheritance": "#bc8cff",
          "edge-dependency": "#8b949e",
          "edge-flow": "#3fb950",
          "edge-data": "#f0883e",
        },
      },
      width: {
        sidebar: "240px",
        inspector: "320px",
      },
      spacing: {
        sidebar: "240px",
        inspector: "320px",
      },
      animation: {
        "slide-in": "slideIn 250ms ease-out",
        "slide-out": "slideOut 200ms ease-in",
        "fade-in": "fadeIn 150ms ease-out",
        "fade-out": "fadeOut 150ms ease-in",
        "modal-in": "modalIn 200ms ease-out",
      },
      keyframes: {
        slideIn: {
          from: { transform: "translateX(100%)" },
          to: { transform: "translateX(0)" },
        },
        slideOut: {
          from: { transform: "translateX(0)" },
          to: { transform: "translateX(100%)" },
        },
        fadeIn: {
          from: { opacity: "0" },
          to: { opacity: "1" },
        },
        fadeOut: {
          from: { opacity: "1" },
          to: { opacity: "0" },
        },
        modalIn: {
          from: { opacity: "0", transform: "translateX(-50%) translateY(-8px)" },
          to: { opacity: "1", transform: "translateX(-50%) translateY(0)" },
        },
      },
    },
  },
  plugins: [],
};

export default config;
```

### 1.7 Configuration `postcss.config.js`

```js
export default {
  plugins: {
    tailwindcss: {},
    autoprefixer: {},
  },
};
```

### 1.8 Scripts npm (`package.json`)

```json
{
  "scripts": {
    "dev": "vite",
    "build": "tsc -b && vite build",
    "preview": "vite preview",
    "parse:sql": "tsx src/parsers/sql-parser.ts ../sql",
    "parse:csharp": "tsx src/parsers/csharp-parser.ts ../app-csharp/CharlesNadejda/CharlesNadejda",
    "parse:assemble": "tsx src/parsers/assemble.ts",
    "parse:all": "npm run parse:sql && npm run parse:csharp && npm run parse:assemble"
  }
}
```

### 1.9 Structure de dossiers finale

```
flowscope/
├── public/
├── output/                          # Sortie des parsers (gitignored)
│   ├── db-system.json
│   ├── csharp-system.json
│   └── overrides.json
├── src/
│   ├── components/
│   │   ├── canvas/
│   │   │   ├── FlowCanvas.tsx       # Wrapper ReactFlow + provider + events
│   │   │   ├── CustomNode.tsx       # Node richement type (tous NodeTypes)
│   │   │   ├── CustomEdge.tsx       # Edge avec rendu visuel par EdgeType
│   │   │   ├── OverviewNode.tsx     # Node gros bloc pour vue Overview
│   │   │   ├── Toolbar.tsx          # Barre d'outils overlay canvas
│   │   │   ├── Breadcrumb.tsx       # Fil d'Ariane de navigation
│   │   │   └── MiniMapWrapper.tsx   # MiniMap configuree dark theme
│   │   ├── inspector/
│   │   │   ├── InspectorPanel.tsx   # Panneau lateral principal
│   │   │   ├── MetadataSection.tsx  # Affichage des metadata (colonnes, methodes...)
│   │   │   ├── ConnectionsList.tsx  # Liste des nodes connectes (entrants/sortants)
│   │   │   └── TagBadge.tsx         # Badge de tag individuel
│   │   ├── search/
│   │   │   ├── SearchModal.tsx      # Modale command palette Ctrl+K
│   │   │   └── SearchResult.tsx     # Ligne de resultat individuelle
│   │   ├── sidebar/
│   │   │   ├── Sidebar.tsx          # Conteneur sidebar complet
│   │   │   ├── SystemItem.tsx       # Ligne de systeme cliquable
│   │   │   └── SearchButton.tsx     # Bouton loupe simulant un input
│   │   └── ui/
│   │       ├── Badge.tsx            # Badge generique (type, tag)
│   │       ├── IconButton.tsx       # Bouton icone avec tooltip
│   │       └── KeyboardShortcuts.tsx # Overlay aide raccourcis (?)
│   ├── hooks/
│   │   ├── useFlowData.ts           # Chargement + acces aux donnees projet
│   │   ├── useNavigation.ts         # Navigation entre vues + breadcrumb + historique
│   │   ├── useSearch.ts             # Integration Fuse.js + index
│   │   ├── useLayout.ts            # Auto-layout Dagre
│   │   ├── useKeyboardShortcuts.ts  # Binding des raccourcis clavier
│   │   └── useInspector.ts          # Etat ouvert/ferme + node selectionne
│   ├── layouts/
│   │   └── AppLayout.tsx            # Shell : sidebar + canvas + inspector
│   ├── types/
│   │   ├── schema.ts               # Schema JSON universel (interfaces donnees)
│   │   ├── ui.ts                    # Types UI (navigation, inspector state)
│   │   └── index.ts                 # Reexport centralise
│   ├── utils/
│   │   ├── layout.ts               # Fonctions Dagre (calcul positions)
│   │   ├── colors.ts               # Palette couleurs par NodeType / EdgeType
│   │   ├── icons.ts                # Mapping NodeType -> icone Lucide
│   │   └── transform.ts            # FlowNode/FlowEdge -> React Flow Node/Edge
│   ├── data/
│   │   ├── project.json             # Donnees assemblees (genere par assemble.ts)
│   │   └── example-project.json     # Donnees d'exemple pour developpement
│   ├── parsers/
│   │   ├── types.ts                 # Reexport des types schema pour les parsers
│   │   ├── sql-parser.ts
│   │   ├── csharp-parser.ts
│   │   └── assemble.ts
│   ├── App.tsx                      # Point d'entree React
│   ├── main.tsx                     # Montage React DOM
│   └── index.css                    # Styles globaux Tailwind + CSS variables
├── index.html
├── package.json
├── tsconfig.json
├── tsconfig.node.json
├── tailwind.config.ts
├── postcss.config.js
├── vite.config.ts
└── .gitignore
```

---

## 2. Composants — Specifications d'implementation

### 2.1 `App.tsx` — Point d'entree

**Chemin :** `src/App.tsx`

```typescript
// Pas de props — composant racine
function App() {
  return <AppLayout />;
}

export default App;
```

**Responsabilites :**
- Importe et rend `AppLayout`
- Aucun state ici — tout le state est gere dans `AppLayout` via les hooks

**Notes :**
- Pas de `<ReactFlowProvider>` ici — il est dans `FlowCanvas`
- Pas de theme provider — le dark theme est applique via Tailwind/CSS global

---

### 2.2 `AppLayout.tsx` — Shell de l'application

**Chemin :** `src/layouts/AppLayout.tsx`

**Props :** Aucune

**Interface de state :**

```typescript
// Pas d'interface exportee — state interne uniquement
```

**State interne :**
- `project` via `useFlowData()` — donnees du projet
- `{ activeSystemId, navigateTo, goBack, breadcrumbs }` via `useNavigation(project.systems)`
- `{ isOpen, selectedNode, openInspector, closeInspector }` via `useInspector()`
- `{ isSearchOpen, openSearch, closeSearch }` — `useState<boolean>(false)`
- `{ isShortcutsOpen }` — `useState<boolean>(false)`

**Hooks utilises :**
- `useFlowData()` — charge les donnees
- `useNavigation(project.systems)` — gestion de la vue active
- `useInspector()` — panneau inspecteur
- `useKeyboardShortcuts()` — raccourcis globaux

**Logique de rendu :**

```tsx
function AppLayout() {
  const { project, getSystem } = useFlowData();
  const { activeSystemId, navigateTo, goBack, breadcrumbs } = useNavigation(project.systems);
  const { isOpen: isInspectorOpen, selectedNode, openInspector, closeInspector } = useInspector();
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const [isShortcutsOpen, setIsShortcutsOpen] = useState(false);

  const activeSystem = getSystem(activeSystemId);

  useKeyboardShortcuts({
    onSearch: () => setIsSearchOpen(true),
    onEscape: () => { closeInspector(); setIsSearchOpen(false); setIsShortcutsOpen(false); },
    onGoBack: goBack,
    onFitView: /* ref passee depuis FlowCanvas */,
    onNavigateToView: navigateTo,
    onShowShortcuts: () => setIsShortcutsOpen(true),
    systems: project.systems,
    isInputFocused: /* check document.activeElement */,
  });

  return (
    <div className="flex h-screen w-screen bg-fs-bg overflow-hidden" style={{ minWidth: 1280 }}>
      {/* Sidebar — fixe a gauche */}
      <Sidebar
        projectName={project.name}
        systems={project.systems}
        activeSystemId={activeSystemId}
        onSystemSelect={navigateTo}
        onSearchClick={() => setIsSearchOpen(true)}
      />

      {/* Zone Canvas — flex-grow */}
      <div className="flex-1 flex flex-col min-w-0">
        <Breadcrumb
          segments={breadcrumbs}
          onSegmentClick={(systemId) => navigateTo(systemId)}
        />
        {activeSystem && (
          <FlowCanvas
            system={activeSystem}
            onNodeClick={openInspector}
            onNodeDoubleClick={(node) => {
              if (activeSystemId === "overview" && node.metadata.systemRef) {
                navigateTo(node.metadata.systemRef as string);
              }
            }}
            onPaneClick={closeInspector}
            selectedNodeId={selectedNode?.id ?? null}
          />
        )}
      </div>

      {/* Inspector — conditionnel */}
      {isInspectorOpen && selectedNode && activeSystem && (
        <InspectorPanel
          node={selectedNode}
          system={activeSystem}
          onClose={closeInspector}
          onNodeNavigate={(nodeId) => {
            const targetNode = activeSystem.nodes.find(n => n.id === nodeId);
            if (targetNode) openInspector(targetNode);
          }}
        />
      )}

      {/* Modales */}
      {isSearchOpen && (
        <SearchModal
          project={project}
          onClose={() => setIsSearchOpen(false)}
          onResultSelect={(node, systemId) => {
            navigateTo(systemId);
            setIsSearchOpen(false);
            // Delai pour laisser le canvas charger avant de selectionner
            setTimeout(() => openInspector(node), 100);
          }}
        />
      )}

      {isShortcutsOpen && (
        <KeyboardShortcuts onClose={() => setIsShortcutsOpen(false)} />
      )}
    </div>
  );
}
```

**Classes Tailwind cles :**
- Container : `flex h-screen w-screen bg-fs-bg overflow-hidden`
- Zone canvas : `flex-1 flex flex-col min-w-0` (min-w-0 empeche le flex de deborder)

---

### 2.3 `Sidebar.tsx` — Navigation systemes

**Chemin :** `src/components/sidebar/Sidebar.tsx`

**Props :**

```typescript
interface SidebarProps {
  projectName: string;
  systems: SystemDefinition[];
  activeSystemId: string;
  onSystemSelect: (systemId: string) => void;
  onSearchClick: () => void;
}
```

**State interne :** Aucun

**Logique de rendu :**

```tsx
function Sidebar({ projectName, systems, activeSystemId, onSystemSelect, onSearchClick }: SidebarProps) {
  return (
    <aside className="w-sidebar h-screen flex flex-col bg-fs-surface border-r border-fs-border z-20 shrink-0">
      {/* Header projet */}
      <div className="p-4 border-b border-fs-border">
        <div className="flex items-center gap-2">
          <Diamond className="h-4 w-4 text-fs-accent" />
          <span className="text-base font-semibold text-fs-text">FlowScope</span>
        </div>
        <p className="text-xs text-fs-text-secondary mt-1">{projectName}</p>
      </div>

      {/* Bouton recherche */}
      <div className="px-4 py-2">
        <SearchButton onClick={onSearchClick} />
      </div>

      {/* Liste des systemes */}
      <div className="px-2 pt-4 pb-2">
        <span className="px-2 text-[11px] font-semibold uppercase tracking-wider text-fs-text-muted">
          Systemes
        </span>
      </div>
      <nav className="flex-1 px-2 space-y-0.5">
        {systems.map((system) => (
          <SystemItem
            key={system.id}
            system={system}
            isActive={system.id === activeSystemId}
            onClick={() => onSystemSelect(system.id)}
          />
        ))}
      </nav>
    </aside>
  );
}

export default memo(Sidebar);
```

**Refs UI/UX :** Section 3.3 — sidebar width 240px, height 100vh, bg-fs-surface, border-right 1px fs-border. Header avec icone Diamond, separateur. Section SYSTEMES en 11px uppercase.

---

### 2.4 `SystemItem.tsx`

**Chemin :** `src/components/sidebar/SystemItem.tsx`

**Props :**

```typescript
interface SystemItemProps {
  system: SystemDefinition;
  isActive: boolean;
  onClick: () => void;
}
```

**Logique :**

```tsx
import { NODE_ICONS } from "@/utils/icons";
// Le systeme "overview" utilise LayoutDashboard, les autres utilisent
// l'icone definie dans leur champ `icon` du JSON.

function SystemItem({ system, isActive, onClick }: SystemItemProps) {
  // Resoudre l'icone : si c'est un nom Lucide connu, utiliser le mapping
  const Icon = resolveIcon(system.icon);

  return (
    <button
      onClick={onClick}
      aria-label={`Naviguer vers ${system.label}`}
      aria-current={isActive ? "page" : undefined}
      className={`
        flex items-center gap-3 w-full h-10 px-3 rounded-md text-sm transition-colors duration-150
        ${isActive
          ? "bg-fs-surface-active text-fs-text border-l-2 border-fs-accent"
          : "text-fs-text-secondary hover:bg-fs-surface-hover hover:text-fs-text"
        }
      `}
    >
      <Icon className={`h-4 w-4 shrink-0 ${isActive ? "text-fs-accent" : "text-fs-text-muted"}`} />
      <span className="truncate">{system.label}</span>
    </button>
  );
}

export default memo(SystemItem);
```

**Fonction helper `resolveIcon` :** Definie dans `src/utils/icons.ts`. Prend un nom d'icone string (depuis le JSON, ex: `"Database"`) et retourne le composant Lucide correspondant. Fallback sur `HelpCircle` si le nom est inconnu.

```typescript
// src/utils/icons.ts (ajout)
import {
  Database, Monitor, Layers, Box, Globe,
  Route, Layout, GitBranch, Package, HelpCircle,
  LayoutDashboard,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";

const ICON_MAP: Record<string, LucideIcon> = {
  Database, Monitor, Layers, Box, Globe,
  Route, Layout, GitBranch, Package, HelpCircle,
  LayoutDashboard,
};

export function resolveIcon(name: string): LucideIcon {
  return ICON_MAP[name] ?? HelpCircle;
}
```

---

### 2.5 `SearchButton.tsx`

**Chemin :** `src/components/sidebar/SearchButton.tsx`

**Props :**

```typescript
interface SearchButtonProps {
  onClick: () => void;
}
```

**Rendu :** Simule un input de recherche. Bouton pleine largeur, hauteur 36px, affiche icone Search + texte "Rechercher" + badge "Ctrl+K" a droite.

```tsx
function SearchButton({ onClick }: SearchButtonProps) {
  return (
    <button
      onClick={onClick}
      className="flex items-center w-full h-9 px-3 rounded-lg bg-fs-bg border border-fs-border
                 text-fs-text-muted text-sm hover:border-fs-accent transition-colors duration-150"
    >
      <Search className="h-4 w-4 shrink-0" />
      <span className="ml-2 flex-1 text-left">Rechercher</span>
      <kbd className="text-[11px] bg-fs-surface px-1.5 py-0.5 rounded text-fs-text-muted">
        Ctrl+K
      </kbd>
    </button>
  );
}

export default memo(SearchButton);
```

---

### 2.6 `Breadcrumb.tsx` — Fil d'Ariane

**Chemin :** `src/components/canvas/Breadcrumb.tsx`

**Props :**

```typescript
interface BreadcrumbSegment {
  id: string;
  label: string;
}

interface BreadcrumbProps {
  segments: BreadcrumbSegment[];
  onSegmentClick: (systemId: string) => void;
}
```

**Logique :**

```tsx
function Breadcrumb({ segments, onSegmentClick }: BreadcrumbProps) {
  return (
    <nav
      aria-label="Fil d'Ariane"
      role="navigation"
      className="h-10 flex items-center px-4 border-b border-fs-border bg-fs-bg z-10 shrink-0"
    >
      {segments.map((segment, index) => {
        const isLast = index === segments.length - 1;
        return (
          <Fragment key={segment.id}>
            {index > 0 && (
              <ChevronRight className="h-3.5 w-3.5 text-fs-text-muted mx-1 shrink-0" />
            )}
            {isLast ? (
              <span className="text-[13px] font-semibold text-fs-text">
                {segment.label}
              </span>
            ) : (
              <button
                onClick={() => onSegmentClick(segment.id)}
                aria-label={`Retour a ${segment.label}`}
                className="text-[13px] text-fs-accent hover:text-fs-accent-hover transition-colors
                           duration-150 focus-visible:outline focus-visible:outline-2
                           focus-visible:outline-fs-accent focus-visible:outline-offset-2"
              >
                {segment.label}
              </button>
            )}
          </Fragment>
        );
      })}
    </nav>
  );
}

export default memo(Breadcrumb);
```

**Refs UI/UX :** Section 3.4 — height 40px, border-bottom, segments 13px, separateur ChevronRight, derniere segment en semibold non cliquable.

---

### 2.7 `FlowCanvas.tsx` — Wrapper React Flow

**Chemin :** `src/components/canvas/FlowCanvas.tsx`

**Props :**

```typescript
interface FlowCanvasProps {
  system: SystemDefinition;
  onNodeClick: (node: FlowNode) => void;
  onNodeDoubleClick: (node: FlowNode) => void;
  onPaneClick: () => void;
  selectedNodeId: string | null;
}
```

**State interne :**
- `rfInstance` — `useState<ReactFlowInstance | null>(null)` pour `fitView()` programmatique
- Nodes et edges React Flow calcules via `useMemo`

**Hooks utilises :**
- `useLayout(system.nodes, system.edges, system.layoutDirection)` — calcul auto-layout
- `useMemo` — transformation FlowNode[] vers React Flow Node[]

**Constants definies hors du composant (V3 — ARCH vigilance) :**

```typescript
// HORS du composant — ne jamais redefinir a chaque render
const nodeTypes = {
  custom: CustomNode,
  overview: OverviewNode,
};

const edgeTypes = {
  custom: CustomEdge,
};
```

**Logique de rendu :**

```tsx
function FlowCanvas({ system, onNodeClick, onNodeDoubleClick, onPaneClick, selectedNodeId }: FlowCanvasProps) {
  const [rfInstance, setRfInstance] = useState<ReactFlowInstance | null>(null);
  const layoutDirection = system.layoutDirection ?? "TB";

  // Auto-layout Dagre
  const layoutNodes = useLayout(system.nodes, system.edges, layoutDirection);

  // Transformer vers format React Flow
  const rfNodes = useMemo(
    () => transformNodes(layoutNodes, system.id === "overview", selectedNodeId),
    [layoutNodes, system.id, selectedNodeId]
  );
  const rfEdges = useMemo(
    () => transformEdges(system.edges),
    [system.edges]
  );

  // Fit view a chaque changement de systeme
  useEffect(() => {
    if (rfInstance) {
      rfInstance.fitView({ duration: 300, padding: 0.1 });
    }
  }, [system.id, rfInstance]);

  const handleNodeClick = useCallback(
    (_event: React.MouseEvent, rfNode: Node) => {
      const flowNode = system.nodes.find((n) => n.id === rfNode.id);
      if (flowNode) onNodeClick(flowNode);
    },
    [system.nodes, onNodeClick]
  );

  const handleNodeDoubleClick = useCallback(
    (_event: React.MouseEvent, rfNode: Node) => {
      const flowNode = system.nodes.find((n) => n.id === rfNode.id);
      if (flowNode) onNodeDoubleClick(flowNode);
    },
    [system.nodes, onNodeDoubleClick]
  );

  const handlePaneClick = useCallback(() => {
    onPaneClick();
  }, [onPaneClick]);

  return (
    <div className="flex-1 relative">
      <ReactFlowProvider>
        <ReactFlow
          nodes={rfNodes}
          edges={rfEdges}
          nodeTypes={nodeTypes}
          edgeTypes={edgeTypes}
          onNodeClick={handleNodeClick}
          onNodeDoubleClick={handleNodeDoubleClick}
          onPaneClick={handlePaneClick}
          onInit={setRfInstance}
          fitView
          fitViewOptions={{ padding: 0.1 }}
          minZoom={0.2}
          maxZoom={2}
          panOnDrag
          selectionOnDrag={false}
          zoomOnScroll
          defaultViewport={{ x: 0, y: 0, zoom: 1 }}
          proOptions={{ hideAttribution: true }}
          className="bg-fs-bg"
        >
          <Background
            variant={BackgroundVariant.Dots}
            gap={20}
            size={1}
            color="#30363d"
          />
          <MiniMapWrapper nodes={layoutNodes} />
          <Toolbar
            onZoomIn={() => rfInstance?.zoomIn({ duration: 200 })}
            onZoomOut={() => rfInstance?.zoomOut({ duration: 200 })}
            onFitView={() => rfInstance?.fitView({ duration: 300, padding: 0.1 })}
            onReLayout={() => { /* force re-layout via state toggle */ }}
            onSearch={() => { /* bubbled up via props or context */ }}
          />
        </ReactFlow>
      </ReactFlowProvider>
    </div>
  );
}

export default memo(FlowCanvas);
```

**Notes d'implementation :**
- `ReactFlowProvider` est interne a `FlowCanvas`, pas global (Arch A3)
- `nodeTypes` et `edgeTypes` sont des constantes hors composant (vigilance V3)
- Le `fitView` s'anime sur changement de `system.id` via `useEffect`
- `useCallback` sur tous les handlers pour eviter les re-renders (Arch A8)

---

### 2.8 `CustomNode.tsx` — Node richement type

**Chemin :** `src/components/canvas/CustomNode.tsx`

**Props :**

```typescript
import type { NodeProps } from "@xyflow/react";

// Les donnees passees dans `data` par React Flow
interface CustomNodeData {
  flowNode: FlowNode;
  isSelected: boolean;
  layoutDirection: LayoutDirection;
}

// React Flow passe NodeProps<CustomNodeData>
type CustomNodeProps = NodeProps<CustomNodeData>;
```

**Logique de rendu :**

```tsx
import { Handle, Position } from "@xyflow/react";
import { NODE_ICONS, resolveIcon } from "@/utils/icons";
import { NODE_BG_CLASSES, NODE_BORDER_COLORS } from "@/utils/colors";
import { getMetadataSummary } from "@/utils/transform";

function CustomNode({ data }: CustomNodeProps) {
  const { flowNode, isSelected, layoutDirection } = data;
  const { type, label, metadata } = flowNode;

  const Icon = NODE_ICONS[type];
  const borderColor = NODE_BORDER_COLORS[type];
  const badgeClasses = NODE_BG_CLASSES[type];
  const metaSummary = getMetadataSummary(type, metadata);

  // Positions des handles selon la direction du layout
  const sourcePos = layoutDirection === "LR" ? Position.Right : Position.Bottom;
  const targetPos = layoutDirection === "LR" ? Position.Left : Position.Top;

  return (
    <div
      role="button"
      aria-label={`${label} — ${type}`}
      tabIndex={0}
      className={`
        group min-w-[180px] max-w-[280px] rounded-lg border bg-fs-surface
        transition-all duration-150 ease-out cursor-pointer
        hover:bg-fs-surface-hover hover:shadow-lg
        focus-visible:outline focus-visible:outline-2
        focus-visible:outline-fs-accent focus-visible:outline-offset-2
        ${isSelected ? "border-current shadow-[0_0_0_2px_rgba(var(--glow),0.25),0_0_16px_rgba(var(--glow),0.12)]" : "border-fs-border"}
      `}
      style={{
        borderLeftWidth: 3,
        borderLeftColor: borderColor,
        // Glow color pour selected state
        ["--glow" as string]: isSelected ? borderColor : "transparent",
        boxShadow: isSelected
          ? `0 0 0 2px ${borderColor}40, 0 0 16px ${borderColor}20`
          : undefined,
      }}
    >
      <div className="px-3 py-2 space-y-1">
        {/* Row 1 : Icone + Label */}
        <div className="flex items-center gap-2">
          <Icon className="h-4 w-4 shrink-0" style={{ color: borderColor }} />
          <span className="text-[13px] font-semibold text-fs-text truncate">{label}</span>
        </div>

        {/* Row 2 : Badge type */}
        <span className={`inline-block text-[10px] font-semibold uppercase tracking-wide px-1.5 py-0.5 rounded ${badgeClasses}`}>
          {type}
        </span>

        {/* Row 3 : Metadata summary (optionnel) */}
        {metaSummary && (
          <p className="text-[11px] text-fs-text-secondary truncate">{metaSummary}</p>
        )}
      </div>

      {/* Handles — invisibles par defaut, visibles au hover */}
      <Handle
        type="target"
        position={targetPos}
        className="!w-2 !h-2 !bg-transparent group-hover:!bg-current !border-0 opacity-0 group-hover:opacity-100 transition-opacity"
        style={{ color: borderColor }}
      />
      <Handle
        type="source"
        position={sourcePos}
        className="!w-2 !h-2 !bg-transparent group-hover:!bg-current !border-0 opacity-0 group-hover:opacity-100 transition-opacity"
        style={{ color: borderColor }}
      />
    </div>
  );
}

export default memo(CustomNode);
```

**Fonction `getMetadataSummary` :** (dans `src/utils/transform.ts`)

```typescript
export function getMetadataSummary(type: NodeType, metadata: NodeMetadata): string | null {
  switch (type) {
    case "table":
      return metadata.columns ? `${metadata.columns.length} cols` : null;
    case "form":
    case "dal":
      return metadata.methods ? `${metadata.methods.length} methods` : null;
    case "model":
      return metadata.properties ? `${metadata.properties.length} props` : null;
    case "process":
    case "stock":
      return metadata.role
        ? metadata.role.length > 30 ? metadata.role.slice(0, 30) + "..." : metadata.role
        : null;
    default: {
      const count = Object.keys(metadata).length;
      return count > 0 ? `${count} attrs` : null;
    }
  }
}
```

**Refs UI/UX :** Section 3.1 — min 180px / max 280px, border-left 3px nodeColor, padding 12px/8px, etats default/hover/selected/dragging. Handles invisibles sauf hover.

---

### 2.9 `OverviewNode.tsx` — Bloc vue Overview

**Chemin :** `src/components/canvas/OverviewNode.tsx`

**Props :**

```typescript
interface OverviewNodeData {
  flowNode: FlowNode;
  systemLabel: string;
  systemIcon: string;
  nodeCount: number;
  description: string;
}

type OverviewNodeProps = NodeProps<OverviewNodeData>;
```

**Logique de rendu :**

```tsx
function OverviewNode({ data }: OverviewNodeProps) {
  const { systemLabel, systemIcon, nodeCount, description } = data;
  const Icon = resolveIcon(systemIcon);

  return (
    <div
      role="button"
      aria-label={`Ouvrir ${systemLabel} — ${nodeCount} elements`}
      tabIndex={0}
      className="w-[240px] h-[128px] rounded-xl border border-fs-border bg-fs-surface p-5
                 transition-all duration-200 ease-out cursor-pointer
                 hover:bg-fs-surface-hover hover:border-fs-accent/40 hover:shadow-lg hover:scale-[1.02]
                 active:bg-fs-surface-active active:border-fs-accent
                 focus-visible:outline focus-visible:outline-2
                 focus-visible:outline-fs-accent focus-visible:outline-offset-2"
    >
      <Icon className="h-6 w-6 text-fs-accent mb-2" />
      <h3 className="text-base font-semibold text-fs-text leading-tight">{systemLabel}</h3>
      <p className="text-xs text-fs-text-secondary leading-normal mt-0.5 line-clamp-2">
        {description}
      </p>
      <p className="text-[13px] font-medium text-fs-accent mt-auto pt-1">
        {nodeCount > 0 ? `${nodeCount} elements` : "Placeholder"}
      </p>

      {/* Handles caches mais necessaires pour React Flow */}
      <Handle type="source" position={Position.Bottom} className="!opacity-0 !w-0 !h-0" />
      <Handle type="target" position={Position.Top} className="!opacity-0 !w-0 !h-0" />
    </div>
  );
}

export default memo(OverviewNode);
```

**Refs UI/UX :** Section 3.2 — 240x128px, rounded-xl, padding 20px, icone 24px, hover scale(1.02).

---

### 2.10 `CustomEdge.tsx` — Edge type visuellement

**Chemin :** `src/components/canvas/CustomEdge.tsx`

**Props :**

```typescript
import type { EdgeProps } from "@xyflow/react";

interface CustomEdgeData {
  edgeType: EdgeType;
}

type CustomEdgeProps = EdgeProps<CustomEdgeData>;
```

**Logique :**

Le composant utilise `getBezierPath` de React Flow pour calculer le chemin SVG, puis applique des styles differents selon `edgeType`.

```tsx
import { getBezierPath, EdgeLabelRenderer } from "@xyflow/react";
import { EDGE_COLORS } from "@/utils/colors";

/** Configuration de style par type d'edge */
const EDGE_STYLES: Record<EdgeType, {
  strokeDasharray?: string;
  strokeWidth: number;
  markerEnd: string;
  showLabel: boolean;
}> = {
  fk:          { strokeWidth: 1.5, markerEnd: "arrowclosed", showLabel: true },
  inheritance: { strokeDasharray: "6 3", strokeWidth: 1.5, markerEnd: "arrow", showLabel: false },
  dependency:  { strokeWidth: 1.5, markerEnd: "arrowclosed", showLabel: false },
  flow:        { strokeWidth: 2, markerEnd: "arrowclosed", showLabel: false },
  data:        { strokeDasharray: "4 6", strokeWidth: 1.5, markerEnd: "arrowclosed", showLabel: true },
};

function CustomEdge({
  id, sourceX, sourceY, targetX, targetY,
  sourcePosition, targetPosition, label, data, selected,
}: CustomEdgeProps) {
  const edgeType = data?.edgeType ?? "dependency";
  const style = EDGE_STYLES[edgeType];
  const color = EDGE_COLORS[edgeType];

  const [edgePath, labelX, labelY] = getBezierPath({
    sourceX, sourceY, targetX, targetY,
    sourcePosition, targetPosition,
  });

  const isHovered = false; // Gere via CSS :hover sur le groupe SVG
  const opacity = selected ? 1 : 0.6;

  return (
    <>
      <path
        id={id}
        d={edgePath}
        stroke={color}
        strokeWidth={selected ? style.strokeWidth + 1 : style.strokeWidth}
        strokeDasharray={style.strokeDasharray}
        fill="none"
        opacity={opacity}
        className="transition-all duration-150 hover:opacity-100"
        style={{
          animation: edgeType === "flow" ? "dashmove 1s linear infinite" : undefined,
        }}
      />
      {/* Path transparent plus large pour faciliter le hover */}
      <path
        d={edgePath}
        stroke="transparent"
        strokeWidth={20}
        fill="none"
      />
      {/* Label */}
      {label && (style.showLabel || selected) && (
        <EdgeLabelRenderer>
          <div
            className="absolute text-[11px] text-fs-text-secondary bg-fs-surface/90 px-1.5 py-0.5
                       rounded pointer-events-none"
            style={{
              transform: `translate(-50%, -50%) translate(${labelX}px, ${labelY}px)`,
            }}
          >
            {label}
          </div>
        </EdgeLabelRenderer>
      )}
    </>
  );
}

export default memo(CustomEdge);
```

**CSS requis (dans `index.css`) pour l'animation flow :**

```css
@keyframes dashmove {
  to {
    stroke-dashoffset: -20;
  }
}
```

**Refs UI/UX :** Section 3.9 — 5 types visuels, opacite 0.6 defaut / 1.0 hover, stroke width +1px au hover/selected.

---

### 2.11 `Toolbar.tsx` — Barre d'outils canvas

**Chemin :** `src/components/canvas/Toolbar.tsx`

**Props :**

```typescript
interface ToolbarProps {
  onZoomIn: () => void;
  onZoomOut: () => void;
  onFitView: () => void;
  onReLayout: () => void;
  onSearch: () => void;
}
```

**Logique de rendu :**

```tsx
const TOOLBAR_BUTTONS = [
  { icon: ZoomIn, action: "onZoomIn" as const, label: "Zoom avant" },
  { icon: ZoomOut, action: "onZoomOut" as const, label: "Zoom arriere" },
  { icon: Maximize2, action: "onFitView" as const, label: "Ajuster la vue" },
  { icon: LayoutGrid, action: "onReLayout" as const, label: "Reorganiser" },
  { icon: Search, action: "onSearch" as const, label: "Rechercher (Ctrl+K)" },
] as const;

function Toolbar(props: ToolbarProps) {
  return (
    <div
      className="absolute top-2 right-4 z-10 flex gap-0.5 p-1 rounded-lg
                 bg-fs-surface/90 backdrop-blur-sm border border-fs-border shadow-lg"
    >
      {TOOLBAR_BUTTONS.map(({ icon, action, label }) => (
        <IconButton
          key={action}
          icon={icon}
          aria-label={label}
          onClick={props[action]}
          size={32}
          iconSize={18}
        />
      ))}
    </div>
  );
}

export default memo(Toolbar);
```

**Position :** `absolute top-2 right-4` — relatif au conteneur du canvas qui est `relative`.

**Refs UI/UX :** Section 3.7 — position absolute top-right, bg-fs-surface/90 + backdrop-blur, 5 boutons 32x32px.

---

### 2.12 `MiniMapWrapper.tsx`

**Chemin :** `src/components/canvas/MiniMapWrapper.tsx`

**Props :**

```typescript
interface MiniMapWrapperProps {
  nodes: FlowNode[];
}
```

**Logique :**

```tsx
import { MiniMap } from "@xyflow/react";
import { NODE_BORDER_COLORS } from "@/utils/colors";

function MiniMapWrapper({ nodes: _nodes }: MiniMapWrapperProps) {
  const nodeColor = useCallback((node: Node) => {
    const flowNode = node.data?.flowNode as FlowNode | undefined;
    if (flowNode) {
      return NODE_BORDER_COLORS[flowNode.type] ?? "#8b949e";
    }
    return "#8b949e";
  }, []);

  return (
    <MiniMap
      nodeColor={nodeColor}
      maskColor="rgba(13, 17, 23, 0.8)"
      style={{
        backgroundColor: "#161b22",
        border: "1px solid #30363d",
        borderRadius: 8,
        width: 180,
        height: 120,
      }}
      zoomable
      pannable
    />
  );
}

export default memo(MiniMapWrapper);
```

**Refs UI/UX :** Section 5.3 — 180x120px, bottom-right, bg fs-surface 90%, mask fs-bg 80%.

---

### 2.13 `InspectorPanel.tsx` — Panneau inspecteur lateral

**Chemin :** `src/components/inspector/InspectorPanel.tsx`

**Props :**

```typescript
interface InspectorPanelProps {
  node: FlowNode;
  system: SystemDefinition;
  onClose: () => void;
  onNodeNavigate: (nodeId: string) => void;
}
```

**State interne :**
- `copied` — `useState<boolean>(false)` pour le feedback copie du chemin fichier

**Logique de rendu :**

```tsx
function InspectorPanel({ node, system, onClose, onNodeNavigate }: InspectorPanelProps) {
  const [copied, setCopied] = useState(false);
  const Icon = NODE_ICONS[node.type];
  const borderColor = NODE_BORDER_COLORS[node.type];
  const badgeClasses = NODE_BG_CLASSES[node.type];

  // Calculer les connexions entrantes et sortantes
  const outgoing = useMemo(() =>
    system.edges
      .filter((e) => e.source === node.id)
      .map((e) => ({
        edge: e,
        targetNode: system.nodes.find((n) => n.id === e.target),
      }))
      .filter((c) => c.targetNode !== undefined),
    [system.edges, system.nodes, node.id]
  );

  const incoming = useMemo(() =>
    system.edges
      .filter((e) => e.target === node.id)
      .map((e) => ({
        edge: e,
        sourceNode: system.nodes.find((n) => n.id === e.source),
      }))
      .filter((c) => c.sourceNode !== undefined),
    [system.edges, system.nodes, node.id]
  );

  const handleCopyPath = async () => {
    if (node.filePath) {
      await navigator.clipboard.writeText(node.filePath);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  return (
    <aside
      className="w-inspector h-screen bg-fs-surface border-l border-fs-border z-20 shrink-0
                 overflow-y-auto animate-slide-in inspector-panel"
    >
      {/* Header */}
      <div className="relative px-6 py-4 border-b border-fs-border">
        <button
          onClick={onClose}
          aria-label="Fermer l'inspecteur"
          className="absolute top-3 right-3 w-8 h-8 flex items-center justify-center rounded-md
                     text-fs-text-secondary hover:bg-fs-surface-hover hover:text-fs-text
                     transition-colors duration-150"
        >
          <X className="h-4 w-4" />
        </button>
        <Icon className="h-5 w-5 mb-2" style={{ color: borderColor }} />
        <h2 className="text-base font-semibold text-fs-text">{node.label}</h2>
        <span className={`inline-block mt-1 text-[10px] font-semibold uppercase tracking-wide
                          px-1.5 py-0.5 rounded ${badgeClasses}`}>
          {node.type}
        </span>
      </div>

      {/* Description */}
      {node.description && (
        <div className="px-6 py-5 border-b border-fs-border">
          <h3 className="text-xs font-medium text-fs-text-secondary mb-1">Description</h3>
          <p className="text-[13px] text-fs-text leading-normal">{node.description}</p>
        </div>
      )}

      {/* Fichier source */}
      {node.filePath && (
        <div className="px-6 py-5 border-b border-fs-border">
          <h3 className="text-xs font-medium text-fs-text-secondary mb-1">Fichier source</h3>
          <div className="flex items-center gap-2 bg-fs-bg px-2 py-1.5 rounded-md">
            <code className="text-xs font-mono text-fs-text flex-1 truncate">{node.filePath}</code>
            <button
              onClick={handleCopyPath}
              aria-label="Copier le chemin"
              className="shrink-0 text-fs-text-secondary hover:text-fs-accent transition-colors"
            >
              {copied ? <Check className="h-3.5 w-3.5 text-fs-success" /> : <Copy className="h-3.5 w-3.5" />}
            </button>
          </div>
        </div>
      )}

      {/* Metadonnees */}
      <MetadataSection metadata={node.metadata} nodeType={node.type} />

      {/* Tags */}
      {node.tags && node.tags.length > 0 && (
        <div className="px-6 py-5 border-b border-fs-border">
          <h3 className="text-xs font-medium text-fs-text-secondary mb-2">Tags</h3>
          <div className="flex flex-wrap gap-1">
            {node.tags.map((tag) => (
              <TagBadge key={tag} label={tag} />
            ))}
          </div>
        </div>
      )}

      {/* Connexions */}
      <ConnectionsList
        outgoing={outgoing}
        incoming={incoming}
        onNodeClick={onNodeNavigate}
      />
    </aside>
  );
}

export default InspectorPanel;
```

**Refs UI/UX :** Section 3.5 — width 320px, height 100vh, fixed right, bg fs-surface, animate slide-in 250ms, scrollbar custom, 6 sections avec separateurs.

---

### 2.14 `MetadataSection.tsx`

**Chemin :** `src/components/inspector/MetadataSection.tsx`

**Props :**

```typescript
interface MetadataSectionProps {
  metadata: NodeMetadata;
  nodeType: NodeType;
}
```

**State interne :**
- `expandedKeys` — `useState<Set<string>>(new Set())` pour les listes > 10 items

**Logique :**

Itere sur les cles de `metadata`. Pour chaque cle :
- Si la valeur est un `string[]` : affiche le label avec le count, puis la liste (tronquee a 10 si besoin, avec bouton "+N de plus")
- Si la valeur est un `string` : affiche label + valeur
- Ignore les cles avec valeur `undefined`, `null`, ou non-string/non-array

```tsx
function MetadataSection({ metadata, nodeType }: MetadataSectionProps) {
  const [expandedKeys, setExpandedKeys] = useState<Set<string>>(new Set());

  // Filtrer les cles pertinentes
  const entries = Object.entries(metadata).filter(
    ([, value]) => value !== undefined && value !== null
  );

  if (entries.length === 0) return null;

  const toggleExpand = (key: string) => {
    setExpandedKeys((prev) => {
      const next = new Set(prev);
      if (next.has(key)) next.delete(key);
      else next.add(key);
      return next;
    });
  };

  return (
    <div className="px-6 py-5 border-b border-fs-border">
      <h3 className="text-xs font-medium text-fs-text-secondary mb-3">Metadonnees</h3>
      <div className="space-y-4">
        {entries.map(([key, value]) => {
          if (Array.isArray(value)) {
            const isExpanded = expandedKeys.has(key);
            const displayItems = isExpanded ? value : value.slice(0, 10);
            const remaining = value.length - 10;

            return (
              <div key={key}>
                <h4 className="text-xs font-medium text-fs-text-secondary mb-1 capitalize">
                  {formatMetadataKey(key)} ({value.length})
                </h4>
                <ul className="space-y-0.5 pl-4">
                  {displayItems.map((item, i) => (
                    <li key={i} className="text-xs font-mono text-fs-text">{String(item)}</li>
                  ))}
                </ul>
                {remaining > 0 && !isExpanded && (
                  <button
                    onClick={() => toggleExpand(key)}
                    className="text-xs text-fs-accent hover:text-fs-accent-hover mt-1 pl-4"
                  >
                    ... +{remaining} de plus
                  </button>
                )}
              </div>
            );
          }
          return (
            <div key={key}>
              <h4 className="text-xs font-medium text-fs-text-secondary capitalize">
                {formatMetadataKey(key)}
              </h4>
              <p className="text-[13px] text-fs-text">{String(value)}</p>
            </div>
          );
        })}
      </div>
    </div>
  );
}

/** Transforme "primaryKeys" en "Primary Keys" */
function formatMetadataKey(key: string): string {
  return key.replace(/([A-Z])/g, " $1").replace(/^./, (s) => s.toUpperCase());
}

export default memo(MetadataSection);
```

---

### 2.15 `ConnectionsList.tsx`

**Chemin :** `src/components/inspector/ConnectionsList.tsx`

**Props :**

```typescript
interface ConnectionEntry {
  edge: FlowEdge;
  targetNode?: FlowNode;
  sourceNode?: FlowNode;
}

interface ConnectionsListProps {
  outgoing: Array<{ edge: FlowEdge; targetNode: FlowNode }>;
  incoming: Array<{ edge: FlowEdge; sourceNode: FlowNode }>;
  onNodeClick: (nodeId: string) => void;
}
```

**Rendu :**

```tsx
function ConnectionsList({ outgoing, incoming, onNodeClick }: ConnectionsListProps) {
  if (outgoing.length === 0 && incoming.length === 0) return null;

  return (
    <div className="px-6 py-5">
      <h3 className="text-xs font-medium text-fs-text-secondary mb-3">Connexions</h3>

      {outgoing.length > 0 && (
        <div className="mb-4">
          <div className="flex items-center gap-1 mb-1.5">
            <ArrowUpRight className="h-3.5 w-3.5 text-fs-text-secondary" />
            <span className="text-xs font-medium text-fs-text-secondary">
              Sortantes ({outgoing.length})
            </span>
          </div>
          <ul className="space-y-1 pl-5">
            {outgoing.map(({ edge, targetNode }) => (
              <li key={edge.id}>
                <button
                  onClick={() => onNodeClick(targetNode.id)}
                  className="text-[13px] text-fs-text hover:text-fs-accent transition-colors cursor-pointer"
                >
                  → {targetNode.label}
                  {edge.label && (
                    <span className="text-fs-text-secondary ml-1">({edge.label})</span>
                  )}
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}

      {incoming.length > 0 && (
        <div>
          <div className="flex items-center gap-1 mb-1.5">
            <ArrowDownLeft className="h-3.5 w-3.5 text-fs-text-secondary" />
            <span className="text-xs font-medium text-fs-text-secondary">
              Entrantes ({incoming.length})
            </span>
          </div>
          <ul className="space-y-1 pl-5">
            {incoming.map(({ edge, sourceNode }) => (
              <li key={edge.id}>
                <button
                  onClick={() => onNodeClick(sourceNode.id)}
                  className="text-[13px] text-fs-text hover:text-fs-accent transition-colors cursor-pointer"
                >
                  ← {sourceNode.label}
                  {edge.label && (
                    <span className="text-fs-text-secondary ml-1">({edge.label})</span>
                  )}
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}

export default memo(ConnectionsList);
```

---

### 2.16 `TagBadge.tsx`

**Chemin :** `src/components/inspector/TagBadge.tsx`

**Props :**

```typescript
interface TagBadgeProps {
  label: string;
}
```

```tsx
function TagBadge({ label }: TagBadgeProps) {
  return (
    <span className="inline-block text-[11px] text-fs-text-secondary px-2 py-0.5 rounded
                     bg-fs-bg border border-fs-border">
      {label}
    </span>
  );
}

export default memo(TagBadge);
```

**Refs UI/UX :** Section 3.8 variante "tag badge" — 11px, bg-fs-bg, border fs-border.

---

### 2.17 `SearchModal.tsx` — Command Palette

**Chemin :** `src/components/search/SearchModal.tsx`

**Props :**

```typescript
interface SearchModalProps {
  project: FlowScopeProject;
  onClose: () => void;
  onResultSelect: (node: FlowNode, systemId: string) => void;
}
```

**State interne :**
- `query` — `useState<string>("")`
- `focusedIndex` — `useState<number>(0)` pour la navigation clavier dans les resultats
- `results` via `useSearch(project, query)`

**Hooks utilises :**
- `useSearch()` — retourne les resultats Fuse.js
- `useRef<HTMLInputElement>` — pour autoFocus

**Logique :**

```tsx
function SearchModal({ project, onClose, onResultSelect }: SearchModalProps) {
  const [query, setQuery] = useState("");
  const [focusedIndex, setFocusedIndex] = useState(0);
  const inputRef = useRef<HTMLInputElement>(null);
  const results = useSearch(project, query);

  // Auto-focus a l'ouverture
  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  // Reset focused index quand query change
  useEffect(() => {
    setFocusedIndex(0);
  }, [query]);

  const handleKeyDown = (e: React.KeyboardEvent) => {
    switch (e.key) {
      case "ArrowDown":
        e.preventDefault();
        setFocusedIndex((prev) => Math.min(prev + 1, results.length - 1));
        break;
      case "ArrowUp":
        e.preventDefault();
        setFocusedIndex((prev) => Math.max(prev - 1, 0));
        break;
      case "Enter":
        e.preventDefault();
        if (results[focusedIndex]) {
          const r = results[focusedIndex];
          onResultSelect(r.node, r.systemId);
        }
        break;
      case "Escape":
        onClose();
        break;
    }
  };

  return (
    <>
      {/* Overlay */}
      <div
        className="fixed inset-0 bg-black/60 z-[49] animate-fade-in"
        onClick={onClose}
      />

      {/* Modal */}
      <div
        role="dialog"
        aria-modal="true"
        aria-label="Recherche globale"
        className="fixed top-[20vh] left-1/2 -translate-x-1/2 z-50 w-[560px] max-h-[480px]
                   bg-fs-surface border border-fs-border rounded-xl shadow-2xl
                   animate-modal-in overflow-hidden flex flex-col"
        onKeyDown={handleKeyDown}
      >
        {/* Input */}
        <div className="flex items-center gap-3 px-4 h-14 border-b border-fs-border shrink-0">
          <Search className="h-5 w-5 text-fs-text-muted shrink-0" />
          <input
            ref={inputRef}
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Rechercher un node..."
            aria-label="Rechercher un node dans tous les systemes"
            className="flex-1 bg-transparent text-base text-fs-text placeholder:text-fs-text-muted
                       outline-none"
          />
          <kbd className="text-[11px] bg-fs-bg text-fs-text-muted px-2 py-0.5 rounded shrink-0">
            ESC
          </kbd>
        </div>

        {/* Resultats */}
        <div className="overflow-y-auto flex-1">
          {query.length > 0 && results.length === 0 && (
            <p className="text-[13px] text-fs-text-secondary text-center py-8">
              Aucun resultat pour &laquo; {query} &raquo;
            </p>
          )}
          {results.map((result, index) => (
            <SearchResult
              key={result.node.id}
              result={result}
              isFocused={index === focusedIndex}
              onClick={() => onResultSelect(result.node, result.systemId)}
            />
          ))}
        </div>
      </div>
    </>
  );
}

export default SearchModal;
```

**Refs UI/UX :** Section 3.6 — 560px, max 480px, rounded-xl, centre a 20vh, overlay bg-black/60, input 56px, ESC badge. Navigation ArrowUp/ArrowDown + Enter.

---

### 2.18 `SearchResult.tsx`

**Chemin :** `src/components/search/SearchResult.tsx`

**Props :**

```typescript
interface SearchResultProps {
  result: SearchResult;
  isFocused: boolean;
  onClick: () => void;
}
```

```tsx
function SearchResultItem({ result, isFocused, onClick }: SearchResultProps) {
  const Icon = NODE_ICONS[result.node.type];
  const color = NODE_BORDER_COLORS[result.node.type];

  return (
    <button
      onClick={onClick}
      aria-label={`${result.node.label} dans ${result.systemLabel}`}
      className={`
        flex items-center gap-3 w-full h-12 px-3 text-left transition-colors
        ${isFocused ? "bg-fs-surface-hover border-l-2 border-fs-accent" : "border-l-2 border-transparent"}
        hover:bg-fs-surface-hover
      `}
    >
      <Icon className="h-4 w-4 shrink-0" style={{ color }} />
      <div className="min-w-0 flex-1">
        <p className="text-sm font-medium text-fs-text truncate">{result.node.label}</p>
        <p className="text-xs text-fs-text-secondary truncate">{result.systemLabel}</p>
      </div>
    </button>
  );
}

export default memo(SearchResultItem);
```

---

### 2.19 `Badge.tsx` — Composant atomique

**Chemin :** `src/components/ui/Badge.tsx`

**Props :**

```typescript
type BadgeVariant = "type" | "tag";

interface BadgeProps {
  label: string;
  variant: BadgeVariant;
  /** Pour variant "type" — le NodeType pour la couleur */
  nodeType?: NodeType;
}
```

```tsx
function Badge({ label, variant, nodeType }: BadgeProps) {
  if (variant === "type" && nodeType) {
    const classes = NODE_BG_CLASSES[nodeType];
    return (
      <span className={`inline-block text-[10px] font-semibold uppercase tracking-wide
                        px-1.5 py-0.5 rounded ${classes}`}>
        {label}
      </span>
    );
  }

  // Variant "tag"
  return (
    <span className="inline-block text-[11px] text-fs-text-secondary px-2 py-0.5
                     rounded bg-fs-bg border border-fs-border">
      {label}
    </span>
  );
}

export default memo(Badge);
```

---

### 2.20 `IconButton.tsx` — Bouton icone avec tooltip

**Chemin :** `src/components/ui/IconButton.tsx`

**Props :**

```typescript
import type { LucideIcon } from "lucide-react";

interface IconButtonProps {
  icon: LucideIcon;
  onClick: () => void;
  "aria-label": string;
  size?: number;      // Default 32
  iconSize?: number;  // Default 18
  disabled?: boolean;
}
```

```tsx
function IconButton({
  icon: Icon,
  onClick,
  "aria-label": ariaLabel,
  size = 32,
  iconSize = 18,
  disabled = false,
}: IconButtonProps) {
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      aria-label={ariaLabel}
      title={ariaLabel}
      className={`
        flex items-center justify-center rounded-md transition-colors duration-150
        focus-visible:outline focus-visible:outline-2 focus-visible:outline-fs-accent
        focus-visible:outline-offset-1
        ${disabled
          ? "text-fs-text-muted opacity-40 cursor-not-allowed"
          : "text-fs-text-secondary hover:bg-fs-surface-hover hover:text-fs-text active:text-fs-accent"
        }
      `}
      style={{ width: size, height: size }}
    >
      <Icon style={{ width: iconSize, height: iconSize }} />
    </button>
  );
}

export default memo(IconButton);
```

**Note :** Le tooltip est implemente via l'attribut natif `title` pour le MVP. Un tooltip custom positionne en bas peut etre ajoute en v2+.

---

### 2.21 `KeyboardShortcuts.tsx` — Overlay aide raccourcis

**Chemin :** `src/components/ui/KeyboardShortcuts.tsx`

**Props :**

```typescript
interface KeyboardShortcutsProps {
  onClose: () => void;
}
```

**Logique :**

```tsx
const SHORTCUT_SECTIONS = [
  {
    title: "Navigation",
    shortcuts: [
      { keys: "1-9", description: "Naviguer vers une vue" },
      { keys: "Backspace", description: "Retour a la vue precedente" },
      { keys: "F", description: "Ajuster le zoom (fit view)" },
    ],
  },
  {
    title: "Recherche",
    shortcuts: [
      { keys: "Ctrl+K", description: "Ouvrir la recherche" },
      { keys: "Escape", description: "Fermer panneau / modale" },
    ],
  },
  {
    title: "Aide",
    shortcuts: [
      { keys: "?", description: "Afficher ce panneau" },
    ],
  },
];

function KeyboardShortcuts({ onClose }: KeyboardShortcutsProps) {
  // Fermer sur Escape
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    document.addEventListener("keydown", handler);
    return () => document.removeEventListener("keydown", handler);
  }, [onClose]);

  return (
    <>
      <div className="fixed inset-0 bg-black/60 z-[49]" onClick={onClose} />
      <div
        role="dialog"
        aria-modal="true"
        aria-label="Raccourcis clavier"
        className="fixed top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 z-50
                   w-[480px] bg-fs-surface border border-fs-border rounded-xl shadow-2xl p-6"
      >
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-base font-semibold text-fs-text">Raccourcis clavier</h2>
          <button
            onClick={onClose}
            aria-label="Fermer"
            className="w-8 h-8 flex items-center justify-center rounded-md
                       text-fs-text-secondary hover:bg-fs-surface-hover"
          >
            <X className="h-4 w-4" />
          </button>
        </div>

        <div className="space-y-5">
          {SHORTCUT_SECTIONS.map((section) => (
            <div key={section.title}>
              <h3 className="text-xs font-semibold uppercase tracking-wider text-fs-text-muted mb-2">
                {section.title}
              </h3>
              <div className="space-y-2">
                {section.shortcuts.map(({ keys, description }) => (
                  <div key={keys} className="flex items-center justify-between">
                    <kbd className="text-xs bg-fs-bg text-fs-text px-2 py-1 rounded border border-fs-border font-mono">
                      {keys}
                    </kbd>
                    <span className="text-sm text-fs-text-secondary">{description}</span>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>
    </>
  );
}

export default KeyboardShortcuts;
```

---

## 3. Hooks Custom

### 3.1 `useFlowData` — Chargement des donnees projet

**Chemin :** `src/hooks/useFlowData.ts`

**Signature :**

```typescript
function useFlowData(): {
  project: FlowScopeProject;
  getSystem: (systemId: string) => SystemDefinition | undefined;
  getAllNodes: () => FlowNode[];
}
```

**State gere :** Aucun state dynamique — les donnees sont importees statiquement.

**Fonctions exposees :**
- `project` — l'objet `FlowScopeProject` complet
- `getSystem(id)` — retourne la `SystemDefinition` par son ID
- `getAllNodes()` — retourne tous les nodes de tous les systemes (pour la recherche)

**Implementation :**

```typescript
import projectData from "@/data/project.json";
import type { FlowScopeProject, SystemDefinition, FlowNode } from "@/types";

// Import statique — les donnees sont bundlees par Vite (Arch A2)
const project = projectData as FlowScopeProject;

export function useFlowData() {
  const getSystem = useCallback(
    (systemId: string): SystemDefinition | undefined => {
      return project.systems.find((s) => s.id === systemId);
    },
    []
  );

  const getAllNodes = useCallback((): FlowNode[] => {
    return project.systems.flatMap((s) => s.nodes);
  }, []);

  return { project, getSystem, getAllNodes };
}
```

**Dependances :** `@/data/project.json` (import statique, Arch decision A2).

**Fallback si `project.json` n'existe pas :** Utiliser `example-project.json` comme donnee par defaut. Le fichier `project.json` sera genere par les parsers, mais `example-project.json` est commite et sert de fallback pendant le developpement.

```typescript
// Alternative avec fallback
let project: FlowScopeProject;
try {
  project = (await import("@/data/project.json")).default as FlowScopeProject;
} catch {
  project = (await import("@/data/example-project.json")).default as FlowScopeProject;
}
```

Pour le MVP, l'import statique direct est preferable (pas de lazy loading). Si `project.json` n'existe pas, renommer `example-project.json` en `project.json`.

---

### 3.2 `useNavigation` — Navigation entre vues

**Chemin :** `src/hooks/useNavigation.ts`

**Signature :**

```typescript
interface BreadcrumbSegment {
  id: string;
  label: string;
}

function useNavigation(systems: SystemDefinition[]): {
  activeSystemId: string;
  navigateTo: (systemId: string) => void;
  goBack: () => void;
  breadcrumbs: BreadcrumbSegment[];
}
```

**State gere :**
- `activeSystemId: string` — ID du systeme affiches (`"overview"` par defaut)
- `history: string[]` — pile de navigation pour le retour arriere

**Implementation :**

```typescript
import { useState, useCallback, useMemo } from "react";
import type { SystemDefinition } from "@/types";

interface NavigationState {
  activeSystemId: string;
  history: string[];
}

export function useNavigation(systems: SystemDefinition[]) {
  const [state, setState] = useState<NavigationState>({
    activeSystemId: "overview",
    history: [],
  });

  const navigateTo = useCallback((systemId: string) => {
    setState((prev) => {
      if (prev.activeSystemId === systemId) return prev; // Deja sur cette vue
      return {
        activeSystemId: systemId,
        history: [...prev.history, prev.activeSystemId],
      };
    });
  }, []);

  const goBack = useCallback(() => {
    setState((prev) => {
      const newHistory = [...prev.history];
      const previousId = newHistory.pop() ?? "overview";
      return { activeSystemId: previousId, history: newHistory };
    });
  }, []);

  const breadcrumbs = useMemo((): BreadcrumbSegment[] => {
    const overviewSegment: BreadcrumbSegment = { id: "overview", label: "Overview" };

    if (state.activeSystemId === "overview") {
      return [overviewSegment];
    }

    const activeSystem = systems.find((s) => s.id === state.activeSystemId);
    return [
      overviewSegment,
      { id: state.activeSystemId, label: activeSystem?.label ?? state.activeSystemId },
    ];
  }, [state.activeSystemId, systems]);

  return {
    activeSystemId: state.activeSystemId,
    navigateTo,
    goBack,
    breadcrumbs,
  };
}
```

---

### 3.3 `useInspector` — Panneau inspecteur

**Chemin :** `src/hooks/useInspector.ts`

**Signature :**

```typescript
function useInspector(): {
  isOpen: boolean;
  selectedNode: FlowNode | null;
  openInspector: (node: FlowNode) => void;
  closeInspector: () => void;
}
```

**State gere :**
- `isOpen: boolean`
- `selectedNode: FlowNode | null`

**Implementation :**

```typescript
import { useState, useCallback } from "react";
import type { FlowNode } from "@/types";

export function useInspector() {
  const [isOpen, setIsOpen] = useState(false);
  const [selectedNode, setSelectedNode] = useState<FlowNode | null>(null);

  const openInspector = useCallback((node: FlowNode) => {
    setSelectedNode(node);
    setIsOpen(true);
  }, []);

  const closeInspector = useCallback(() => {
    setIsOpen(false);
    setSelectedNode(null);
  }, []);

  return { isOpen, selectedNode, openInspector, closeInspector };
}
```

---

### 3.4 `useSearch` — Integration Fuse.js

**Chemin :** `src/hooks/useSearch.ts`

**Signature :**

```typescript
function useSearch(project: FlowScopeProject, query: string): SearchResult[]
```

**State gere :** Aucun state interne — les resultats sont calcules via `useMemo`.

**Fonctions exposees :** Retourne un tableau de `SearchResult[]`, max 10, tries par pertinence.

**Implementation :**

```typescript
import { useMemo } from "react";
import Fuse from "fuse.js";
import type { FlowScopeProject, FlowNode } from "@/types";
import type { SearchResult } from "@/types/ui";

interface SearchableItem {
  node: FlowNode;
  systemId: string;
  systemLabel: string;
}

export function useSearch(project: FlowScopeProject, query: string): SearchResult[] {
  // Construire l'index une seule fois
  const { items, fuse } = useMemo(() => {
    const allItems: SearchableItem[] = project.systems.flatMap((system) =>
      system.nodes.map((node) => ({
        node,
        systemId: system.id,
        systemLabel: system.label,
      }))
    );

    const fuseInstance = new Fuse(allItems, {
      keys: [
        { name: "node.label", weight: 0.6 },
        { name: "node.description", weight: 0.2 },
        { name: "node.tags", weight: 0.2 },
      ],
      threshold: 0.4, // Tolerance fuzzy
      includeScore: true,
      minMatchCharLength: 1,
    });

    return { items: allItems, fuse: fuseInstance };
  }, [project]);

  // Rechercher
  return useMemo(() => {
    if (query.trim().length === 0) return [];

    return fuse
      .search(query)
      .slice(0, 10) // Max 10 resultats
      .map((result) => ({
        node: result.item.node,
        systemId: result.item.systemId,
        systemLabel: result.item.systemLabel,
        score: result.score ?? 1,
      }));
  }, [fuse, query]);
}
```

**Dependances :** `fuse.js`

---

### 3.5 `useLayout` — Auto-layout Dagre

**Chemin :** `src/hooks/useLayout.ts`

**Signature :**

```typescript
function useLayout(
  nodes: FlowNode[],
  edges: FlowEdge[],
  direction: LayoutDirection
): FlowNode[]
```

**State gere :** Aucun — resultat calcule via `useMemo`.

**Fonctions exposees :** Retourne les nodes avec des positions calculees par Dagre (sauf si `node.position` est deja defini dans le JSON — vigilance V4).

**Implementation :**

```typescript
import { useMemo } from "react";
import dagre from "@dagrejs/dagre";
import type { FlowNode, FlowEdge, LayoutDirection } from "@/types";

const NODE_WIDTH = 220;
const NODE_HEIGHT = 80;
const OVERVIEW_NODE_WIDTH = 240;
const OVERVIEW_NODE_HEIGHT = 128;

export function useLayout(
  nodes: FlowNode[],
  edges: FlowEdge[],
  direction: LayoutDirection = "TB"
): FlowNode[] {
  return useMemo(() => {
    if (nodes.length === 0) return [];

    // Si tous les nodes ont une position manuelle, pas besoin de Dagre
    const allHavePosition = nodes.every((n) => n.position);
    if (allHavePosition) return nodes;

    return applyDagreLayout(nodes, edges, direction);
  }, [nodes, edges, direction]);
}

export function applyDagreLayout(
  nodes: FlowNode[],
  edges: FlowEdge[],
  direction: LayoutDirection
): FlowNode[] {
  const g = new dagre.graphlib.Graph();
  g.setDefaultEdgeLabel(() => ({}));

  const isOverview = nodes.some((n) => n.type === "custom" && n.metadata.systemRef);
  const nodeW = isOverview ? OVERVIEW_NODE_WIDTH : NODE_WIDTH;
  const nodeH = isOverview ? OVERVIEW_NODE_HEIGHT : NODE_HEIGHT;

  g.setGraph({
    rankdir: direction,
    nodesep: direction === "LR" ? 60 : 80,
    ranksep: direction === "LR" ? 100 : 64,
    marginx: 40,
    marginy: 40,
  });

  for (const node of nodes) {
    g.setNode(node.id, { width: nodeW, height: nodeH });
  }

  for (const edge of edges) {
    g.setEdge(edge.source, edge.target);
  }

  dagre.layout(g);

  return nodes.map((node) => {
    // Respecter les positions manuelles (V4)
    if (node.position) return node;

    const dagreNode = g.node(node.id);
    return {
      ...node,
      position: {
        x: dagreNode.x - nodeW / 2,
        y: dagreNode.y - nodeH / 2,
      },
    };
  });
}
```

**Dependances :** `@dagrejs/dagre`

---

### 3.6 `useKeyboardShortcuts` — Binding des raccourcis

**Chemin :** `src/hooks/useKeyboardShortcuts.ts`

**Signature :**

```typescript
interface KeyboardShortcutsConfig {
  onSearch: () => void;
  onEscape: () => void;
  onGoBack: () => void;
  onFitView: (() => void) | null;
  onNavigateToView: (systemId: string) => void;
  onShowShortcuts: () => void;
  systems: SystemDefinition[];
}

function useKeyboardShortcuts(config: KeyboardShortcutsConfig): void
```

**State gere :** Aucun — hook d'effet pur.

**Implementation :**

```typescript
import { useEffect } from "react";
import type { SystemDefinition } from "@/types";

export function useKeyboardShortcuts(config: KeyboardShortcutsConfig): void {
  const {
    onSearch, onEscape, onGoBack, onFitView,
    onNavigateToView, onShowShortcuts, systems,
  } = config;

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      // Ne pas intercepter si un input est focus (V7)
      const target = e.target as HTMLElement;
      if (target.tagName === "INPUT" || target.tagName === "TEXTAREA" || target.isContentEditable) {
        // Seul Escape est autorise dans un input
        if (e.key === "Escape") {
          onEscape();
          return;
        }
        return;
      }

      // Ctrl+K / Cmd+K — ouvrir la recherche
      if ((e.ctrlKey || e.metaKey) && e.key === "k") {
        e.preventDefault();
        onSearch();
        return;
      }

      // Escape — fermer
      if (e.key === "Escape") {
        onEscape();
        return;
      }

      // Backspace ou Alt+ArrowLeft — retour arriere
      if (e.key === "Backspace" || (e.altKey && e.key === "ArrowLeft")) {
        e.preventDefault();
        onGoBack();
        return;
      }

      // F — fit view
      if (e.key === "f" || e.key === "F") {
        onFitView?.();
        return;
      }

      // ? — afficher les raccourcis
      if (e.key === "?") {
        onShowShortcuts();
        return;
      }

      // 1-9 — navigation rapide
      const num = parseInt(e.key, 10);
      if (num >= 1 && num <= 9 && num <= systems.length) {
        const targetSystem = systems[num - 1];
        if (targetSystem) {
          onNavigateToView(targetSystem.id);
        }
        return;
      }
    };

    document.addEventListener("keydown", handler);
    return () => document.removeEventListener("keydown", handler);
  }, [onSearch, onEscape, onGoBack, onFitView, onNavigateToView, onShowShortcuts, systems]);
}
```

---

## 4. State Management

### 4.1 Global vs Local

| State | Scope | Gere par | Justification |
|-------|-------|----------|---------------|
| `project` (FlowScopeProject) | Global | `useFlowData` dans AppLayout | Donnee unique, consommee partout |
| `activeSystemId` + `history` | Global | `useNavigation` dans AppLayout | Affecte Sidebar, Breadcrumb, FlowCanvas |
| `isInspectorOpen` + `selectedNode` | Global | `useInspector` dans AppLayout | Affecte FlowCanvas (selection) + InspectorPanel |
| `isSearchOpen` | Global | `useState` dans AppLayout | Affecte SearchModal |
| `isShortcutsOpen` | Global | `useState` dans AppLayout | Affecte KeyboardShortcuts overlay |
| `query` (recherche) | Local | `useState` dans SearchModal | N'affecte que SearchModal |
| `focusedIndex` (recherche) | Local | `useState` dans SearchModal | N'affecte que SearchModal |
| `copied` (clipboard) | Local | `useState` dans InspectorPanel | N'affecte que le bouton copier |
| `expandedKeys` (metadata) | Local | `useState` dans MetadataSection | N'affecte que la section |
| `rfInstance` (React Flow) | Local | `useState` dans FlowCanvas | N'affecte que le canvas |

### 4.2 Data Flow Diagram

```
                          AppLayout
                             │
         ┌───────────────────┼────────────────────────┐
         │                   │                         │
    useFlowData()  useNavigation(project.systems) useInspector()
    → project           → activeSystemId         → isOpen
    → getSystem()       → navigateTo()           → selectedNode
                        → goBack()               → openInspector()
                        → breadcrumbs            → closeInspector()
         │                   │                         │
         ▼                   ▼                         ▼
    ┌─────────┐    ┌──────────────┐           ┌──────────────┐
    │ Sidebar │    │  FlowCanvas  │           │  Inspector   │
    │         │    │              │           │  Panel       │
    │ systems │    │ system=      │           │              │
    │ active  │    │  getSystem   │           │ node=        │
    │ onClick │    │  (activeId)  │           │  selectedNode│
    │ =navi-  │    │              │           │              │
    │  gateTo │    │ onNodeClick  │──────────>│ openInspector│
    │         │    │ =openInspec. │           │              │
    └─────────┘    │              │           └──────────────┘
                   │ onPaneClick  │──────────> closeInspector
                   │ =closeInsp.  │
                   │              │
                   │ selectedNode │<── selectedNode?.id
                   │ Id (glow)    │
                   └──────────────┘
```

### 4.3 Propagation de la selection d'un node

1. Utilisateur clique sur un node dans `FlowCanvas`
2. `onNodeClick` (handler React Flow) appelle `openInspector(flowNode)`
3. `useInspector` met a jour `isOpen = true` + `selectedNode = flowNode`
4. `AppLayout` re-render :
   - Passe `selectedNodeId` a `FlowCanvas` → le node recoit le style `selected` (glow)
   - Rend `InspectorPanel` avec les donnees du node

### 4.4 Propagation du changement de vue

1. Utilisateur clique sur un `SystemItem` dans la Sidebar (ou double-clic sur un OverviewNode)
2. `navigateTo(systemId)` est appele
3. `useNavigation` met a jour `activeSystemId` + ajoute l'ancien ID dans `history`
4. `AppLayout` re-render :
   - `Sidebar` : le `SystemItem` actif change de style
   - `Breadcrumb` : les segments sont recalcules
   - `FlowCanvas` : recoit un nouveau `system` prop
     - `useLayout` recalcule les positions Dagre (via `useMemo`)
     - `transformNodes` / `transformEdges` recalculent les objets React Flow (via `useMemo`)
     - `useEffect` sur `system.id` declenche `fitView({ duration: 300 })`
5. `closeInspector()` est appele pour fermer l'inspecteur lors d'un changement de vue

---

## 5. Integration React Flow

### 5.1 Configuration du ReactFlowProvider

Le `<ReactFlowProvider>` enveloppe uniquement le composant `<ReactFlow>` dans `FlowCanvas.tsx`, pas l'application entiere (Arch section 4.4). Il n'y a qu'une seule instance de graphe.

```tsx
// Dans FlowCanvas.tsx
<ReactFlowProvider>
  <ReactFlow
    nodes={rfNodes}
    edges={rfEdges}
    nodeTypes={nodeTypes}
    edgeTypes={edgeTypes}
    /* ... */
  >
    <Background />
    <MiniMapWrapper />
    <Toolbar />
  </ReactFlow>
</ReactFlowProvider>
```

### 5.2 Enregistrement des nodeTypes custom

```typescript
// Definir HORS du composant (vigilance V3 Arch)
import CustomNode from "@/components/canvas/CustomNode";
import OverviewNode from "@/components/canvas/OverviewNode";

const nodeTypes = {
  custom: CustomNode,
  overview: OverviewNode,
};
```

Tous les nodes de donnees utilisent le type React Flow `"custom"`. Le `CustomNode` lit `data.flowNode.type` (le `NodeType` metier) pour determiner l'apparence. Le type `"overview"` est utilise uniquement pour les nodes de la vue Overview.

### 5.3 Enregistrement des edgeTypes custom

```typescript
import CustomEdge from "@/components/canvas/CustomEdge";

const edgeTypes = {
  custom: CustomEdge,
};
```

Tous les edges utilisent le type React Flow `"custom"`. Le `CustomEdge` lit `data.edgeType` (le `EdgeType` metier) pour determiner le style visuel (couleur, dasharray, animation, marker).

**Les 5 types visuels d'edges :**

| EdgeType | Style stroke | Dash | Anime | Marker | Couleur |
|----------|-------------|------|-------|--------|---------|
| `fk` | Plein | Non | Non | arrowclosed | `#58a6ff` (bleu) |
| `inheritance` | Pointilles | `6 3` | Non | arrow (ouvert) | `#bc8cff` (violet) |
| `dependency` | Plein | Non | Non | arrowclosed | `#8b949e` (gris) |
| `flow` | Plein | Non | Oui (dashmove) | arrowclosed | `#3fb950` (vert) |
| `data` | Pointilles espaces | `4 6` | Non | arrowclosed | `#f0883e` (orange) |

### 5.4 Events React Flow

| Event | Handler | Action |
|-------|---------|--------|
| `onNodeClick` | `handleNodeClick` | Trouve le `FlowNode` correspondant, appelle `openInspector(node)` |
| `onNodeDoubleClick` | `handleNodeDoubleClick` | Si vue Overview et `metadata.systemRef` existe : `navigateTo(systemRef)` |
| `onPaneClick` | `handlePaneClick` | Appelle `closeInspector()` |
| `onInit` | `setRfInstance` | Stocke l'instance `ReactFlowInstance` pour `fitView()` / `zoomIn()` / `zoomOut()` programmatiques |

**Tous les handlers utilisent `useCallback` pour eviter les re-renders inutiles (Arch A8).**

### 5.5 Configuration MiniMap

```tsx
<MiniMap
  nodeColor={(node) => NODE_BORDER_COLORS[node.data?.flowNode?.type] ?? "#8b949e"}
  maskColor="rgba(13, 17, 23, 0.8)"  // fs-bg a 80%
  style={{
    backgroundColor: "#161b22",       // fs-surface
    border: "1px solid #30363d",       // fs-border
    borderRadius: 8,
    width: 180,
    height: 120,
  }}
  zoomable
  pannable
/>
```

### 5.6 Transformation FlowNode/FlowEdge vers React Flow

**Fichier :** `src/utils/transform.ts`

```typescript
import type { Node, Edge } from "@xyflow/react";
import type { FlowNode, FlowEdge, SystemDefinition, LayoutDirection } from "@/types";

/**
 * Transforme les FlowNodes en nodes React Flow.
 * - Nodes overview utilisent le type "overview"
 * - Tous les autres utilisent le type "custom"
 */
export function transformNodes(
  nodes: FlowNode[],
  isOverview: boolean,
  selectedNodeId: string | null,
  layoutDirection: LayoutDirection = "TB"
): Node[] {
  return nodes.map((node) => ({
    id: node.id,
    type: isOverview ? "overview" : "custom",
    position: node.position ?? { x: 0, y: 0 },
    data: isOverview
      ? {
          flowNode: node,
          systemLabel: node.label,
          systemIcon: (node.metadata.icon as string) ?? "HelpCircle",
          nodeCount: (node.metadata.nodeCount as number) ?? 0,
          description: node.description ?? "",
        }
      : {
          flowNode: node,
          isSelected: node.id === selectedNodeId,
          layoutDirection,
        },
    selected: node.id === selectedNodeId,
  }));
}

/**
 * Transforme les FlowEdges en edges React Flow.
 * Tous les edges utilisent le type "custom" avec l'EdgeType dans data.
 */
export function transformEdges(edges: FlowEdge[]): Edge[] {
  return edges.map((edge) => ({
    id: edge.id,
    source: edge.source,
    target: edge.target,
    type: "custom",
    label: edge.label,
    animated: edge.animated ?? false,
    data: {
      edgeType: edge.type ?? "dependency",
    },
  }));
}

// NOTE : getSystemIcon() a ete supprime. L'icone est lue directement
// depuis node.metadata.icon (provenant du JSON genere par les parsers).
// Chaque systeme dans overview-system.json definit son champ "icon"
// (ex: "Database", "Monitor", "GitBranch", "Globe").
// Cela evite un mapping en dur qui devrait etre mis a jour manuellement.
```

---

## 6. Styles & Theme

### 6.1 Fichier CSS global — `src/index.css`

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

/* ─── CSS Variables (fallback + React Flow theming) ─── */
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

/* ─── Base ─── */
body {
  margin: 0;
  font-family: Inter, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
  background-color: var(--fs-bg);
  color: var(--fs-text);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

/* ─── React Flow overrides ─── */
.react-flow__renderer {
  background-color: var(--fs-bg) !important;
}

.react-flow__attribution {
  display: none !important;
}

/* ─── Animation edges flow ─── */
@keyframes dashmove {
  to {
    stroke-dashoffset: -20;
  }
}

/* ─── Scrollbar custom (inspector) ─── */
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

/* ─── Focus visible ─── */
:focus-visible {
  outline: 2px solid var(--fs-accent);
  outline-offset: 2px;
}

/* ─── Reduced motion ─── */
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

### 6.2 Couleurs dynamiques par NodeType

Les couleurs par `NodeType` sont appliquees dynamiquement via les mappings dans `src/utils/colors.ts` (definis dans Arch section 5.6). Les composants utilisent :

- **Classes Tailwind** (pour les badges) : `NODE_BG_CLASSES[type]` retourne ex: `"bg-fs-node-table/20 text-fs-node-table"`
- **Couleurs hex** (pour les styles inline React Flow) : `NODE_BORDER_COLORS[type]` retourne ex: `"#58a6ff"`

Le choix entre Tailwind et inline style depend du contexte :
- Tailwind : pour les elements HTML standard (badges, texte)
- Inline style : pour les elements SVG/React Flow (borders, handles, glow) ou quand la couleur doit etre dynamique

### 6.3 Animations CSS

| Animation | Duree | Easing | Declencheur | Composant |
|-----------|-------|--------|-------------|-----------|
| `slide-in` | 250ms | ease-out | Ouverture inspecteur | InspectorPanel |
| `slide-out` | 200ms | ease-in | Fermeture inspecteur | InspectorPanel |
| `fade-in` | 150ms | ease-out | Overlay modale recherche | SearchModal overlay |
| `modal-in` | 200ms | ease-out | Ouverture modale recherche | SearchModal contenu |
| `dashmove` | 1s linear infinite | linear | Edges de type `flow` | CustomEdge |

**Note sur la fermeture de l'inspecteur :** Pour le MVP, la fermeture de l'inspecteur est instantanee (unmount du composant, pas d'animation slide-out). L'animation slide-out 200ms est une amelioration Sprint 5 / v2 qui necessite un state intermediaire `isClosing` avec un `setTimeout` pour demonter apres l'animation.

---

## 7. Sprint-by-Sprint Implementation Guide

### Sprint 1 : Fondations (US-1.1 a US-1.5)

**Objectif :** Projet initialise, schema defini, graphe minimal rendu, layout avec sidebar.

#### Etape 1 — Initialisation (US-1.1)

Fichiers a creer dans l'ordre :

1. `package.json` — Initialisation via `npm create vite@latest`
2. `tsconfig.json` — Configuration TypeScript strict + paths
3. `tsconfig.node.json` — Config pour scripts Node
4. `vite.config.ts` — Alias `@/`
5. `tailwind.config.ts` — Tokens complets `fs-`
6. `postcss.config.js` — Plugin Tailwind
7. `src/index.css` — Directives Tailwind + CSS variables + overrides
8. `src/main.tsx` — `ReactDOM.createRoot`
9. `src/App.tsx` — Composant racine minimal

**Checkpoint :** `npm run dev` demarre, `npm run build` compile, page affiche "FlowScope".

#### Etape 2 — Schema JSON (US-1.2)

1. `src/types/schema.ts` — Toutes les interfaces (copie de Arch section 2.1)
2. `src/types/ui.ts` — Types UI (copie de Arch section 2.2)
3. `src/types/index.ts` — Reexports
4. `src/data/example-project.json` — Donnees d'exemple (copie de Arch section 2.3)

**Dependances :** Aucune — juste des fichiers TypeScript/JSON.

**Checkpoint :** Les types sont importables via `@/types` sans erreur.

#### Etape 3 — Utilitaires (prerequis composants)

1. `src/utils/colors.ts` — `NODE_BG_CLASSES`, `NODE_BORDER_COLORS`, `EDGE_COLORS`
2. `src/utils/icons.ts` — `NODE_ICONS`, `resolveIcon()`
3. `src/utils/transform.ts` — `transformNodes()`, `transformEdges()`, `getMetadataSummary()`
4. `src/utils/layout.ts` — `applyDagreLayout()`

**Dependances :** `@/types`

**Checkpoint :** Les utilitaires sont importables et typesafe.

#### Etape 4 — FlowCanvas + CustomNode (US-1.3, US-1.4)

1. `src/components/canvas/CustomNode.tsx`
2. `src/components/canvas/CustomEdge.tsx`
3. `src/components/canvas/MiniMapWrapper.tsx`
4. `src/components/canvas/FlowCanvas.tsx` — assemble tout

**Dependances :** `@/utils/*`, `@/types`, `@xyflow/react`

**Checkpoint :** Un graphe React Flow s'affiche avec les nodes de `example-project.json`, zoom/pan fonctionnent, MiniMap visible.

#### Etape 5 — Layout + Sidebar (US-1.5)

1. `src/hooks/useFlowData.ts`
2. `src/hooks/useNavigation.ts`
3. `src/hooks/useInspector.ts` (stub — sera complete au Sprint 4)
4. `src/components/ui/IconButton.tsx`
5. `src/components/sidebar/SearchButton.tsx`
6. `src/components/sidebar/SystemItem.tsx`
7. `src/components/sidebar/Sidebar.tsx`
8. `src/components/canvas/Breadcrumb.tsx`
9. `src/layouts/AppLayout.tsx` — shell complet
10. Mettre a jour `src/App.tsx` pour rendre `AppLayout`

**Dependances :** Tous les fichiers precedents.

**Checkpoint :** Sidebar affiche les systemes, clic change la vue, breadcrumb a jour, thème dark coherent, pas de scroll vertical.

---

### Sprint 2 : Parsers & Donnees (US-2.1 a US-2.4)

**Objectif :** Les parsers generent les donnees reelles du projet Charles & Nadejda.

> Ce sprint est principalement Backend (Agent #4 — Parsers). Cote Frontend, les fichiers concernes sont :

1. `src/parsers/types.ts` — Reexport des types schema
2. `src/parsers/sql-parser.ts` — Parser SQL (US-2.1)
3. `src/parsers/csharp-parser.ts` — Parser C# (US-2.2)
4. `src/parsers/assemble.ts` — Assembleur (US-2.3)
5. `output/bom-system.json` — Cree manuellement (US-2.4)
6. `output/overview-system.json` — Cree manuellement (US-2.4)
7. `src/data/project.json` — Genere par l'assembleur

**Checkpoint Frontend :** Le `project.json` genere se charge dans l'application sans erreur. Les nodes des 4 systemes apparaissent.

---

### Sprint 3 : Vues & Navigation (US-3.1 a US-3.5)

**Objectif :** Les vues sont navigables, l'Overview fonctionne comme point d'entree.

#### Etape 1 — OverviewNode (US-3.1)

1. `src/components/canvas/OverviewNode.tsx`
2. Mettre a jour `FlowCanvas.tsx` — gerer le type `"overview"` dans `nodeTypes`
3. Mettre a jour `transform.ts` — logique `isOverview` pour mapper les donnees OverviewNode

**Checkpoint :** Vue Overview affiche 4 gros blocs, double-clic navigue vers le systeme.

#### Etape 2 — Navigation breadcrumb (US-3.2)

1. Verifier `useNavigation.ts` — breadcrumbs corrects en toute situation
2. Verifier `Breadcrumb.tsx` — clic sur segments, transitions
3. Verifier `AppLayout.tsx` — synchronisation sidebar ↔ breadcrumb ↔ canvas

**Checkpoint :** Navigation Overview ↔ vues detaillees fluide, breadcrumb a jour, sidebar reflete la vue active.

#### Etape 3 — Auto-layout (US-3.3, US-3.4, US-3.5)

1. `src/hooks/useLayout.ts` — implementer avec support `direction` TB/LR
2. Verifier que la vue DB utilise layout TB
3. Verifier que la vue BOM utilise layout LR
4. Verifier que les nodes C# sont groupes par type visuellement (via le `group` field)

**Checkpoint :** Les 3 vues detaillees (DB, C#, BOM) affichent des graphes lisibles avec auto-layout.

---

### Sprint 4 : Inspecteur & Recherche (US-4.1 a US-4.3)

**Objectif :** L'utilisateur peut inspecter et chercher.

#### Etape 1 — Inspecteur (US-4.1)

1. `src/components/inspector/TagBadge.tsx`
2. `src/components/inspector/MetadataSection.tsx`
3. `src/components/inspector/ConnectionsList.tsx`
4. `src/components/inspector/InspectorPanel.tsx`
5. Mettre a jour `AppLayout.tsx` — rendre `InspectorPanel` conditionnel, passer les props

**Dependances :** `useInspector` (deja stub depuis Sprint 1)

**Checkpoint :** Clic sur un node → panneau slide-in a droite avec toutes les sections. Clic sur une connexion → centre le node cible. Fermeture via X ou clic canvas.

#### Etape 2 — Recherche (US-4.2)

1. `src/hooks/useSearch.ts` — integration Fuse.js
2. `src/components/search/SearchResult.tsx`
3. `src/components/search/SearchModal.tsx`
4. Mettre a jour `AppLayout.tsx` — etat `isSearchOpen`, rendu `SearchModal`

**Checkpoint :** Ctrl+K ouvre la modale, fuzzy search instantanee, selection navigue vers le node, Escape ferme.

#### Etape 3 — Raccourcis clavier (US-4.3)

1. `src/hooks/useKeyboardShortcuts.ts`
2. `src/components/ui/KeyboardShortcuts.tsx`
3. Mettre a jour `AppLayout.tsx` — brancher le hook, etat `isShortcutsOpen`

**Checkpoint :** Tous les raccourcis fonctionnent (1-9, F, ?, Ctrl+K, Escape, Backspace). Les raccourcis ne se declenchent pas dans un input.

**Note accessibilite — navigation Tab entre nodes :** La navigation Tab entre nodes dans React Flow v12 depend du support natif de la librairie. Si non supporte nativement, c'est une limitation acceptable pour le MVP. Tester apres integration.

---

### Sprint 5 : Polish & Integration (US-5.1 a US-5.5)

**Objectif :** Application visuellement finie et fonctionnelle sur les donnees reelles.

#### Etape 1 — Theme dark coherent (US-5.1)

1. Audit visuel de tous les composants — verifier l'utilisation exclusive des tokens `fs-`
2. Verifier les contrastes WCAG AA (cf. UIUX section 6.3)
3. Ajouter les transitions hover manquantes
4. Verifier la coherence des ombres

**Checkpoint :** Aucune couleur en dur hors des tokens `fs-`. Contrastes conformes.

#### Etape 2 — Toolbar (US-5.3)

1. `src/components/canvas/Toolbar.tsx`
2. Brancher les callbacks `zoomIn`, `zoomOut`, `fitView`, `reLayout`, `openSearch`
3. Mettre a jour `FlowCanvas.tsx` — exposer `rfInstance` pour les actions toolbar

**Checkpoint :** 5 boutons fonctionnels avec tooltips, position overlay top-right.

#### Etape 3 — Edges types visuellement (US-5.4)

1. Verifier `CustomEdge.tsx` — les 5 types visuels sont distincts
2. Verifier les etats hover (opacite, epaisseur)
3. Verifier l'animation `dashmove` sur les edges `flow`
4. Verifier les labels d'edges (fond semi-transparent)

**Checkpoint :** Les 5 types d'edges sont visuellement distincts. Hover met en evidence.

#### Etape 4 — Integration finale (US-5.5)

1. Regenerer `project.json` avec les parsers a jour
2. Verifier les 4 vues (Overview, DB, C#, BOM)
3. Tester la recherche transversale
4. Verifier les metadonnees dans l'inspecteur sur 10+ nodes
5. Verifier la performance (navigation fluide, pas de lag)
6. Verifier `npm run build` sans erreur ni warning
7. Verifier la console — aucune erreur

**Checkpoint final :** L'application FlowScope affiche correctement le projet Charles & Nadejda. Navigation fluide, recherche fonctionnelle, inspecteur renseigne, aucune erreur console, build propre.

---

## Annexe A — Fichier `src/types/index.ts`

```typescript
export type {
  NodeType,
  EdgeType,
  LayoutDirection,
  NodeMetadata,
  NodePosition,
  FlowNode,
  FlowEdge,
  SystemDefinition,
  FlowScopeProject,
} from "./schema";

export type {
  NavigationState,
  InspectorState,
  SearchResult,
} from "./ui";
```

---

## Annexe B — Fichier `src/utils/colors.ts` complet

```typescript
import type { NodeType, EdgeType } from "@/types";

/** Classes Tailwind pour le fond du badge de chaque type de node */
export const NODE_BG_CLASSES: Record<NodeType, string> = {
  table:      "bg-fs-node-table/20 text-fs-node-table",
  form:       "bg-fs-node-form/20 text-fs-node-form",
  dal:        "bg-fs-node-dal/20 text-fs-node-dal",
  model:      "bg-fs-node-model/20 text-fs-node-model",
  controller: "bg-fs-node-controller/20 text-fs-node-controller",
  route:      "bg-fs-node-route/20 text-fs-node-route",
  view:       "bg-fs-node-view/20 text-fs-node-view",
  process:    "bg-fs-node-process/20 text-fs-node-process",
  stock:      "bg-fs-node-stock/20 text-fs-node-stock",
  custom:     "bg-fs-node-custom/20 text-fs-node-custom",
};

/** Couleur hex de bordure pour chaque type de node */
export const NODE_BORDER_COLORS: Record<NodeType, string> = {
  table:      "#58a6ff",
  form:       "#bc8cff",
  dal:        "#f0883e",
  model:      "#3fb950",
  controller: "#f85149",
  route:      "#79c0ff",
  view:       "#f778ba",
  process:    "#d29922",
  stock:      "#2ea043",
  custom:     "#8b949e",
};

/** Couleur hex de chaque type d'edge */
export const EDGE_COLORS: Record<EdgeType, string> = {
  fk:          "#58a6ff",
  inheritance: "#bc8cff",
  dependency:  "#8b949e",
  flow:        "#3fb950",
  data:        "#f0883e",
};
```

---

## Annexe C — Fichier `src/utils/icons.ts` complet

```typescript
import {
  Database, Monitor, Layers, Box, Globe,
  Route, Layout, GitBranch, Package, HelpCircle,
  LayoutDashboard, Diamond,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
import type { NodeType } from "@/types";

/** Mapping NodeType -> composant Lucide */
export const NODE_ICONS: Record<NodeType, LucideIcon> = {
  table:      Database,
  form:       Monitor,
  dal:        Layers,
  model:      Box,
  controller: Globe,
  route:      Route,
  view:       Layout,
  process:    GitBranch,
  stock:      Package,
  custom:     HelpCircle,
};

/** Mapping nom string (depuis JSON) -> composant Lucide */
const ICON_MAP: Record<string, LucideIcon> = {
  Database, Monitor, Layers, Box, Globe,
  Route, Layout, GitBranch, Package, HelpCircle,
  LayoutDashboard, Diamond,
};

/** Resout un nom d'icone string en composant Lucide */
export function resolveIcon(name: string): LucideIcon {
  return ICON_MAP[name] ?? HelpCircle;
}
```

---

*Document produit par Agent #5 — Frontend Developer. Pret pour implementation par mentalyas.*
