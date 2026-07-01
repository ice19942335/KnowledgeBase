import { type FormEvent } from "react";
import { DocumentSourceLinks } from "../../../entities/document/ui/DocumentSourceLinks";
import { Button, MarkdownContent, PageGuide } from "../../../shared/ui";
import { useDraftOrSubmitted } from "../../../shared/lib/useDraftOrSubmitted";
import { useRagChat } from "../../../features/rag-chat/model/useRagChat";
import styles from "./ChatPage.module.css";

const chatGuideSteps = [
  {
    title: "Retrieve context (Search API)",
    description:
      "The same hybrid search pipeline as the Search tab finds the top matching chunks for your question.",
  },
  {
    title: "Build RAG prompt",
    description:
      "Retrieved passages are combined with your question into a prompt that instructs the model to answer only from context.",
  },
  {
    title: "Generate answer (Gemini)",
    description:
      "POST /api/chat calls the chat model. If the answer is not in the context, the assistant replies with \"I don't know.\"",
  },
  {
    title: "Attach sources",
    description:
      "Document and chunk references are returned so you can open or download the original file.",
  },
  {
    title: "Save conversation",
    description:
      "User and assistant messages are stored in the Chat database for follow-up questions in the same thread.",
  },
] as const;

export function ChatPage() {
  const { mutate, data: answer, variables, isPending, isError } = useRagChat();
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
    <section className={styles.page} data-testid="chat-page">
      <header className={styles.header}>
        <h1 className={styles.title}>Ask the Knowledge Base</h1>
        <PageGuide
          testId="chat-guide"
          summary="Ask questions in natural language and get answers grounded in your documents, with links to the source passages."
          steps={[...chatGuideSteps]}
        />
      </header>

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
