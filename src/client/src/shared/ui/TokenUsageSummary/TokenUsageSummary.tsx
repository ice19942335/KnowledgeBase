import type { TokenUsageSummary } from "../../model/tokenUsage";
import styles from "./TokenUsageSummary.module.css";

interface TokenUsageSummaryProps {
  tokenUsage: TokenUsageSummary;
  testId?: string;
}

export function TokenUsageSummaryPanel({ tokenUsage, testId = "token-usage-summary" }: TokenUsageSummaryProps) {
  const totalTokens = tokenUsage.requestTokens + tokenUsage.indexedTokens;

  return (
    <dl className={styles.summary} data-testid={testId}>
      <div className={styles.item}>
        <dt>Request tokens</dt>
        <dd data-testid={`${testId}-request`}>{tokenUsage.requestTokens}</dd>
      </div>
      <div className={styles.item}>
        <dt>Indexed (one-time)</dt>
        <dd data-testid={`${testId}-indexed`}>{tokenUsage.indexedTokens}</dd>
      </div>
      <div className={styles.item}>
        <dt>Total</dt>
        <dd data-testid={`${testId}-total`}>{totalTokens}</dd>
      </div>
    </dl>
  );
}
