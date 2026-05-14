/**
 * Parser SQL pour FlowScope — Charles & Nadejda
 *
 * Scanne les fichiers .sql du dossier sql/, extrait les CREATE TABLE, ALTER TABLE,
 * DROP TABLE, et génère un SystemDefinition JSON avec nodes (tables) et edges (FK).
 *
 * Usage : npx tsx src/parsers/sql-parser.ts
 */

import { readFileSync, writeFileSync, readdirSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";

// ── Types alignés sur schema.ts ─────────────────────────────────────────────

type NodeType = "table";
type EdgeType = "fk";

interface FlowNode {
  id: string;
  type: NodeType;
  label: string;
  description?: string;
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

// ── Données intermédiaires ──────────────────────────────────────────────────

interface ColumnInfo {
  name: string;
  type: string;
  constraints: string[];
}

interface ForeignKeyInfo {
  constraintName: string;
  column: string;
  refTable: string;
  refColumn: string;
}

interface TableInfo {
  name: string;
  columns: ColumnInfo[];
  primaryKeys: string[];
  foreignKeys: ForeignKeyInfo[];
  dropped: boolean;
}

// ── Helpers ─────────────────────────────────────────────────────────────────

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const SQL_DIR = join(__dirname, "..", "..", "..", "sql");
const OUTPUT_FILE = join(__dirname, "..", "data", "db-system.json");

/** Supprime les commentaires SQL (-- et /* *​/) */
function stripComments(sql: string): string {
  // Remove block comments
  let result = sql.replace(/\/\*[\s\S]*?\*\//g, "");
  // Remove line comments
  result = result.replace(/--.*$/gm, "");
  return result;
}

/** Normalise les espaces blancs */
function normalizeWhitespace(sql: string): string {
  return sql.replace(/\s+/g, " ").trim();
}

/** Extrait le contenu entre parenthèses au niveau supérieur */
function extractParenContent(body: string): string {
  let depth = 0;
  let start = -1;

  for (let i = 0; i < body.length; i++) {
    if (body[i] === "(") {
      if (depth === 0) start = i + 1;
      depth++;
    } else if (body[i] === ")") {
      depth--;
      if (depth === 0 && start !== -1) {
        return body.substring(start, i);
      }
    }
  }

  return "";
}

/** Sépare les clauses au niveau supérieur (split par virgule hors parenthèses) */
function splitTopLevel(content: string): string[] {
  const parts: string[] = [];
  let depth = 0;
  let current = "";

  for (const ch of content) {
    if (ch === "(") {
      depth++;
      current += ch;
    } else if (ch === ")") {
      depth--;
      current += ch;
    } else if (ch === "," && depth === 0) {
      parts.push(current.trim());
      current = "";
    } else {
      current += ch;
    }
  }

  if (current.trim()) {
    parts.push(current.trim());
  }

  return parts;
}

// ── Parsing ─────────────────────────────────────────────────────────────────

/**
 * Parse un type de colonne SQL (gère les ENUM avec parenthèses)
 * Retourne [type, restOfLine]
 */
function parseColumnType(tokens: string[]): [string, string[]] {
  if (tokens.length === 0) return ["UNKNOWN", []];

  const firstToken = tokens[0].toUpperCase();

  // ENUM(...) ou SET(...)
  if (firstToken === "ENUM" || firstToken === "SET") {
    // Reconstruct and find closing paren
    const full = tokens.join(" ");
    const openIdx = full.indexOf("(");
    if (openIdx === -1) return [firstToken, tokens.slice(1)];

    let depth = 0;
    let closeIdx = -1;
    for (let i = openIdx; i < full.length; i++) {
      if (full[i] === "(") depth++;
      else if (full[i] === ")") {
        depth--;
        if (depth === 0) { closeIdx = i; break; }
      }
    }

    if (closeIdx === -1) return [firstToken, tokens.slice(1)];

    const enumType = full.substring(0, closeIdx + 1);
    const rest = full.substring(closeIdx + 1).trim();
    return [enumType, rest ? rest.split(/\s+/) : []];
  }

  // DECIMAL(x,y), VARCHAR(n), INT, etc.
  if (tokens.length > 1 && tokens[1].startsWith("(")) {
    // Type with size in next token
    return [tokens[0] + tokens[1], tokens.slice(2)];
  }

  // Check if type itself contains parens like VARCHAR(100)
  return [tokens[0], tokens.slice(1)];
}

/** Parse une clause d'un CREATE TABLE */
function parseClause(
  clause: string,
  table: TableInfo
): void {
  const normalized = normalizeWhitespace(clause);
  const upper = normalized.toUpperCase();

  // PRIMARY KEY (col1, col2)
  if (upper.startsWith("PRIMARY KEY")) {
    const content = extractParenContent(normalized);
    if (content) {
      const keys = content.split(",").map((k) => k.trim().replace(/`/g, ""));
      table.primaryKeys.push(...keys);
    }
    return;
  }

  // UNIQUE KEY / KEY / INDEX — skip
  if (
    upper.startsWith("UNIQUE KEY") ||
    upper.startsWith("UNIQUE INDEX") ||
    upper.startsWith("KEY ") ||
    upper.startsWith("INDEX ")
  ) {
    return;
  }

  // CHECK constraint — skip
  if (upper.startsWith("CONSTRAINT") && upper.includes("CHECK")) {
    return;
  }

  // CONSTRAINT ... FOREIGN KEY
  if (upper.startsWith("CONSTRAINT") && upper.includes("FOREIGN KEY")) {
    parseForeignKeyConstraint(normalized, table);
    return;
  }

  // Inline FOREIGN KEY (without CONSTRAINT name)
  if (upper.startsWith("FOREIGN KEY")) {
    parseForeignKeyInline(normalized, table);
    return;
  }

  // Column definition
  parseColumnDefinition(normalized, table);
}

/** Parse une contrainte FOREIGN KEY avec CONSTRAINT */
function parseForeignKeyConstraint(clause: string, table: TableInfo): void {
  const fkRegex =
    /CONSTRAINT\s+`?(\w+)`?\s+FOREIGN\s+KEY\s*\(\s*`?(\w+)`?\s*\)\s*REFERENCES\s+`?(\w+)`?\s*\(\s*`?(\w+)`?\s*\)/i;
  const match = clause.match(fkRegex);
  if (match) {
    table.foreignKeys.push({
      constraintName: match[1],
      column: match[2],
      refTable: match[3],
      refColumn: match[4],
    });
  }
}

/** Parse une FOREIGN KEY inline (sans CONSTRAINT) */
function parseForeignKeyInline(clause: string, table: TableInfo): void {
  const fkRegex =
    /FOREIGN\s+KEY\s*\(\s*`?(\w+)`?\s*\)\s*REFERENCES\s+`?(\w+)`?\s*\(\s*`?(\w+)`?\s*\)/i;
  const match = clause.match(fkRegex);
  if (match) {
    table.foreignKeys.push({
      constraintName: `fk_inline_${match[1]}`,
      column: match[1],
      refTable: match[2],
      refColumn: match[3],
    });
  }
}

/** Parse une définition de colonne */
function parseColumnDefinition(clause: string, table: TableInfo): void {
  // Skip GENERATED ALWAYS AS columns' complex expressions
  const tokens = clause.split(/\s+/);
  if (tokens.length < 2) return;

  const colName = tokens[0].replace(/`/g, "");

  // Skip if the name is a SQL keyword (malformed parse)
  const keywords = new Set([
    "PRIMARY", "FOREIGN", "CONSTRAINT", "UNIQUE", "KEY",
    "INDEX", "CHECK", "ENGINE", "AUTO_INCREMENT",
  ]);
  if (keywords.has(colName.toUpperCase())) return;

  const [colType, restTokens] = parseColumnType(tokens.slice(1));
  const constraints: string[] = [];

  const restUpper = restTokens.join(" ").toUpperCase();

  if (restUpper.includes("PRIMARY KEY") || restUpper.includes("AUTO_INCREMENT")) {
    constraints.push("PK");
    if (!table.primaryKeys.includes(colName)) {
      table.primaryKeys.push(colName);
    }
  }
  if (restUpper.includes("NOT NULL")) constraints.push("NOT NULL");
  if (restUpper.includes("UNIQUE")) constraints.push("UNIQUE");
  if (restUpper.includes("DEFAULT")) constraints.push("DEFAULT");
  if (restUpper.includes("GENERATED")) constraints.push("GENERATED");

  table.columns.push({
    name: colName,
    type: colType,
    constraints,
  });
}

/** Parse les CREATE TABLE d'un contenu SQL */
function parseCreateTables(sql: string, tables: Map<string, TableInfo>): void {
  const cleaned = stripComments(sql);

  // Match CREATE TABLE [IF NOT EXISTS] table_name (...)
  const createRegex =
    /CREATE\s+TABLE\s+(?:IF\s+NOT\s+EXISTS\s+)?`?(\w+)`?\s*\(/gi;

  let match: RegExpExecArray | null;
  while ((match = createRegex.exec(cleaned)) !== null) {
    const tableName = match[1].toLowerCase();
    const startIdx = match.index + match[0].length - 1; // position of "("

    // Extract the full body between parens
    const body = cleaned.substring(startIdx);
    const content = extractParenContent(body);
    if (!content) continue;

    const existing = tables.get(tableName);
    const hasIfNotExists = match[0].toUpperCase().includes("IF NOT EXISTS");

    // IF NOT EXISTS + table already exists and not dropped → skip (MySQL behavior)
    if (existing && !existing.dropped && hasIfNotExists) {
      continue;
    }

    const tableInfo: TableInfo = (existing && existing.dropped)
      ? { ...existing, columns: [], primaryKeys: [], foreignKeys: [], dropped: false }
      : existing ?? {
          name: tableName,
          columns: [],
          primaryKeys: [],
          foreignKeys: [],
          dropped: false,
        };

    const clauses = splitTopLevel(content);
    for (const clause of clauses) {
      parseClause(clause, tableInfo);
    }

    if (!existing || existing.dropped) {
      tables.set(tableName, tableInfo);
    }

    // Un-drop if re-created
    tableInfo.dropped = false;
  }
}

/** Parse les ALTER TABLE ADD COLUMN */
function parseAlterAddColumns(sql: string, tables: Map<string, TableInfo>): void {
  const cleaned = stripComments(sql);

  // Match both:
  //   ALTER TABLE t ADD COLUMN col ...
  //   ALTER TABLE t ADD COLUMN col1 ..., ADD COLUMN col2 ...
  // Strategy: find all ALTER TABLE, then scan for ADD COLUMN within each
  const alterTableRegex = /ALTER\s+TABLE\s+`?(\w+)`?\s+/gi;

  let alterMatch: RegExpExecArray | null;
  while ((alterMatch = alterTableRegex.exec(cleaned)) !== null) {
    const tableName = alterMatch[1].toLowerCase();
    const table = tables.get(tableName);
    if (!table) continue;

    // Find the end of this ALTER TABLE statement (semicolon)
    const stmtStart = alterMatch.index + alterMatch[0].length;
    const semiIdx = findStatementEnd(cleaned, stmtStart);
    const stmtBody = cleaned.substring(stmtStart, semiIdx);

    // Find all ADD COLUMN within this statement
    const addColRegex =
      /ADD\s+COLUMN\s+(?:IF\s+NOT\s+EXISTS\s+)?`?(\w+)`?\s+/gi;

    let addMatch: RegExpExecArray | null;
    while ((addMatch = addColRegex.exec(stmtBody)) !== null) {
      const colName = addMatch[1];

      // Check if column already exists
      if (table.columns.some((c) => c.name === colName)) continue;

      // Extract the column definition after the column name
      const defStart = addMatch.index + addMatch[0].length;
      const rest = stmtBody.substring(defStart);
      let colDef = "";
      let depth = 0;

      for (let i = 0; i < rest.length; i++) {
        const ch = rest[i];
        if (ch === "(") depth++;
        else if (ch === ")") depth--;

        if (depth === 0) {
          if (ch === ";") break;
          if (ch === ",") break;
          // Check for AFTER keyword
          const ahead = rest.substring(i).toUpperCase();
          if (ahead.startsWith("AFTER ") || ahead.startsWith("FIRST")) break;
        }
        colDef += ch;
      }
      colDef = colDef.trim();

      // Remove trailing COMMENT '...' if present
      colDef = colDef.replace(/COMMENT\s+'[^']*'/gi, "").trim();

      // Parse the column type
      const tokens = colDef.split(/\s+/);
      const [colType, restTokens] = parseColumnType(tokens);
      const constraints: string[] = [];
      const restUpper = restTokens.join(" ").toUpperCase();

      if (restUpper.includes("NOT NULL")) constraints.push("NOT NULL");
      if (restUpper.includes("DEFAULT")) constraints.push("DEFAULT");

      table.columns.push({ name: colName, type: colType, constraints });
    }
  }
}

/** Find the end of a SQL statement (semicolon, respecting parentheses and quotes) */
function findStatementEnd(sql: string, from: number): number {
  let depth = 0;
  let inSingleQuote = false;

  for (let i = from; i < sql.length; i++) {
    const ch = sql[i];
    if (inSingleQuote) {
      if (ch === "'" && sql[i + 1] !== "'") inSingleQuote = false;
      else if (ch === "'" && sql[i + 1] === "'") i++; // escaped quote
      continue;
    }
    if (ch === "'") { inSingleQuote = true; continue; }
    if (ch === "(") depth++;
    else if (ch === ")") depth--;
    else if (ch === ";" && depth === 0) return i;
  }
  return sql.length;
}

/** Parse les ALTER TABLE ADD CONSTRAINT FOREIGN KEY */
function parseAlterAddFK(sql: string, tables: Map<string, TableInfo>): void {
  const cleaned = stripComments(sql);

  // First, find all ALTER TABLE statements
  const alterTableRegex = /ALTER\s+TABLE\s+`?(\w+)`?\s+/gi;

  let alterMatch: RegExpExecArray | null;
  while ((alterMatch = alterTableRegex.exec(cleaned)) !== null) {
    const tableName = alterMatch[1].toLowerCase();
    const table = tables.get(tableName);
    if (!table) continue;

    const stmtStart = alterMatch.index + alterMatch[0].length;
    const semiIdx = findStatementEnd(cleaned, stmtStart);
    const stmtBody = cleaned.substring(stmtStart, semiIdx);

    // Find all ADD CONSTRAINT ... FOREIGN KEY within this statement
    const addFkRegex =
      /ADD\s+CONSTRAINT\s+`?(\w+)`?\s+FOREIGN\s+KEY\s*\(\s*`?(\w+)`?\s*\)\s*REFERENCES\s+`?(\w+)`?\s*\(\s*`?(\w+)`?\s*\)/gi;

    let fkMatch: RegExpExecArray | null;
    while ((fkMatch = addFkRegex.exec(stmtBody)) !== null) {
      const constraintName = fkMatch[1];
      const column = fkMatch[2];
      const refTable = fkMatch[3].toLowerCase();
      const refColumn = fkMatch[4];

      // Check for duplicate constraint — replace if exists
      const idx = table.foreignKeys.findIndex((fk) => fk.constraintName === constraintName);
      if (idx !== -1) {
        table.foreignKeys[idx] = { constraintName, column, refTable, refColumn };
      } else {
        table.foreignKeys.push({ constraintName, column, refTable, refColumn });
      }
    }
  }
}

/** Parse les ALTER TABLE DROP COLUMN */
function parseAlterDropColumn(sql: string, tables: Map<string, TableInfo>): void {
  const cleaned = stripComments(sql);

  const dropColRegex =
    /ALTER\s+TABLE\s+`?(\w+)`?\s+DROP\s+COLUMN\s+(?:IF\s+EXISTS\s+)?`?(\w+)`?/gi;

  let match: RegExpExecArray | null;
  while ((match = dropColRegex.exec(cleaned)) !== null) {
    const tableName = match[1].toLowerCase();
    const colName = match[2];

    const table = tables.get(tableName);
    if (!table) continue;

    table.columns = table.columns.filter((c) => c.name !== colName);
    // Also remove FK referencing this column
    table.foreignKeys = table.foreignKeys.filter((fk) => fk.column !== colName);
  }
}

/** Parse les ALTER TABLE DROP FOREIGN KEY */
function parseAlterDropFK(sql: string, tables: Map<string, TableInfo>): void {
  const cleaned = stripComments(sql);

  const dropFkRegex =
    /ALTER\s+TABLE\s+`?(\w+)`?\s+DROP\s+FOREIGN\s+KEY\s+(?:IF\s+EXISTS\s+)?`?(\w+)`?/gi;

  let match: RegExpExecArray | null;
  while ((match = dropFkRegex.exec(cleaned)) !== null) {
    const tableName = match[1].toLowerCase();
    const constraintName = match[2];

    const table = tables.get(tableName);
    if (!table) continue;

    table.foreignKeys = table.foreignKeys.filter(
      (fk) => fk.constraintName !== constraintName
    );
  }
}

/** Parse les DROP TABLE */
function parseDropTables(sql: string, tables: Map<string, TableInfo>): void {
  const cleaned = stripComments(sql);

  const dropRegex =
    /DROP\s+TABLE\s+(?:IF\s+EXISTS\s+)?`?(\w+)`?/gi;

  let match: RegExpExecArray | null;
  while ((match = dropRegex.exec(cleaned)) !== null) {
    const tableName = match[1].toLowerCase();
    const table = tables.get(tableName);
    if (table) {
      table.dropped = true;
    }
  }
}

// ── Auto-grouping ───────────────────────────────────────────────────────────

function autoGroup(tableName: string): string {
  const name = tableName.toLowerCase();

  // BOM
  if (
    name.startsWith("bom_") ||
    ["stocks", "activites_stocks"].includes(name)
  ) {
    return "BOM";
  }

  // Pâtisserie
  if (name.startsWith("pat_")) return "Patisserie";

  // Auth
  if (["utilisateurs", "sessions", "users"].includes(name)) return "Auth";

  // Catalogue
  if (
    ["categories", "produits", "parfums", "fournisseurs", "produits_parfums"].includes(name)
  ) {
    return "Catalogue";
  }

  // Vente
  if (
    [
      "commandes", "factures", "lignes_commandes",
      "selections_parfums", "zones_livraison", "contacts",
    ].includes(name) ||
    name.startsWith("lignes_") ||
    name.startsWith("paniers")
  ) {
    return "Vente";
  }

  // Activités
  if (name === "activites") return "Activites";

  // Production
  if (
    [
      "fiches_ingredients", "lots_ingredients",
      "fiches_recettes", "recettes_ingredients",
      "fiches_compositions", "compositions_recettes",
      "productions_recettes", "productions_compositions",
      "mouvements_lots_ingredients", "mouvements_stock_produits",
      "stock_produits", "stock_compositions",
      "frais_production_variables", "frais_generaux_config",
    ].includes(name)
  ) {
    return "Production";
  }

  return "Autre";
}

// ── Conversion vers FlowScope ───────────────────────────────────────────────

function formatColumnDisplay(col: ColumnInfo): string {
  const parts = [col.name, col.type.toUpperCase()];
  if (col.constraints.length > 0) {
    parts.push(col.constraints.join(" "));
  }
  return parts.join(": ");
}

function buildSystemDefinition(tables: Map<string, TableInfo>): SystemDefinition {
  const nodes: FlowNode[] = [];
  const edges: FlowEdge[] = [];
  const activeTables = new Set<string>();

  // Collect active (non-dropped) tables
  for (const [name, table] of tables) {
    if (table.dropped) continue;
    activeTables.add(name);
  }

  for (const [name, table] of tables) {
    if (table.dropped) continue;

    const nodeId = `db:${name}`;
    const group = autoGroup(name);

    const columnsDisplay = table.columns.map(formatColumnDisplay);
    const fkDisplay = table.foreignKeys
      .filter((fk) => activeTables.has(fk.refTable))
      .map((fk) => `${fk.column} \u2192 ${fk.refTable}.${fk.refColumn}`);

    const node: FlowNode = {
      id: nodeId,
      type: "table",
      label: name,
      metadata: {
        columns: columnsDisplay,
        primaryKeys: [...table.primaryKeys],
      },
      group,
    };

    if (fkDisplay.length > 0) {
      node.metadata["foreignKeys"] = fkDisplay;
    }

    nodes.push(node);

    // Build edges from FK
    for (const fk of table.foreignKeys) {
      if (!activeTables.has(fk.refTable)) continue;

      const edgeId = `db:${name}--fk--db:${fk.refTable}`;
      // Avoid duplicate edges (same source→target can have multiple FK)
      const existingEdge = edges.find((e) => e.id === edgeId);
      if (existingEdge) {
        // Append to label
        existingEdge.label = `${existingEdge.label}, ${fk.column}`;
      } else {
        edges.push({
          id: edgeId,
          source: nodeId,
          target: `db:${fk.refTable}`,
          label: fk.column,
          type: "fk",
        });
      }
    }
  }

  return {
    id: "db",
    label: "Base de donn\u00e9es",
    icon: "Database",
    description: `Sch\u00e9ma MySQL — ${nodes.length} tables, ${edges.length} relations FK`,
    layoutDirection: "TB",
    nodes,
    edges,
  };
}

// ── Main ────────────────────────────────────────────────────────────────────

function main(): void {
  console.log("=== FlowScope SQL Parser ===");
  console.log(`Dossier SQL : ${SQL_DIR}`);

  // 1. Lister et trier les fichiers SQL
  const files = readdirSync(SQL_DIR)
    .filter((f) => f.endsWith(".sql"))
    .sort((a, b) => {
      // create_database.sql first, then migrations by version number
      if (a === "create_database.sql") return -1;
      if (b === "create_database.sql") return 1;

      const vA = a.match(/v(\d+)/);
      const vB = b.match(/v(\d+)/);
      if (vA && vB) return parseInt(vA[1]) - parseInt(vB[1]);
      return a.localeCompare(b);
    });

  console.log(`Fichiers trouvés : ${files.length}`);
  for (const f of files) {
    console.log(`  - ${f}`);
  }

  // 2. Parser chaque fichier dans l'ordre
  const tables = new Map<string, TableInfo>();

  for (const file of files) {
    const filePath = join(SQL_DIR, file);
    const content = readFileSync(filePath, "utf-8");

    console.log(`\nParsing ${file}...`);

    // Parse in correct order: drops first (for re-creation), then creates, then alters
    parseDropTables(content, tables);
    parseCreateTables(content, tables);
    parseAlterDropFK(content, tables);
    parseAlterDropColumn(content, tables);
    parseAlterAddColumns(content, tables);
    parseAlterAddFK(content, tables);

    const activeCount = [...tables.values()].filter((t) => !t.dropped).length;
    console.log(`  Tables actives : ${activeCount}`);
  }

  // 3. Build output
  const system = buildSystemDefinition(tables);

  console.log(`\n=== Résultat ===`);
  console.log(`Tables  : ${system.nodes.length}`);
  console.log(`FK edges: ${system.edges.length}`);

  // Group summary
  const groups = new Map<string, number>();
  for (const node of system.nodes) {
    const g = node.group ?? "Autre";
    groups.set(g, (groups.get(g) ?? 0) + 1);
  }
  console.log("Groupes :");
  for (const [g, count] of [...groups.entries()].sort()) {
    console.log(`  ${g}: ${count} tables`);
  }

  // 4. Write JSON
  const output = JSON.stringify(system, null, 2);
  writeFileSync(OUTPUT_FILE, output, "utf-8");
  console.log(`\nJSON écrit dans : ${OUTPUT_FILE}`);
}

main();
