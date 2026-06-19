import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { fetchDocumentById, fetchDocumentContentBlob, downloadDocument } from "../../../entities/document/api/documentApi";
import { Button } from "../../../shared/ui";
import styles from "./DocumentViewPage.module.css";

interface DocumentViewPageProps {
  documentId: string;
}

function isTextContentType(contentType: string): boolean {
  return contentType.startsWith("text/") || contentType.includes("markdown");
}

function isPdfContentType(contentType: string): boolean {
  return contentType === "application/pdf";
}

export function DocumentViewPage({ documentId }: DocumentViewPageProps) {
  const [textContent, setTextContent] = useState<string | null>(null);
  const [blobUrl, setBlobUrl] = useState<string | null>(null);

  const documentQuery = useQuery({
    queryKey: ["documents", documentId],
    queryFn: () => fetchDocumentById(documentId),
  });

  const contentQuery = useQuery({
    queryKey: ["documents", documentId, "content"],
    queryFn: () => fetchDocumentContentBlob(documentId),
  });

  useEffect(() => {
    const blob = contentQuery.data;
    if (!blob || !documentQuery.data) {
      return;
    }

    let cancelled = false;
    const nextBlobUrl = URL.createObjectURL(blob);
    setBlobUrl(nextBlobUrl);

    if (isTextContentType(documentQuery.data.contentType)) {
      void blob.text().then((text) => {
        if (!cancelled) {
          setTextContent(text);
        }
      });
    }

    return () => {
      cancelled = true;
      URL.revokeObjectURL(nextBlobUrl);
    };
  }, [contentQuery.data, documentQuery.data]);

  if (documentQuery.isLoading || contentQuery.isLoading) {
    return <p className={styles.state}>Loading document…</p>;
  }

  if (documentQuery.isError || contentQuery.isError || !documentQuery.data || !contentQuery.data) {
    return <p className={styles.state} data-testid="document-view-error">Failed to load document.</p>;
  }

  const document = documentQuery.data;

  return (
    <section className={styles.page} data-testid="document-view-page">
      <header className={styles.header}>
        <div>
          <h1 className={styles.title}>{document.name}</h1>
          <p className={styles.meta}>{document.fileName}</p>
        </div>
        <Button
          type="button"
          data-testid="document-view-download"
          onClick={() => void downloadDocument(documentId, document.fileName)}
        >
          Download
        </Button>
      </header>

      {isTextContentType(document.contentType) && textContent && (
        <pre className={styles.textContent} data-testid="document-view-text">
          {textContent}
        </pre>
      )}

      {isPdfContentType(document.contentType) && blobUrl && (
        <iframe
          className={styles.pdfFrame}
          title={document.name}
          src={blobUrl}
          data-testid="document-view-pdf"
        />
      )}

      {!isTextContentType(document.contentType) && !isPdfContentType(document.contentType) && (
        <p className={styles.state}>
          Preview is not available for this file type. Use Download to open the original file.
        </p>
      )}
    </section>
  );
}
