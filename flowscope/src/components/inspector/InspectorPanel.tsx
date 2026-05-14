import { useState } from "react";
import { X, Copy, Plus, Trash2, CheckSquare, AlertTriangle, Info, HelpCircle, Zap } from "lucide-react";
import type { FlowNode, FlowScopeProject, SystemDefinition, Annotation, AnnotationType } from "@/types";
import type { ImpactStats } from "@/hooks/useImpactAnalysis";
import { NODE_ICONS } from "@/utils/icons";
import { NODE_BG_CLASSES } from "@/utils/colors";
import { formatRelativeDate } from "@/utils/dates";
import { ImpactPanel } from "./ImpactPanel";

/** Couleurs par type d'annotation */
const ANNOTATION_COLORS: Record<AnnotationType, string> = {
  todo: "var(--color-fs-accent, #58a6ff)",
  warning: "#d29922",
  info: "var(--color-fs-text-secondary, #8b949e)",
  question: "#bc8cff",
};

const ANNOTATION_ICONS: Record<AnnotationType, typeof CheckSquare> = {
  todo: CheckSquare,
  warning: AlertTriangle,
  info: Info,
  question: HelpCircle,
};

const ANNOTATION_LABELS: Record<AnnotationType, string> = {
  todo: "À faire",
  warning: "Attention",
  info: "Info",
  question: "Question",
};

interface InspectorPanelProps {
  node: FlowNode;
  system: SystemDefinition;
  project?: FlowScopeProject;
  onClose: () => void;
  onNodeNavigate?: (nodeId: string) => void;
  annotations: Annotation[];
  onAddAnnotation: (nodeId: string, systemId: string, type: AnnotationType, text: string) => void;
  onUpdateAnnotation: (id: string, text: string, type: AnnotationType) => void;
  onRemoveAnnotation: (id: string) => void;
  onAnalyzeImpact?: (nodeId: string) => void;
  impactActive?: boolean;
  impactUpstream?: Set<string>;
  impactDownstream?: Set<string>;
  impactStats?: ImpactStats;
  onClearImpact?: () => void;
}

