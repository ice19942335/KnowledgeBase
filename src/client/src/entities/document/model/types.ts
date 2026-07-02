export type DocumentStatus = "Uploaded" | "Processing" | "Processed" | "Failed";

export interface DocumentDto {
  id: string;
  name: string;
  fileName: string;
  contentType: string;
  status: DocumentStatus;
  chunkCount: number;
  error: string | null;
  createdAt: string;
  processedAt: string | null;
}

export interface BatchUploadItemResult {
  fileName: string;
  document: DocumentDto | null;
  error: string | null;
}

export interface BatchUploadResultDto {
  results: BatchUploadItemResult[];
  succeededCount: number;
  failedCount: number;
}
