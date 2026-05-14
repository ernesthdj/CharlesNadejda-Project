import { useState, useCallback, useMemo } from "react";
import Fuse from "fuse.js";
import type { FlowNode, FlowEdge, SystemDefinition } from "@/types";
import type { SearchResult } from "@/types/ui";

/** Item indexable pour Fuse.js — node + contexte système */
interface SearchableItem {
  node: FlowNode;
  systemId: string;
  systemLabel: string;
  /** Valeurs de metadata aplaties pour la recherche */
  metadataValues: string;
}

/** Patterns spéciaux reconnus par la command palette */
type QueryPattern =
  | { kind: "uses"; target: string }
  | { kind: "deps"; target: string }
  | { kind: "type"; value: string }
  | { kind: "group"; value: string }
  | { kind: "fuzzy"; query: string };

const USES_RE = /^(?:qui utilise|uses)\s+(.+)$/i;
const DEPS_RE = /^(?:d[eé]pendances de|deps)\s+(.+)$/i;
const TYPE_RE = /^type:(\S+)$/i;
const GROUP_RE = /^group:(.+)$/i;

function parseQuery(raw: string): QueryPattern {
  const q = raw.trim();

  let m = USES_RE.exec(q);
  if (m) return { kind: "uses", target: m[1].trim() };

  m = DEPS_RE.exec(q);
  if (m) return { kind: "deps", target: m[1].trim() };

  m = TYPE_RE.exec(q);
  if (m) return { kind: "type", value: m[1].toLowerCase() };

  m = GROUP_RE.exec(q);
  if (m) return { kind: "group", value: m[1].trim().toLowerCase() };

  return { kind: "fuzzy", query: q };
}

/**
 * Trouve un node par label (insensible à la casse) dans tous les systèmes.
 * Retourne l'id du node ou null.
 */
function findNodeIdByLabel(
  systems: SystemDefinition[],
  label: string,
): string | null {
  const lower = label.toLowerCase();
  for (const sys of systems) {
    for (const n of sys.nodes) {
      if (n.label.toLowerCase() === lower || n.id.toLowerCase() === lower) {
        return n.id;
      }
    }
  }
  return null;
}

/** Collecte tous les edges de tous les systèmes */
function allEdges(systems: SystemDefinition[]): FlowEdge[] {
  return systems.flatMap((s) => s.edges);
}

/** Map nodeId -> { node, systemId, systemLabel } pour lookup rapide */
function buildNodeMap(
  systems: SystemDefinition[],
): Map<string, { node: FlowNode; systemId: string; systemLabel: string }> {
  const map = new Map<
    string,
    { node: FlowNode; systemId: string; systemLabel: string }
  >();
  for (const sys of systems) {
    for (const n of sys.nodes) {
      map.set(n.id, { node: n, systemId: sys.id, systemLabel: sys.label });
    }
  }
  return map;
}

export function useSearch(systems: SystemDefinition[]) {
  const [results, setResults] = useState<SearchResult[]>([]);
  const [isOpen, setIsOpen] = useState(false);

  // --- Index Fuse.js : construit une seule fois par ensemble de systèmes ---
  const { fuse, nodeMap, edges: allEdgeList } = useMemo(() => {
    const items: SearchableItem[] = [];
    for (const sys of systems) {
      // Exclure le système overview de l'indexation
      if (sys.id === "overview") continue;
      for (const node of sys.nodes) {
        items.push({
          node,
          systemId: sys.id,
          systemLabel: sys.label,
          metadataValues: Object.values(node.metadata)
            .flat()
            .join(" "),
        });
      }
    }

    const fuseInstance = new Fuse(items, {
      keys: [
        { name: "node.label", weight: 3 },
        { name: "node.description", weight: 1.5 },
        { name: "node.tags", weight: 1.2 },
        { name: "node.group", weight: 1 },
        { name: "metadataValues", weight: 0.8 },
      ],
      threshold: 0.35,
      includeScore: true,
      ignoreLocation: true,
    });

    return {
      fuse: fuseInstance,
      nodeMap: buildNodeMap(systems),
      edges: allEdges(systems),
    };
  }, [systems]);

  const search = useCallback(
    (raw: string) => {
      const trimmed = raw.trim();
      if (!trimmed) {
        setResults([]);
        return;
      }

      const pattern = parseQuery(trimmed);

      switch (pattern.kind) {
        case "fuzzy": {
          const fuseResults = fuse.search(pattern.query, { limit: 20 });
          setResults(
            fuseResults.map((r) => ({
              node: r.item.node,
              systemId: r.item.systemId,
              systemLabel: r.item.systemLabel,
              score: r.score ?? 0,
            })),
          );
          break;
        }

        case "type": {
          const matched: SearchResult[] = [];
          for (const sys of systems) {
            if (sys.id === "overview") continue;
            for (const n of sys.nodes) {
              if (n.type.toLowerCase() === pattern.value) {
                matched.push({
                  node: n,
                  systemId: sys.id,
                  systemLabel: sys.label,
                  score: 0,
                });
              }
            }
          }
          setResults(matched);
          break;
        }

        case "group": {
          const matched: SearchResult[] = [];
          for (const sys of systems) {
            if (sys.id === "overview") continue;
            for (const n of sys.nodes) {
              if (n.group?.toLowerCase() === pattern.value) {
                matched.push({
                  node: n,
                  systemId: sys.id,
                  systemLabel: sys.label,
                  score: 0,
                });
              }
            }
          }
          setResults(matched);
          break;
        }

        case "uses": {
          // Nodes qui ont un edge VERS le target (entrant)
          const targetId = findNodeIdByLabel(systems, pattern.target);
          if (!targetId) {
            setResults([]);
            break;
          }
          const sourceIds = allEdgeList
            .filter((e) => e.target === targetId)
            .map((e) => e.source);

          const uniqueIds = [...new Set(sourceIds)];
          const matched: SearchResult[] = [];
          for (const id of uniqueIds) {
            const entry = nodeMap.get(id);
            if (entry) {
              matched.push({
                node: entry.node,
                systemId: entry.systemId,
                systemLabel: entry.systemLabel,
                score: 0,
              });
            }
          }
          setResults(matched);
          break;
        }

        case "deps": {
          // Nodes vers lesquels le target a un edge (sortant)
          const targetId = findNodeIdByLabel(systems, pattern.target);
          if (!targetId) {
            setResults([]);
            break;
          }
          const depIds = allEdgeList
            .filter((e) => e.source === targetId)
            .map((e) => e.target);

          const uniqueIds = [...new Set(depIds)];
          const matched: SearchResult[] = [];
          for (const id of uniqueIds) {
            const entry = nodeMap.get(id);
            if (entry) {
              matched.push({
                node: entry.node,
                systemId: entry.systemId,
                systemLabel: entry.systemLabel,
                score: 0,
              });
            }
          }
          setResults(matched);
          break;
        }
      }
    },
    [fuse, systems, nodeMap, allEdgeList],
  );

  const open = useCallback(() => setIsOpen(true), []);
  const close = useCallback(() => {
    setIsOpen(false);
    setResults([]);
  }, []);

  return { results, isOpen, search, open, close };
}
