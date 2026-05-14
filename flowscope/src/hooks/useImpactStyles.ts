import { useMemo } from "react";
import type { Node, Edge } from "@xyflow/react";
import type { ImpactResult } from "./useImpactAnalysis";

/** Impact analysis color palette */
const COLORS = {
  upstream: "#58a6ff",
  downstream: "#f85149",
  target: "#d29922",
} as const;

const DIMMED_OPACITY = 0.15;
const DIMMED_EDGE_OPACITY = 0.1;

interface UseImpactStylesParams {
  nodes: Node[];
  edges: Edge[];
  isActive: boolean;
  targetNodeId: string | null;
  impact: ImpactResult;
}

/**
 * Applies visual styles to nodes and edges based on impact analysis results.
 * Returns new arrays — does NOT mutate the originals.
 */
export function useImpactStyles({
  nodes,
  edges,
  isActive,
  targetNodeId,
  impact,
}: UseImpactStylesParams) {
  const styledNodes = useMemo(() => {
    if (!isActive || !targetNodeId) return nodes;

    return nodes.map((node) => {
      const id = node.id;

      // Target node
      if (id === targetNodeId) {
        return {
          ...node,
          style: {
            ...node.style,
            opacity: 1,
            borderWidth: 3,
            borderColor: COLORS.target,
            borderStyle: "solid" as const,
            boxShadow: `0 0 16px ${COLORS.target}80`,
            borderRadius: 8,
          },
        };
      }

      const isUpstream = impact.upstreamNodes.has(id);
      const isDownstream = impact.downstreamNodes.has(id);
      const isDirect = impact.directDeps.has(id);

      // Node is in the impact chain
      if (isUpstream || isDownstream) {
        const color = isUpstream ? COLORS.upstream : COLORS.downstream;
        const width = isDirect ? 3 : 2;

        return {
          ...node,
          style: {
            ...node.style,
            opacity: 1,
            borderWidth: width,
            borderColor: color,
            borderStyle: "solid" as const,
            borderRadius: 8,
          },
        };
      }

      // Node is NOT in the impact chain — dim it
      return {
        ...node,
        style: {
          ...node.style,
          opacity: DIMMED_OPACITY,
        },
      };
    });
  }, [nodes, isActive, targetNodeId, impact]);

  const styledEdges = useMemo(() => {
    if (!isActive) return edges;

    return edges.map((edge) => {
      if (impact.impactedEdges.has(edge.id)) {
        return {
          ...edge,
          style: {
            ...edge.style,
            opacity: 1,
            strokeWidth: 2.5,
          },
        };
      }

      return {
        ...edge,
        style: {
          ...edge.style,
          opacity: DIMMED_EDGE_OPACITY,
        },
      };
    });
  }, [edges, isActive, impact]);

  return { nodes: styledNodes, edges: styledEdges };
}
