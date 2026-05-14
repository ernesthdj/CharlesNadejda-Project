import { readFileSync, writeFileSync, readdirSync } from "fs";
import { join } from "path";

interface FlowNode {
  id: string;
  type: string;
  label: string;
  description?: string;
  filePath?: string;
  metadata: Record<string, string[]>;
  tags?: string[];
  group?: string;
  position?: { x: number; y: number };
}

interface FlowEdge {
  id: string;
  source: string;
  target: string;
  label?: string;
  type?: string;
  animated?: boolean;
}

interface SystemDefinition {
  id: string;
  label: string;
  icon: string;
  description: string;
  layoutDirection?: string;
  nodes: FlowNode[];
  edges: FlowEdge[];
}

interface FlowScopeProject {
  name: string;
  version: string;
  lastParsed: string;
  systems: SystemDefinition[];
}

const DATA_DIR = join(import.meta.dirname, "../data");
const OUTPUT = join(DATA_DIR, "project.json");

// Collect all *-system.json files
const systemFiles = readdirSync(DATA_DIR).filter(
  (f) => f.endsWith("-system.json") && f !== "example-project.json",
);

console.log(`📦 Assembleur FlowScope`);
console.log(`   Fichiers trouvés : ${systemFiles.join(", ")}`);

const systems: SystemDefinition[] = [];

for (const file of systemFiles) {
  const raw = readFileSync(join(DATA_DIR, file), "utf-8");
  const system = JSON.parse(raw) as SystemDefinition;
  systems.push(system);
  console.log(`   ✅ ${system.id} : ${system.nodes.length} nodes, ${system.edges.length} edges`);
}

// Build overview system
const overview: SystemDefinition = {
  id: "overview",
  label: "Vue d'ensemble",
  icon: "LayoutDashboard",
  description: "Architecture globale du projet Charles & Nadejda",
  nodes: systems.map((s) => ({
    id: `overview:${s.id}`,
    type: s.id === "db" ? "table" : s.id === "csharp" ? "form" : "process",
    label: s.label,
    description: s.description,
    metadata: { info: [`${s.nodes.length} éléments`, `${s.edges.length} relations`] },
    tags: ["système"],
    group: "Systèmes",
  })),
  edges: [],
};

// BOM pipeline (manual, always included)
const bom: SystemDefinition = {
  id: "bom",
  label: "Pipeline BOM",
  icon: "GitBranch",
  description: "Flux de production multi-niveaux — Ingrédients → Stocks",
  layoutDirection: "LR",
  nodes: [
    { id: "bom:ingredients", type: "stock", label: "Ingrédients", description: "Stock global niveau 0 — lots FIFO", metadata: { info: ["Fournisseurs", "Lots FIFO", "Unités + densité"] }, tags: ["entrée"] },
    { id: "bom:contextes", type: "process", label: "Contextes", description: "Configs de production nommées par activité", metadata: { info: ["Par activité", "Stock propre", "Niveaux configurables"] }, tags: ["config"] },
    { id: "bom:niveaux", type: "process", label: "Niveaux", description: "Profondeur de transformation variable", metadata: { info: ["N consomme N-1", "Ordre topologique", "Suppression par le haut"] }, tags: ["structure"] },
    { id: "bom:fiches", type: "custom", label: "Fiches Recettes", description: "Recettes globales réutilisables entre contextes", metadata: { info: ["Multi-contextes", "Conversion unités auto", "Lignes avec quantités"] }, tags: ["recette"] },
    { id: "bom:productions", type: "process", label: "Productions", description: "Exécution réelle — validation → consommation → création stock", metadata: { info: ["Validation stock N-1", "Consommation FIFO", "Lignes détaillées"] }, tags: ["exécution"] },
    { id: "bom:stocks", type: "stock", label: "Stocks", description: "Stock produit par niveau dans chaque contexte", metadata: { info: ["Par contexte", "Discriminants activité", "Quantité disponible"] }, tags: ["stock"] },
    { id: "bom:reservations", type: "stock", label: "Réservations", description: "Réservations de stock pour simulations", metadata: { info: ["Dispo réelle = qté - réservations", "Libération auto à production", "Par lot + contexte"] }, tags: ["réservation"] },
  ],
  edges: [
    { id: "bom:e1", source: "bom:ingredients", target: "bom:niveaux", label: "alimente N-1", type: "flow", animated: true },
    { id: "bom:e2", source: "bom:contextes", target: "bom:niveaux", label: "configure", type: "flow", animated: true },
    { id: "bom:e3", source: "bom:niveaux", target: "bom:fiches", label: "rattachées à", type: "data" },
    { id: "bom:e4", source: "bom:fiches", target: "bom:productions", label: "déclenche", type: "flow", animated: true },
    { id: "bom:e5", source: "bom:productions", target: "bom:stocks", label: "produit", type: "flow", animated: true },
    { id: "bom:e6", source: "bom:stocks", target: "bom:reservations", label: "réserve", type: "data" },
  ],
};

// Assemble project
const allSystems = [overview, ...systems, bom];

const project: FlowScopeProject = {
  name: "Charles & Nadejda",
  version: "1.0.0",
  lastParsed: new Date().toISOString(),
  systems: allSystems,
};

writeFileSync(OUTPUT, JSON.stringify(project, null, 2), "utf-8");

console.log(`\n✅ Projet assemblé : ${allSystems.length} systèmes`);
console.log(`   → ${OUTPUT}`);
const totalNodes = allSystems.reduce((acc, s) => acc + s.nodes.length, 0);
const totalEdges = allSystems.reduce((acc, s) => acc + s.edges.length, 0);
console.log(`   Total : ${totalNodes} nodes, ${totalEdges} edges`);
