import {
  Database, Monitor, Layers, Box, Globe,
  Route, Layout, GitBranch, Package, HelpCircle,
  LayoutDashboard,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";
import type { NodeType } from "@/types";

/** Mapping NodeType -> composant Lucide */
export const NODE_ICONS: Record<NodeType, LucideIcon> = {
  table:      Database,
  form:       Monitor,
  dal:        Layers,
  model:      Box,
  controller: Globe,
  route:      Route,
  view:       Layout,
  process:    GitBranch,
  stock:      Package,
  custom:     HelpCircle,
};

/** Mapping nom string (depuis JSON system.icon) -> composant Lucide */
const ICON_MAP: Record<string, LucideIcon> = {
  Database, Monitor, Layers, Box, Globe,
  Route, Layout, GitBranch, Package, HelpCircle,
  LayoutDashboard,
};

/** Résout un nom d'icône string en composant Lucide */
export function resolveIcon(name: string): LucideIcon {
  return ICON_MAP[name] ?? HelpCircle;
}
