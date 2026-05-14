import { useCallback } from "react";
import {
  ReactFlow,
  Background,
  BackgroundVariant,
  ConnectionLineType,
  type Node,
  type Edge,
  type NodeChange,
  type NodeMouseHandler,
  type DefaultEdgeOptions,
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import { CustomNode } from "./CustomNode";
import { GroupNode } from "./GroupNode";
import { MiniMapWrapper } from "./MiniMapWrapper";
import { Toolbar } from "./Toolbar";
import type { LayoutStrategy } from "@/utils/layout";

const nodeTypes = { custom: CustomNode, group: GroupNode };

const defaultEdgeOptions: DefaultEdgeOptions = {
  type: "smoothstep",
};

interface FlowCanvasProps {
  nodes: Node[];
  edges: Edge[];
  onNodeClick?: (nodeId: string) => void;
  onPaneClick?: () => void;
  onNodesChange?: (changes: NodeChange[]) => void;
  onRelayout?: () => void;
  isLayouting?: boolean;
  strategy?: LayoutStrategy;
  onStrategyChange?: (strategy: LayoutStrategy) => void;
  showGroups?: boolean;
  onToggleGroups?: () => void;
}

export function FlowCanvas({ nodes, edges, onNodeClick, onPaneClick, onNodesChange, onRelayout, isLayouting, strategy, onStrategyChange, showGroups, onToggleGroups }: FlowCanvasProps) {
  const handleNodeClick: NodeMouseHandler = useCallback(
    (_event, node) => {
      onNodeClick?.(node.id);
    },
    [onNodeClick],
  );

  const handlePaneClick = useCallback(() => {
    onPaneClick?.();
  }, [onPaneClick]);

  return (
    <ReactFlow
      nodes={nodes}
      edges={edges}
      nodeTypes={nodeTypes}
      defaultEdgeOptions={defaultEdgeOptions}
      connectionLineType={ConnectionLineType.SmoothStep}
      onNodesChange={onNodesChange}
      onNodeClick={handleNodeClick}
      onPaneClick={handlePaneClick}
      nodesDraggable
      fitView
      fitViewOptions={{ padding: 0.15 }}
      minZoom={0.05}
      maxZoom={2.5}
      proOptions={{ hideAttribution: true }}
    >
      <Background
        variant={BackgroundVariant.Dots}
        gap={20}
        size={1}
        color="var(--color-fs-canvas-dot)"
      />
      <MiniMapWrapper />
      {onRelayout && strategy && onStrategyChange && (
        <Toolbar
          onRelayout={onRelayout}
          isLayouting={isLayouting ?? false}
          strategy={strategy}
          onStrategyChange={onStrategyChange}
          showGroups={showGroups ?? false}
          onToggleGroups={onToggleGroups}
        />
      )}
    </ReactFlow>
  );
}
