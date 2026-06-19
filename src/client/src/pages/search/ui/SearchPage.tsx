import { useState, type FormEvent } from "react";
import { Button } from "../../../shared/ui";
import { useSemanticSearch } from "../../../features/semantic-search/model/useSemanticSearch";
import styles from "./SearchPage.module.css";

export function SearchPage() {
  const [query, setQuery] = useState("");
  const { mutate, data: results, isPending, isError } = useSemanticSearch();

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const trimmed = query.trim();
    if (trimmed) {
      mutate(trimmed);
    }
  };

  return (
    <section className={styles.page}>
      <h1 className={styles.title}>Semantic Search</h1>

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
      {results && results.length === 0 && <p className={styles.state}>No matches found.</p>}

      {results && results.length > 0 && (
        <ul className={styles.results}>
          {results.map((result) => (
            <li key={`${result.documentId}-${result.chunkIndex}`} className={styles.result}>
              <div className={styles.resultHeader}>
                <span>{result.documentName}</span>
                <span className={styles.score}>score: {result.score.toFixed(3)}</span>
              </div>
              <p className={styles.content}>{result.content}</p>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
