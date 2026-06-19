export interface DocumentSourceReference {
  documentId: string;
  documentName: string;
}

export function uniqueDocumentSources<T extends DocumentSourceReference>(sources: T[]): T[] {
  const seen = new Set<string>();

  return sources.filter((source) => {
    if (seen.has(source.documentId)) {
      return false;
    }

    seen.add(source.documentId);
    return true;
  });
}
