import { useState } from "react";
import type { SystemDefinition, Annotation } from "@/types";
import { SystemItem } from "./SystemItem";
import { AnnotationsList } from "./AnnotationsList";
import { Search, MessageSquare } from "lucide-react";

type SidebarView = "systems" | "annotations";

interface SidebarProps {
  projectName: string;
  systems: SystemDefinition[];
  activeSystemId: string;
  onSystemClick: (systemId: string) => void;
  onSearchClick: () => void;
  annotations: Annotation[];
  onAnnotationClick: (annotation: Annotation) => void;
}

export function Sidebar({
  projectName,
  systems,
  activeSystemId,
  onSystemClick,
  onSearchClick,
  annotations,
  onAnnotationClick,
}: SidebarProps) {
  const [view, setView] = useState<SidebarView>("systems");

  return (
    <aside className="w-sidebar h-full bg-fs-surface border-r border-fs-border flex flex-col shrink-0">
      {/* Header */}
      <div className="px-4 py-4 border-b border-fs-border">
        <h1 className="text-fs-accent text-base font-bold tracking-tight">FlowScope</h1>
        <p className="text-fs-text-secondary text-xs mt-0.5 truncate">{projectName}</p>
      </div>

      {/* Search button */}
      <div className="px-3 py-2 space-y-1.5">
        <button
          onClick={onSearchClick}
          className="w-full flex items-center gap-2 px-3 py-1.5 rounded-md bg-fs-surface-hover text-fs-text-muted text-xs hover:text-fs-text-secondary transition-colors cursor-pointer"
        >
          <Search size={14} />
          <span>Rechercher...</span>
          <kbd className="ml-auto text-[10px] bg-fs-bg px-1.5 py-0.5 rounded border border-fs-border">
            Ctrl+K
          </kbd>
        </button>

        {/* Notes toggle button */}
        <button
          onClick={() => setView(view === "systems" ? "annotations" : "systems")}
          className={`
            w-full flex items-center gap-2 px-3 py-1.5 rounded-md text-xs transition-colors cursor-pointer
            ${view === "annotations"
              ? "bg-fs-accent/10 text-fs-accent"
              : "bg-fs-surface-hover text-fs-text-muted hover:text-fs-text-secondary"
            }
          `}
        >
          <MessageSquare size={14} />
          <span>Notes</span>
          {annotations.length > 0 && (
            <span className="ml-auto text-[10px] bg-fs-accent/20 text-fs-accent px-1.5 py-0.5 rounded-full font-medium">
              {annotations.length}
            </span>
          )}
        </button>
      </div>

      {/* Content area */}
      <div className="flex-1 overflow-y-auto py-2">
        {view === "systems" ? (
          <>
            <div className="px-4 py-1">
              <span className="text-[10px] uppercase tracking-wider text-fs-text-muted font-semibold">
                Systèmes
              </span>
            </div>
            {systems.map((system) => (
              <SystemItem
                key={system.id}
                system={system}
                isActive={system.id === activeSystemId}
                onClick={() => onSystemClick(system.id)}
              />
            ))}
          </>
        ) : (
          <AnnotationsList
            annotations={annotations}
            systems={systems}
            onAnnotationClick={onAnnotationClick}
          />
        )}
      </div>

      {/* Footer */}
      <div className="px-4 py-3 border-t border-fs-border">
        <p className="text-[10px] text-fs-text-muted">
          {systems.reduce((acc, s) => acc + s.nodes.length, 0)} nodes · {systems.length} systèmes
        </p>
      </div>
    </aside>
  );
}
