import styles from "./AiStatusBanner.module.css";

type AiStatusBannerProps = {
  message: string;
};

export function AiStatusBanner({ message }: AiStatusBannerProps) {
  return (
    <div className={styles.banner} role="alert" data-testid="ai-status-banner">
      <p className={styles.text}>{message}</p>
    </div>
  );
}
