import { memo } from "react";
import type { NodeProps } from "@xyflow/react";

/** Couleurs de fond par nom de groupe */
const GROUP_COLORS: Record<string, string> = {
  Forms:      "rgba(188, 140, 255, 0.06)",
  DAL:        "rgba(240, 136, 62, 0.06)",
  Models:     "rgba(63, 185, 80, 0.06)",
  Shell:      "rgba(88, 166, 255, 0.06)",
  Utils:      "rgba(139, 148, 158, 0.06)",
  Navigation: "rgba(121, 192, 255, 0.06)",
  Services:   "rgba(210, 153, 34, 0.06)",
  BOM:        "rgba(240, 136, 62, 0.06)",
  Catalogue:  "rgba(88, 166, 255, 0.06)",
  Auth:       "rgba(248, 81, 73, 0.06)",
  Production: "rgba(63, 185, 80, 0.06)",
  Vente:      "rgba(210, 153, 34, 0.06)",
  Activités:  "rgba(188, 140, 255, 0.06)",
};

const GROUP_BORDER_COLORS: Record<string, string> = {
  Forms:      "rgba(188, 140, 255, 0.25)",
  DAL:        "rgba(240, 136, 62, 0.25)",
  Models:     "rgba(63, 185, 80, 0.25)",
  Shell:      "rgba(88, 166, 255, 0.25)",
  Utils:      "rgba(139, 148, 158, 0.25)",
  Navigation: "rgba(121, 192, 255, 0.25)",
  Services:   "rgba(210, 153, 34, 0.25)",
  BOM:        "rgba(240, 136, 62, 0.25)",
  Catalogue:  "rgba(88, 166, 255, 0.25)",
  Auth:       "rgba(248, 81, 73, 0.25)",
  Production: "rgba(63, 185, 80, 0.25)",
  Vente:      "rgba(210, 153, 34, 0.25)",
  Activités:  "rgba(188, 140, 255, 0.25)",
};

const GROUP_TEXT_COLORS: Record<string, string> = {
  Forms:      "#bc8cff",
  DAL:        "#f0883e",
  Models:     "#3fb950",
  Shell:      "#58a6ff",
  Utils:      "#8b949e",
  Navigation: "#79c0ff",
  Services:   "#d29922",
  BOM:        "#f0883e",
  Catalogue:  "#58a6ff",
  Auth:       "#f85149",
  Production: "#3fb950",
  Vente:      "#d29922",
  Activités:  "#bc8cff",
};

function GroupNodeComponent({ data }: NodeProps) {
  const nodeData = data as { label: string; childCount: number };
  const bg = GROUP_COLORS[nodeData.label] ?? "rgba(139, 148, 158, 0.04)";
  const border = GROUP_BORDER_COLORS[nodeData.label] ?? "rgba(139, 148, 158, 0.2)";
  const textColor = GROUP_TEXT_COLORS[nodeData.label] ?? "#8b949e";

  return (
    <div
      className="rounded-xl w-full h-full"
      style={{
        backgroundColor: bg,
        border: `1.5px dashed ${border}`,
      }}
    >
      <div
        className="absolute -top-0 left-3 px-2 py-0.5 rounded-b-md text-[11px] font-semibold uppercase tracking-wider"
        style={{
          color: textColor,
          backgroundColor: "var(--color-fs-bg)",
          border: `1px solid ${border}`,
          borderTop: "none",
        }}
      >
        {nodeData.label}
        <span className="ml-1.5 opacity-60 font-normal normal-case">
          ({nodeData.childCount})
        </span>
      </div>
    </div>
  );
}

export const GroupNode = memo(GroupNodeComponent);
