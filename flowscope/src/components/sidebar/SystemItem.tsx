import type { SystemDefinition } from "@/types";
import { resolveIcon } from "@/utils/icons";

interface SystemItemProps {
  system: SystemDefinition;
  isActive: boolean;
  onClick: () => void;
}

export function SystemItem({ system, isActive, onClick }: SystemItemProps) {
  const Icon = resolveIcon(system.icon);

  return (
    <button
      onClick={onClick}
      className={`
        w-full flex items-center gap-3 px-4 py-2.5 text-left text-sm
        transition-colors duration-150 rounded-r-md
        ${isActive
          ? "bg-fs-surface-active text-fs-text border-l-2 border-fs-accent"
          : "text-fs-text-secondary hover:bg-fs-surface-hover hover:text-fs-text border-l-2 border-transparent"
        }
      `}
    >
      <Icon size={18} className={isActive ? "text-fs-accent" : ""} />
      <span className="truncate">{system.label}</span>
      <span className="ml-auto text-xs text-fs-text-muted">
        {system.nodes.length}
      </span>
    </button>
  );
}
