import { useState, type FormEvent } from "react";
import { DocumentSourceLinks } from "../../../entities/document/ui/DocumentSourceLinks";
import { Button, MarkdownContent } from "../../../shared/ui";
import { useRagChat } from "../../../features/rag-chat/model/useRagChat";
import styles from "./ChatPage.module.css";

export function ChatPage() {
  const [question, setQuestion] = useState("");
  const { mutate, data: answer, isPending, isError } = useRagChat();

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const trimmed = question.trim();
    if (trimmed) {
      mutate(trimmed);
    }
  };

  return (
    <section className={styles.page}>
      <h1 className={styles.title}>Ask the Knowledge Base</h1>

      <form className={styles.form} onSubmit={handleSubmit}>
        <input
          className={styles.input}
          type="text"
          placeholder="Ask a question about your documents…"
          aria-label="Question"
          value={question}
          onChange={(event) => setQuestion(event.target.value)}
        />
        <Button type="submit" disabled={isPending || !question.trim()}>
          {isPending ? "Thinking…" : "Ask"}
        </Button>
      </form>

      {isError && <p className={styles.state}>Something went wrong. Please try again.</p>}

      {answer && (
        <article className={styles.answer}>
          <MarkdownContent
            content={answer.answer}
            className={styles.answerText}
            testId="chat-answer-markdown"
          />
          {answer.sources.length > 0 && (
            <>
              <h2 className={styles.sourcesTitle}>Sources</h2>
              <DocumentSourceLinks sources={answer.sources} />
            </>
          )}
        </article>
      )}
    </section>
  );
}
