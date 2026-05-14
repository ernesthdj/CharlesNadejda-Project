import { X } from "lucide-react";
import type { FlowScopeProject } from "@/types";
import type { ImpactStats } from "@/hooks/useImpactAnalysis";
import { NODE_ICONS } from "@/utils/icons";

interface ImpactPanelProps {
  project: FlowScopeProject;
  upstreamNodes: Set<string>;
  downstreamNodes: Set<string>;
  stats: ImpactStats;
  onClear: () => void;
  onNodeClick: (nodeId: string) => void;
}

/** Resolves a nodeId to its FlowNode across all systems */
function findNode(project: FlowScopeProject, nodeId: string) {
  for (const sys of project.systems) {
    const node = sys.nodes.find((n) => n.id === nodeId);
    if (node) return node;
  }
  return null;
}

export function ImpactPanel({
  project,
  upstreamNodes,
  downstreamNodes,
  stats,
  onClear,
  onNodeClick,
}: ImpactPanelProps) {
  const upstreamList = Array.from(upstreamNodes);
  const downstreamList = Array.from(downstreamNodes);

  return (
    <div className="border-b border-fs-border px-4 py-3 space-y-3">
      {/* Header badge */}
      <div className="flex items-center justify-between">
        <span className="inline-flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wider px-2 py-1 rounded bg-amber-500/15 text-amber-400">
          Impact Analysis
        </span>
        <button
          onClick={onClear}
          className="text-fs-text-muted hover:text-fs-text p-1 rounded"
          title="Fermer l'analyse d'impact"
        >
          <X size={14} />
        </button>
      </div>

      {/* Summary */}
      <p className="text-fs-text-secondary text-xs">
        {stats.total} {stats.total === 1 ? "element" : "elements"} {stats.total === 1 ? "impacte" : "impactes"} dans {stats.systems} {stats.systems === 1 ? "systeme" : "systemes"}
      </p>

      {/* Upstream section */}
      {upstreamList.length > 0 && (
        <div>
          <h4 className="text-[10px] uppercase tracking-wider font-semibold mb-1" style={{ color: "#58a6ff" }}>
            Upstream ({stats.upstream})
          </h4>
          <ul className="space-y-0.5">
            {upstreamList.map((nodeId) => {
              const node = findNode(project, nodeId);
              if (!node) return null;
              const Icon = NODE_ICONS[node.type];
              return (
                <li key={nodeId}>
                  <button
                    onClick={() => onNodeClick(nodeId)}
                    className="w-full text-left flex items-center gap-2 text-xs text-fs-text-secondary hover:text-fs-accent px-2 py-1 rounded hover:bg-fs-surface-hover transition-colors"
                  >
                    <Icon size={12} className="shrink-0" style={{ color: "#58a6ff" }} />
                    <span className="truncate">{node.label}</span>
                  </button>
                </li>
              );
            })}
          </ul>
        </div>
      )}

      {/* Downstream section */}
      {downstreamList.length > 0 && (
        <div>
          <h4 className="text-[10px] uppercase tracking-wider font-semibold mb-1" style={{ color: "#f85149" }}>
            Downstream ({stats.downstream})
          </h4>
          <ul className="space-y-0.5">
            {downstreamList.map((nodeId) => {
              const node = findNode(project, nodeId);
              if (!node) return null;
              const Icon = NODE_ICONS[node.type];
              return (
                <li key={nodeId}>
                  <button
                    onClick={() => onNodeClick(nodeId)}
                    className="w-full text-left flex items-center gap-2 text-xs text-fs-text-secondary hover:text-fs-accent px-2 py-1 rounded hover:bg-fs-surface-hover transition-colors"
                  >
                    <Icon size={12} className="shrink-0" style={{ color: "#f85149" }} />
                    <span className="truncate">{node.label}</span>
                  </button>
                </li>
              );
            })}
          </ul>
        </div>
      )}
    </div>
  );
}
