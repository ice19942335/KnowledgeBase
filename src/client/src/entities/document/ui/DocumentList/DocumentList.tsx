import { Link } from "react-router-dom";
import type { DocumentDto, DocumentStatus } from "../../model/types";
import { Button } from "../../../../shared/ui";
import styles from "./DocumentList.module.css";

interface DocumentListProps {
  documents: DocumentDto[];
  onDelete: (id: string) => void;
  onRetry?: (id: string) => void;
  deletingId?: string;
  retryingId?: string;
}

const statusClassName: Record<DocumentStatus, string> = {
  Uploaded: styles.processing,
  Processing: styles.processing,
  Processed: styles.processed,
  Failed: styles.failed,
};

export function DocumentList({ documents, onDelete, onRetry, deletingId, retryingId }: DocumentListProps) {
  if (documents.length === 0) {
    return <p className={styles.empty}>No documents yet. Upload your first file above.</p>;
  }

  return (
    <ul className={styles.list} data-testid="document-list">
      {documents.map((document) => (
        <li key={document.id} className={styles.item}>
          <div className={styles.info}>
            <Link
              to={`/documents/${document.id}`}
              className={styles.nameLink}
              data-testid={`document-details-link-${document.id}`}
            >
              <span className={styles.name}>{document.name}</span>
            </Link>
            <span className={styles.meta}>
              {document.fileName} · {document.chunkCount} chunks
            </span>
          </div>
          <div className={styles.trailing}>
            <span className={`${styles.status} ${statusClassName[document.status]}`}>
              {document.status}
            </span>
            <div className={styles.actions}>
              {document.status === "Failed" && onRetry && (
                <Button
                  variant="secondary"
                  onClick={() => onRetry(document.id)}
                  disabled={retryingId === document.id}
                  data-testid={`retry-document-${document.id}`}
                >
                  Retry
                </Button>
              )}
              <Button
                variant="danger"
                onClick={() => onDelete(document.id)}
                disabled={deletingId === document.id}
              >
                Delete
              </Button>
            </div>
          </div>
        </li>
      ))}
    </ul>
  );
}
