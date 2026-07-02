import { useState } from "react";
import { Button } from "../../../shared/ui";
import { useDeleteAllDocuments } from "../model/useDeleteAllDocuments";
import styles from "./DeleteAllDocuments.module.css";

interface DeleteAllDocumentsProps {
  documentCount: number;
}

export function DeleteAllDocuments({ documentCount }: DeleteAllDocumentsProps) {
  const [confirming, setConfirming] = useState(false);
  const deleteAllMutation = useDeleteAllDocuments();

  if (documentCount === 0) {
    return null;
  }

  const handleConfirm = () => {
    deleteAllMutation.mutate(undefined, {
      onSuccess: () => {
        setConfirming(false);
      },
    });
  };

  if (confirming) {
    return (
      <div className={styles.confirm} data-testid="delete-all-documents-confirm">
        <p className={styles.message}>
          Delete all {documentCount} document{documentCount === 1 ? "" : "s"}? Indexed chunks
          will be removed from Search and Chat. This cannot be undone.
        </p>
        <div className={styles.actions}>
          <Button
            variant="danger"
            onClick={handleConfirm}
            disabled={deleteAllMutation.isPending}
            data-testid="delete-all-documents-confirm-button"
          >
            {deleteAllMutation.isPending ? "Deleting…" : "Yes, delete all"}
          </Button>
          <Button
            variant="secondary"
            onClick={() => setConfirming(false)}
            disabled={deleteAllMutation.isPending}
            data-testid="delete-all-documents-cancel-button"
          >
            Cancel
          </Button>
        </div>
        {deleteAllMutation.isError && (
          <p className={styles.error} data-testid="delete-all-documents-error">
            Failed to delete documents. Please try again.
          </p>
        )}
      </div>
    );
  }

  return (
    <Button
      variant="danger"
      onClick={() => setConfirming(true)}
      data-testid="delete-all-documents-button"
    >
      Delete all documents
    </Button>
  );
}
