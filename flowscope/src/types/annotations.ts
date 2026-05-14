/** Types d'annotation disponibles */
export type AnnotationType = "todo" | "warning" | "info" | "question";

/** Une annotation attachée à un node */
export interface Annotation {
  id: string;
  nodeId: string;
  systemId: string;
  type: AnnotationType;
  text: string;
  createdAt: string;
  updatedAt?: string;
}

/** Structure stockée dans localStorage */
export interface AnnotationsStore {
  annotations: Annotation[];
}
