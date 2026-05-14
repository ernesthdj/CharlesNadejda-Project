import { useState, useEffect, useCallback } from "react";
import {
  type Node,
  type Edge,
  type NodeChange,
  applyNodeChanges,
} from "@xyflow/react";
import type { SystemDefinition } from "@/types";
import { transformNodes, transformEdges } from "@/utils/transform";
import { applyElkLayout, applyGroupedElkLayout, type LayoutStrategy } from "@/utils/layout";

export function useFlowData(system: SystemDefinition | null) {
  const [nodes, setNodes] = useState<Node[]>([]);
  const [edges, setEdges] = useState<Edge[]>([]);
  const [isLayouting, setIsLayouting] = useState(false);
  const [strategy, setStrategy] = useState<LayoutStrategy>("hierarchical");
  const [showGroups, setShowGroups] = useState(false);

  const doLayout = useCallback(async (
    sys: SystemDefinition,
    strat: LayoutStrategy,
    grouped: boolean,
  ) => {
    setIsLayouting(true);
    const rfEdges = transformEdges(sys.edges);
    const rfNodes = transformNodes(sys.nodes);

    const layoutFn = grouped ? applyGroupedElkLayout : applyElkLayout;
    const layouted = await layoutFn(
      rfNodes,
      rfEdges,
      sys.layoutDirection ?? "TB",
      strat,
    );
    setNodes(layouted);
    setEdges(rfEdges);
    setIsLayouting(false);
  }, []);

  useEffect(() => {
    if (!system) {
      setNodes([]);
      setEdges([]);
      return;
    }
    doLayout(system, strategy, showGroups);
  }, [system, doLayout, strategy, showGroups]);

  const onNodesChange = useCallback(
    (changes: NodeChange[]) => {
      setNodes((prev) => applyNodeChanges(changes, prev));
    },
    [],
  );

  const relayout = useCallback(() => {
    if (system) doLayout(system, strategy, showGroups);
  }, [system, doLayout, strategy, showGroups]);

  const changeStrategy = useCallback(
    (newStrategy: LayoutStrategy) => setStrategy(newStrategy),
    [],
  );

  const toggleGroups = useCallback(
    () => setShowGroups((prev) => !prev),
    [],
  );

  return {
    nodes, edges, isLayouting, strategy, showGroups,
    relayout, onNodesChange, changeStrategy, toggleGroups,
  };
}
