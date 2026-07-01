import { DocumentChunksExplorer } from "../DocumentChunksExplorer";
import { buildHighlightedChunkMap, extractSourceDocuments } from "../../lib/sourceDocuments";
import { useSearchExplorer } from "../../model/queries";
import type { SourceReference } from "../../model/types";
import styles from "./RelatedDocumentChunksPanel.module.css";

interface RelatedDocumentChunksPanelProps {
  sources: SourceReference[] | undefined;
  hasPipelineResult: boolean;
}

export function RelatedDocumentChunksPanel({
  sources,
  hasPipelineResult,
}: RelatedDocumentChunksPanelProps) {
  const sourceDocuments = sources ? extractSourceDocuments(sources) : [];
  const documentIds = sourceDocuments.map((document) => document.documentId);
  const { data, isLoading, isError } = useSearchExplorer(
    hasPipelineResult && documentIds.length > 0 ? documentIds : undefined,
  );

  if (!hasPipelineResult) {
    return (
      <DocumentChunksExplorer
        documents={[]}
        totalChunks={0}
        variant="sidebar"
        emptyState="awaiting-request"
      />
    );
  }

  if (sourceDocuments.length === 0) {
    return (
      <DocumentChunksExplorer
        documents={[]}
        totalChunks={0}
        variant="sidebar"
        emptyState="no-sources"
      />
    );
  }

  if (isLoading) {
    return <p className={styles.state} data-testid="related-chunks-loading">Loading document chunks…</p>;
  }

  if (isError) {
    return (
      <p className={styles.state} data-testid="related-chunks-error">
        Failed to load chunks for the source documents.
      </p>
    );
  }

  return (
    <DocumentChunksExplorer
      documents={data?.documents ?? []}
      totalChunks={data?.totalChunks ?? 0}
      variant="sidebar"
      highlightedChunkIndices={sources ? buildHighlightedChunkMap(sources) : undefined}
      emptyState="no-chunks"
    />
  );
}
