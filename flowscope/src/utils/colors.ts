import type { NodeType, EdgeType } from "@/types";

/** Classes Tailwind pour le badge de chaque type de node */
export const NODE_BG_CLASSES: Record<NodeType, string> = {
  table:      "bg-fs-node-table/20 text-fs-node-table",
  form:       "bg-fs-node-form/20 text-fs-node-form",
  dal:        "bg-fs-node-dal/20 text-fs-node-dal",
  model:      "bg-fs-node-model/20 text-fs-node-model",
  controller: "bg-fs-node-controller/20 text-fs-node-controller",
  route:      "bg-fs-node-route/20 text-fs-node-route",
  view:       "bg-fs-node-view/20 text-fs-node-view",
  process:    "bg-fs-node-process/20 text-fs-node-process",
  stock:      "bg-fs-node-stock/20 text-fs-node-stock",
  custom:     "bg-fs-node-custom/20 text-fs-node-custom",
};

/** Couleur hex de bordure pour chaque type de node */
export const NODE_BORDER_COLORS: Record<NodeType, string> = {
  table:      "#58a6ff",
  form:       "#bc8cff",
  dal:        "#f0883e",
  model:      "#3fb950",
  controller: "#f85149",
  route:      "#79c0ff",
  view:       "#f778ba",
  process:    "#d29922",
  stock:      "#2ea043",
  custom:     "#8b949e",
};

/** Couleur hex de chaque type d'edge */
export const EDGE_COLORS: Record<EdgeType, string> = {
  fk:          "#58a6ff",
  inheritance: "#bc8cff",
  dependency:  "#8b949e",
  flow:        "#3fb950",
  data:        "#f0883e",
};
