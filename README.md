# AI Knowledge Base

## Project Overview

AI Knowledge Base is a SaaS-style document management and AI search platform.

The system allows users to:

* Upload documents
* Index document content
* Perform semantic search
* Ask questions in natural language
* Receive AI-generated answers with references to source documents

The project is intended to demonstrate modern .NET development skills, including:

* ASP.NET Core
* React
* PostgreSQL
* Vector Search
* Retrieval Augmented Generation (RAG)
* OpenAI Integration
* Semantic Kernel
* RabbitMQ
* Docker
* Distributed Architecture

---

# Business Problem

Companies accumulate large amounts of documentation:

* Policies
* Contracts
* Technical Documentation
* User Manuals
* Internal Knowledge Bases

Finding information manually is slow and inefficient.

This project provides an AI-powered assistant capable of answering questions based on uploaded company documents.

Example:

Question:

How many vacation days does an employee receive?

Answer:

Employees receive 25 vacation days annually.

Source:
HR_Policy.pdf

---

# Technology Stack

## Backend

* .NET 10 / ASP.NET Core 10
* .NET Aspire (local orchestration and service composition)
* Entity Framework Core
* PostgreSQL (database-per-service)
* pgvector

## API Gateway

* YARP (Yet Another Reverse Proxy)
* Rate limiting, request aggregation, tenant routing

## Identity & Access

* Google OAuth 2.0 / OpenID Connect (the only sign-in method)
* OpenIddict (token issuance, refresh tokens, downstream validation)
* JWT access tokens + refresh tokens
* No self-registration — accounts are created on first Google sign-in

## Frontend

* React
* TypeScript
* Vite
* Feature-Sliced Design
* Zustand (state management)
* TanStack Query (server state)
* Pages: Documents, Search, Chat, **Explorer** (`/explorer`) — each page includes
  a short summary and an expandable **How it works under the hood** guide:
  * **Documents** — upload/list/delete; triggers async ingestion
    (extract → chunk → contextual embed → pgvector index via RabbitMQ).
  * **Search** — hybrid vector + keyword retrieval, RRF merge, neighbor expansion,
    LLM rerank; returns ranked passages (not a generated answer).
  * **Chat** — RAG: Search retrieves context → prompt → Gemini answer with sources
    and conversation history.
  * **Explorer** — split view: traced chat pipeline (left) and source-document
    chunks (right) loaded after each traced answer; per-document subtabs when
    multiple files contributed; under each document, **All chunks** and
    **Used for answer** (priority order from RAG sources); collapsible steps
    with Input/Output tabs.

## AI

* OpenAI API / Azure OpenAI
* Semantic Kernel

## Messaging

* RabbitMQ
* MassTransit (messaging abstraction, sagas, outbox)

## Caching

* Redis (distributed cache, tenant context, rate limiting)

## Search

* PostgreSQL Vector Search (pgvector)
* Elasticsearch (hybrid search)

## Infrastructure

* Docker
* Docker Compose (local)
* Kubernetes + Helm (production)

## API Documentation

* Swagger / OpenAPI (Swashbuckle.AspNetCore) — **required** on every API service
* Swagger UI exposed in Development for manual testing
* Bearer (JWT) + `X-Tenant-Id` wired into Swagger so endpoints are testable
* Gateway aggregates the per-service OpenAPI documents

## Observability

* OpenTelemetry (traces, metrics, logs)
* Prometheus
* Grafana
* Loki + Tempo

---

# Architecture

The system follows a **microservice-oriented architecture** designed for a
**multi-tenant SaaS**. The React client talks only to the API Gateway, which
handles authentication, tenant resolution, and routing to the individual
services. Services communicate asynchronously through RabbitMQ and own their
data (database-per-service).

```text
                         React Client (src/client)
                                   |
                                   v
                         API Gateway (YARP)
              authn / tenant resolution / rate limiting
                                   |
   +-------------+-------------+-------------+-------------+-------------+
   |             |             |             |             |             |
   v             v             v             v             v             v
 Identity     Tenant       Document      Ingestion      Search         Chat
 Service      Service      Service       Service        Service        Service
(Google      (provision-  (upload /     (extraction /  (vector /      (RAG /
 OAuth /      ing /        metadata /    chunking /     keyword /      Semantic
 OIDC)        billing)     storage)      embeddings)    hybrid)        Kernel)
   |             |             |             |             |             |
   +-------------+------+------+------+------+------+-------+-------------+
                        |             |
                        v             v
                  RabbitMQ        Redis (cache /
                (MassTransit)     tenant context)
                        |
        +---------------+--------------------------------+
        |               |               |               |
        v               v               v               v
   PostgreSQL       PostgreSQL      Elasticsearch    Object Storage
   + pgvector       (per service)   (hybrid search)  (blobs / files)
        |
        v
   OpenAI / Azure OpenAI
```

Cross-cutting concerns are shared as libraries (BuildingBlocks): tenant context,
authentication, messaging, telemetry, and result/error handling. The whole
solution is composed and run locally with **.NET Aspire**.

# Solution Structure

Each microservice is independently deployable and internally follows Clean
Architecture (`Api` / `Application` / `Domain` / `Infrastructure`). Shared,
reusable code lives in `BuildingBlocks`. The frontend lives in `src/client`.

