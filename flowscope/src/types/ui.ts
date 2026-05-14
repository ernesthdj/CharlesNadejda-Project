import type { FlowNode, SystemDefinition } from "./schema";

/** État de la navigation entre vues */
export interface NavigationState {
  activeSystemId: string | null;
  history: string[];
}

/** État du panneau inspecteur */
export interface InspectorState {
  isOpen: boolean;
  selectedNode: FlowNode | null;
  system: SystemDefinition | null;
}

/** Résultat de recherche Fuse.js */
export interface SearchResult {
  node: FlowNode;
  systemId: string;
  systemLabel: string;
  score: number;
}
