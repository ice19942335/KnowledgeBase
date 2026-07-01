import { useState } from "react";
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

type PayloadTab = "input" | "output";

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

function PayloadTabs({
  stepOrder,
  activeTab,
  onTabChange,
  input,
  output,
  nested,
}: {
  stepOrder: number;
  activeTab: PayloadTab;
  onTabChange: (tab: PayloadTab) => void;
  input: unknown;
  output: unknown;
  nested?: boolean;
}) {
  const suffix = nested ? "-nested" : "";

  return (
    <div className={styles.payloadSection}>
      <div className={styles.payloadTabs} role="tablist" aria-label={`Step ${stepOrder} payloads`}>
        <button
          type="button"
          role="tab"
          className={activeTab === "input" ? styles.payloadTabActive : styles.payloadTab}
          aria-selected={activeTab === "input"}
          data-testid={`pipeline-step-${stepOrder}${suffix}-tab-input`}
          onClick={() => onTabChange("input")}
        >
          Input
        </button>
        <button
          type="button"
          role="tab"
          className={activeTab === "output" ? styles.payloadTabActive : styles.payloadTab}
          aria-selected={activeTab === "output"}
          data-testid={`pipeline-step-${stepOrder}${suffix}-tab-output`}
          onClick={() => onTabChange("output")}
        >
          Output
        </button>
      </div>

      <pre
        className={styles.payloadContent}
        role="tabpanel"
        data-testid={`pipeline-step-${stepOrder}${suffix}-${activeTab}`}
      >
        {formatJson(activeTab === "input" ? input : output)}
      </pre>
    </div>
  );
}

function TraceStepItem({
  step,
  nested = false,
  isExpanded,
  onToggle,
  activePayloadTab,
  onPayloadTabChange,
}: {
  step: TraceStep;
  nested?: boolean;
  isExpanded: boolean;
  onToggle: () => void;
  activePayloadTab: PayloadTab;
  onPayloadTabChange: (tab: PayloadTab) => void;
}) {
  const nestedSteps = extractNestedSteps(step.output);
  const suffix = nested ? "-nested" : "";

  return (
    <li
      className={nested ? styles.nestedStep : styles.step}
      data-testid={`pipeline-step-${step.order}${suffix}`}
      data-expanded={isExpanded}
    >
      <button
        type="button"
        className={styles.stepHeader}
        aria-expanded={isExpanded}
        data-testid={`pipeline-step-${step.order}${suffix}-toggle`}
        onClick={onToggle}
      >
        <span className={styles.stepOrder}>{step.order}</span>
        <div className={styles.stepInfo}>
          <h3 className={styles.stepName}>{step.name}</h3>
          <p className={styles.stepDescription}>{step.description}</p>
        </div>
        <span className={styles.duration}>{step.durationMs} ms</span>
        <span className={styles.expandIcon} aria-hidden="true">
          {isExpanded ? "−" : "+"}
        </span>
      </button>

      {isExpanded && (
        <div className={styles.stepBody}>
          <PayloadTabs
            stepOrder={step.order}
            activeTab={activePayloadTab}
            onTabChange={onPayloadTabChange}
            input={step.input}
            output={step.output}
            nested={nested}
          />

          {nestedSteps && nestedSteps.length > 0 && (
            <NestedTimeline steps={nestedSteps} />
          )}
        </div>
      )}
    </li>
  );
}

function NestedTimeline({ steps }: { steps: TraceStep[] }) {
  const [expandedOrder, setExpandedOrder] = useState<number | null>(steps[0]?.order ?? null);
  const [payloadTabs, setPayloadTabs] = useState<Record<number, PayloadTab>>({});

  return (
    <div className={styles.nestedTimeline}>
      <p className={styles.nestedTitle}>Nested search pipeline</p>
      <ol className={styles.nestedList}>
        {steps.map((nestedStep) => (
          <TraceStepItem
            key={nestedStep.order}
            step={nestedStep}
            nested
            isExpanded={expandedOrder === nestedStep.order}
            onToggle={() =>
              setExpandedOrder((current) =>
                current === nestedStep.order ? null : nestedStep.order,
              )
            }
            activePayloadTab={payloadTabs[nestedStep.order] ?? "output"}
            onPayloadTabChange={(tab) =>
              setPayloadTabs((current) => ({ ...current, [nestedStep.order]: tab }))
            }
          />
        ))}
      </ol>
    </div>
  );
}

export function PipelineTraceTimeline({ steps, totalDurationMs }: PipelineTraceTimelineProps) {
  const [expandedOrder, setExpandedOrder] = useState<number | null>(steps[0]?.order ?? null);
  const [payloadTabs, setPayloadTabs] = useState<Record<number, PayloadTab>>({});

  const handleStepNav = (order: number) => {
    setExpandedOrder(order);
    const element = document.querySelector(`[data-testid="pipeline-step-${order}"]`);
    if (element && typeof element.scrollIntoView === "function") {
      element.scrollIntoView({ behavior: "smooth", block: "nearest" });
    }
  };

  return (
    <section className={styles.timeline} data-testid="pipeline-trace-timeline">
      <div className={styles.timelineHeader}>
        {totalDurationMs !== undefined && (
          <p className={styles.summary} data-testid="pipeline-total-duration">
            Total pipeline time: {totalDurationMs} ms
          </p>
        )}

        <nav className={styles.stepNav} aria-label="Pipeline steps">
          {steps.map((step) => (
            <button
              key={step.order}
              type="button"
              className={
                expandedOrder === step.order ? styles.stepNavItemActive : styles.stepNavItem
              }
              data-testid={`pipeline-step-nav-${step.order}`}
              onClick={() => handleStepNav(step.order)}
            >
              <span className={styles.stepNavOrder}>{step.order}</span>
              <span className={styles.stepNavName}>{step.name}</span>
            </button>
          ))}
        </nav>
      </div>

      <ol className={styles.list}>
        {steps.map((step) => (
          <TraceStepItem
            key={step.order}
            step={step}
            isExpanded={expandedOrder === step.order}
            onToggle={() =>
              setExpandedOrder((current) => (current === step.order ? null : step.order))
            }
            activePayloadTab={payloadTabs[step.order] ?? "output"}
            onPayloadTabChange={(tab) =>
              setPayloadTabs((current) => ({ ...current, [step.order]: tab }))
            }
          />
        ))}
      </ol>
    </section>
  );
}
