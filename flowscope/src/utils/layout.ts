import ELK, { type ElkNode } from "elkjs/lib/elk.bundled.js";
import type { Node, Edge } from "@xyflow/react";
import type { LayoutDirection } from "@/types";

const NODE_WIDTH = 250;
const NODE_HEIGHT = 85;
const GROUP_PADDING = 50;

const elk = new ELK();

/** Stratégies de layout disponibles */
export type LayoutStrategy =
  | "hierarchical"
  | "tree"
  | "force"
  | "radial"
  | "box"
  | "layered-horizontal";

export interface LayoutOption {
  id: LayoutStrategy;
  label: string;
  description: string;
}

export const LAYOUT_OPTIONS: LayoutOption[] = [
  { id: "hierarchical", label: "Hiérarchique", description: "Couches verticales — minimise les croisements" },
  { id: "layered-horizontal", label: "Horizontal", description: "Couches horizontales — flux gauche → droite" },
  { id: "tree", label: "Arbre", description: "Structure arborescente depuis les racines" },
  { id: "force", label: "Force", description: "Simulation physique — nodes connectés se rapprochent" },
  { id: "radial", label: "Radial", description: "Disposition en cercles concentriques" },
  { id: "box", label: "Grille", description: "Rangées compactes — pas de croisements" },
];

/** Config ELK par stratégie */
function getElkOptions(
  strategy: LayoutStrategy,
  direction: LayoutDirection,
): Record<string, string> {
  const base: Record<string, string> = {
    "elk.separateConnectedComponents": "true",
    "elk.spacing.componentComponent": "120",
    "elk.padding": "[top=40,left=40,bottom=40,right=40]",
  };

  switch (strategy) {
    case "hierarchical":
      return {
        ...base,
        "elk.algorithm": "layered",
        "elk.direction": direction === "LR" ? "RIGHT" : "DOWN",
        "elk.layered.crossingMinimization.strategy": "LAYER_SWEEP",
        "elk.layered.crossingMinimization.greedySwitch.type": "TWO_SIDED",
        "elk.layered.thoroughness": "100",
        "elk.layered.nodePlacement.strategy": "BRANDES_KOEPF",
        "elk.layered.nodePlacement.bk.fixedAlignment": "BALANCED",
        "elk.spacing.nodeNode": "60",
        "elk.layered.spacing.nodeNodeBetweenLayers": "100",
        "elk.spacing.edgeNode": "40",
        "elk.spacing.edgeEdge": "20",
        "elk.layered.cycleBreaking.strategy": "INTERACTIVE",
        "elk.edgeRouting": "POLYLINE",
      };

    case "layered-horizontal":
      return {
        ...base,
        "elk.algorithm": "layered",
        "elk.direction": "RIGHT",
        "elk.layered.crossingMinimization.strategy": "LAYER_SWEEP",
        "elk.layered.crossingMinimization.greedySwitch.type": "TWO_SIDED",
        "elk.layered.thoroughness": "100",
        "elk.layered.nodePlacement.strategy": "BRANDES_KOEPF",
        "elk.layered.nodePlacement.bk.fixedAlignment": "BALANCED",
        "elk.spacing.nodeNode": "50",
        "elk.layered.spacing.nodeNodeBetweenLayers": "120",
        "elk.spacing.edgeNode": "40",
        "elk.spacing.edgeEdge": "20",
        "elk.edgeRouting": "POLYLINE",
      };

    case "tree":
      return {
        ...base,
        "elk.algorithm": "mrtree",
        "elk.direction": direction === "LR" ? "RIGHT" : "DOWN",
        "elk.spacing.nodeNode": "50",
        "elk.mrtree.searchOrder": "DFS",
        "elk.edgeRouting": "POLYLINE",
      };

    case "force":
      return {
        ...base,
        "elk.algorithm": "force",
        "elk.force.iterations": "300",
        "elk.force.repulsivePower": "2",
        "elk.spacing.nodeNode": "100",
        "elk.force.temperature": "0.01",
      };

    case "radial":
      return {
        ...base,
        "elk.algorithm": "radial",
        "elk.radial.orderId": "1",
        "elk.spacing.nodeNode": "60",
      };

    case "box":
      return {
        ...base,
        "elk.algorithm": "box",
        "elk.spacing.nodeNode": "30",
        "elk.box.packingMode": "GROUP_MIXED",
        "elk.contentAlignment": "V_CENTER H_CENTER",
      };
  }
}

/** Applique un layout ELK flat (sans groupes visuels) */
export async function applyElkLayout(
  nodes: Node[],
  edges: Edge[],
  direction: LayoutDirection = "TB",
  strategy: LayoutStrategy = "hierarchical",
): Promise<Node[]> {
  if (nodes.length === 0) return nodes;

  const nodeIds = new Set(nodes.map((n) => n.id));

  const elkGraph = {
    id: "root",
    layoutOptions: getElkOptions(strategy, direction),
    children: nodes.map((n) => ({
      id: n.id,
      width: NODE_WIDTH,
      height: NODE_HEIGHT,
    })),
    edges: edges
      .filter((e) => nodeIds.has(e.source) && nodeIds.has(e.target))
      .map((e) => ({
        id: e.id,
        sources: [e.source],
        targets: [e.target],
      })),
  };

  try {
    const layout = await elk.layout(elkGraph);
    return applyPositions(nodes, layout);
  } catch {
    return applyGridFallback(nodes);
  }
}

