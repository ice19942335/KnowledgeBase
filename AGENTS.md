# KnowledgeBase — Agent Context

AI-powered multi-tenant SaaS: upload documents → ingest/chunk/embed → semantic search → RAG chat with source references.

Full product spec, roadmap, and API reference: [README.md](README.md).

## Quick Start

```bash
# Infra only (optional; Aspire provisions its own containers)
docker compose -f deploy/docker-compose.infra.yml up -d

# Preferred: run entire stack (Postgres, RabbitMQ, Redis, all services, gateway, Vite client)
dotnet run --project src/AppHost

# Secrets (required for AI features)
cd src/AppHost
dotnet user-secrets set "Gemini:ApiKey" "your-key"
dotnet user-secrets set "Google:ClientId" "..."
dotnet user-secrets set "Google:ClientSecret" "..."

# Tests
dotnet test
cd src/client && npm run test
```

| URL | Purpose |
|-----|---------|
| Aspire Dashboard | ports printed in console |
| Gateway | `/api/*` — single entry point for client |
| Client | `http://localhost:5173` (Vite proxy `/api` → gateway) |
| Per-service Swagger | Development only, via gateway aggregation |

## Architecture

```text
React (src/client) → YARP Gateway → microservices
                                    ↓
                              RabbitMQ (MassTransit)
                                    ↓
                    Ingestion Worker → Search (pgvector) → Chat (RAG)
```

- **Database-per-service** on one Postgres instance (pgvector image): `identitydb`, `tenantdb`, `documentdb`, `searchdb`, `chatdb`.
- **Tenant isolation**: `X-Tenant-Id` header is the single source of truth; gateway verifies membership before propagating.
- **Auth**: Google OAuth only → OpenIddict JWT. Roles per tenant: Admin, Manager, Employee.
- **AI**: Gemini (embeddings `gemini-embedding-001`, chat `gemini-3.5-flash`, 1536 dims). Without `Gemini:ApiKey` → HTTP 503 on AI endpoints.
- **Contextual embeddings**: index-time wrapper (document name, LLM summary, section title) for `RETRIEVAL_DOCUMENT`; raw `Content` stored for RAG prompts.
- **Retrieval pipeline**: hybrid vector+keyword (RRF) → neighbor expansion → LLM rerank → `FinalTopK` chunks. Config section `Search`.
- **RAG test doc**: `assets/HR_Policy_MeatKombinat_EN.md` + `assets/QuestionExamples.md` (cross-chunk XREF scenarios).
- **Messaging flow**: `DocumentUploaded` → Ingestion (extract/chunk/embed) → `ChunksGenerated` → Search indexes → `DocumentProcessingCompleted`.

## Solution Layout

```text
src/
  AppHost/                  # .NET Aspire orchestration
  ServiceDefaults/          # OpenTelemetry, health, resilience, Swagger helpers
  Gateway/                  # YARP reverse proxy
  BuildingBlocks/           # Shared libraries (Auth, Tenancy, Messaging, Ai, Web, Contracts, SharedKernel)
  Services/
    Identity/               # Google OAuth + OpenIddict
    Tenant/                 # Tenant + membership CRUD
    Document/               # Upload, blob storage, publish events
    Ingestion/              # Worker: PDF extract, chunk, embed
    Search/                 # pgvector semantic search + explorer/trace APIs
    Chat/                   # RAG pipeline, conversation history
  client/                   # React + TS + Vite (Feature-Sliced Design)
tests/KnowledgeBase.UnitTests/
deploy/                     # docker-compose, .env.example
```

Legacy monolith projects (`src/KnowledgeBase.Api`, `KnowledgeBase.Application`, etc.) exist but are **not** part of the active Aspire stack — prefer `Services/*`.

## Layer Conventions (per microservice)

Clean Architecture: `Api` → `Application` → `Domain` ← `Infrastructure`.

| Layer | Responsibility |
|-------|----------------|
| Api | Controllers, `Program.cs`, DI wiring, Swagger |
| Application | Handlers, services, `*Options.cs`, DTOs |
| Domain | Entities, value objects, domain events |
| Infrastructure | EF Core `DbContext`, migrations, external clients |

