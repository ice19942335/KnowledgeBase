import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { Header } from "../widgets/header";
import { AiStatusBanner } from "../widgets/ai-status-banner";
import { useAiStatus } from "../entities/ai";
import { DocumentsPage } from "../pages/documents";
import { SearchPage } from "../pages/search";
import { ChatPage } from "../pages/chat";
import { ExplorerPage } from "../pages/explorer";
import { DocumentViewRoute } from "../pages/document-view";
import { QueryProvider } from "./providers/QueryProvider";
import styles from "./App.module.css";

function AppContent() {
  const { data: aiStatus } = useAiStatus();

  return (
    <div className={styles.layout}>
      {aiStatus && !aiStatus.isConfigured && <AiStatusBanner message={aiStatus.message} />}
      <Header />
      <main className={styles.main}>
        <Routes>
          <Route path="/" element={<DocumentsPage />} />
          <Route path="/search" element={<SearchPage />} />
          <Route path="/chat" element={<ChatPage />} />
          <Route path="/explorer" element={<ExplorerPage />} />
          <Route path="/documents/:documentId/view" element={<DocumentViewRoute />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  );
}

export function App() {
  return (
    <QueryProvider>
      <BrowserRouter>
        <AppContent />
      </BrowserRouter>
    </QueryProvider>
  );
}
