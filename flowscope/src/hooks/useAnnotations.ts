import { useState, useCallback, useMemo } from "react";
import type { Annotation, AnnotationType } from "@/types";

const STORAGE_KEY = "flowscope-annotations";

function loadAnnotations(): Annotation[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw) as { annotations: Annotation[] };
    return parsed.annotations ?? [];
  } catch {
    return [];
  }
}

function saveAnnotations(annotations: Annotation[]): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify({ annotations }));
}

function generateId(): string {
  return `ann-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`;
}

export function useAnnotations() {
  const [annotations, setAnnotations] = useState<Annotation[]>(loadAnnotations);

  const persist = useCallback((next: Annotation[]) => {
    setAnnotations(next);
    saveAnnotations(next);
  }, []);

  const getForNode = useCallback(
    (nodeId: string): Annotation[] =>
      annotations.filter((a) => a.nodeId === nodeId),
    [annotations],
  );

  const add = useCallback(
    (nodeId: string, systemId: string, type: AnnotationType, text: string): void => {
      const annotation: Annotation = {
        id: generateId(),
        nodeId,
        systemId,
        type,
        text,
        createdAt: new Date().toISOString(),
      };
      persist([annotation, ...annotations]);
    },
    [annotations, persist],
  );

  const update = useCallback(
    (id: string, text: string, type: AnnotationType): void => {
      persist(
        annotations.map((a) =>
          a.id === id
            ? { ...a, text, type, updatedAt: new Date().toISOString() }
            : a,
        ),
      );
    },
    [annotations, persist],
  );

  const remove = useCallback(
    (id: string): void => {
      persist(annotations.filter((a) => a.id !== id));
    },
    [annotations, persist],
  );

  const getAll = useCallback(
    (): Annotation[] =>
      [...annotations].sort(
        (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
      ),
    [annotations],
  );

  const countBySystem = useCallback(
    (systemId: string): number =>
      annotations.filter((a) => a.systemId === systemId).length,
    [annotations],
  );

  const totalCount = useMemo(() => annotations.length, [annotations]);

  return {
    annotations,
    getForNode,
    add,
    update,
    remove,
    getAll,
    countBySystem,
    totalCount,
  };
}