```text
src/

  AppHost/                          # .NET Aspire orchestration host (Postgres, RabbitMQ, Redis, all services)
  ServiceDefaults/                  # Shared Aspire service defaults (OpenTelemetry, health, resilience)

  Gateway/
    KnowledgeBase.Gateway/          # YARP API gateway (routing, auth, tenant context)

  BuildingBlocks/
    KnowledgeBase.SharedKernel/     # Blob storage (IFileStorage), IEventPublisher
    KnowledgeBase.Contracts/        # Integration events (DocumentUploaded, ChunksGenerated, etc.)
    KnowledgeBase.Messaging/        # MassTransit / RabbitMQ registration
    KnowledgeBase.Tenancy/          # ITenantContext, X-Tenant-Id middleware
    KnowledgeBase.Auth/             # JWT Bearer, roles (Admin/Manager/Employee), policies
    KnowledgeBase.Ai/               # Gemini clients (embeddings + chat completion)
    KnowledgeBase.Web/              # Swagger (X-Tenant-Id + JWT), ProblemDetails, JSON enum

  Services/

    Identity/                       # Google OAuth + OpenIddict token issuance
      KnowledgeBase.Identity.Api/

    Tenant/                         # Tenant + membership provisioning
      KnowledgeBase.Tenant.Api/
      KnowledgeBase.Tenant.Application/
      KnowledgeBase.Tenant.Domain/
      KnowledgeBase.Tenant.Infrastructure/

    Document/                       # Upload, metadata, blob storage, event publishing
      KnowledgeBase.Document.Api/
      KnowledgeBase.Document.Application/
      KnowledgeBase.Document.Domain/
      KnowledgeBase.Document.Infrastructure/

    Ingestion/                      # Background worker: extraction, chunking, embeddings
      KnowledgeBase.Ingestion.Worker/
      KnowledgeBase.Ingestion.Application/
      KnowledgeBase.Ingestion.Infrastructure/

    Search/                         # pgvector search + chunk indexing
      KnowledgeBase.Search.Api/
      KnowledgeBase.Search.Application/
      KnowledgeBase.Search.Domain/
      KnowledgeBase.Search.Infrastructure/

    Chat/                           # RAG, conversation history, calls Search via HTTP
      KnowledgeBase.Chat.Api/
      KnowledgeBase.Chat.Application/
      KnowledgeBase.Chat.Domain/
      KnowledgeBase.Chat.Infrastructure/

  client/                          # React + TypeScript + Vite frontend (FSD)

tests/
  KnowledgeBase.UnitTests/         # 21 backend unit tests

deploy/
  docker-compose.infra.yml         # Standalone infra: Postgres+pgvector
  .env.example                     # Default environment variables
```

# Authentication & Authorization

Authentication is centralized in the **Identity Service**, which authenticates
users **exclusively through Google** (OAuth 2.0 / OpenID Connect) and issues the
application's own tokens via OpenIddict. The API Gateway and every downstream
service validate the issued JWT access tokens. Tokens carry the user identity and
role/permission claims. The active tenant is **not** taken from the token — it
comes solely from the `X-Tenant-Id` request header (see Multi-Tenancy).

## Login Method

* **Google** — the only sign-in option (OAuth 2.0 / OpenID Connect).

There is **no registration form and no password-based login**. A user account is
created automatically on the first successful Google sign-in (just-in-time
provisioning) and linked to a tenant membership. Subsequent logins reuse that
account.

## Token Flow

```text
User
 ↓ (Sign in with Google)
Identity Service (validates Google id_token, JIT-provisions user)
 ↓ issues application access token (JWT) + refresh token via OpenIddict
API Gateway (validates token, resolves tenant)
 ↓ forwards request with claims
Downstream Service (validates token, enforces policy)
```

## Roles

Roles are scoped **per tenant** — a user can be an `Admin` in one tenant and an
`Employee` in another. Because the active tenant is resolved from the
`X-Tenant-Id` header, the user's role is determined for that tenant after
resolution. Authorization is enforced through policy-based checks that combine
the user's role in the active tenant, permissions, and the resolved tenant
context.

| Role         | Description                                                                 |
|--------------|-----------------------------------------------------------------------------|
| **Admin**    | Full control of a tenant: manage members, roles, billing/plan, all documents, and tenant settings. |
| **Manager**  | Manages content and team workflows: upload/manage all tenant documents, manage collections, view usage; cannot manage billing or change member roles. |
| **Employee** | Day-to-day usage: upload own documents, search, ask questions in chat, view sources; read-only access to shared documents. |

### Permission Matrix

| Capability                         | Admin | Manager | Employee |
|------------------------------------|:-----:|:-------:|:--------:|
| Manage tenant settings & billing   |   ✓   |    –    |    –     |
| Invite / remove members            |   ✓   |    –    |    –     |
| Assign roles                       |   ✓   |    –    |    –     |
| Upload documents                   |   ✓   |    ✓    |    ✓     |
| Manage all tenant documents        |   ✓   |    ✓    |    –     |
| Delete any document                |   ✓   |    ✓    |    –     |
| Manage own documents               |   ✓   |    ✓    |    ✓     |
| Semantic search & RAG chat         |   ✓   |    ✓    |    ✓     |
| View usage & analytics             |   ✓   |    ✓    |    –     |

