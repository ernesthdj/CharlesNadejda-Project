/**
 * Parser C# pour FlowScope — Charles & Nadejda
 *
 * Scanne les fichiers .cs du projet WinForms, extrait classes/propriétés/méthodes/héritage,
 * détecte les relations (inheritance, dependency, data) et génère un SystemDefinition JSON.
 *
 * Usage : npx tsx src/parsers/csharp-parser.ts
 */

import { readFileSync, writeFileSync, readdirSync, statSync } from "node:fs";
import { join, relative, basename, dirname } from "node:path";
import { fileURLToPath } from "node:url";

// ── Types alignés sur schema.ts ──────────────────────────────────────────────

type NodeType = "form" | "dal" | "model" | "custom";
type EdgeType = "inheritance" | "dependency" | "data";

interface FlowNode {
  id: string;
  type: NodeType;
  label: string;
  description?: string;
  filePath?: string;
  metadata: Record<string, string[]>;
  tags?: string[];
  group?: string;
}

interface FlowEdge {
  id: string;
  source: string;
  target: string;
  label?: string;
  type?: EdgeType;
  animated?: boolean;
}

interface SystemDefinition {
  id: string;
  label: string;
  icon: string;
  description: string;
  layoutDirection?: "TB" | "LR";
  nodes: FlowNode[];
  edges: FlowEdge[];
}

// ── Données intermédiaires du parsing ────────────────────────────────────────

interface ParsedClass {
  className: string;
  filePath: string;          // chemin relatif depuis la racine projet
  inherits: string | null;   // classe parente brute (ex: "FrmListeBase<Ingredient>")
  genericArg: string | null; // argument générique extrait (ex: "Ingredient")
  properties: string[];      // noms de propriétés publiques
  methods: string[];         // signatures simplifiées
  isStatic: boolean;
  isAbstract: boolean;
  isPartial: boolean;
  isEnum: boolean;
  rawContent: string;        // contenu complet du fichier pour analyse des dépendances
}

// ── Configuration ────────────────────────────────────────────────────────────

const __dirname = dirname(fileURLToPath(import.meta.url));
const FLOWSCOPE_ROOT = join(__dirname, "..", "..");
const CSHARP_ROOT = join(FLOWSCOPE_ROOT, "..", "app-csharp", "CharlesNadejda", "CharlesNadejda");
const OUTPUT_PATH = join(FLOWSCOPE_ROOT, "src", "data", "csharp-system.json");

const EXCLUDED_FILES = new Set([
  "Program.cs",
  "AssemblyInfo.cs",
]);

// ── Fonctions utilitaires ────────────────────────────────────────────────────

function scanCsFiles(dir: string): string[] {
  const results: string[] = [];

  let entries: string[];
  try {
    entries = readdirSync(dir);
  } catch {
    return results;
  }

  for (const entry of entries) {
    const fullPath = join(dir, entry);
    let stat;
    try {
      stat = statSync(fullPath);
    } catch {
      continue;
    }

    if (stat.isDirectory()) {
      // Exclure obj/ et bin/
      if (entry === "obj" || entry === "bin") continue;
      results.push(...scanCsFiles(fullPath));
    } else if (
      entry.endsWith(".cs") &&
      !entry.endsWith(".Designer.cs") &&
      !EXCLUDED_FILES.has(entry)
    ) {
      results.push(fullPath);
    }
  }

  return results;
}

function extractGenericArg(typeStr: string): string | null {
  const match = typeStr.match(/<(\w+)>/);
  return match ? match[1] : null;
}

function stripGeneric(typeStr: string): string {
  return typeStr.replace(/<\w+>/, "");
}

// ── Parsing d'un fichier C# ─────────────────────────────────────────────────

