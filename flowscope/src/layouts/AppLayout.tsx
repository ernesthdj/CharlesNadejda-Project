import { useCallback, useMemo } from "react";
import { ReactFlowProvider } from "@xyflow/react";
import type { FlowScopeProject, Annotation, AnnotationType } from "@/types";
import { useNavigation } from "@/hooks/useNavigation";
import { useFlowData } from "@/hooks/useFlowData";
import { useInspector } from "@/hooks/useInspector";
import { useSearch } from "@/hooks/useSearch";
import { useImpactAnalysis } from "@/hooks/useImpactAnalysis";
import { useImpactStyles } from "@/hooks/useImpactStyles";
import { useKeyboardShortcuts } from "@/hooks/useKeyboardShortcuts";
import { useAnnotations } from "@/hooks/useAnnotations";
import { Sidebar } from "@/components/sidebar/Sidebar";
import { Breadcrumb } from "@/components/canvas/Breadcrumb";
import { FlowCanvas } from "@/components/canvas/FlowCanvas";
import { InspectorPanel } from "@/components/inspector/InspectorPanel";
import { SearchModal } from "@/components/search/SearchModal";
import { ANNOTATION_PRIORITY } from "@/components/canvas/CustomNode";

interface AppLayoutProps {
  project: FlowScopeProject;
}

export function AppLayout({ project }: AppLayoutProps) {
  const {
    activeSystemId,
    activeSystem,
    breadcrumb,
    navigateTo,
    goToOverview,
  } = useNavigation(project.systems);

  const { nodes, edges, isLayouting, strategy, showGroups, relayout, onNodesChange, changeStrategy, toggleGroups } = useFlowData(activeSystem);
  const { isOpen, selectedNode, system, openInspector, closeInspector } = useInspector();
  const impact = useImpactAnalysis(project);
  const { nodes: styledNodes, edges: styledEdges } = useImpactStyles({
    nodes,
    edges,
    isActive: impact.isActive,
    targetNodeId: impact.targetNodeId,
    impact,
  });
  const {
    results: searchResults,
    isOpen: isSearchOpen,
    search,
    open: openSearch,
    close: closeSearch,
  } = useSearch(project.systems);

  const {
    annotations: allAnnotations,
    getForNode,
    add: addAnnotation,
    update: updateAnnotation,
    remove: removeAnnotation,
    getAll,
  } = useAnnotations();

  // Enrich nodes with annotation badge data (applied after impact styles)
  const enrichedNodes = useMemo(() => {
    return styledNodes.map((node) => {
      const nodeAnnotations = allAnnotations.filter((a) => a.nodeId === node.id);
      if (nodeAnnotations.length === 0) return node;

      let topType: AnnotationType = "info";
      let topPriority = -1;
      for (const ann of nodeAnnotations) {
        const p = ANNOTATION_PRIORITY[ann.type];
        if (p > topPriority) {
          topPriority = p;
          topType = ann.type;
        }
      }

      return {
        ...node,
        data: {
          ...node.data,
          annotationCount: nodeAnnotations.length,
          annotationTopType: topType,
        },
      };
    });
  }, [styledNodes, allAnnotations]);

  // Annotations for the currently selected node
  const nodeAnnotations = useMemo(
    () => (selectedNode ? getForNode(selectedNode.id) : []),
    [selectedNode, getForNode],
  );

  const handleSearchSelect = useCallback(
    (result: import("@/types/ui").SearchResult) => {
      closeSearch();
      // Navigate to the correct system if needed
      if (result.systemId !== activeSystemId) {
        navigateTo(result.systemId);
      }
      // Open inspector for the selected node
      const sys = project.systems.find((s) => s.id === result.systemId);
      if (sys) {
        openInspector(result.node, sys);
      }
    },
    [closeSearch, activeSystemId, navigateTo, project.systems, openInspector],
  );

  useKeyboardShortcuts({
    onSearchOpen: openSearch,
    onSearchClose: closeSearch,
    isSearchOpen,
    onCloseInspector: closeInspector,
  });

  const handleSystemClick = useCallback(
    (systemId: string) => {
      closeInspector();
      impact.clear();
      navigateTo(systemId);
    },
    [closeInspector, impact, navigateTo],
  );

  const handleBreadcrumbNavigate = useCallback(
    (id: string) => {
      closeInspector();
      impact.clear();
      if (id === "overview") {
        goToOverview();
      } else {
        navigateTo(id);
      }
    },
    [closeInspector, impact, goToOverview, navigateTo],
  );

  // Handle annotation click from sidebar: navigate to system + select node + open inspector
  const handleAnnotationClick = useCallback(
    (annotation: Annotation) => {
      if (annotation.systemId !== activeSystemId) {
        navigateTo(annotation.systemId);
      }
      const sys = project.systems.find((s) => s.id === annotation.systemId);
      if (sys) {
        const node = sys.nodes.find((n) => n.id === annotation.nodeId);
        if (node) openInspector(node, sys);
      }
    },
    [activeSystemId, navigateTo, project.systems, openInspector],
  );

  const handleAnalyzeImpact = useCallback(
    (nodeId: string) => {
      impact.analyze(nodeId);
    },
    [impact],
  );

  const handleNodeClick = useCallback(
    (nodeId: string) => {
      if (!activeSystem) return;
      const node = activeSystem.nodes.find((n) => n.id === nodeId);
      if (node) openInspector(node, activeSystem);
    },
    [activeSystem, openInspector],
  );

  const handlePaneClick = useCallback(() => {
    closeInspector();
  }, [closeInspector]);

  return (
    <div className="flex h-full w-full">
      <Sidebar
        projectName={project.name}
        systems={project.systems}
        activeSystemId={activeSystemId}
        onSystemClick={handleSystemClick}
        onSearchClick={openSearch}
        annotations={getAll()}
        onAnnotationClick={handleAnnotationClick}
      />

      <div className="flex-1 flex flex-col min-w-0">
        <Breadcrumb items={breadcrumb} onNavigate={handleBreadcrumbNavigate} />

        <div className="flex-1 relative">
          <ReactFlowProvider>
            <FlowCanvas
              nodes={enrichedNodes}
              edges={styledEdges}
              onNodeClick={handleNodeClick}
              onPaneClick={handlePaneClick}
              onNodesChange={onNodesChange}
              onRelayout={relayout}
              isLayouting={isLayouting}
              strategy={strategy}
              onStrategyChange={changeStrategy}
              showGroups={showGroups}
              onToggleGroups={toggleGroups}
            />
          </ReactFlowProvider>
        </div>
      </div>

      {isOpen && selectedNode && system && (
        <InspectorPanel
          node={selectedNode}
          system={system}
          project={project}
          onClose={closeInspector}
          onNodeNavigate={handleNodeClick}
          annotations={nodeAnnotations}
          onAddAnnotation={addAnnotation}
          onUpdateAnnotation={updateAnnotation}
          onRemoveAnnotation={removeAnnotation}
          onAnalyzeImpact={handleAnalyzeImpact}
          impactActive={impact.isActive}
          impactUpstream={impact.upstreamNodes}
          impactDownstream={impact.downstreamNodes}
          impactStats={impact.stats}
          onClearImpact={impact.clear}
        />
      )}

      <SearchModal
        isOpen={isSearchOpen}
        results={searchResults}
        onSearch={search}
        onSelect={handleSearchSelect}
        onClose={closeSearch}
      />
    </div>
  );
}