# Multi-Tenancy

The platform is multi-tenant by design. Every request is associated with a
tenant, resolved by the gateway and propagated to all services.

## Tenant Resolution

The tenant id has a **single source of truth: the `X-Tenant-Id` request header**.
The gateway reads it from the request, verifies that the authenticated user is a
member of that tenant, and only then propagates the resolved tenant context
downstream. Requests without `X-Tenant-Id`, or where the user is not a member of
the requested tenant, are rejected.

```text
Request (X-Tenant-Id header)
 ↓
API Gateway
 ↓ read X-Tenant-Id → verify user membership for that tenant
Tenant Context (propagated downstream)
 ↓
Downstream Service applies tenant filter
```

## Data Isolation

* **Shared database, shared schema** with a mandatory `TenantId` column and EF
  Core global query filters (default, cost-efficient).
* **Row isolation** enforced at the persistence layer so no query can leak data
  across tenants.
* Optional upgrade path to **database-per-tenant** for premium/enterprise plans.

## Principles

* The `X-Tenant-Id` header is the single source of the tenant id, but it is
  never trusted blindly — the gateway verifies it against the authenticated
  user's tenant membership before propagating it.
* All cross-service messages carry the `TenantId` for end-to-end isolation.
* Background workers restore tenant context from message metadata before
  processing.

# API Documentation (Swagger)

Swagger / OpenAPI is **mandatory for development and testing**. Every service
that exposes HTTP endpoints must serve a Swagger UI in the `Development`
environment.

## Per-service requirements

* Package: `Swashbuckle.AspNetCore`.
* Swagger UI available at `/swagger` (Development only).
* OpenAPI JSON available at `/swagger/v1/swagger.json`.
* `Bearer` (JWT) security scheme registered so endpoints can be authorized.
* `X-Tenant-Id` registered as a global header parameter so tenant-scoped
  endpoints are testable from the UI.

## Standard Swagger setup (reused via `ServiceDefaults`)

```csharp
public static IServiceCollection AddKnowledgeBaseSwagger(
    this IServiceCollection services,
    string title)
{
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = title, Version = "v1" });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        });

        options.OperationFilter<TenantHeaderOperationFilter>(); // adds X-Tenant-Id
    });

    return services;
}
```

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

The gateway exposes an aggregated Swagger UI listing every service document so a
developer (or an AI agent) can exercise the whole API from one place.

# Local Development Environment

This section is the source of truth for ports, container images, and environment
variables. It is intentionally concrete so the setup can be reproduced
automatically.

## Service & Port Map

| Component       | Type        | Local URL                       | Container Port |
|-----------------|-------------|---------------------------------|:--------------:|
| Gateway         | YARP        | http://localhost:8080           | 8080           |
| Identity        | API         | http://localhost:8081/swagger   | 8081           |
| Tenant          | API         | http://localhost:8082/swagger   | 8082           |
| Document        | API         | http://localhost:8083/swagger   | 8083           |
| Search          | API         | http://localhost:8084/swagger   | 8084           |
| Chat            | API         | http://localhost:8085/swagger   | 8085           |
| Ingestion       | Worker      | (no HTTP, health only :8086)    | 8086           |
| Client          | React/Vite  | http://localhost:5173           | 5173           |
| PostgreSQL      | pgvector    | localhost:5432                  | 5432           |
| RabbitMQ        | broker      | http://localhost:15672 (mgmt)   | 5672 / 15672   |
| Redis           | cache       | localhost:6379                  | 6379           |
| Elasticsearch   | search      | http://localhost:9200           | 9200           |

## Databases (one PostgreSQL instance, database-per-service)

```text
kb_identity   kb_tenant   kb_document   kb_search   kb_chat
```

Each service owns and migrates its own database. `Ingestion` shares the
`kb_document` database (it processes documents) but only through its own
`DbContext`.

## `deploy/docker-compose.infra.yml`

Infrastructure-only compose used during day-to-day development (services run from
the IDE or via .NET Aspire, infra runs in containers).

```yaml
services:
  postgres:
    image: pgvector/pgvector:pg17
    environment:
      POSTGRES_USER: kb
      POSTGRES_PASSWORD: kb_dev_password
      POSTGRES_DB: knowledgebase
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U kb"]
      interval: 10s
      timeout: 5s
      retries: 5

  rabbitmq:
    image: rabbitmq:3-management
    environment:
      RABBITMQ_DEFAULT_USER: kb
      RABBITMQ_DEFAULT_PASS: kb_dev_password
    ports:
      - "5672:5672"
      - "15672:15672"
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "-q", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.15.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - ES_JAVA_OPTS=-Xms512m -Xmx512m
    ports:
      - "9200:9200"
    healthcheck:
      test: ["CMD-SHELL", "curl -fs http://localhost:9200 || exit 1"]
      interval: 10s
      timeout: 5s
      retries: 10

volumes:
  pgdata:
```

## `deploy/docker-compose.yml`

Full-stack compose (infra + all services + gateway + client). Each service is
built from its own `Dockerfile`. Shown abbreviated; every API service follows the
same shape.

