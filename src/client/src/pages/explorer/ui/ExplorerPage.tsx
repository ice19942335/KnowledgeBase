import { useEffect, useState } from "react";
import { RelatedDocumentChunksPanel } from "../../../entities/explorer";
import { usePipelineChatSessionStore } from "../../../features/pipeline-chat/model/pipelineChatSessionStore";
import { PipelineChatDemo } from "../../../features/pipeline-chat";
import { PageGuide } from "../../../shared/ui";
import styles from "./ExplorerPage.module.css";

const explorerGuideSteps = [
  {
    title: "Indexed chunks panel (GET /api/search/explorer)",
    description:
      "After a traced chat answer, browse indexed chunks for the source documents only — with per-document tabs when multiple files contributed.",
  },
  {
    title: "Traced chat (POST /api/chat/trace)",
    description:
      "Ask a question and inspect each step: tenant resolution, conversation load, full search trace, prompt assembly, LLM call, and persistence.",
  },
  {
    title: "Search sub-trace",
    description:
      "Inside the chat trace, expand search steps to see embedding, vector/keyword hits, hybrid merge, neighbor expansion, and reranking.",
  },
  {
    title: "Input / Output tabs",
    description:
      "Each pipeline step shows what went in and what came out, with timings — useful for debugging retrieval quality.",
  },
  {
    title: "Token usage",
    description:
      "Each indexed chunk stores one-time embedding tokens. Traced requests show request tokens (query, rerank, answer) plus indexed tokens from retrieved chunks.",
  },
] as const;

type WorkspaceView = "pipeline" | "chunks";

export function ExplorerPage() {
  const pipelineData = usePipelineChatSessionStore((state) => state.data);
  const [mobileView, setMobileView] = useState<WorkspaceView>("pipeline");

  useEffect(() => {
    if (pipelineData) {
      setMobileView("chunks");
    }
  }, [pipelineData]);

  const sourceDocumentCount = pipelineData
    ? new Set(pipelineData.sources.map((source) => source.documentId)).size
    : 0;

  const chunksPanel = (
    <aside className={styles.chunksPanel} aria-labelledby="indexed-chunks-heading">
      <div className={styles.panelHeader}>
        <h2 id="indexed-chunks-heading" className={styles.panelTitle}>
          Indexed chunks
        </h2>
        {pipelineData && sourceDocumentCount > 0 && (
          <span className={styles.panelBadge} data-testid="related-chunks-badge">
            {sourceDocumentCount} source {sourceDocumentCount === 1 ? "document" : "documents"}
          </span>
        )}
      </div>

      {!pipelineData && (
        <p className={styles.panelHint}>
          Chunks appear here after you run a traced chat question.
        </p>
      )}

      <RelatedDocumentChunksPanel
        sources={pipelineData?.sources}
        hasPipelineResult={!!pipelineData}
      />
    </aside>
  );

  const pipelinePanel = (
    <section className={styles.pipelinePanel} aria-labelledby="pipeline-demo-heading">
      <div className={styles.panelHeader}>
        <h2 id="pipeline-demo-heading" className={styles.panelTitle}>
          Chat pipeline
        </h2>
        <p className={styles.panelHint}>
          Ask a question and inspect each step: tenant, search, prompt, LLM, persist.
        </p>
      </div>
      <PipelineChatDemo />
    </section>
  );

  return (
    <section className={styles.page} data-testid="explorer-page">
      <header className={styles.header}>
        <h1 className={styles.title}>Pipeline Explorer</h1>
        <PageGuide
          testId="explorer-guide"
          summary="Developer view: run a traced RAG chat on the left, then inspect indexed chunks for the source documents on the right. Use this to understand why Search or Chat returned specific passages."
          steps={[...explorerGuideSteps]}
        />
      </header>

      <div
        className={styles.mobileTabs}
        role="tablist"
        aria-label="Explorer workspace"
        data-testid="explorer-mobile-tabs"
      >
        <button
          type="button"
          role="tab"
          className={mobileView === "pipeline" ? styles.mobileTabActive : styles.mobileTab}
          aria-selected={mobileView === "pipeline"}
          data-testid="explorer-tab-pipeline"
          onClick={() => setMobileView("pipeline")}
        >
          Pipeline
        </button>
        <button
          type="button"
          role="tab"
          className={mobileView === "chunks" ? styles.mobileTabActive : styles.mobileTab}
          aria-selected={mobileView === "chunks"}
          data-testid="explorer-tab-chunks"
          onClick={() => setMobileView("chunks")}
        >
          Chunks
        </button>
      </div>

      <div className={styles.workspace} data-testid="explorer-workspace">
        <div
          className={styles.pipelineColumn}
          data-mobile-visible={mobileView === "pipeline"}
        >
          {pipelinePanel}
        </div>
        <div
          className={styles.chunksColumn}
          data-mobile-visible={mobileView === "chunks"}
        >
          {chunksPanel}
        </div>
      </div>
    </section>
  );
}
