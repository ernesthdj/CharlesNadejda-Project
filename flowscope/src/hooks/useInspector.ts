import { useState, useCallback } from "react";
import type { FlowNode, SystemDefinition } from "@/types";

export function useInspector() {
  const [selectedNode, setSelectedNode] = useState<FlowNode | null>(null);
  const [system, setSystem] = useState<SystemDefinition | null>(null);

  const openInspector = useCallback(
    (node: FlowNode, sys: SystemDefinition) => {
      setSelectedNode(node);
      setSystem(sys);
    },
    [],
  );

  const closeInspector = useCallback(() => {
    setSelectedNode(null);
    setSystem(null);
  }, []);

  return {
    isOpen: selectedNode !== null,
    selectedNode,
    system,
    openInspector,
    closeInspector,
  };
}
