import { useEffect, useState } from "react";
import { getOrderedUsedChunks } from "../../lib/sourceDocuments";
import type { ChunkDetail, DocumentChunksGroup } from "../../model/types";
import styles from "./DocumentChunksExplorer.module.css";

type ExplorerEmptyState = "no-chunks" | "awaiting-request" | "no-sources";
type ChunkView = "all" | "used";

interface DocumentChunksExplorerProps {
  documents: DocumentChunksGroup[];
  totalChunks: number;
  variant?: "default" | "sidebar";
  highlightedChunkIndices?: Record<string, number[]>;
  emptyState?: ExplorerEmptyState;
}

function formatIndexedAt(value: string | undefined): string {
  if (!value) {
    return "—";
  }

  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : date.toLocaleString();
}

function getEmptyMessage(emptyState: ExplorerEmptyState): string {
  switch (emptyState) {
    case "awaiting-request":
      return "Run a traced chat question to view indexed chunks for the documents used in the answer.";
    case "no-sources":
      return "No document sources were used in this answer.";
    default:
      return "No indexed chunks found for the selected documents.";
  }
}

function ChunkItem({
  chunk,
  priorityRank,
}: {
  chunk: ChunkDetail;
  priorityRank?: number;
}) {
  return (
    <li
      className={priorityRank ? styles.chunkItemUsed : styles.chunkItem}
      data-testid={`chunk-${chunk.id}`}
      data-priority-rank={priorityRank ?? undefined}
    >
      <div className={styles.chunkMeta}>
        <span>
          {priorityRank && (
            <span className={styles.priorityBadge} data-testid={`chunk-priority-${chunk.id}`}>
              #{priorityRank}
            </span>
          )}
          Chunk #{chunk.chunkIndex}
        </span>
        <span>
          {chunk.embeddingTokenCount > 0 && (
            <span className={styles.tokenBadge} data-testid={`chunk-tokens-${chunk.id}`}>
              {chunk.embeddingTokenCount} tokens
            </span>
          )}
          <span data-testid={`chunk-indexed-at-${chunk.id}`}>{formatIndexedAt(chunk.indexedAt)}</span>
        </span>
      </div>
      <pre className={styles.chunkContent}>{chunk.content}</pre>
    </li>
  );
}

function AllChunksList({ document }: { document: DocumentChunksGroup }) {
  return (
    <ul className={styles.chunkList} data-testid={`document-chunks-${document.documentId}`}>
      {document.chunks.map((chunk) => (
        <ChunkItem key={chunk.id} chunk={chunk} />
      ))}
    </ul>
  );
}

function UsedChunksList({
  document,
  orderedChunkIndices,
}: {
  document: DocumentChunksGroup;
  orderedChunkIndices: number[];
}) {
  const usedChunks = getOrderedUsedChunks(document.chunks, orderedChunkIndices);

  if (usedChunks.length === 0) {
    return (
      <p className={styles.usedEmpty} data-testid={`document-used-empty-${document.documentId}`}>
        No chunks from this document were used in the answer.
      </p>
    );
  }

  return (
    <ul
      className={styles.chunkList}
      data-testid={`document-used-chunks-${document.documentId}`}
    >
      {usedChunks.map(({ chunk, priorityRank }) => (
        <ChunkItem key={chunk.id} chunk={chunk} priorityRank={priorityRank} />
      ))}
    </ul>
  );
}