```yaml
services:
  postgres:
    extends:
      file: docker-compose.infra.yml
      service: postgres
  rabbitmq:
    extends:
      file: docker-compose.infra.yml
      service: rabbitmq
  redis:
    extends:
      file: docker-compose.infra.yml
      service: redis
  elasticsearch:
    extends:
      file: docker-compose.infra.yml
      service: elasticsearch

  identity:
    build:
      context: ..
      dockerfile: src/Services/Identity/KnowledgeBase.Identity.Api/Dockerfile
    env_file: .env
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__Database: "Host=postgres;Database=kb_identity;Username=kb;Password=kb_dev_password"
    ports:
      - "8081:8081"
    depends_on:
      postgres:
        condition: service_healthy

  document:
    build:
      context: ..
      dockerfile: src/Services/Document/KnowledgeBase.Document.Api/Dockerfile
    env_file: .env
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__Database: "Host=postgres;Database=kb_document;Username=kb;Password=kb_dev_password"
      RabbitMq__Host: "rabbitmq"
    ports:
      - "8083:8083"
    depends_on:
      postgres:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy

  # tenant, search, chat: same shape as above
  # ingestion: worker, no published HTTP port (exposes :8086 health only)

  gateway:
    build:
      context: ..
      dockerfile: src/Gateway/KnowledgeBase.Gateway/Dockerfile
    env_file: .env
    environment:
      ASPNETCORE_ENVIRONMENT: Development
    ports:
      - "8080:8080"
    depends_on:
      - identity
      - document

  client:
    build:
      context: ../src/client
      dockerfile: Dockerfile
    environment:
      VITE_API_BASE_URL: "http://localhost:8080"
    ports:
      - "5173:5173"

volumes:
  pgdata:
```

## `deploy/.env.example`

```dotenv
# Database
POSTGRES_USER=kb
POSTGRES_PASSWORD=kb_dev_password

# RabbitMQ
RabbitMq__Host=localhost
RabbitMq__Username=kb
RabbitMq__Password=kb_dev_password

# Redis
ConnectionStrings__Redis=localhost:6379

# Elasticsearch
Elasticsearch__Url=http://localhost:9200

# Google OAuth (required for sign-in)
Authentication__Google__ClientId=your-google-client-id
Authentication__Google__ClientSecret=your-google-client-secret

# Gemini (embeddings + chat)
Gemini__ApiKey=your-gemini-api-key
Gemini__EmbeddingModel=gemini-embedding-001
Gemini__ChatModel=gemini-3.5-flash
Gemini__EmbeddingDimensions=1536
```

# AI Implementation Guide

This blueprint lets an AI agent implement the system phase by phase with minimal
ambiguity. Follow the conventions, then execute each phase until its **Definition
of Done** passes.

## Global Conventions

* Solution file: `KnowledgeBase.sln` at the repository root.
* Namespaces and project names: `KnowledgeBase.<Service>.<Layer>`.
* Every service follows Clean Architecture: `Api` / `Application` / `Domain` /
  `Infrastructure`.
* Endpoints are registered with concrete classes/extension methods — **no inline
  lambda endpoint registrations**.
* Each API service: enables Swagger (Development), exposes `/health` and
  `/health/ready`, registers `ServiceDefaults`.
* Configuration is bound to strongly-typed `*Options` classes (no raw
  `IConfiguration["..."]` reads) and injected via DI.
* EF Core: every migration is generated together with its `.Designer.cs` file;
  apply migrations automatically on startup in Development only.
* Tests live in `tests/` — write unit tests for application logic and an
  integration test that boots the service with Testcontainers.

## Standard Commands

```bash
# 0. Prerequisites: .NET 10 SDK, Node 20+, Docker.

# 1. Start infrastructure
docker compose -f deploy/docker-compose.infra.yml up -d

# 2. Restore & build
dotnet restore
dotnet build

# 3. Run everything via .NET Aspire (preferred for local dev; starts backend + Vite client)
dotnet run --project src/AppHost

# 3b. Or run the full stack in containers
docker compose -f deploy/docker-compose.yml up --build

# 4. Frontend only (when running backend without AppHost; set VITE_API_BASE_URL in src/client/.env.development)
cd src/client && npm install && npm run dev

# 5. Add an EF Core migration (with Designer file, generated by default)
dotnet ef migrations add <Name> \
  --project src/Services/Document/KnowledgeBase.Document.Infrastructure \
  --startup-project src/Services/Document/KnowledgeBase.Document.Api

# 6. Run tests
dotnet test
```

## Per-phase Definition of Done (global rules)

A phase is complete only when **all** of the following hold:

1. `dotnet build` and `dotnet test` succeed.
2. `docker compose -f deploy/docker-compose.infra.yml up -d` is enough to run the
   phase locally (plus `dotnet run`).
3. Every new/changed API endpoint is visible and callable in Swagger UI.
4. New endpoints enforce authentication and tenant isolation (except the
   Identity sign-in endpoints).
5. New configuration is added to `deploy/.env.example`.
6. Unit tests (and integration tests where applicable) cover the new logic.
7. This document is updated to reflect any added/changed/removed functionality.

# Development Roadmap

> Each phase below lists concrete artifacts. Combined with the conventions and
> the **Definition of Done** above, the steps are intended to be executed
> automatically by an AI agent.

## Phase 1 – Project Setup

### Goal

Create base architecture, shared building blocks, and local infrastructure.

### Tasks