function parseFile(filePath: string, projectRoot: string): ParsedClass | null {
  const content = readFileSync(filePath, "utf-8");
  const _fileName = basename(filePath);

  // Retirer les commentaires pour éviter les faux positifs dans la détection de classe
  const contentNoComments = content
    .replace(/\/\/\/.*$/gm, "")   // XML doc comments
    .replace(/\/\/.*$/gm, "")     // single-line comments
    .replace(/\/\*[\s\S]*?\*\//g, ""); // multi-line comments

  // Détecter la déclaration de classe ou enum
  // Patterns : public class X, public abstract class X, public static class X,
  //            public partial class X, internal sealed class X, partial class X, public enum X
  // Les modificateurs peuvent apparaitre dans n'importe quel ordre
  const classMatch = contentNoComments.match(
    /(?:(?:public|internal|private|protected)\s+)?(?:(?:static|abstract|partial|sealed)\s+)*(?:class|enum)\s+(\w+)(?:<\w+>)?(?:\s*:\s*([^\n{]+))?/
  );

  if (!classMatch) return null;

  const declLine = classMatch[0];
  const isStatic = /\bstatic\b/.test(declLine);
  const isAbstract = /\babstract\b/.test(declLine);
  const isPartial = /\bpartial\b/.test(declLine);
  const className = classMatch[1];
  const isEnum = /\benum\b/.test(declLine);

  // Héritage
  let inheritsRaw: string | null = null;
  let genericArg: string | null = null;
  if (classMatch[2]) {
    // Prendre le premier type avant une virgule (les interfaces viennent après)
    const parts = classMatch[2].split(",").map((p) => p.trim());
    // Filtrer les contraintes `where T : class`
    const baseType = parts[0].replace(/\s*where\s+.*$/, "").trim();
    if (baseType && baseType !== "class") {
      inheritsRaw = baseType;
      genericArg = extractGenericArg(baseType);
    }
  }

  // Propriétés publiques : public Type Name { get; set; }
  const properties: string[] = [];
  const propRegex = /public\s+(?:static\s+)?(?:readonly\s+)?\S+\s+(\w+)\s*\{[^}]*get/g;
  let propMatch;
  while ((propMatch = propRegex.exec(content)) !== null) {
    // Exclure les propriétés calculées avec => si elles sont des propriétés expression-bodied
    properties.push(propMatch[1]);
  }

  // Méthodes publiques (hors propriétés, constructeurs, et event handlers)
  const methods: string[] = [];
  const methodRegex =
    /(?:public|protected)\s+(?:static\s+)?(?:virtual\s+)?(?:abstract\s+)?(?:override\s+)?(\w+(?:<\w+>)?)\s+(\w+)\s*\(([^)]*)\)/g;
  let methodMatch;
  while ((methodMatch = methodRegex.exec(content)) !== null) {
    const returnType = methodMatch[1];
    const methodName = methodMatch[2];
    const params = methodMatch[3].trim();

    // Exclure les constructeurs (nom == className) et les propriétés déjà capturées
    if (methodName === className) continue;
    // Exclure les event handlers générés (OnLoad, InitializeComponent, etc.)
    if (methodName === "InitializeComponent") continue;

    // Format simplifié
    const paramList = params
      ? params
          .split(",")
          .map((p) => {
            const parts = p.trim().split(/\s+/);
            return parts[parts.length - 1]; // nom du param seulement
          })
          .join(", ")
      : "";

    methods.push(`${methodName}(${paramList})`);
  }

  // Enum : extraire les valeurs comme "propriétés"
  const enumValues: string[] = [];
  if (isEnum) {
    const enumBodyMatch = content.match(/enum\s+\w+\s*\{([^}]+)\}/s);
    if (enumBodyMatch) {
      const body = enumBodyMatch[1];
      const valueRegex = /^\s*(\w+)/gm;
      let valMatch;
      while ((valMatch = valueRegex.exec(body)) !== null) {
        // Exclure les commentaires
        if (!valMatch[1].startsWith("//")) {
          enumValues.push(valMatch[1]);
        }
      }
    }
  }

  const relPath = relative(projectRoot, filePath).replace(/\\/g, "/");

  return {
    className,
    filePath: relPath,
    inherits: inheritsRaw,
    genericArg,
    properties: isEnum ? enumValues : properties,
    methods,
    isStatic,
    isAbstract,
    isPartial,
    isEnum,
    rawContent: content,
  };
}

// ── Classification ───────────────────────────────────────────────────────────

