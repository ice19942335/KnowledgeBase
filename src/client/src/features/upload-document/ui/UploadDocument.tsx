import { useRef, useState, type FormEvent } from "react";
import { Button } from "../../../shared/ui";
import { useUploadDocument } from "../model/useUploadDocument";
import styles from "./UploadDocument.module.css";

function formatSelectedFiles(files: File[]): string {
  if (files.length === 1) {
    return files[0].name;
  }

  return `${files.length} files selected`;
}

export function UploadDocument() {
  const formRef = useRef<HTMLFormElement>(null);
  const [files, setFiles] = useState<File[]>([]);
  const [name, setName] = useState("");
  const [batchSummary, setBatchSummary] = useState<string | null>(null);
  const { mutate, isPending, isError } = useUploadDocument();

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (files.length === 0) {
      return;
    }

    setBatchSummary(null);

    mutate(
      { files, name: files.length === 1 ? name.trim() || undefined : undefined },
      {
        onSuccess: (result) => {
          if (result.mode === "batch" && result.result.failedCount > 0) {
            const failedNames = result.result.results
              .filter((item) => item.document === null)
              .map((item) => item.fileName)
              .join(", ");
            setBatchSummary(
              `Uploaded ${result.result.succeededCount} of ${result.result.results.length} files. Failed: ${failedNames}.`,
            );
          }

          setFiles([]);
          setName("");
          formRef.current?.reset();
        },
      },
    );
  };

  return (
    <form ref={formRef} className={styles.form} onSubmit={handleSubmit}>
      <input
        className={styles.field}
        type="file"
        accept=".pdf,.txt,.md"
        multiple
        aria-label="Document files"
        data-testid="document-files-input"
        onChange={(event) => setFiles(Array.from(event.target.files ?? []))}
      />
      {files.length > 0 && (
        <span className={styles.selection} data-testid="selected-files-summary">
          {formatSelectedFiles(files)}
        </span>
      )}
      {files.length === 1 && (
        <input
          className={styles.field}
          type="text"
          placeholder="Optional display name"
          aria-label="Document name"
          value={name}
          onChange={(event) => setName(event.target.value)}
        />
      )}
      <Button type="submit" disabled={files.length === 0 || isPending} data-testid="upload-documents-button">
        {isPending ? "Uploading…" : files.length > 1 ? `Upload ${files.length} files` : "Upload"}
      </Button>
      {isError && <span className={styles.error}>Upload failed. Please try again.</span>}
      {batchSummary && (
        <span className={styles.warning} data-testid="batch-upload-summary">
          {batchSummary}
        </span>
      )}
    </form>
  );
}