* Create Git repository and `KnowledgeBase.sln`.
* Create `BuildingBlocks` projects: `SharedKernel`, `Tenancy`, `Auth`,
  `Messaging`, `Observability`.
* Create `ServiceDefaults` with shared Swagger, health checks, OpenTelemetry,
  and resilience wiring.
* Set up `.NET Aspire` `AppHost` that provisions Postgres, RabbitMQ, Redis, and
  Elasticsearch and references the services.
* Add the `Document` service as the first vertical slice (Api/Application/
  Domain/Infrastructure) with Swagger enabled.
* Add the YARP `Gateway` routing to the `Document` service.
* Create `deploy/docker-compose.infra.yml`, `deploy/docker-compose.yml`, and
  `deploy/.env.example` (see Local Development Environment).
* Configure EF Core with an initial migration (+ `.Designer.cs`).
* Create the React app in `src/client` (Vite + TypeScript, FSD layout).

### Deliverable

`docker compose -f deploy/docker-compose.infra.yml up -d` + `dotnet run --project
src/AppHost` brings up the gateway and the `Document` service with a reachable
Swagger UI and a connected database.

### Definition of Done

Meets the global Definition of Done; Swagger UI for the `Document` service is
reachable through the gateway.

---

## Phase 2 – Document Management

### Goal

Upload and store documents.

### Tasks

Create Document entity:

```csharp
public class Document
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string FileName { get; set; }
    public DateTime UploadDate { get; set; }
}
```

Implement APIs:

```http
POST /documents
GET /documents
GET /documents/{id}
DELETE /documents/{id}
```

Frontend:

* Document List
* Upload Page
* Delete Action

### Deliverable

Users can upload and manage documents.

---

## Phase 3 – Text Extraction

### Goal

Extract text from uploaded files.

### Tasks

Install PDF library:

```text
UglyToad.PdfPig
```

Add field:

```csharp
Content
```

Process:

```text
PDF
 ↓
Text Extraction
 ↓
Database
```

### Deliverable

Document text is stored in database.

---

## Phase 4 – Document Chunking

### Goal

Split large documents into chunks.

### New Entity

```csharp
DocumentChunk
```

```csharp
public class DocumentChunk
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; }
}
```

Chunk size:

* 500–1000 characters

### Deliverable

Document content is split into searchable chunks.

---

## Phase 5 – Embeddings

### Goal

Generate vector embeddings.

### Tasks

Install pgvector.

Add embedding column:

```csharp
float[]
```

Process:

```text
Chunk
 ↓
Embedding API
 ↓
Vector
 ↓
Database
```

### Deliverable

Every chunk contains vector representation.

---

## Phase 6 – Semantic Search

### Goal

Find relevant chunks.

### API

```http
POST /search
```

Request:

```json
{
  "query": "vacation policy"
}
```

Process:

```text
Question
 ↓
Embedding
 ↓
Vector Search
 ↓
Top Chunks
```

### Deliverable

Semantic search works without AI generation.

---

## Phase 7 – RAG Chat

### Goal

Build AI Question Answering.

### Flow

```text
Question
 ↓
Embedding
 ↓
Vector Search
 ↓
Top Chunks
 ↓
Prompt
 ↓
LLM
 ↓
Answer
```

Prompt Rules:

* Use only provided context
* Do not invent information
* If information is unavailable say:
  "I don't know."

### API

```http
POST /chat
```

### Deliverable

Users can ask questions about uploaded documents.

---

## Phase 8 – Source References

### Goal

Increase trust in AI answers.

Example:

```text
Employees receive 25 vacation days.

Sources:

HR_Policy.pdf
Employee_Handbook.pdf
```

### Deliverable

Answers include references.

---

## Phase 9 – Chat History

### Entities

```csharp
Conversation
Message
```

### Features

* Create Chat
* Continue Chat
* View History

### Deliverable

ChatGPT-like experience.

---

## Phase 10 – Semantic Kernel

### Goal

Introduce AI orchestration.

### Plugins

```csharp
SearchDocuments()
GetDocument()
ListDocuments()
```

### Deliverable

AI can call application functions.

---

## Phase 11 – Background Processing

### Goal

Move heavy operations to workers.

### Infrastructure

RabbitMQ

### Workflow

```text
Upload
 ↓
Queue
 ↓
Worker
 ↓
Chunking
 ↓
Embeddings
```

### Deliverable

Non-blocking document processing.

---

## Phase 12 – Authentication & Authorization

### Goal

Stand up the Identity Service and secure all services through the gateway.

### Features

* Google OAuth 2.0 / OpenID Connect sign-in (the only login method)
* Just-in-time user provisioning on first Google sign-in (no registration form)
* Application token issuance via OpenIddict (JWT access + refresh tokens)
* Policy-based authorization enforced at gateway and services
* Swagger configured with the `Bearer` scheme so protected endpoints are testable
  with a pasted JWT during development

### Roles

```text
Admin
Manager
Employee
```

### Deliverable

Centralized Google-based authentication with role-based access control.

---

## Phase 13 – Multi-Tenancy

### New Entities

```csharp
Tenant
TenantMembership   // links User ↔ Tenant with a per-tenant Role
User
```

### Goal

Each company (tenant) accesses only its own data, with isolation enforced at the
persistence layer and tenant context propagated across all services.

### Tasks

