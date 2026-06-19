import { httpClient } from "../../../shared/api/httpClient";
import type { DocumentDto } from "../model/types";

export async function fetchDocuments(): Promise<DocumentDto[]> {
  const response = await httpClient.get<DocumentDto[]>("/api/documents");
  return response.data;
}

export async function fetchDocumentById(documentId: string): Promise<DocumentDto> {
  const response = await httpClient.get<DocumentDto>(`/api/documents/${documentId}`);
  return response.data;
}

export async function fetchDocumentContentBlob(
  documentId: string,
  download = false,
): Promise<Blob> {
  const response = await httpClient.get<Blob>(`/api/documents/${documentId}/content`, {
    params: download ? { download: true } : undefined,
    responseType: "blob",
  });

  return response.data;
}

export async function downloadDocument(documentId: string, fileName: string): Promise<void> {
  const blob = await fetchDocumentContentBlob(documentId, true);
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = fileName;
  anchor.click();
  URL.revokeObjectURL(url);
}

export async function uploadDocument(file: File, name?: string): Promise<DocumentDto> {
  const formData = new FormData();
  formData.append("file", file);
  if (name) {
    formData.append("name", name);
  }

  const response = await httpClient.post<DocumentDto>("/api/documents", formData);
  return response.data;
}

export async function deleteDocument(id: string): Promise<void> {
  await httpClient.delete(`/api/documents/${id}`);
}
