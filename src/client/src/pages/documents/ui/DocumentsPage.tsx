import { DocumentList, useDocuments } from "../../../entities/document";
import { UploadDocument } from "../../../features/upload-document/ui/UploadDocument";
import { useDeleteDocument } from "../../../features/delete-document/model/useDeleteDocument";
import { PageGuide } from "../../../shared/ui";
import styles from "./DocumentsPage.module.css";

const documentsGuideSteps = [
  {
    title: "Upload (Document API)",
    description:
      "POST /api/documents stores the file in blob storage and saves metadata for your tenant.",
  },
  {
    title: "Publish event (RabbitMQ)",
    description:
      "DocumentUploaded is sent to the message bus so processing can run asynchronously.",
  },
  {
    title: "Ingest (Ingestion Worker)",
    description:
      "Extract text from PDF/Markdown, split into chunks, generate a document summary, and create contextual embeddings with Gemini.",
  },
  {
    title: "Index (Search service)",
    description:
      "ChunksGenerated adds each chunk with its vector to pgvector so Search and Chat can retrieve it.",
  },
  {
    title: "Ready status",
    description:
      "DocumentProcessingCompleted updates the document status — once Ready, it appears in Search, Chat, and Explorer.",
  },
] as const;

export function DocumentsPage() {
  const { data: documents, isLoading, isError } = useDocuments();
  const deleteMutation = useDeleteDocument();

  return (
    <section className={styles.page} data-testid="documents-page">
      <header>
        <h1 className={styles.title}>Documents</h1>
        <PageGuide
          testId="documents-guide"
          summary="Upload and manage source files. Processing runs in the background — after ingestion finishes, chunks become searchable and usable in Chat."
          steps={[...documentsGuideSteps]}
        />
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
