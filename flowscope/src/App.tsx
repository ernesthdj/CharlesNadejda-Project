import { AppLayout } from "@/layouts/AppLayout";
import type { FlowScopeProject } from "@/types";
import projectData from "@/data/project.json";

function App() {
  const project = projectData as FlowScopeProject;

  return <AppLayout project={project} />;
}

export default App;
