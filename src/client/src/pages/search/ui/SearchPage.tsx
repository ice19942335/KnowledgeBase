import { type FormEvent } from "react";
import { Button, PageGuide, TokenUsageSummaryPanel } from "../../../shared/ui";
import { useDraftOrSubmitted } from "../../../shared/lib/useDraftOrSubmitted";
import { useSemanticSearch } from "../../../features/semantic-search/model/useSemanticSearch";
import styles from "./SearchPage.module.css";

const searchGuideSteps = [
  {
    title: "Embed query (Gemini)",
    description: "Your question is converted into a 1536-dimensional vector.",
  },
  {
    title: "Vector + keyword retrieval (pgvector)",
    description:
      "Closest chunks are found by cosine similarity; keyword overlap is scored in parallel when hybrid search is enabled.",
  },
  {
    title: "Merge & expand (RRF + neighbors)",
    description:
      "Vector and keyword rankings are fused with reciprocal rank fusion, then adjacent chunks around top hits are included.",
  },
  {
    title: "Rerank (LLM)",
    description: "An LLM re-orders candidates so the most relevant passages rise to the top.",
  },
  {
    title: "Return excerpts",
    description:
      "POST /api/search returns ranked chunk text with relevance scores and a token usage summary (request vs one-time indexed tokens).",
  },
] as const;

export function SearchPage() {
  const { mutate, data: results, variables, isPending, isError } = useSemanticSearch();
  const { value: query, setValue: setQuery, clearDraft } = useDraftOrSubmitted(variables);

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const trimmed = query.trim();
    if (trimmed) {
      clearDraft();
      mutate(trimmed);
    }
  };

  return (
    <section className={styles.page} data-testid="search-page">
      <header className={styles.header}>
        <h1 className={styles.title}>Semantic Search</h1>
        <PageGuide
          testId="search-guide"
          summary="Find the most relevant passages across all indexed documents. You get ranked text excerpts — use Chat when you need a synthesized answer."
          steps={[...searchGuideSteps]}
        />
      </header>

      <form className={styles.form} onSubmit={handleSubmit}>
        <input
          className={styles.input}
          type="search"
          placeholder="Search across your documents…"
          aria-label="Search query"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
        />
        <Button type="submit" disabled={isPending || !query.trim()}>
          {isPending ? "Searching…" : "Search"}
        </Button>
      </form>

      {isError && <p className={styles.state}>Search failed. Please try again.</p>}
      {results && results.results.length === 0 && <p className={styles.state}>No matches found.</p>}

      {results && results.results.length > 0 && (
        <>
          <TokenUsageSummaryPanel tokenUsage={results.tokenUsage} testId="search-token-usage" />
          <ul className={styles.results}>
            {results.results.map((result) => (
              <li key={`${result.documentId}-${result.chunkIndex}`} className={styles.result}>
                <div className={styles.resultHeader}>
                  <span>{result.documentName}</span>
                  <span className={styles.score}>
                    score: {result.score.toFixed(3)}
                    {result.embeddingTokenCount > 0 && ` · indexed: ${result.embeddingTokenCount} tokens`}
                  </span>
                </div>
                <p className={styles.content}>{result.content}</p>
              </li>
            ))}
          </ul>
        </>
      )}
    </section>
  );
}
