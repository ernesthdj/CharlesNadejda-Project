import { useState, useRef, useEffect, useCallback } from "react";
import { createPortal } from "react-dom";
import { Search } from "lucide-react";
import type { SearchResult } from "@/types/ui";
import { SearchResultItem } from "./SearchResultItem";

interface SearchModalProps {
  isOpen: boolean;
  results: SearchResult[];
  onSearch: (query: string) => void;
  onSelect: (result: SearchResult) => void;
  onClose: () => void;
}

const MAX_VISIBLE = 12;

export function SearchModal({
  isOpen,
  results,
  onSearch,
  onSelect,
  onClose,
}: SearchModalProps) {
  const [query, setQuery] = useState("");
  const [activeIndex, setActiveIndex] = useState(0);
  const inputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLDivElement>(null);

  // Reset state when modal opens
  useEffect(() => {
    if (isOpen) {
      setQuery("");
      setActiveIndex(0);
      // Focus input after portal renders
      requestAnimationFrame(() => inputRef.current?.focus());
    }
  }, [isOpen]);

  // Reset active index when results change
  useEffect(() => {
    setActiveIndex(0);
  }, [results]);

  // Scroll active item into view
  useEffect(() => {
    if (!listRef.current) return;
    const active = listRef.current.children[activeIndex] as HTMLElement | undefined;
    active?.scrollIntoView({ block: "nearest" });
  }, [activeIndex]);

  const handleChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const val = e.target.value;
      setQuery(val);
      onSearch(val);
    },
    [onSearch],
  );

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      switch (e.key) {
        case "ArrowDown":
          e.preventDefault();
          setActiveIndex((i) => (i + 1) % Math.max(results.length, 1));
          break;
        case "ArrowUp":
          e.preventDefault();
          setActiveIndex((i) =>
            i <= 0 ? Math.max(results.length - 1, 0) : i - 1,
          );
          break;
        case "Enter":
          e.preventDefault();
          if (results[activeIndex]) {
            onSelect(results[activeIndex]);
          }
          break;
        case "Escape":
          e.preventDefault();
          onClose();
          break;
      }
    },
    [results, activeIndex, onSelect, onClose],
  );

  if (!isOpen) return null;

  return createPortal(
    <div
      className="fixed inset-0 z-50 flex justify-center items-start pt-[15vh]"
      onClick={onClose}
      role="presentation"
    >
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/50" />

      {/* Modal */}
      <div
        className="relative w-[560px] max-w-[90vw] bg-fs-surface border border-fs-border rounded-xl shadow-2xl overflow-hidden"
        onClick={(e) => e.stopPropagation()}
        role="dialog"
        aria-label="Recherche de nodes"
      >
        {/* Search input */}
        <div className="flex items-center gap-3 px-4 py-3 border-b border-fs-border">
          <Search size={18} className="text-fs-text-muted shrink-0" />
          <input
            ref={inputRef}
            type="text"
            value={query}
            onChange={handleChange}
            onKeyDown={handleKeyDown}
            placeholder='Rechercher un node, type:form, qui utilise...'
            className="flex-1 bg-transparent text-sm text-fs-text placeholder:text-fs-text-muted outline-none"
            spellCheck={false}
          />
          <kbd className="text-[10px] bg-fs-bg px-1.5 py-0.5 rounded border border-fs-border text-fs-text-muted">
            Esc
          </kbd>
        </div>

        {/* Results */}
        {query.trim() !== "" && (
          <div
            ref={listRef}
            className="max-h-[384px] overflow-y-auto"
            role="listbox"
          >
            {results.length === 0 ? (
              <div className="px-4 py-8 text-center text-sm text-fs-text-muted">
                Aucun résultat pour &laquo;{query}&raquo;
              </div>
            ) : (
              results.slice(0, MAX_VISIBLE).map((r, i) => (
                <SearchResultItem
                  key={`${r.systemId}:${r.node.id}`}
                  result={r}
                  isActive={i === activeIndex}
                  onClick={() => onSelect(r)}
                />
              ))
            )}

            {results.length > MAX_VISIBLE && (
              <div className="px-4 py-2 text-center text-[11px] text-fs-text-muted border-t border-fs-border">
                {results.length - MAX_VISIBLE} résultats supplémentaires...
              </div>
            )}
          </div>
        )}

        {/* Hints */}
        {query.trim() === "" && (
          <div className="px-4 py-4 space-y-1.5">
            <p className="text-xs text-fs-text-muted">Raccourcis de recherche :</p>
            <div className="grid grid-cols-2 gap-1 text-[11px] text-fs-text-muted">
              <span><kbd className="bg-fs-bg px-1 rounded border border-fs-border">type:form</kbd> Filtrer par type</span>
              <span><kbd className="bg-fs-bg px-1 rounded border border-fs-border">group:DAL</kbd> Filtrer par groupe</span>
              <span><kbd className="bg-fs-bg px-1 rounded border border-fs-border">qui utilise X</kbd> Edges entrants</span>
              <span><kbd className="bg-fs-bg px-1 rounded border border-fs-border">deps X</kbd> Edges sortants</span>
            </div>
          </div>
        )}
      </div>
    </div>,
    document.body,
  );
}
