import { memo } from "react";
import { Handle, Position } from "@xyflow/react";
import type { NodeProps } from "@xyflow/react";
import type { FlowNode, AnnotationType } from "@/types";
import { NODE_ICONS } from "@/utils/icons";
import { NODE_BG_CLASSES } from "@/utils/colors";
import { getMetadataSummary } from "@/utils/transform";

/** Priorité des types d'annotation (plus haut = plus prioritaire) */
const ANNOTATION_PRIORITY: Record<AnnotationType, number> = {
  warning: 3,
  todo: 2,
  question: 1,
  info: 0,
};

/** Couleur de badge selon le type d'annotation le plus prioritaire */
const BADGE_COLORS: Record<AnnotationType, string> = {
  warning: "#d29922",
  todo: "#58a6ff",
  question: "#bc8cff",
  info: "#8b949e",
};

type CustomNodeData = FlowNode & {
  borderColor: string;
  annotationCount?: number;
  annotationTopType?: AnnotationType;
};

function CustomNodeComponent({ data, selected }: NodeProps) {
  const nodeData = data as unknown as CustomNodeData;
  const Icon = NODE_ICONS[nodeData.type];
  const badgeClass = NODE_BG_CLASSES[nodeData.type];
  const summary = getMetadataSummary(nodeData.metadata);

  const hasAnnotations = (nodeData.annotationCount ?? 0) > 0;

  return (
    <div
      className={`
        rounded-lg border px-3 py-2 min-w-[180px] max-w-[280px]
        bg-fs-surface transition-all duration-150 relative
        ${selected ? "shadow-lg shadow-fs-accent/20" : "hover:bg-fs-surface-hover"}
      `}
      style={{
        borderColor: selected ? nodeData.borderColor : "var(--color-fs-border)",
        boxShadow: selected ? `0 0 12px ${nodeData.borderColor}40` : undefined,
      }}
    >
      <Handle type="target" position={Position.Top} className="!bg-fs-border !w-2 !h-2" />

      {/* Annotation badge */}
      {hasAnnotations && nodeData.annotationTopType && (
        <div
          className="absolute -top-1.5 -right-1.5 flex items-center justify-center rounded-full text-white font-bold"
          style={{
            width: 18,
            height: 18,
            fontSize: 10,
            lineHeight: 1,
            backgroundColor: BADGE_COLORS[nodeData.annotationTopType],
          }}
        >
          {nodeData.annotationCount}
        </div>
      )}

      <div className="flex items-center gap-2 mb-1">
        <Icon size={16} style={{ color: nodeData.borderColor }} />
        <span className="text-fs-text text-sm font-semibold truncate flex-1">
          {nodeData.label}
        </span>
        <span className={`text-[10px] font-medium uppercase px-1.5 py-0.5 rounded ${badgeClass}`}>
          {nodeData.type}
        </span>
      </div>

      {summary && (
        <div className="text-fs-text-secondary text-xs">
          {summary}
        </div>
      )}

      <Handle type="source" position={Position.Bottom} className="!bg-fs-border !w-2 !h-2" />
    </div>
  );
}

export const CustomNode = memo(CustomNodeComponent);

export { ANNOTATION_PRIORITY };
