import { useState, type FormEvent } from "react";
import { Button } from "../../../shared/ui";
import { useUploadDocument } from "../model/useUploadDocument";
import styles from "./UploadDocument.module.css";

export function UploadDocument() {
  const [file, setFile] = useState<File | null>(null);
  const [name, setName] = useState("");
  const { mutate, isPending, isError } = useUploadDocument();

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!file) {
      return;
    }

    mutate(
      { file, name: name.trim() || undefined },
      {
        onSuccess: () => {
          setFile(null);
          setName("");
          event.currentTarget.reset();
        },
      },
    );
  };

  return (
    <form className={styles.form} onSubmit={handleSubmit}>
      <input
        className={styles.field}
        type="file"
        accept=".pdf,.txt,.md"
        aria-label="Document file"
        onChange={(event) => setFile(event.target.files?.[0] ?? null)}
      />
      <input
        className={styles.field}
        type="text"
        placeholder="Optional display name"
        aria-label="Document name"
        value={name}
        onChange={(event) => setName(event.target.value)}
      />
      <Button type="submit" disabled={!file || isPending}>
        {isPending ? "Uploading…" : "Upload"}
      </Button>
      {isError && <span className={styles.error}>Upload failed. Please try again.</span>}
    </form>
  );
}