/**
 * Applique un layout ELK avec compound nodes par groupe.
 * Retourne les nodes originaux + les group nodes parents.
 * Les positions des enfants sont relatives à leur parent.
 */
export async function applyGroupedElkLayout(
  nodes: Node[],
  edges: Edge[],
  direction: LayoutDirection = "TB",
  strategy: LayoutStrategy = "hierarchical",
): Promise<Node[]> {
  if (nodes.length === 0) return nodes;

  // Collecter les groupes
  const groups = new Map<string, Node[]>();
  const ungrouped: Node[] = [];

  for (const node of nodes) {
    const group = (node.data as Record<string, unknown>).group as string | undefined;
    if (group) {
      if (!groups.has(group)) groups.set(group, []);
      groups.get(group)!.push(node);
    } else {
      ungrouped.push(node);
    }
  }

  const nodeIds = new Set(nodes.map((n) => n.id));
  const opts = getElkOptions(strategy, direction);

  // ELK graph avec compound nodes
  const elkChildren: ElkNode[] = [];

  for (const [groupName, groupNodes] of groups) {
    const groupId = `group:${groupName}`;
    elkChildren.push({
      id: groupId,
      layoutOptions: {
        ...opts,
        "elk.padding": `[top=${GROUP_PADDING + 10},left=${GROUP_PADDING - 20},bottom=${GROUP_PADDING - 20},right=${GROUP_PADDING - 20}]`,
      },
      children: groupNodes.map((n) => ({
        id: n.id,
        width: NODE_WIDTH,
        height: NODE_HEIGHT,
      })),
    });
  }

  for (const node of ungrouped) {
    elkChildren.push({ id: node.id, width: NODE_WIDTH, height: NODE_HEIGHT });
  }

  // Edges : ELK gère les edges inter-groupes automatiquement avec INCLUDE_CHILDREN
  const elkEdges = edges
    .filter((e) => nodeIds.has(e.source) && nodeIds.has(e.target))
    .map((e) => ({
      id: e.id,
      sources: [e.source],
      targets: [e.target],
    }));

  const elkGraph = {
    id: "root",
    layoutOptions: {
      ...opts,
      "elk.hierarchyHandling": "INCLUDE_CHILDREN",
    },
    children: elkChildren,
    edges: elkEdges,
  };

  try {
    const layout = await elk.layout(elkGraph);
    return buildGroupedNodes(nodes, groups, ungrouped, layout);
  } catch {
    // Fallback : layout flat sans groupes
    return applyElkLayout(nodes, edges, direction, strategy);
  }
}

/** Construit les nodes React Flow avec parents group nodes */
function buildGroupedNodes(
  originalNodes: Node[],
  groups: Map<string, Node[]>,
  ungrouped: Node[],
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  layout: any,
): Node[] {
  const result: Node[] = [];

  if (!layout.children) return originalNodes;

  for (const child of layout.children) {
    if (child.children) {
      // C'est un group node
      const groupName = child.id.replace("group:", "");
      const groupNodes = groups.get(groupName) ?? [];

      // Créer le group node parent
      result.push({
        id: child.id,
        type: "group",
        position: { x: child.x ?? 0, y: child.y ?? 0 },
        data: { label: groupName, childCount: groupNodes.length },
        style: {
          width: child.width ?? 400,
          height: child.height ?? 300,
        },
        draggable: true,
        selectable: false,
      });

      // Positionner les enfants relativement au parent
      for (const subChild of child.children) {
        const original = originalNodes.find((n) => n.id === subChild.id);
        if (original) {
          result.push({
            ...original,
            position: { x: subChild.x ?? 0, y: subChild.y ?? 0 },
            parentId: child.id,
            extent: "parent" as const,
          });
        }
      }
    } else {
      // Node sans groupe
      const original = ungrouped.find((n) => n.id === child.id);
      if (original) {
        result.push({
          ...original,
          position: { x: child.x ?? 0, y: child.y ?? 0 },
        });
      }
    }
  }

  return result;
}

/** Extraire les positions depuis le résultat ELK */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function applyPositions(nodes: Node[], layout: any): Node[] {
  const positions = new Map<string, { x: number; y: number }>();

  if (layout.children) {
    for (const child of layout.children) {
      positions.set(child.id, { x: child.x ?? 0, y: child.y ?? 0 });
    }
  }

  return nodes.map((node) => {
    const pos = positions.get(node.id);
    if (!pos) return node;
    return { ...node, position: pos };
  });
}

/** Fallback : grille simple */
function applyGridFallback(nodes: Node[]): Node[] {
  const cols = 5;
  const spacingX = NODE_WIDTH + 100;
  const spacingY = NODE_HEIGHT + 80;

  return nodes.map((node, i) => ({
    ...node,
    position: {
      x: (i % cols) * spacingX,
      y: Math.floor(i / cols) * spacingY,
    },
  }));
}
