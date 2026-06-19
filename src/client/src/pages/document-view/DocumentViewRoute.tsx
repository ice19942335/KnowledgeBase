import { Navigate, useParams } from "react-router-dom";
import { DocumentViewPage } from "./ui/DocumentViewPage";

export function DocumentViewRoute() {
  const { documentId } = useParams();

  if (!documentId) {
    return <Navigate to="/" replace />;
  }

  return <DocumentViewPage documentId={documentId} />;
}