export function InspectorPanel({
  node,
  system,
  project,
  onClose,
  onNodeNavigate,
  annotations,
  onAddAnnotation,
  onUpdateAnnotation,
  onRemoveAnnotation,
  onAnalyzeImpact,
  impactActive,
  impactUpstream,
  impactDownstream,
  impactStats,
  onClearImpact,
}: InspectorPanelProps) {
  const Icon = NODE_ICONS[node.type];
  const badgeClass = NODE_BG_CLASSES[node.type];

  const connections = system.edges.filter(
    (e) => e.source === node.id || e.target === node.id,
  );

  const inbound = connections.filter((e) => e.target === node.id);
  const outbound = connections.filter((e) => e.source === node.id);

  const copyPath = () => {
    if (node.filePath) navigator.clipboard.writeText(node.filePath);
  };

  // --- Notes state ---
  const [showTypeMenu, setShowTypeMenu] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editText, setEditText] = useState("");
  const [newNoteText, setNewNoteText] = useState("");
  const [newNoteType, setNewNoteType] = useState<AnnotationType | null>(null);

  const handleAddNote = (type: AnnotationType) => {
    setNewNoteType(type);
    setNewNoteText("");
    setShowTypeMenu(false);
  };

  const confirmAddNote = () => {
    if (newNoteType && newNoteText.trim()) {
      onAddAnnotation(node.id, system.id, newNoteType, newNoteText.trim());
      setNewNoteType(null);
      setNewNoteText("");
    }
  };

  const cancelAddNote = () => {
    setNewNoteType(null);
    setNewNoteText("");
  };

  const startEdit = (annotation: Annotation) => {
    setEditingId(annotation.id);
    setEditText(annotation.text);
  };

  const confirmEdit = (annotation: Annotation) => {
    if (editText.trim()) {
      onUpdateAnnotation(annotation.id, editText.trim(), annotation.type);
    }
    setEditingId(null);
    setEditText("");
  };

  const cancelEdit = () => {
    setEditingId(null);
    setEditText("");
  };

  return (
    <div className="w-inspector h-full bg-fs-surface border-l border-fs-border flex flex-col shrink-0 animate-slide-in overflow-y-auto">
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-fs-border">
        <div className="flex items-center gap-2 min-w-0">
          <Icon size={18} className="text-fs-accent shrink-0" />
          <span className="text-fs-text font-semibold text-sm truncate">{node.label}</span>
        </div>
        <button onClick={onClose} className="text-fs-text-muted hover:text-fs-text p-1 rounded">
          <X size={16} />
        </button>
      </div>

      {/* Impact Analysis panel — shown when active */}
      {impactActive && project && impactUpstream && impactDownstream && impactStats && onClearImpact && (
        <ImpactPanel
          project={project}
          upstreamNodes={impactUpstream}
          downstreamNodes={impactDownstream}
          stats={impactStats}
          onClear={onClearImpact}
          onNodeClick={(nodeId) => onNodeNavigate?.(nodeId)}
        />
      )}

      <div className="p-4 space-y-4">
        {/* Type badge */}
        <span className={`inline-block text-xs font-medium uppercase px-2 py-1 rounded ${badgeClass}`}>
          {node.type}
        </span>

        {/* Description */}
        {node.description && (
          <div>
            <h3 className="text-[10px] uppercase tracking-wider text-fs-text-muted font-semibold mb-1">Description</h3>
            <p className="text-fs-text-secondary text-sm">{node.description}</p>
          </div>
        )}

        {/* Analyze Impact button */}
        {onAnalyzeImpact && (
          <button
            onClick={() => onAnalyzeImpact(node.id)}
            className={`flex items-center gap-2 text-xs font-medium px-3 py-2 rounded border transition-colors w-full ${
              impactActive
                ? "bg-amber-500/15 text-amber-400 border-amber-500/30"
                : "bg-fs-bg text-fs-text-secondary border-fs-border hover:text-fs-accent hover:border-fs-accent/40"
            }`}
          >
            <Zap size={14} />
            <span>{impactActive ? "Recalculer l'impact" : "Analyser l'impact"}</span>
          </button>
        )}

        {/* File path */}
        {node.filePath && (
          <div>
            <h3 className="text-[10px] uppercase tracking-wider text-fs-text-muted font-semibold mb-1">Fichier</h3>
            <div className="flex items-center gap-2">
              <code className="text-fs-accent text-xs bg-fs-bg px-2 py-1 rounded flex-1 truncate">
                {node.filePath}
              </code>
              <button onClick={copyPath} className="text-fs-text-muted hover:text-fs-text p-1">
                <Copy size={14} />
              </button>
            </div>
          </div>
        )}

        {/* Metadata */}
        {Object.keys(node.metadata).length > 0 && (
          <div>
            <h3 className="text-[10px] uppercase tracking-wider text-fs-text-muted font-semibold mb-1">Métadonnées</h3>
            {Object.entries(node.metadata).map(([key, values]) => (
              <div key={key} className="mb-2">
                <span className="text-fs-text-secondary text-xs font-medium capitalize">{key}</span>
                <ul className="mt-1 space-y-0.5">
                  {values.map((val, i) => (
                    <li key={i} className="text-fs-text text-xs pl-3 border-l border-fs-border">
                      {val}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        )}

        {/* Tags */}
        {node.tags && node.tags.length > 0 && (
          <div>
            <h3 className="text-[10px] uppercase tracking-wider text-fs-text-muted font-semibold mb-1">Tags</h3>
            <div className="flex flex-wrap gap-1">
              {node.tags.map((tag) => (
                <span key={tag} className="text-xs bg-fs-bg text-fs-text-secondary px-2 py-0.5 rounded-full border border-fs-border">
                  {tag}
                </span>
              ))}
            </div>
          </div>
        )}

        {/* Connections */}
        {connections.length > 0 && (
          <div>
            <h3 className="text-[10px] uppercase tracking-wider text-fs-text-muted font-semibold mb-1">
              Connexions ({connections.length})
            </h3>

            {inbound.length > 0 && (
              <div className="mb-2">
                <span className="text-fs-text-muted text-[10px]">Entrantes</span>
                {inbound.map((edge) => (
                  <button
                    key={edge.id}
                    onClick={() => onNodeNavigate?.(edge.source)}
                    className="w-full text-left text-xs text-fs-text-secondary hover:text-fs-accent px-2 py-1 rounded hover:bg-fs-surface-hover transition-colors"
                  >
                    ← {edge.source.split(":").pop()} {edge.label && `(${edge.label})`}
                  </button>
                ))}
              </div>
            )}

            {outbound.length > 0 && (
              <div>
                <span className="text-fs-text-muted text-[10px]">Sortantes</span>
                {outbound.map((edge) => (
                  <button
                    key={edge.id}
                    onClick={() => onNodeNavigate?.(edge.target)}
                    className="w-full text-left text-xs text-fs-text-secondary hover:text-fs-accent px-2 py-1 rounded hover:bg-fs-surface-hover transition-colors"
                  >
                    → {edge.target.split(":").pop()} {edge.label && `(${edge.label})`}
                  </button>
                ))}
              </div>
            )}
          </div>
        )}

        {/* --- Notes section --- */}
        <div>
          <div className="flex items-center justify-between mb-2">
            <h3 className="text-[10px] uppercase tracking-wider text-fs-text-muted font-semibold">
              Notes ({annotations.length})
            </h3>
            <div className="relative">
              <button
                onClick={() => setShowTypeMenu(!showTypeMenu)}
                className="flex items-center gap-1 text-xs text-fs-accent hover:text-fs-text transition-colors"
              >
                <Plus size={14} />
                <span>Ajouter</span>
              </button>

              {showTypeMenu && (
                <div className="absolute right-0 top-full mt-1 bg-fs-surface border border-fs-border rounded-md shadow-lg z-10 py-1 min-w-[140px]">
                  {(["todo", "warning", "info", "question"] as AnnotationType[]).map((type) => {
                    const TypeIcon = ANNOTATION_ICONS[type];
                    return (
                      <button
                        key={type}
                        onClick={() => handleAddNote(type)}
                        className="w-full flex items-center gap-2 px-3 py-1.5 text-xs text-fs-text-secondary hover:bg-fs-surface-hover transition-colors"
                      >
                        <TypeIcon size={14} style={{ color: ANNOTATION_COLORS[type] }} />
                        <span>{ANNOTATION_LABELS[type]}</span>
                      </button>
                    );
                  })}
                </div>
              )}
            </div>
          </div>

          {/* New note input */}
          {newNoteType && (
            <div className="mb-3 p-2 rounded border border-fs-border bg-fs-bg">
              <div className="flex items-center gap-1.5 mb-1.5">
                {(() => {
                  const NIcon = ANNOTATION_ICONS[newNoteType];
                  return <NIcon size={12} style={{ color: ANNOTATION_COLORS[newNoteType] }} />;
                })()}
                <span className="text-[10px] font-medium" style={{ color: ANNOTATION_COLORS[newNoteType] }}>
                  {ANNOTATION_LABELS[newNoteType]}
                </span>
              </div>
              <textarea
                value={newNoteText}
                onChange={(e) => setNewNoteText(e.target.value)}
                placeholder="Saisir la note..."
                autoFocus
                className="w-full bg-transparent text-fs-text text-xs resize-none border-none outline-none min-h-[48px]"
                onKeyDown={(e) => {
                  if (e.key === "Enter" && !e.shiftKey) {
                    e.preventDefault();
                    confirmAddNote();
                  }
                  if (e.key === "Escape") cancelAddNote();
                }}
              />
              <div className="flex justify-end gap-1 mt-1">
                <button onClick={cancelAddNote} className="text-[10px] text-fs-text-muted hover:text-fs-text px-2 py-0.5 rounded">
                  Annuler
                </button>
                <button onClick={confirmAddNote} className="text-[10px] text-fs-accent hover:text-fs-text px-2 py-0.5 rounded bg-fs-surface-hover">
                  Ajouter
                </button>
              </div>
            </div>
          )}

          {/* Annotation list */}
          <div className="space-y-2">
            {annotations.map((annotation) => {
              const AIcon = ANNOTATION_ICONS[annotation.type];
              const isEditing = editingId === annotation.id;

              return (
                <div
                  key={annotation.id}
                  className="group p-2 rounded border border-fs-border bg-fs-bg hover:border-fs-border transition-colors"
                  style={{ borderLeftColor: ANNOTATION_COLORS[annotation.type], borderLeftWidth: 2 }}
                >
                  <div className="flex items-start gap-1.5">
                    <AIcon
                      size={13}
                      className="shrink-0 mt-0.5"
                      style={{ color: ANNOTATION_COLORS[annotation.type] }}
                    />
                    <div className="flex-1 min-w-0">
                      {isEditing ? (
                        <textarea
                          value={editText}
                          onChange={(e) => setEditText(e.target.value)}
                          autoFocus
                          className="w-full bg-transparent text-fs-text text-xs resize-none border-none outline-none min-h-[32px]"
                          onKeyDown={(e) => {
                            if (e.key === "Enter" && !e.shiftKey) {
                              e.preventDefault();
                              confirmEdit(annotation);
                            }
                            if (e.key === "Escape") cancelEdit();
                          }}
                        />
                      ) : (
                        <p
                          className="text-fs-text text-xs cursor-pointer"
                          onClick={() => startEdit(annotation)}
                        >
                          {annotation.text}
                        </p>
                      )}
                      <span className="text-[10px] text-fs-text-muted mt-0.5 block">
                        {formatRelativeDate(annotation.updatedAt ?? annotation.createdAt)}
                      </span>
                    </div>
                    <button
                      onClick={() => onRemoveAnnotation(annotation.id)}
                      className="opacity-0 group-hover:opacity-100 text-fs-text-muted hover:text-red-400 p-0.5 transition-opacity"
                    >
                      <Trash2 size={12} />
                    </button>
                  </div>
                </div>
              );
            })}
          </div>

          {annotations.length === 0 && !newNoteType && (
            <p className="text-fs-text-muted text-[10px] italic">Aucune note sur ce node.</p>
          )}
        </div>
      </div>
    </div>
  );
}
