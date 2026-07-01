import { useState } from "react";
import type { DocumentChunksGroup } from "../../model/types";
import styles from "./DocumentChunksExplorer.module.css";

interface DocumentChunksExplorerProps {
  documents: DocumentChunksGroup[];
  totalChunks: number;
  variant?: "default" | "sidebar";
}

export function DocumentChunksExplorer({
  documents,
  totalChunks,
  variant = "default",
}: DocumentChunksExplorerProps) {
  const [expandedDocumentId, setExpandedDocumentId] = useState<string | null>(
    documents[0]?.documentId ?? null,
  );

  const sectionClass = variant === "sidebar" ? styles.sectionSidebar : styles.section;

  if (documents.length === 0) {
    return (
      <p className={styles.empty} data-testid="document-chunks-empty">
        No indexed chunks yet. Upload a document and wait until processing completes.
      </p>
    );
  }

  return (
    <section className={sectionClass} data-testid="document-chunks-explorer">
      {variant === "default" && (
        <p className={styles.summary}>
          {documents.length} documents · {totalChunks} chunks indexed
        </p>
      )}

      <ul className={styles.documentList}>
        {documents.map((document) => {
          const isExpanded = expandedDocumentId === document.documentId;

          return (
            <li key={document.documentId} className={styles.documentItem}>
              <button
                type="button"
                className={styles.documentToggle}
                data-testid={`document-toggle-${document.documentId}`}
                aria-expanded={isExpanded}
                onClick={() =>
                  setExpandedDocumentId(isExpanded ? null : document.documentId)
                }
              >
                <span className={styles.documentName}>{document.documentName}</span>
                <span className={styles.chunkCount}>{document.chunks.length} chunks</span>
              </button>

              {isExpanded && (
                <ul className={styles.chunkList} data-testid={`document-chunks-${document.documentId}`}>
                  {document.chunks.map((chunk) => (
                    <li key={chunk.id} className={styles.chunkItem} data-testid={`chunk-${chunk.id}`}>
                      <div className={styles.chunkMeta}>
                        <span>Chunk #{chunk.chunkIndex}</span>
                        <span>{new Date(chunk.indexedAtUtc).toLocaleString()}</span>
                      </div>
                      <pre className={styles.chunkContent}>{chunk.content}</pre>
                    </li>
                  ))}
                </ul>
              )}
            </li>
          );
        })}
      </ul>
    </section>
  );
}
