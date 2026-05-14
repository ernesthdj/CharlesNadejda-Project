import type { Node, Edge } from "@xyflow/react";
import type { FlowNode, FlowEdge, EdgeType } from "@/types";
import { NODE_BORDER_COLORS } from "./colors";
import { EDGE_COLORS } from "./colors";

/** Résumé lisible des métadonnées d'un node */
export function getMetadataSummary(metadata: Record<string, string[]>): string {
  const parts: string[] = [];
  for (const [key, values] of Object.entries(metadata)) {
    if (Array.isArray(values) && values.length > 0) {
      parts.push(`${values.length} ${key}`);
    }
  }
  return parts.join(" · ");
}

/** Transforme les FlowNodes du schéma en nodes React Flow */
export function transformNodes(nodes: FlowNode[]): Node[] {
  return nodes.map((node) => ({
    id: node.id,
    type: "custom",
    position: node.position ?? { x: 0, y: 0 },
    data: {
      ...node,
      borderColor: NODE_BORDER_COLORS[node.type],
    },
  }));
}

/** Style d'un edge selon son type */
function getEdgeStyle(type: EdgeType | undefined) {
  const color = type ? EDGE_COLORS[type] : EDGE_COLORS.dependency;

  switch (type) {
    case "inheritance":
      return { stroke: color, strokeDasharray: "6 3", strokeWidth: 1.5 };
    case "flow":
      return { stroke: color, strokeWidth: 2 };
    case "data":
      return { stroke: color, strokeDasharray: "8 4", strokeWidth: 1.5 };
    default:
      return { stroke: color, strokeWidth: 1.5 };
  }
}

/** Transforme les FlowEdges du schéma en edges React Flow */
export function transformEdges(edges: FlowEdge[]): Edge[] {
  return edges.map((edge) => ({
    id: edge.id,
    source: edge.source,
    target: edge.target,
    type: "smoothstep",
    label: edge.label,
    animated: edge.animated ?? edge.type === "flow",
    style: getEdgeStyle(edge.type),
    labelStyle: { fill: "#8b949e", fontSize: 11 },
    labelBgStyle: { fill: "#161b22", fillOpacity: 0.9 },
    labelBgPadding: [4, 2] as [number, number],
    labelBgBorderRadius: 4,
    markerEnd: edge.type === "inheritance"
      ? { type: "arrow" as const, color: EDGE_COLORS.inheritance }
      : { type: "arrowclosed" as const, color: (edge.type ? EDGE_COLORS[edge.type] : EDGE_COLORS.dependency) },
  }));
}
