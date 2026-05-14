/** Types de nodes supportés par FlowScope */
export type NodeType =
  | "table"
  | "form"
  | "dal"
  | "model"
  | "controller"
  | "route"
  | "view"
  | "process"
  | "stock"
  | "custom";

/** Types de relations entre nodes */
export type EdgeType =
  | "fk"
  | "inheritance"
  | "dependency"
  | "flow"
  | "data";

/** Direction du layout automatique */
export type LayoutDirection = "TB" | "LR";

/** Position d'un node sur le canvas */
export interface NodePosition {
  x: number;
  y: number;
}

/** Un node dans le graphe */
export interface FlowNode {
  id: string;
  type: NodeType;
  label: string;
  description?: string;
  filePath?: string;
  metadata: Record<string, string[]>;
  tags?: string[];
  group?: string;
  position?: NodePosition;
}

/** Un edge (relation) entre deux nodes */
export interface FlowEdge {
  id: string;
  source: string;
  target: string;
  label?: string;
  type?: EdgeType;
  animated?: boolean;
}

/** Un système = une vue (DB, C# Forms, BOM Pipeline, etc.) */
export interface SystemDefinition {
  id: string;
  label: string;
  icon: string;
  description: string;
  layoutDirection?: LayoutDirection;
  nodes: FlowNode[];
  edges: FlowEdge[];
}

/** Projet FlowScope complet */
export interface FlowScopeProject {
  name: string;
  version: string;
  lastParsed: string;
  systems: SystemDefinition[];
}