* Tenant resolution in the gateway from the `X-Tenant-Id` header (single source), verified against user membership
* Tenant context propagation across services and message bus
* EF Core global query filters on `TenantId`
* Per-tenant role assignment via `TenantMembership`

### Deliverable

SaaS-ready, isolated multi-tenant architecture.

---

## Phase 14 – Hybrid Search

### Add Elasticsearch

Search Strategy:

```text
Vector Search
+
Keyword Search
```

### Deliverable

Improved retrieval quality.

---

## Phase 15 – Observability

### Add

* OpenTelemetry
* Prometheus
* Grafana

Monitor:

* Requests
* Errors
* Latency
* Token Consumption

### Deliverable

Production-ready monitoring.

---

# MVP Definition

MVP is completed after:

* Document Upload
* Text Extraction
* Chunking
* Embeddings
* Semantic Search
* AI Chat
* Source References

Expected Duration:

4–8 weekends for a single developer.

---

# Future Enhancements

* Microsoft Entra ID Authentication
* Azure OpenAI
* Streaming Responses
* OCR Support
* DOCX Support
* Image Analysis
* Voice Questions
* AI Agents
* Kubernetes Deployment
* Distributed Cache
* Event Sourcing Experiments
* MCP Integration

# Success Criteria

The project is considered successful when a user can:

1. Upload documents
2. Ask questions in natural language
3. Receive accurate AI-generated answers
4. See supporting sources
5. Continue conversations
6. Search across thousands of documents in seconds

---

# Running the System

The system is composed and orchestrated via **.NET Aspire**. All infrastructure
(PostgreSQL+pgvector, RabbitMQ, Redis) and services are started with one command.

```bash
# 1. Set secrets in AppHost user-secrets
cd src/AppHost
dotnet user-secrets set "Gemini:ApiKey" "your-gemini-api-key"
dotnet user-secrets set "Google:ClientId" "..."
dotnet user-secrets set "Google:ClientSecret" "..."
cd ../..

# 2. Run the Aspire AppHost (starts Postgres, RabbitMQ, Redis, all services, gateway, Vite client)
dotnet run --project src/AppHost

# 3. Aspire Dashboard: https://localhost:15xxxxx (shown in console)
#    Gateway: dynamic port (shown in dashboard)
#    React client: http://localhost:5173 (API via Vite proxy `/api` → gateway; no CORS setup needed)

# 4. Frontend without AppHost (optional)
cd src/client && npm install && npm run dev
# Set VITE_API_BASE_URL in .env.development to your gateway URL (default http://localhost:8080)

# Tests
dotnet test                       # backend
cd src/client && npm run test     # frontend
```

## Local startup checklist

| Requirement | Required for | How to set |
|-------------|--------------|------------|
| Docker Desktop running | Postgres, RabbitMQ, Redis (Aspire containers) | Start Docker before F5 |
| `Gemini:ApiKey` | Search, Chat, Ingestion (embeddings + RAG) | AppHost user-secrets (see step 1) |
| `Google:ClientId` / `Google:ClientSecret` | Identity OAuth login | AppHost user-secrets (optional for document-only testing) |

Services start in Development **without** `Gemini:ApiKey`, but AI features (upload indexing, semantic search, chat) return HTTP 503 until the key is configured. The client shows a banner via `GET /api/chat/ai/status`. Set the key with `dotnet user-secrets set "Gemini:ApiKey" "..."` in `src/AppHost`.

If Aspire appears stuck on `Waiting for resource 'postgres' to become healthy`:

1. Stop any parallel infra (`deploy/docker-compose.infra.yml`) so only Aspire owns Postgres/RabbitMQ/Redis.
2. Restart Docker Desktop (keep it updated; older 4.37–4.38 builds had health-check hangs on Windows).
3. Stop AppHost, remove stale Aspire containers if needed, then start again — containers use `Persistent` lifetime to reduce startup races.
4. Wait until **document-api**, **search-api**, and **gateway** show `Running` in the dashboard before using the client URL.

If **redis** shows `Running (Unhealthy)` in the Aspire dashboard, restart AppHost after a clean build. Local AppHost uses plain TCP on port `6379` (no TLS) via `WithoutHttpsCertificate()` so health checks match `docker-compose.infra.yml`. Remove the stale Redis container from Docker Desktop if the status persists after restart.

MassTransit uses **v8.x** (Apache 2.0, no commercial license required for local dev).

Identity Service applies EF Core migrations on startup in Development (`identitydb` schema includes ASP.NET Identity + OpenIddict tables).

## HTTP API (via Gateway)

All requests go through the YARP API Gateway which routes to services:

```http
# Identity
GET    /api/auth/login/google       # initiates Google OAuth flow
GET    /api/auth/callback/google    # Google callback, issues JWT

# Tenants
POST   /api/tenants                 # create tenant
GET    /api/tenants                 # list user's tenants
GET    /api/tenants/{id}
POST   /api/tenants/{id}/members    # add member (Admin only)
DELETE /api/tenants/{id}/members/{userId}

# Documents (requires X-Tenant-Id header)
POST   /api/documents               # multipart upload (file, optional name)
GET    /api/documents
GET    /api/documents/{id}
GET    /api/documents/{id}/content   # inline preview; ?download=true to download
DELETE /api/documents/{id}

# Search (requires X-Tenant-Id header)
POST   /api/search                  # { "query": "..." }
GET    /api/search/explorer          # list indexed chunks; optional ?documentIds= for filter
POST   /api/search/trace             # semantic search with pipeline trace

# Chat (requires X-Tenant-Id header)
POST   /api/chat                    # { "conversationId": null, "question": "..." }
POST   /api/chat/trace              # RAG chat with full pipeline trace
GET    /api/chat/conversations
GET    /api/chat/conversations/{id}
```