function DocumentChunkViews({
  document,
  highlightedChunkIndices,
}: {
  document: DocumentChunksGroup;
  highlightedChunkIndices?: Record<string, number[]>;
}) {
  const usedChunkIndices = highlightedChunkIndices?.[document.documentId] ?? [];
  const hasUsedChunks = usedChunkIndices.length > 0;
  const [activeChunkView, setActiveChunkView] = useState<ChunkView>(
    hasUsedChunks ? "used" : "all",
  );

  useEffect(() => {
    setActiveChunkView(hasUsedChunks ? "used" : "all");
  }, [document.documentId, hasUsedChunks]);

  const showUsedTab = highlightedChunkIndices !== undefined;

  return (
    <>
      {showUsedTab && (
        <div
          className={styles.chunkViewTabs}
          role="tablist"
          aria-label={`Chunk views for ${document.documentName}`}
          data-testid={`chunk-view-tabs-${document.documentId}`}
        >
          <button
            type="button"
            role="tab"
            className={activeChunkView === "all" ? styles.chunkViewTabActive : styles.chunkViewTab}
            aria-selected={activeChunkView === "all"}
            data-testid={`chunk-view-all-${document.documentId}`}
            onClick={() => setActiveChunkView("all")}
          >
            All chunks
            <span className={styles.chunkViewTabCount}>{document.chunks.length}</span>
          </button>
          <button
            type="button"
            role="tab"
            className={activeChunkView === "used" ? styles.chunkViewTabActive : styles.chunkViewTab}
            aria-selected={activeChunkView === "used"}
            data-testid={`chunk-view-used-${document.documentId}`}
            onClick={() => setActiveChunkView("used")}
          >
            Used for answer
            <span className={styles.chunkViewTabCount}>{usedChunkIndices.length}</span>
          </button>
        </div>
      )}

      {activeChunkView === "used" && showUsedTab ? (
        <UsedChunksList document={document} orderedChunkIndices={usedChunkIndices} />
      ) : (
        <AllChunksList document={document} />
      )}
    </>
  );
}

export function DocumentChunksExplorer({
  documents,
  totalChunks,
  variant = "default",
  highlightedChunkIndices,
  emptyState = "no-chunks",
}: DocumentChunksExplorerProps) {
  const [activeDocumentId, setActiveDocumentId] = useState<string | null>(
    documents[0]?.documentId ?? null,
  );

  useEffect(() => {
    if (documents.length === 0) {
      setActiveDocumentId(null);
      return;
    }

    if (!activeDocumentId || !documents.some((document) => document.documentId === activeDocumentId)) {
      setActiveDocumentId(documents[0].documentId);
    }
  }, [activeDocumentId, documents]);

  const sectionClass = variant === "sidebar" ? styles.sectionSidebar : styles.section;
  const activeDocument = documents.find((document) => document.documentId === activeDocumentId);
  const useDocumentTabs = documents.length > 1;

  if (documents.length === 0) {
    return (
      <p className={styles.empty} data-testid="document-chunks-empty">
        {getEmptyMessage(emptyState)}
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

      {useDocumentTabs && (
        <div
          className={styles.documentTabs}
          role="tablist"
          aria-label="Source documents"
          data-testid="document-chunks-tabs"
        >
          {documents.map((document) => {
            const isActive = document.documentId === activeDocumentId;

            return (
              <button
                key={document.documentId}
                type="button"
                role="tab"
                className={isActive ? styles.documentTabActive : styles.documentTab}
                aria-selected={isActive}
                data-testid={`document-tab-${document.documentId}`}
                onClick={() => setActiveDocumentId(document.documentId)}
              >
                <span className={styles.documentTabName}>{document.documentName}</span>
                <span className={styles.documentTabCount}>{document.chunks.length}</span>
              </button>
            );
          })}
        </div>
      )}

      {activeDocument && (
        <div
          role={useDocumentTabs ? "tabpanel" : undefined}
          aria-labelledby={useDocumentTabs ? `document-tab-${activeDocument.documentId}` : undefined}
          data-testid={`document-panel-${activeDocument.documentId}`}
        >
          {!useDocumentTabs && (
            <div className={styles.singleDocumentHeader}>
              <h3 className={styles.singleDocumentTitle}>{activeDocument.documentName}</h3>
              <span className={styles.chunkCount}>{activeDocument.chunks.length} chunks</span>
            </div>
          )}

          <DocumentChunkViews
            document={activeDocument}
            highlightedChunkIndices={highlightedChunkIndices}
          />
        </div>
      )}
    </section>
  );
}
