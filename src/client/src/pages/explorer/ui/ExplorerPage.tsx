import { DocumentChunksExplorer, useSearchExplorer } from "../../../entities/explorer";
import { PipelineChatDemo } from "../../../features/pipeline-chat";
import styles from "./ExplorerPage.module.css";

export function ExplorerPage() {
  const { data, isLoading, isError } = useSearchExplorer();

  return (
    <section className={styles.page} data-testid="explorer-page">
      <header className={styles.header}>
        <h1 className={styles.title}>Pipeline Explorer</h1>
        <p className={styles.subtitle}>
          Inspect indexed document chunks and trace every step of a chat request with inputs and
          outputs.
        </p>
      </header>

      <section className={styles.panel} aria-labelledby="indexed-chunks-heading">
        <h2 id="indexed-chunks-heading" className={styles.panelTitle}>
          Indexed documents and chunks
        </h2>

        {isLoading && <p className={styles.state}>Loading indexed chunks…</p>}
        {isError && (
          <p className={styles.state} data-testid="explorer-chunks-error">
            Failed to load indexed chunks.
          </p>
        )}

        {data && (
          <DocumentChunksExplorer documents={data.documents} totalChunks={data.totalChunks} />
        )}
      </section>

      <section className={styles.panel} aria-labelledby="pipeline-demo-heading">
        <h2 id="pipeline-demo-heading" className={styles.panelTitle}>
          Chat pipeline trace
        </h2>
        <p className={styles.panelHint}>
          Submit a question to see tenant resolution, conversation handling, semantic search,
          prompt assembly, LLM completion, and persistence in order.
        </p>
        <PipelineChatDemo />
      </section>
    </section>
  );
}