function classifyNode(parsed: ParsedClass): { type: NodeType; group: string } {
  const fp = parsed.filePath.toLowerCase();

  if (fp.includes("/models/") || fp.includes("\\models\\")) {
    return { type: "model", group: "Models" };
  }

  if (fp.includes("/dal/") || fp.includes("\\dal\\") || parsed.className.endsWith("DAL")) {
    return { type: "dal", group: "DAL" };
  }

  if (fp.includes("/services/")) {
    return { type: "custom", group: "Services" };
  }

  if (fp.includes("/navigation/")) {
    return { type: "custom", group: "Navigation" };
  }

  if (fp.includes("/forms/shell/") || fp.includes("/forms\\shell\\")) {
    return { type: "form", group: "Shell" };
  }

  if (
    fp.includes("/forms/") ||
    fp.includes("\\forms\\") ||
    parsed.className.startsWith("Frm")
  ) {
    // Vérifier si c'est un utilitaire dans Forms/ (pas un vrai Form)
    const utils = ["AppColors", "FormHelper", "UnitConvertisseur", "StringExtensions"];
    if (utils.includes(parsed.className)) {
      return { type: "custom", group: "Utils" };
    }
    return { type: "form", group: "Forms" };
  }

  return { type: "custom", group: "Utils" };
}

function buildDescription(parsed: ParsedClass): string | undefined {
  if (parsed.isEnum) return `Enum — ${parsed.properties.length} valeurs`;
  if (parsed.isAbstract) return `Classe abstraite${parsed.isStatic ? " statique" : ""}`;
  if (parsed.isStatic) return "Classe statique utilitaire";
  return undefined;
}

function buildTags(parsed: ParsedClass, group: string): string[] {
  const tags: string[] = [];
  if (parsed.isAbstract) tags.push("abstract");
  if (parsed.isStatic) tags.push("static");
  if (parsed.isEnum) tags.push("enum");
  if (parsed.isPartial) tags.push("partial");
  if (group === "Shell") tags.push("shell-erp");
  if (parsed.className.includes("Bom")) tags.push("bom");
  if (parsed.className.includes("Stock")) tags.push("stock");
  return tags.length > 0 ? tags : undefined as unknown as string[];
}

// ── Détection des relations ──────────────────────────────────────────────────

