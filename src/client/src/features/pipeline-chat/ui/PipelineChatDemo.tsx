import { type FormEvent } from "react";
import { Button, MarkdownContent, PipelineTraceTimeline, TokenUsageSummaryPanel } from "../../../shared/ui";
import { DocumentSourceLinks } from "../../../entities/document/ui/DocumentSourceLinks";
import { useDraftOrSubmitted } from "../../../shared/lib/useDraftOrSubmitted";
import { usePipelineChat } from "../model/usePipelineChat";
import styles from "./PipelineChatDemo.module.css";

export function PipelineChatDemo() {
  const { mutate, data, variables, isPending, isError } = usePipelineChat();
  const { value: question, setValue: setQuestion, clearDraft } = useDraftOrSubmitted(variables);

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const trimmed = question.trim();
    if (trimmed) {
      clearDraft();
      mutate(trimmed);
    }
  };

  return (
    <section className={styles.section} data-testid="pipeline-chat-demo">
      <form className={styles.form} onSubmit={handleSubmit}>
        <input
          className={styles.input}
          type="text"
          placeholder="Ask a question to inspect the RAG pipeline…"
          aria-label="Pipeline question"
          data-testid="pipeline-chat-question"
          value={question}
          onChange={(event) => setQuestion(event.target.value)}
        />
        <Button type="submit" disabled={isPending || !question.trim()}>
          {isPending ? "Running pipeline…" : "Run with trace"}
        </Button>
      </form>

      {isError && (
        <p className={styles.error} data-testid="pipeline-chat-error">
          Pipeline request failed. Please try again.
        </p>
      )}

      {data && (
        <div className={styles.result}>
          <TokenUsageSummaryPanel tokenUsage={data.tokenUsage} testId="pipeline-token-usage" />
          <article className={styles.answer} data-testid="pipeline-chat-answer">
            <h3 className={styles.answerTitle}>Answer</h3>
            <MarkdownContent content={data.answer} testId="pipeline-chat-answer-markdown" />
            {data.sources.length > 0 && (
              <>
                <h4 className={styles.sourcesTitle}>Sources</h4>
                <DocumentSourceLinks sources={data.sources} testIdPrefix="pipeline" />
              </>
            )}
          </article>

          <div className={styles.process}>
            <h3 className={styles.processTitle}>Pipeline trace</h3>
            <PipelineTraceTimeline steps={data.steps} totalDurationMs={data.totalDurationMs} />
          </div>
        </div>
      )}
    </section>
  );
}
