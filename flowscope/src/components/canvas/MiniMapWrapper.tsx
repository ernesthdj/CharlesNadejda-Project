import { MiniMap } from "@xyflow/react";
import { NODE_BORDER_COLORS } from "@/utils/colors";
import type { NodeType } from "@/types";

export function MiniMapWrapper() {
  return (
    <MiniMap
      nodeColor={(node) => {
        const type = (node.data as { type?: NodeType }).type ?? "custom";
        return NODE_BORDER_COLORS[type] ?? "#8b949e";
      }}
      maskColor="rgba(13, 17, 23, 0.7)"
      style={{ width: 180, height: 120 }}
      pannable
      zoomable
    />
  );
}
