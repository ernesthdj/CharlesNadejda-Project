import { useState, useCallback, useMemo } from "react";
import type { FlowScopeProject, FlowEdge } from "@/types";

export interface ImpactStats {
  total: number;
  upstream: number;
  downstream: number;
  direct: number;
  systems: number;
}

export interface ImpactResult {
  impactedNodes: Set<string>;
  upstreamNodes: Set<string>;
  downstreamNodes: Set<string>;
  directDeps: Set<string>;
  impactedEdges: Set<string>;
  stats: ImpactStats;
}

const EMPTY_RESULT: ImpactResult = {
  impactedNodes: new Set(),
  upstreamNodes: new Set(),
  downstreamNodes: new Set(),
  directDeps: new Set(),
  impactedEdges: new Set(),
  stats: { total: 0, upstream: 0, downstream: 0, direct: 0, systems: 0 },
};

export function useImpactAnalysis(project: FlowScopeProject) {
  const [targetNodeId, setTargetNodeId] = useState<string | null>(null);
  const [result, setResult] = useState<ImpactResult>(EMPTY_RESULT);

  // Pre-build adjacency maps across ALL systems for fast traversal
  const { outgoing, incoming } = useMemo(() => {
    const edges: FlowEdge[] = [];
    const out = new Map<string, FlowEdge[]>();
    const inc = new Map<string, FlowEdge[]>();

    for (const sys of project.systems) {
      for (const edge of sys.edges) {
        edges.push(edge);

        const outList = out.get(edge.source);
        if (outList) outList.push(edge);
        else out.set(edge.source, [edge]);

        const incList = inc.get(edge.target);
        if (incList) incList.push(edge);
        else inc.set(edge.target, [edge]);
      }
    }

    return { outgoing: out, incoming: inc };
  }, [project]);

  // Collect all node IDs mapped to their system IDs for stats
  const nodeToSystems = useMemo(() => {
    const map = new Map<string, Set<string>>();
    for (const sys of project.systems) {
      for (const node of sys.nodes) {
        const existing = map.get(node.id);
        if (existing) existing.add(sys.id);
        else map.set(node.id, new Set([sys.id]));
      }
    }
    return map;
  }, [project]);

  const analyze = useCallback(
    (nodeId: string) => {
      const upstreamNodes = new Set<string>();
      const downstreamNodes = new Set<string>();
      const impactedEdges = new Set<string>();
      const directDeps = new Set<string>();

      // Collect direct connections (1st level)
      const directOut = outgoing.get(nodeId) ?? [];
      for (const edge of directOut) {
        directDeps.add(edge.target);
        impactedEdges.add(edge.id);
      }
      const directIn = incoming.get(nodeId) ?? [];
      for (const edge of directIn) {
        directDeps.add(edge.source);
        impactedEdges.add(edge.id);
      }

      // BFS upstream: follow incoming edges recursively
      const upVisited = new Set<string>([nodeId]);
      const upQueue = [nodeId];
      while (upQueue.length > 0) {
        const current = upQueue.shift()!;
        const inEdges = incoming.get(current) ?? [];
        for (const edge of inEdges) {
          impactedEdges.add(edge.id);
          if (!upVisited.has(edge.source)) {
            upVisited.add(edge.source);
            upstreamNodes.add(edge.source);
            upQueue.push(edge.source);
          }
        }
      }

      // BFS downstream: follow outgoing edges recursively
      const downVisited = new Set<string>([nodeId]);
      const downQueue = [nodeId];
      while (downQueue.length > 0) {
        const current = downQueue.shift()!;
        const outEdges = outgoing.get(current) ?? [];
        for (const edge of outEdges) {
          impactedEdges.add(edge.id);
          if (!downVisited.has(edge.target)) {
            downVisited.add(edge.target);
            downstreamNodes.add(edge.target);
            downQueue.push(edge.target);
          }
        }
      }

      // Combine all impacted nodes
      const impactedNodes = new Set<string>([
        ...upstreamNodes,
        ...downstreamNodes,
      ]);

      // Count distinct systems touched
      const touchedSystems = new Set<string>();
      for (const nid of impactedNodes) {
        const sysList = nodeToSystems.get(nid);
        if (sysList) {
          for (const s of sysList) touchedSystems.add(s);
        }
      }
      // Also count the target node's systems
      const targetSystems = nodeToSystems.get(nodeId);
      if (targetSystems) {
        for (const s of targetSystems) touchedSystems.add(s);
      }

      const stats: ImpactStats = {
        total: impactedNodes.size,
        upstream: upstreamNodes.size,
        downstream: downstreamNodes.size,
        direct: directDeps.size,
        systems: touchedSystems.size,
      };

      setTargetNodeId(nodeId);
      setResult({
        impactedNodes,
        upstreamNodes,
        downstreamNodes,
        directDeps,
        impactedEdges,
        stats,
      });
    },
    [outgoing, incoming, nodeToSystems],
  );

  const clear = useCallback(() => {
    setTargetNodeId(null);
    setResult(EMPTY_RESULT);
  }, []);

  return {
    ...result,
    targetNodeId,
    isActive: targetNodeId !== null,
    analyze,
    clear,
  };
}
