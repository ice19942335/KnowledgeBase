import { useState } from "react";
import { DocumentChunksExplorer, useSearchExplorer } from "../../../entities/explorer";
import { PipelineChatDemo } from "../../../features/pipeline-chat";
import styles from "./ExplorerPage.module.css";

type WorkspaceView = "pipeline" | "chunks";

export function ExplorerPage() {
  const { data, isLoading, isError } = useSearchExplorer();
  const [mobileView, setMobileView] = useState<WorkspaceView>("pipeline");

  const chunksPanel = (
    <aside className={styles.chunksPanel} aria-labelledby="indexed-chunks-heading">
      <div className={styles.panelHeader}>
        <h2 id="indexed-chunks-heading" className={styles.panelTitle}>
          Indexed chunks
        </h2>
        {data && (
          <span className={styles.panelBadge}>
            {data.documents.length} docs · {data.totalChunks} chunks
          </span>
        )}
      </div>

      {isLoading && <p className={styles.state}>Loading indexed chunks…</p>}
      {isError && (
        <p className={styles.state} data-testid="explorer-chunks-error">
          Failed to load indexed chunks.
        </p>
      )}

      {data && (
        <DocumentChunksExplorer
          documents={data.documents}
          totalChunks={data.totalChunks}
          variant="sidebar"
        />
      )}
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
        <p className={styles.subtitle}>
          Run a traced chat on the left and browse indexed chunks on the right.
        </p>
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
