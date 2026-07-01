import { NavLink } from "react-router-dom";
import styles from "./Header.module.css";

const links = [
  {
    to: "/",
    label: "Documents",
    end: true,
    title: "Upload files and trigger async ingestion (extract → chunk → embed → index)",
  },
  {
    to: "/search",
    label: "Search",
    end: false,
    title: "Hybrid vector + keyword search with reranking — returns ranked passages",
  },
  {
    to: "/chat",
    label: "Chat",
    end: false,
    title: "RAG answers: retrieve chunks, build prompt, generate response with sources",
  },
  {
    to: "/explorer",
    label: "Explorer",
    end: false,
    title: "Inspect indexed chunks and step-by-step pipeline trace for each question",
  },
] as const;

export function Header() {
  return (
    <header className={styles.header}>
      <div className={styles.brandBlock} data-testid="header-brand">
        <span className={styles.brand}>AI Knowledge Base</span>
        <span className={styles.brandBy}>
          by <span className={styles.brandAuthor}>Aleksejs Birula</span>
        </span>
      </div>
      <nav className={styles.nav}>
        {links.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            end={link.end}
            title={link.title}
            className={({ isActive }) =>
              isActive ? `${styles.link} ${styles.active}` : styles.link
            }
          >
            {link.label}
          </NavLink>
        ))}
      </nav>
    </header>
  );
}
