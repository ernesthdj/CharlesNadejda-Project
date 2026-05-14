import { useState, useCallback, useMemo } from "react";
import type { SystemDefinition } from "@/types";

export interface BreadcrumbItem {
  id: string;
  label: string;
}

export function useNavigation(systems: SystemDefinition[]) {
  const [activeSystemId, setActiveSystemId] = useState<string>(
    systems[0]?.id ?? "",
  );
  const [history, setHistory] = useState<string[]>([]);

  const activeSystem = useMemo(
    () => systems.find((s) => s.id === activeSystemId) ?? null,
    [systems, activeSystemId],
  );

  const navigateTo = useCallback(
    (systemId: string) => {
      setHistory((prev) => [...prev, activeSystemId]);
      setActiveSystemId(systemId);
    },
    [activeSystemId],
  );

  const goBack = useCallback(() => {
    setHistory((prev) => {
      if (prev.length === 0) return prev;
      const newHistory = [...prev];
      const last = newHistory.pop()!;
      setActiveSystemId(last);
      return newHistory;
    });
  }, []);

  const goToOverview = useCallback(() => {
    const overview = systems.find((s) => s.id === "overview");
    if (overview) {
      setHistory([]);
      setActiveSystemId("overview");
    }
  }, [systems]);

  const breadcrumb: BreadcrumbItem[] = useMemo(() => {
    const items: BreadcrumbItem[] = [{ id: "overview", label: "Overview" }];
    if (activeSystemId !== "overview" && activeSystem) {
      items.push({ id: activeSystem.id, label: activeSystem.label });
    }
    return items;
  }, [activeSystemId, activeSystem]);

  return {
    activeSystemId,
    activeSystem,
    breadcrumb,
    history,
    navigateTo,
    goBack,
    goToOverview,
  };
}
