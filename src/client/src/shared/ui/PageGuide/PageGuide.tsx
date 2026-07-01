import styles from "./PageGuide.module.css";

export interface PageGuideStep {
  title: string;
  description: string;
}

interface PageGuideProps {
  summary: string;
  steps: PageGuideStep[];
  testId?: string;
}

export function PageGuide({ summary, steps, testId }: PageGuideProps) {
  return (
    <div className={styles.guide} data-testid={testId}>
      <p className={styles.summary}>{summary}</p>
      <details className={styles.details} data-testid={testId ? `${testId}-details` : undefined}>
        <summary className={styles.summaryToggle}>How it works under the hood</summary>
        <ol className={styles.steps}>
          {steps.map((step) => (
            <li key={step.title} className={styles.step}>
              <span className={styles.stepTitle}>{step.title}</span>
              <span className={styles.stepDescription}>{step.description}</span>
            </li>
          ))}
        </ol>
      </details>
    </div>
  );
}