# Implementation Status (Done / Not Done)

Mapped against the full roadmap above.

## Done

* **.NET Aspire orchestration**: `AppHost` composes PostgreSQL (pgvector image,
  database-per-service: identitydb, tenantdb, documentdb, searchdb, chatdb),
  RabbitMQ (management plugin), Redis, all services with proper `WaitFor`
  dependencies and environment propagation, and the Vite React client via
  `AddViteApp` (`VITE_API_BASE_URL` is injected from the gateway HTTP endpoint).
* **ServiceDefaults**: OpenTelemetry (traces, metrics, logs), health checks,
  service discovery, and resilience shared by every service.
* **BuildingBlocks**: `SharedKernel` (blob storage, `IEventPublisher`),
  `Contracts` (integration events), `Tenancy` (`ITenantContext` + middleware
  reading `X-Tenant-Id` — single source of truth), `Messaging` (MassTransit /
  RabbitMQ), `Ai` (OpenAI embeddings + chat completion), `Auth` (JWT Bearer +
  Admin/Manager/Employee roles + policies), `Web` (Swagger with X-Tenant-Id +
  JWT Bearer + ProblemDetails + JSON enum converter).
* **API Gateway (YARP)**: routes `/api/auth`, `/api/tenants`, `/api/documents`,
  `/api/search`, `/api/chat` to the corresponding services via Aspire service
  discovery.
* **Identity Service**: Google OAuth login → JIT user provisioning → OpenIddict
  token issuance (JWT access + refresh), ASP.NET Core Identity + EF Core, role
  seeding (Admin, Manager, Employee).
* **Tenant Service**: `TenantEntity` + `TenantMembership` aggregates, CRUD API,
  add/remove members, EF Core migration, database-per-service (`tenantdb`).
* **Document Service (4 layers)**: upload file to shared blob storage, persist
  `StoredDocument` metadata, publish `DocumentUploaded` event via MassTransit,
  consume `DocumentProcessingCompleted` / `DocumentProcessingFailed` to update
  status, list/get/delete endpoints, EF Core migration (`documentdb`).
* **Ingestion Worker**: MassTransit consumer for `DocumentUploaded`, PDF/text
  extraction (PdfPig), word-aware overlapping chunker, OpenAI embedding
  generation, publishes `ChunksGenerated` + `DocumentProcessingCompleted` /
  `DocumentProcessingFailed`.
* **Search Service (4 layers)**: consumes `ChunksGenerated` to index
  `SearchableChunk` in pgvector (`vector(1536)`, HNSW index), consumes
  `DocumentDeleted` to remove chunks, cosine similarity search API,
  EF Core migration (`searchdb`).
* **Chat Service (4 layers)**: RAG pipeline — calls Search API via HTTP (service
  discovery + `X-Tenant-Id` propagation), builds context prompt, calls OpenAI
  chat, stores `Conversation` / `ChatMessage` history with source references,
  list/get conversations, EF Core migration (`chatdb`).
* **Phase 2 — Document Management**: upload, list, get, delete via dedicated
  Document service.
* **Phase 3 — Text Extraction**: PDF (PdfPig) and plain-text extractors in the
  Ingestion worker.
* **Phase 4 — Chunking**: section-aware, word-overlapping chunker (`HeadingParser` +
  `TextChunker`) with full unit coverage.
* **Phase 5 — Embeddings**: contextual embeddings at index time (document summary +
  section title + metadata wrapped for `RETRIEVAL_DOCUMENT`; raw chunk text stored
  for RAG). Gemini vectors in `vector(1536)` columns. Re-upload documents after
  changing `ContextualEmbedding` settings.
* **Phase 6 — Semantic Search**: hybrid retrieval pipeline — vector + keyword RRF
  fusion, neighbor chunk expansion (`NeighborExpansionRadius`), LLM reranking
  (`RerankingEnabled`). Config: `RetrievalTopK`, `FinalTopK`, `HybridSearchEnabled`.
  Explorer trace shows `search.vector`, `search.keyword`, `search.hybrid_merge`,
  `search.expand_neighbors`, `search.rerank`.
* **RAG test documents**: `assets/HR_Policy_MeatKombinat_EN.md` (XREF-01..06) and
  `assets/Logistics_Policy_MeatKombinat_EN.md` (XREF-LOG-01..08) with cross-chunk
  references for contextual-embedding validation. See `assets/QuestionExamples.md`.
* **Phase 7 — RAG Chat**: retrieve → prompt → LLM via dedicated Chat service.
* **Phase 8 — Source References**: answers return distinct source documents.
* **Phase 9 — Chat History**: `Conversation` / `ChatMessage` entities with full
  CRUD, stored in `chatdb`.
