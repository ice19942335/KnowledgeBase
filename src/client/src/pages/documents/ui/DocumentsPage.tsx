import { DocumentList, useDocuments } from "../../../entities/document";
import { UploadDocument } from "../../../features/upload-document/ui/UploadDocument";
import { useDeleteDocument } from "../../../features/delete-document/model/useDeleteDocument";
import styles from "./DocumentsPage.module.css";

export function DocumentsPage() {
  const { data: documents, isLoading, isError } = useDocuments();
  const deleteMutation = useDeleteDocument();

  return (
    <section className={styles.page}>
      <header>
        <h1 className={styles.title}>Documents</h1>
        <p className={styles.subtitle}>Upload PDF or text files to make them searchable.</p>
      </header>

      <UploadDocument />

      {isLoading && <p className={styles.state}>Loading documents…</p>}
      {isError && <p className={styles.state}>Failed to load documents.</p>}

      {documents && (
        <DocumentList
          documents={documents}
          onDelete={(id) => deleteMutation.mutate(id)}
          deletingId={deleteMutation.isPending ? deleteMutation.variables : undefined}
        />
      )}
    </section>
  );
}
