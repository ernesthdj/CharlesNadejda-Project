import { CheckSquare, AlertTriangle, Info, HelpCircle } from "lucide-react";
import type { Annotation, AnnotationType, SystemDefinition, FlowNode } from "@/types";
import { formatRelativeDate } from "@/utils/dates";

const ANNOTATION_ICONS: Record<AnnotationType, typeof CheckSquare> = {
  todo: CheckSquare,
  warning: AlertTriangle,
  info: Info,
  question: HelpCircle,
};

const ANNOTATION_COLORS: Record<AnnotationType, string> = {
  todo: "#58a6ff",
  warning: "#d29922",
  info: "#8b949e",
  question: "#bc8cff",
};

interface AnnotationsListProps {
  annotations: Annotation[];
  systems: SystemDefinition[];
  onAnnotationClick: (annotation: Annotation) => void;
}

/** Résout le label d'un node à partir de son ID dans un système */
function resolveNodeLabel(nodeId: string, systems: SystemDefinition[]): string {
  for (const sys of systems) {
    const node = sys.nodes.find((n: FlowNode) => n.id === nodeId);
    if (node) return node.label;
  }
  return nodeId.split(":").pop() ?? nodeId;
}

/** Résout le label d'un système à partir de son ID */
function resolveSystemLabel(systemId: string, systems: SystemDefinition[]): string {
  const sys = systems.find((s) => s.id === systemId);
  return sys?.label ?? systemId;
}

export function AnnotationsList({ annotations, systems, onAnnotationClick }: AnnotationsListProps) {
  // Grouper par système
  const grouped = annotations.reduce<Record<string, Annotation[]>>((acc, ann) => {
    if (!acc[ann.systemId]) acc[ann.systemId] = [];
    acc[ann.systemId].push(ann);
    return acc;
  }, {});

  // Trier les annotations par date desc dans chaque groupe
  for (const key of Object.keys(grouped)) {
    grouped[key].sort(
      (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    );
  }

  if (annotations.length === 0) {
    return (
      <div className="px-4 py-8 text-center">
        <p className="text-fs-text-muted text-xs italic">Aucune note pour le moment.</p>
      </div>
    );
  }

  return (
    <div className="py-2">
      {Object.entries(grouped).map(([systemId, anns]) => (
        <div key={systemId} className="mb-3">
          <div className="px-4 py-1">
            <span className="text-[10px] uppercase tracking-wider text-fs-text-muted font-semibold">
              {resolveSystemLabel(systemId, systems)} ({anns.length})
            </span>
          </div>
          {anns.map((annotation) => {
            const AIcon = ANNOTATION_ICONS[annotation.type];
            return (
              <button
                key={annotation.id}
                onClick={() => onAnnotationClick(annotation)}
                className="w-full flex items-start gap-2 px-4 py-2 text-left hover:bg-fs-surface-hover transition-colors"
              >
                <AIcon
                  size={14}
                  className="shrink-0 mt-0.5"
                  style={{ color: ANNOTATION_COLORS[annotation.type] }}
                />
                <div className="flex-1 min-w-0">
                  <p className="text-fs-text text-xs truncate">{annotation.text}</p>
                  <div className="flex items-center gap-1.5 mt-0.5">
                    <span className="text-[10px] text-fs-text-muted truncate">
                      {resolveNodeLabel(annotation.nodeId, systems)}
                    </span>
                    <span className="text-[10px] text-fs-text-muted">·</span>
                    <span className="text-[10px] text-fs-text-muted">
                      {formatRelativeDate(annotation.updatedAt ?? annotation.createdAt)}
                    </span>
                  </div>
                </div>
              </button>
            );
          })}
        </div>
      ))}
    </div>
  );
}
