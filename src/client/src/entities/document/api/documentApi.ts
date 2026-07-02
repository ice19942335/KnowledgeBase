import { httpClient } from "../../../shared/api/httpClient";
import type { BatchUploadResultDto, DocumentDto } from "../model/types";

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

export async function uploadDocumentsBatch(files: File[]): Promise<BatchUploadResultDto> {
  const formData = new FormData();
  for (const file of files) {
    formData.append("files", file);
  }

  const response = await httpClient.post<BatchUploadResultDto>("/api/documents/batch", formData);
  return response.data;
}

export async function deleteDocument(id: string): Promise<void> {
  await httpClient.delete(`/api/documents/${id}`);
}

export async function retryDocument(id: string): Promise<DocumentDto> {
  const response = await httpClient.post<DocumentDto>(`/api/documents/${id}/retry`);
  return response.data;
}

export async function deleteAllDocuments(): Promise<{ deletedCount: number }> {
  const response = await httpClient.delete<{ deletedCount: number }>("/api/documents");
  return response.data;
}
