import type { DocumentDto, DocumentStatus } from "./types";

const pendingStatuses: ReadonlySet<DocumentStatus> = new Set(["Uploaded", "Processing"]);

export const documentPollingIntervalMs = 2_000;

export function hasPendingDocuments(documents: DocumentDto[] | undefined): boolean {
  return documents?.some((document) => pendingStatuses.has(document.status)) ?? false;
}

export function isDocumentPending(document: DocumentDto | undefined): boolean {
  return document ? pendingStatuses.has(document.status) : false;
}
