import { useState } from "react";
import { useReactFlow } from "@xyflow/react";
import { ZoomIn, ZoomOut, Maximize2, LayoutGrid, ChevronDown, Group } from "lucide-react";
import { LAYOUT_OPTIONS, type LayoutStrategy } from "@/utils/layout";

interface ToolbarProps {
  onRelayout: () => void;
  isLayouting: boolean;
  strategy: LayoutStrategy;
  onStrategyChange: (strategy: LayoutStrategy) => void;
  showGroups: boolean;
  onToggleGroups?: () => void;
}

export function Toolbar({ onRelayout, isLayouting, strategy, onStrategyChange, showGroups, onToggleGroups }: ToolbarProps) {
  const { zoomIn, zoomOut, fitView } = useReactFlow();
  const [menuOpen, setMenuOpen] = useState(false);

  const activeOption = LAYOUT_OPTIONS.find((o) => o.id === strategy)!;

  const btnClass =
    "p-2 rounded-lg bg-fs-surface/90 border border-fs-border text-fs-text-secondary hover:text-fs-text hover:bg-fs-surface-hover transition-colors backdrop-blur-sm";

  return (
    <div className="absolute top-3 right-3 flex items-center gap-1.5 z-10">
      {/* Layout selector */}
      <div className="relative">
        <button
          onClick={() => setMenuOpen(!menuOpen)}
          className={`${btnClass} flex items-center gap-1.5 pr-2`}
          title="Changer l'organisation"
        >
          <LayoutGrid size={16} />
          <span className="text-xs">{activeOption.label}</span>
          <ChevronDown size={12} className={`transition-transform ${menuOpen ? "rotate-180" : ""}`} />
        </button>

        {menuOpen && (
          <>
            {/* Backdrop */}
            <div className="fixed inset-0 z-10" onClick={() => setMenuOpen(false)} />

            {/* Menu */}
            <div className="absolute top-full right-0 mt-1 w-64 bg-fs-surface border border-fs-border rounded-lg shadow-xl z-20 overflow-hidden animate-fade-in">
              {LAYOUT_OPTIONS.map((option) => (
                <button
                  key={option.id}
                  onClick={() => {
                    onStrategyChange(option.id);
                    setMenuOpen(false);
                  }}
                  className={`w-full text-left px-3 py-2.5 flex flex-col gap-0.5 transition-colors ${
                    option.id === strategy
                      ? "bg-fs-accent/10 border-l-2 border-fs-accent"
                      : "hover:bg-fs-surface-hover border-l-2 border-transparent"
                  }`}
                >
                  <span className={`text-sm font-medium ${option.id === strategy ? "text-fs-accent" : "text-fs-text"}`}>
                    {option.label}
                  </span>
                  <span className="text-xs text-fs-text-muted">{option.description}</span>
                </button>
              ))}
            </div>
          </>
        )}
      </div>

      {/* Group toggle */}
      {onToggleGroups && (
        <button
          onClick={onToggleGroups}
          className={`${btnClass} ${showGroups ? "!border-fs-accent !text-fs-accent" : ""}`}
          title={showGroups ? "Masquer les groupes" : "Afficher les groupes"}
        >
          <Group size={16} />
        </button>
      )}

      {/* Relayout */}
      <button
        onClick={onRelayout}
        disabled={isLayouting}
        className={`${btnClass} ${isLayouting ? "opacity-50 cursor-wait" : ""}`}
        title="Recalculer le layout"
      >
        <LayoutGrid size={16} className={isLayouting ? "animate-spin" : ""} />
      </button>

      {/* Zoom controls */}
      <div className="w-px h-6 bg-fs-border" />

      <button onClick={() => zoomIn()} className={btnClass} title="Zoom in">
        <ZoomIn size={16} />
      </button>
      <button onClick={() => zoomOut()} className={btnClass} title="Zoom out">
        <ZoomOut size={16} />
      </button>
      <button onClick={() => fitView({ padding: 0.15 })} className={btnClass} title="Ajuster la vue (F)">
        <Maximize2 size={16} />
      </button>
    </div>
  );
}