### Backend rules

- **Controllers**, not minimal API lambdas, for HTTP endpoints.
- Configuration: `*Options.cs` + `services.Configure<T>()` + DI — never `IConfiguration["key"]` in business code.
- EF migrations: always include `.Designer.cs`; auto-apply on startup in Development only.
- C# private/internal fields: camelCase, no leading underscore.
- Comments in English only; keep minimal.
- `TreatWarningsAsErrors` is enabled globally (`Directory.Build.props`).
- Integration events live in `BuildingBlocks/KnowledgeBase.Contracts`.
- Shared cross-cutting: `KnowledgeBase.Tenancy` (middleware), `KnowledgeBase.Auth` (JWT + policies), `KnowledgeBase.Web` (Swagger + ProblemDetails).

### Frontend rules (`src/client`)

- **Feature-Sliced Design**: `app/`, `pages/`, `widgets/`, `features/`, `entities/`, `shared/`.
- State: **Zustand** (client), **TanStack Query** (server/API).
- CSS Modules only — no inline styles.
- Add `data-testid` / semantic selectors for testability.
- Pages: Documents, Search, Chat, Explorer (`/explorer`).

## Key Files to Read First

| Task | Start here |
|------|------------|
| Orchestration | `src/AppHost/AppHost.cs` |
| Gateway routes | `src/Gateway/KnowledgeBase.Gateway/` |
| Document upload | `src/Services/Document/KnowledgeBase.Document.Api/Controllers/DocumentsController.cs` |
| Ingestion pipeline | `src/Services/Ingestion/KnowledgeBase.Ingestion.Worker/` |
| Embeddings | `src/BuildingBlocks/KnowledgeBase.Ai/GeminiEmbeddingGenerator.cs` |
| Vector search | `src/Services/Search/KnowledgeBase.Search.Application/` |
| RAG chat | `src/Services/Chat/KnowledgeBase.Chat.Application/` |
| Integration events | `src/BuildingBlocks/KnowledgeBase.Contracts/` |
| Frontend API client | `src/client/src/shared/api/httpClient.ts` |

## API Surface (via Gateway)

All tenant-scoped endpoints require `Authorization: Bearer <jwt>` and `X-Tenant-Id: <guid>`.

```http
GET  /api/auth/login/google
POST /api/documents          # multipart upload
GET  /api/documents
POST /api/search             # { "query": "..." }
POST /api/chat               # { "conversationId": null, "question": "..." }
GET  /api/search/explorer    # indexed chunks
POST /api/search/trace       # search with pipeline trace
POST /api/chat/trace         # RAG with full trace
GET  /api/chat/ai/status     # AI availability banner
```

## Implementation Status

**Done**: Aspire orchestration, all 6 services + gateway + client, document CRUD, async ingestion (RabbitMQ), pgvector search, RAG chat with sources & history, Google OAuth, tenant service, Swagger everywhere, 38 backend + 13 frontend unit tests.

**Not done** (do not assume these exist):
- Semantic Kernel orchestration (direct Gemini calls via interfaces instead)
- EF global query filters on Document/Search/Chat DbContexts (tenant isolation partial)
- Elasticsearch hybrid search
- Prometheus/Grafana dashboards
- Frontend Google OAuth integration
- Integration tests (Testcontainers)
- Gateway rate limiting
- Helm / K8s manifests

## Testing

- Backend: `tests/KnowledgeBase.UnitTests/` — xUnit, focus on chunker, ingestion, search, RAG, prompts.
- Frontend: Vitest in `src/client/src/**/*.test.tsx`.
- After logic changes: run affected tests, not the entire suite unless warranted.
- No `Thread.Sleep`; max wait `TimeSpan.FromSeconds(3)` in tests.
- Use `DateTime.UtcNow.AddDays(...)` instead of hardcoded dates.
- TDD for new functionality; add meaningful unit tests on both backend and frontend.

## When Changing Functionality

1. Update [README.md](README.md) implementation status if scope changes.
2. Add `deploy/.env.example` entries for new config.
3. Create EF migration + `.Designer.cs` for schema changes.
4. Ensure Swagger works in Development with Bearer + `X-Tenant-Id`.