function detectEdges(
  parsedClasses: ParsedClass[],
  knownClassNames: Set<string>
): FlowEdge[] {
  const edges: FlowEdge[] = [];
  const edgeSet = new Set<string>(); // pour dédupliquer

  function addEdge(source: string, target: string, type: EdgeType, label: string) {
    const key = `${source}→${target}→${type}`;
    if (edgeSet.has(key)) return;
    edgeSet.add(key);
    edges.push({
      id: `edge:${source}:${target}:${type}`,
      source: `cs:${source}`,
      target: `cs:${target}`,
      label,
      type,
    });
  }

  for (const parsed of parsedClasses) {
    // 1. Héritage
    if (parsed.inherits) {
      const baseClass = stripGeneric(parsed.inherits);
      if (knownClassNames.has(baseClass)) {
        addEdge(parsed.className, baseClass, "inheritance", "hérite");
      }

      // Dépendance générique : FrmListeBase<Ingredient> → dépend de Ingredient
      if (parsed.genericArg && knownClassNames.has(parsed.genericArg)) {
        addEdge(parsed.className, parsed.genericArg, "dependency", "utilise");
      }
    }

    // 2. Instanciation : new XxxDAL() ou XxxDAL.Method()
    const instantiationRegex = /\bnew\s+(\w+)\s*\(/g;
    let instMatch;
    while ((instMatch = instantiationRegex.exec(parsed.rawContent)) !== null) {
      const instClass = instMatch[1];
      if (knownClassNames.has(instClass) && instClass !== parsed.className) {
        addEdge(parsed.className, instClass, "dependency", "utilise");
      }
    }

    // Appels statiques : XxxDAL.GetAll()
    const staticCallRegex = /\b(\w+DAL)\.\w+\s*\(/g;
    let staticMatch;
    while ((staticMatch = staticCallRegex.exec(parsed.rawContent)) !== null) {
      const calledClass = staticMatch[1];
      if (knownClassNames.has(calledClass) && calledClass !== parsed.className) {
        addEdge(parsed.className, calledClass, "dependency", "utilise");
      }
    }

    // Appels statiques non-DAL : DbHelper.GetConnection(), AppColors.X, FormHelper.X
    const staticUtilRegex = /\b(DbHelper|AppColors|FormHelper|UnitConvertisseur)\.\w+/g;
    let utilMatch;
    while ((utilMatch = staticUtilRegex.exec(parsed.rawContent)) !== null) {
      const utilClass = utilMatch[1];
      if (knownClassNames.has(utilClass) && utilClass !== parsed.className) {
        addEdge(parsed.className, utilClass, "dependency", "utilise");
      }
    }

    // Instanciation de Forms : new FrmXxx(...)
    const formInstRegex = /\bnew\s+(Frm\w+)\s*\(/g;
    let formMatch;
    while ((formMatch = formInstRegex.exec(parsed.rawContent)) !== null) {
      const formClass = formMatch[1];
      if (knownClassNames.has(formClass) && formClass !== parsed.className) {
        addEdge(parsed.className, formClass, "dependency", "ouvre");
      }
    }

    // Instanciation de Models : new Ingredient { ... }, paramètre Model
    const modelInstRegex = /\bnew\s+(\w+)\s*\{/g;
    let modelMatch;
    while ((modelMatch = modelInstRegex.exec(parsed.rawContent)) !== null) {
      const modelClass = modelMatch[1];
      if (knownClassNames.has(modelClass) && modelClass !== parsed.className) {
        // Si c'est un DAL qui instancie un Model, c'est une relation data
        const { type: srcType } = classifyNode(parsed);
        if (srcType === "dal") {
          addEdge(parsed.className, modelClass, "data", "manipule");
        }
      }
    }

    // 3. DAL ↔ Model : List<Model>, paramètre de type Model
    if (parsed.className.endsWith("DAL")) {
      // Chercher List<ModelName> dans le contenu
      const listRegex = /List<(\w+)>/g;
      let listMatch;
      while ((listMatch = listRegex.exec(parsed.rawContent)) !== null) {
        const modelName = listMatch[1];
        if (knownClassNames.has(modelName) && modelName !== parsed.className) {
          addEdge(parsed.className, modelName, "data", "manipule");
        }
      }

      // Chercher des paramètres de type Model dans les méthodes
      const paramRegex = /(?:public|private|protected)\s+(?:static\s+)?\w+\s+\w+\s*\(([^)]+)\)/g;
      let paramMatch;
      while ((paramMatch = paramRegex.exec(parsed.rawContent)) !== null) {
        const params = paramMatch[1];
        for (const known of knownClassNames) {
          if (params.includes(known) && known !== parsed.className) {
            addEdge(parsed.className, known, "data", "manipule");
          }
        }
      }
    }

    // 4. Forms utilisant des Models directement (cast, DataBoundItem is Model)
    const castRegex = /\bis\s+(\w+)\b|\(\s*(\w+)\s*\)/g;
    let castMatch;
    while ((castMatch = castRegex.exec(parsed.rawContent)) !== null) {
      const castClass = castMatch[1] || castMatch[2];
      if (
        castClass &&
        knownClassNames.has(castClass) &&
        castClass !== parsed.className &&
        castClass !== "object" &&
        castClass !== "int" &&
        castClass !== "string" &&
        castClass !== "decimal" &&
        castClass !== "bool" &&
        castClass !== "Stock" // Avoid noise from (Stock) casts in forms that already have the dependency
      ) {
        // Only add if it's a known model/class and not a primitive
        const targetInfo = classifyNode(
          parsedClasses.find((p) => p.className === castClass)!
        );
        if (targetInfo.type === "model") {
          addEdge(parsed.className, castClass, "dependency", "utilise");
        }
      }
    }
  }

  return edges;
}

// ── Main ─────────────────────────────────────────────────────────────────────

function main() {
  console.log("=== FlowScope C# Parser ===\n");
  console.log(`Source : ${CSHARP_ROOT}`);
  console.log(`Output : ${OUTPUT_PATH}\n`);

  // 1. Scanner les fichiers
  const csFiles = scanCsFiles(CSHARP_ROOT);
  console.log(`Fichiers .cs trouvés : ${csFiles.length}`);

  // 2. Parser chaque fichier
  const projectRoot = join(CSHARP_ROOT, "..", "..", "..");
  const parsedClasses: ParsedClass[] = [];

  for (const file of csFiles) {
    const parsed = parseFile(file, projectRoot);
    if (parsed) {
      parsedClasses.push(parsed);
    } else {
      console.warn(`  [SKIP] ${basename(file)} — pas de classe/enum détectée`);
    }
  }

  console.log(`Classes/enums parsées : ${parsedClasses.length}\n`);

  // Fusionner les partial classes (ex: FrmPrincipal + FrmPrincipal.Production)
  const mergedMap = new Map<string, ParsedClass>();
  for (const parsed of parsedClasses) {
    const existing = mergedMap.get(parsed.className);
    if (existing) {
      // Fusionner méthodes et propriétés
      existing.methods.push(...parsed.methods);
      existing.properties.push(...parsed.properties);
      existing.rawContent += "\n" + parsed.rawContent;
      // Garder l'héritage du fichier principal (celui qui a inherits)
      if (parsed.inherits && !existing.inherits) {
        existing.inherits = parsed.inherits;
        existing.genericArg = parsed.genericArg;
      }
    } else {
      mergedMap.set(parsed.className, { ...parsed });
    }
  }

  const mergedClasses = Array.from(mergedMap.values());
  const knownClassNames = new Set(mergedClasses.map((p) => p.className));

  // 3. Construire les nodes
  const nodes: FlowNode[] = mergedClasses.map((parsed) => {
    const { type, group } = classifyNode(parsed);
    const tags = buildTags(parsed, group);
    const description = buildDescription(parsed);

    const metadata: Record<string, string[]> = {};
    if (parsed.methods.length > 0) metadata.methods = [...new Set(parsed.methods)];
    if (parsed.properties.length > 0) metadata.properties = [...new Set(parsed.properties)];
    if (parsed.inherits) metadata.inherits = [parsed.inherits];

    const node: FlowNode = {
      id: `cs:${parsed.className}`,
      type,
      label: parsed.className,
      filePath: parsed.filePath,
      metadata,
      group,
    };

    if (description) node.description = description;
    if (tags) node.tags = tags;

    return node;
  });

  // 4. Détecter les relations
  const edges = detectEdges(mergedClasses, knownClassNames);

  // 5. Stats
  const groupCounts = new Map<string, number>();
  for (const node of nodes) {
    const g = node.group ?? "?";
    groupCounts.set(g, (groupCounts.get(g) ?? 0) + 1);
  }

  const edgeTypeCounts = new Map<string, number>();
  for (const edge of edges) {
    const t = edge.type ?? "?";
    edgeTypeCounts.set(t, (edgeTypeCounts.get(t) ?? 0) + 1);
  }

  console.log("── Classes par groupe ──");
  for (const [group, count] of [...groupCounts.entries()].sort()) {
    console.log(`  ${group.padEnd(12)} : ${count}`);
  }

  console.log(`\n── Relations ──`);
  console.log(`  Total : ${edges.length}`);
  for (const [type, count] of [...edgeTypeCounts.entries()].sort()) {
    console.log(`  ${type.padEnd(14)} : ${count}`);
  }

  // 6. Construire le SystemDefinition
  const system: SystemDefinition = {
    id: "csharp",
    label: "Application C# WinForms",
    icon: "Monitor",
    description: `${nodes.length} classes — Models, DAL, Forms, Shell, Utils, Navigation, Services`,
    layoutDirection: "LR",
    nodes,
    edges,
  };

  // 7. Écrire le JSON
  writeFileSync(OUTPUT_PATH, JSON.stringify(system, null, 2), "utf-8");
  console.log(`\nJSON écrit : ${OUTPUT_PATH}`);
  console.log(`  ${nodes.length} nodes, ${edges.length} edges`);
}

main();
