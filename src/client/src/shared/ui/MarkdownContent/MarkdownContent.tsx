import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import styles from "./MarkdownContent.module.css";

interface MarkdownContentProps {
  content: string;
  className?: string;
  testId?: string;
}

export function MarkdownContent({
  content,
  className,
  testId = "markdown-content",
}: MarkdownContentProps) {
  return (
    <div className={[styles.markdown, className].filter(Boolean).join(" ")} data-testid={testId}>
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        components={{
          a: ({ href, children }) => (
            <a href={href} target="_blank" rel="noopener noreferrer">
              {children}
            </a>
          ),
        }}
      >
        {content}
      </ReactMarkdown>
    </div>
  );
}
