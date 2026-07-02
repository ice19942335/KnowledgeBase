import { Link } from "react-router-dom";
import { DocumentChunksExplorer, useSearchExplorer } from "../../../entities/explorer";
import { useDocument } from "../../../entities/document/model/queries";
import { sumEmbeddingTokens } from "../../../entities/document/lib/indexingTokens";
import { DocumentIndexingTokens } from "../../../entities/document/ui/DocumentIndexingTokens";
import type { DocumentStatus } from "../../../entities/document/model/types";
import { Button } from "../../../shared/ui";
import styles from "./DocumentDetailPage.module.css";

interface DocumentDetailPageProps {
  documentId: string;
}

const statusClassName: Record<DocumentStatus, string> = {
  Uploaded: styles.processing,
  Processing: styles.processing,
  Processed: styles.processed,
  Failed: styles.failed,
};

function formatDate(value: string | null): string {
  if (!value) {
    return "—";
  }

  return new Date(value).toLocaleString();
}

export function DocumentDetailPage({ documentId }: DocumentDetailPageProps) {
  const documentQuery = useDocument(documentId);
  const showChunks = documentQuery.data?.status === "Processed";
  const chunksQuery = useSearchExplorer(showChunks ? [documentId] : undefined);

  if (documentQuery.isLoading) {
    return <p className={styles.state} data-testid="document-detail-loading">Loading document details…</p>;
  }

  if (documentQuery.isError || !documentQuery.data) {
    return <p className={styles.state} data-testid="document-detail-error">Failed to load document details.</p>;
  }

  const document = documentQuery.data;
  const chunkGroup = chunksQuery.data?.documents[0];
  const chunks = chunkGroup?.chunks ?? [];
  const indexingTokens = sumEmbeddingTokens(chunks);

  return (
    <section className={styles.page} data-testid="document-detail-page">
      <Link to="/" className={styles.backLink} data-testid="document-detail-back">
        ← Back to documents
      </Link>

      <header className={styles.header}>
        <div className={styles.titleBlock}>
          <h1 className={styles.title}>{document.name}</h1>
          <span
            className={`${styles.status} ${statusClassName[document.status]}`}
            data-testid="document-detail-status"
          >
            {document.status}
          </span>
        </div>

        <div className={styles.actions}>
          <Link to={`/documents/${documentId}/view`} data-testid="document-detail-view-file">
            <Button variant="secondary" type="button">
              View file
            </Button>
          </Link>
        </div>
      </header>

      <dl className={styles.metaList} data-testid="document-detail-meta">
        <div className={styles.metaItem}>
          <dt>File name</dt>
          <dd>{document.fileName}</dd>
        </div>
        <div className={styles.metaItem}>
          <dt>Content type</dt>
          <dd>{document.contentType}</dd>
        </div>
        <div className={styles.metaItem}>
          <dt>Uploaded</dt>
          <dd>{formatDate(document.createdAt)}</dd>
        </div>
        <div className={styles.metaItem}>
          <dt>Processed</dt>
          <dd>{formatDate(document.processedAt)}</dd>
        </div>
        <div className={styles.metaItem}>
          <dt>Chunk count</dt>
          <dd data-testid="document-detail-chunk-count">{document.chunkCount}</dd>
        </div>
      </dl>

      {document.error && (
        <p className={styles.error} data-testid="document-detail-error-message">
          {document.error}
        </p>
      )}

      {showChunks && (
        <section className={styles.section} data-testid="document-detail-indexing">
          <h2 className={styles.sectionTitle}>Indexing cost</h2>
          <p className={styles.sectionHint}>
            One-time Gemini embedding tokens spent when this document was chunked and indexed.
          </p>
          {chunksQuery.isLoading && <p className={styles.state}>Loading indexed chunks…</p>}
          {chunksQuery.isError && (
            <p className={styles.state} data-testid="document-detail-chunks-error">
              Failed to load indexed chunks.
            </p>
          )}
          {chunksQuery.isSuccess && (
            <DocumentIndexingTokens chunkCount={chunks.length} tokenCount={indexingTokens} />
          )}
        </section>
      )}

      {!showChunks && document.status !== "Failed" && (
        <p className={styles.state} data-testid="document-detail-processing-hint">
          Chunks and token usage will appear after ingestion finishes.
        </p>
      )}

      {showChunks && chunksQuery.isSuccess && (
        <section className={styles.section} data-testid="document-detail-chunks">
          <h2 className={styles.sectionTitle}>Indexed chunks</h2>
          <DocumentChunksExplorer
            documents={chunksQuery.data.documents}
            totalChunks={chunksQuery.data.totalChunks}
          />
        </section>
      )}
    </section>
  );
}
