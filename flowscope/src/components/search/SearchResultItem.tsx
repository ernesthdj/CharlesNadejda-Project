import type { SearchResult } from "@/types/ui";
import { NODE_ICONS } from "@/utils/icons";
import { NODE_BG_CLASSES } from "@/utils/colors";

interface SearchResultItemProps {
  result: SearchResult;
  isActive: boolean;
  onClick: () => void;
}

export function SearchResultItem({ result, isActive, onClick }: SearchResultItemProps) {
  const Icon = NODE_ICONS[result.node.type];
  const badgeClass = NODE_BG_CLASSES[result.node.type];

  return (
    <button
      type="button"
      className={`w-full flex items-center gap-3 px-4 py-2.5 text-left transition-colors cursor-pointer ${
        isActive
          ? "bg-fs-surface-active text-fs-text"
          : "text-fs-text-secondary hover:bg-fs-surface-hover"
      }`}
      onClick={onClick}
      onMouseDown={(e) => e.preventDefault()}
    >
      {/* Icone type */}
      <span className={`shrink-0 p-1.5 rounded ${badgeClass}`}>
        <Icon size={14} />
      </span>

      {/* Label + description */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <span className="font-semibold text-sm truncate">
            {result.node.label}
          </span>
          <span className="shrink-0 text-[10px] px-1.5 py-0.5 rounded bg-fs-surface-hover text-fs-text-muted border border-fs-border">
            {result.systemLabel}
          </span>
        </div>
        {result.node.description && (
          <p className="text-xs text-fs-text-muted truncate mt-0.5">
            {result.node.description}
          </p>
        )}
      </div>

      {/* Type badge */}
      <span className="shrink-0 text-[10px] text-fs-text-muted uppercase tracking-wide">
        {result.node.type}
      </span>
    </button>
  );
}
