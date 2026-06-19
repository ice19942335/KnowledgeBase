export type DocumentStatus = "Uploaded" | "Processing" | "Processed" | "Failed";

export interface DocumentDto {
  id: string;
  name: string;
  fileName: string;
  contentType: string;
  uploadedAtUtc: string;
  status: DocumentStatus;
  chunkCount: number;
}