* **Phase 11 — Background Processing**: RabbitMQ + MassTransit with
  `DocumentUploaded` → `ChunksGenerated` → `DocumentProcessingCompleted` flow.
* **Phase 12 — Authentication**: Google OAuth + OpenIddict in Identity service.
* **Phase 13 — Multi-Tenancy (partial)**: `TenantEntity` / `TenantMembership`
  service, `X-Tenant-Id` header middleware (single source of truth), scoped
  `ITenantContext`.
* **Swagger** on every API service with `X-Tenant-Id` header and JWT Bearer.
* **Frontend**: Documents, Search, Chat, and **Explorer** (`/explorer`) pages
  (React + TS, Feature-Sliced Design, TanStack Query). Each page shows what the
  tab does for the user and an expandable pipeline guide (`PageGuide` component).
  * **Documents** — `POST /api/documents` → `DocumentUploaded` → Ingestion Worker
    (extract, chunk, Gemini embeddings) → Search indexes chunks in pgvector.
  * **Search** — `POST /api/search` — embed query, hybrid pgvector + keyword,
    RRF, neighbor expansion, LLM rerank; returns scored excerpts.
  * **Chat** — `POST /api/chat` — calls Search for context, builds RAG prompt,
    Gemini completion, source links, persists `Conversation` / `ChatMessage`.
  * **Explorer** — `POST /api/chat/trace` first, then `GET /api/search/explorer`
    for source documents only; split layout with full pipeline trace (tenant →
    conversation → search → prompt → LLM → persist), nested search sub-steps,
    and per-document chunk tabs when multiple sources are used; each document
    panel has **All chunks** / **Used for answer** sub-tabs (latter sorted by
    RAG priority).
* **Tests**: 44 backend unit tests (chunker, contextual embeddings, hybrid search,
  keyword search) and 14 frontend tests (Button, DocumentList, documentPolling,
  PageGuide, PipelineTraceTimeline, DocumentChunksExplorer).
* **EF migrations** for all 5 databases, each with `.Designer.cs`.
* `Directory.Build.props` with `TreatWarningsAsErrors` (NU19xx audit warnings
  excluded for transitive Aspire dependencies).
* Global exception handling via `IExceptionHandler` + ProblemDetails.
* **Production Docker deploy**: `deploy/docker-compose.yml` (full stack +
  reverse proxy on port 80), shared `deploy/Dockerfile.dotnet`, React
  `deploy/Dockerfile.client`, GitHub Actions CI + deploy to self-hosted runner
  (`knowledgebase` label) with secrets in the `prod` environment.

## Production deployment

Prerequisites on the server (`46.62.249.94`):

1. Self-hosted runner registered for this repo with label `knowledgebase` (user
   `runner`, member of the `docker` group).
2. Cloudflare DNS: `kb.bookly.lv` → server IP (proxied).
3. Shared **Traefik** (`bookly-proxy` network, resolver `letsencrypt`) routes
   `kb.bookly.lv` via Docker labels — see `deploy/docker-compose.traefik.yml`
   (included automatically in the deploy workflow).

GitHub **Environment `prod`**:

| Type | Name |
|------|------|
| Secrets | `POSTGRES_USER`, `POSTGRES_PASSWORD`, `RABBITMQ_USER`, `RABBITMQ_PASSWORD`, `GEMINI_API_KEY` |
| Variables | `CORS_ALLOWED_ORIGINS` (`https://kb.bookly.lv`), `VITE_API_BASE_URL` (empty = same-origin `/api`) |

Push to `main` runs tests on GitHub-hosted runners, then deploy on the
self-hosted runner via `deploy/scripts/github-deploy.sh`:

1. Creates `deploy/.env` from GitHub Environment `prod` secrets (never commit `.env`).
2. Removes orphan containers left by interrupted runs.
3. `docker compose down` → `up -d --build` with Traefik overlay.
4. Smoke test: `GET /api/documents` via gateway must return **200** or the job fails.

Chat service calls Search over HTTP inside Docker (`Chat__SearchServiceBaseUrl=http://search-api:8080`).
Aspire local dev keeps `https+http://search-api` service discovery.

Deploy jobs are **queued** (`cancel-in-progress: false`) so a new push does not
tear down a deploy that is already running.

**Do not run `docker compose` manually on the server** without the same `.env`
secrets — empty variables break Postgres health checks and the whole stack.

After a green Deploy workflow, verify `https://kb.bookly.lv` in the browser.

## Not Done (remaining work)

* **Phase 10 — Semantic Kernel**: direct OpenAI calls behind interfaces instead
  of full Semantic Kernel orchestration.
* **Phase 13 (remaining)** — EF Core global query filters for tenant isolation
  are not yet wired on Document/Search/Chat DbContexts.
* **Phase 14 — Hybrid Search** (Elasticsearch) not yet integrated alongside
  pgvector.
* **Phase 15 — Observability** — OpenTelemetry is wired via ServiceDefaults but
  Prometheus/Grafana/Loki/Tempo dashboards are not configured.
* **Helm charts and Kubernetes manifests** not created.
* **Integration tests** (Testcontainers) not implemented; only unit tests.
* **Rate limiting** on the Gateway is not configured.
* **Frontend auth flow** — the React client does not yet integrate with the
  Google OAuth / JWT flow.
* **DOCX / OCR / streaming / voice** and other Future Enhancements not started.
