import { ChevronRight } from "lucide-react";
import type { BreadcrumbItem } from "@/hooks/useNavigation";

interface BreadcrumbProps {
  items: BreadcrumbItem[];
  onNavigate: (id: string) => void;
}

export function Breadcrumb({ items, onNavigate }: BreadcrumbProps) {
  return (
    <div className="flex items-center gap-1 px-4 py-2 bg-fs-surface border-b border-fs-border text-sm">
      {items.map((item, index) => {
        const isLast = index === items.length - 1;

        return (
          <span key={item.id} className="flex items-center gap-1">
            {index > 0 && <ChevronRight size={14} className="text-fs-text-muted" />}
            {isLast ? (
              <span className="text-fs-text font-medium">{item.label}</span>
            ) : (
              <button
                onClick={() => onNavigate(item.id)}
                className="text-fs-text-secondary hover:text-fs-accent transition-colors"
              >
                {item.label}
              </button>
            )}
          </span>
        );
      })}
    </div>
  );
}
