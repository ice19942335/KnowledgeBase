import { NavLink } from "react-router-dom";
import styles from "./Header.module.css";

const links = [
  { to: "/", label: "Documents", end: true },
  { to: "/search", label: "Search", end: false },
  { to: "/chat", label: "Chat", end: false },
  { to: "/explorer", label: "Explorer", end: false },
];

export function Header() {
  return (
    <header className={styles.header}>
      <span className={styles.brand}>AI Knowledge Base</span>
      <nav className={styles.nav}>
        {links.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            end={link.end}
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
