import { Navigate, useParams } from "react-router-dom";
import { DocumentDetailPage } from "./ui/DocumentDetailPage";

export function DocumentDetailRoute() {
  const { documentId } = useParams();

  if (!documentId) {
    return <Navigate to="/" replace />;
  }

  return <DocumentDetailPage documentId={documentId} />;
}
