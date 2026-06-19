import { Link } from "react-router-dom";
import { downloadDocument } from "../../api/documentApi";
import { uniqueDocumentSources } from "../../lib/uniqueDocumentSources";
import styles from "./DocumentSourceLinks.module.css";

export interface DocumentSourceLinkItem {
  documentId: string;
  documentName: string;
}

interface DocumentSourceLinksProps {
  sources: DocumentSourceLinkItem[];
  testIdPrefix?: string;
}

export function DocumentSourceLinks({ sources, testIdPrefix = "chat" }: DocumentSourceLinksProps) {
  const documents = uniqueDocumentSources(sources);

  if (documents.length === 0) {
    return null;
  }

  return (
    <ul className={styles.list} data-testid={`${testIdPrefix}-sources`}>
      {documents.map((document) => (
        <li
          key={document.documentId}
          className={styles.item}
          data-testid={`${testIdPrefix}-source-${document.documentId}`}
        >
          <span className={styles.name}>{document.documentName}</span>
          <div className={styles.actions}>
            <Link
              to={`/documents/${document.documentId}/view`}
              target="_blank"
              rel="noopener noreferrer"
              className={styles.actionLink}
              data-testid={`${testIdPrefix}-source-open-${document.documentId}`}
            >
              Open
            </Link>
            <button
              type="button"
              className={styles.actionButton}
              data-testid={`${testIdPrefix}-source-download-${document.documentId}`}
              onClick={() => void downloadDocument(document.documentId, document.documentName)}
            >
              Download
            </button>
          </div>
        </li>
      ))}
    </ul>
  );
}
