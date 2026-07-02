import styles from "../../../../shared/ui/TokenUsageSummary/TokenUsageSummary.module.css";

interface DocumentIndexingTokensProps {
  chunkCount: number;
  tokenCount: number;
}

export function DocumentIndexingTokens({ chunkCount, tokenCount }: DocumentIndexingTokensProps) {
  return (
    <dl className={styles.summary} data-testid="document-indexing-tokens">
      <div className={styles.item}>
        <dt>Chunks indexed</dt>
        <dd data-testid="document-indexing-tokens-chunks">{chunkCount}</dd>
      </div>
      <div className={styles.item}>
        <dt>Embedding tokens</dt>
        <dd data-testid="document-indexing-tokens-total">{tokenCount}</dd>
      </div>
    </dl>
  );
}
