import type { DocumentDto, DocumentStatus } from "../../model/types";
import { Button } from "../../../../shared/ui";
import styles from "./DocumentList.module.css";

interface DocumentListProps {
  documents: DocumentDto[];
  onDelete: (id: string) => void;
  deletingId?: string;
}

const statusClassName: Record<DocumentStatus, string> = {
  Uploaded: styles.processing,
  Processing: styles.processing,
  Processed: styles.processed,
  Failed: styles.failed,
};

export function DocumentList({ documents, onDelete, deletingId }: DocumentListProps) {
  if (documents.length === 0) {
    return <p className={styles.empty}>No documents yet. Upload your first file above.</p>;
  }

  return (
    <ul className={styles.list} data-testid="document-list">
      {documents.map((document) => (
        <li key={document.id} className={styles.item}>
          <div className={styles.info}>
            <span className={styles.name}>{document.name}</span>
            <span className={styles.meta}>
              {document.fileName} · {document.chunkCount} chunks
            </span>
          </div>
          <span className={`${styles.status} ${statusClassName[document.status]}`}>
            {document.status}
          </span>
          <Button
            variant="danger"
            onClick={() => onDelete(document.id)}
            disabled={deletingId === document.id}
          >
            Delete
          </Button>
        </li>
      ))}
    </ul>
  );
}
