import styles from "./PipelineTraceTimeline.module.css";

interface TraceStep {
  order: number;
  name: string;
  description: string;
  durationMs: number;
  input: unknown;
  output: unknown;
}

interface PipelineTraceTimelineProps {
  steps: TraceStep[];
  totalDurationMs?: number;
  nested?: boolean;
}

export function formatJson(value: unknown): string {
  return JSON.stringify(value, null, 2);
}

function extractNestedSteps(output: unknown): TraceStep[] | null {
  if (!output || typeof output !== "object" || !("nestedSteps" in output)) {
    return null;
  }

  const nestedSteps = (output as { nestedSteps?: unknown }).nestedSteps;
  return Array.isArray(nestedSteps) ? (nestedSteps as TraceStep[]) : null;
}

function TraceStepItem({ step, nested = false }: { step: TraceStep; nested?: boolean }) {
  const nestedSteps = extractNestedSteps(step.output);

  return (
    <li
      className={nested ? styles.nestedStep : styles.step}
      data-testid={`pipeline-step-${step.order}${nested ? "-nested" : ""}`}
    >
      <div className={styles.stepHeader}>
        <span className={styles.stepOrder}>{step.order}</span>
        <div>
          <h3 className={styles.stepName}>{step.name}</h3>
          <p className={styles.stepDescription}>{step.description}</p>
        </div>
        <span className={styles.duration}>{step.durationMs} ms</span>
      </div>

      <div className={styles.payloadGrid}>
        <div className={styles.payloadBlock}>
          <h4 className={styles.payloadTitle}>Input</h4>
          <pre className={styles.payloadContent} data-testid={`pipeline-step-${step.order}-input`}>
            {formatJson(step.input)}
          </pre>
        </div>
        <div className={styles.payloadBlock}>
          <h4 className={styles.payloadTitle}>Output</h4>
          <pre className={styles.payloadContent} data-testid={`pipeline-step-${step.order}-output`}>
            {formatJson(step.output)}
          </pre>
        </div>
      </div>

      {nestedSteps && nestedSteps.length > 0 && (
        <div className={styles.nestedTimeline}>
          <p className={styles.nestedTitle}>Nested search pipeline</p>
          <ol className={styles.nestedList}>
            {nestedSteps.map((nestedStep) => (
              <TraceStepItem key={nestedStep.order} step={nestedStep} nested />
            ))}
          </ol>
        </div>
      )}
    </li>
  );
}

export function PipelineTraceTimeline({ steps, totalDurationMs }: PipelineTraceTimelineProps) {
  return (
    <section className={styles.timeline} data-testid="pipeline-trace-timeline">
      {totalDurationMs !== undefined && (
        <p className={styles.summary} data-testid="pipeline-total-duration">
          Total pipeline time: {totalDurationMs} ms
        </p>
      )}

      <ol className={styles.list}>
        {steps.map((step) => (
          <TraceStepItem key={step.order} step={step} />
        ))}
      </ol>
    </section>
  );
}
