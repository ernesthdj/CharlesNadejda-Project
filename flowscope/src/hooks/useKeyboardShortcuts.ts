import { useEffect } from "react";

interface KeyboardShortcutsOptions {
  onSearchOpen: () => void;
  onSearchClose: () => void;
  isSearchOpen: boolean;
  onCloseInspector: () => void;
  onFitView?: () => void;
}

/** Retourne true si l'élément actif est un champ de saisie */
function isInputFocused(): boolean {
  const el = document.activeElement;
  if (!el) return false;
  const tag = el.tagName.toLowerCase();
  if (tag === "input" || tag === "textarea" || tag === "select") return true;
  if ((el as HTMLElement).isContentEditable) return true;
  return false;
}

export function useKeyboardShortcuts({
  onSearchOpen,
  onSearchClose,
  isSearchOpen,
  onCloseInspector,
  onFitView,
}: KeyboardShortcutsOptions) {
  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      // Ctrl+K / Cmd+K — ouvrir la search modal
      if ((e.ctrlKey || e.metaKey) && e.key === "k") {
        e.preventDefault();
        if (isSearchOpen) {
          onSearchClose();
        } else {
          onSearchOpen();
        }
        return;
      }

      // Escape — fermer search ou inspecteur
      if (e.key === "Escape") {
        if (isSearchOpen) {
          onSearchClose();
        } else {
          onCloseInspector();
        }
        return;
      }

      // F — fitView (seulement si aucun input n'est focus)
      if (e.key === "f" || e.key === "F") {
        if (!isInputFocused() && !isSearchOpen && onFitView) {
          onFitView();
        }
      }
    }

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isSearchOpen, onSearchOpen, onSearchClose, onCloseInspector, onFitView]);
}
