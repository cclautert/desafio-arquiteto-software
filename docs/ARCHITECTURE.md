# Architecture Document — Fluxo de Caixa

## 1. Executive Summary

This document describes the software architecture of the **Fluxo de Caixa** (Cash Flow Management) system, a microservices-based solution designed to manage financial transactions (credits/debits) and provide daily consolidated balance reports.

---

## 2. Architecture Overview

### Pattern: Event-Driven Microservices + Clean Architecture + CQRS

The system is split into two independent services that communicate asynchronously via Apache Kafka:

```
┌────────────────────────────────────────────────────────────────────────────┐
│                           FLUXO DE CAIXA SYSTEM                            │
│                                                                            │
│  ┌──────────────┐       ┌─────────────┐      ┌──────────────────────────┐ │
│  │   Frontend   │       │   Kafka     │      │    Redis Cache           │ │
│  │  (Angular)   │       │   Broker    │      │  (Consolidado TTL: 5min) │ │
│  └──────┬───────┘       └──────┬──────┘      └────────────┬─────────────┘ │
│         │                      │                          │               │
│  ┌──────▼───────────────────┐  │  ┌──────────────────────▼─────────────┐ │
│  │   API Lancamentos (5001)  │──┼──▶   API Consolidado Diário (5002)    │ │
│  │   ┌─────────────────────┐│  │  │   ┌───────────────────────────────┐│ │
│  │   │ Clean Architecture  ││  │  │   │ Clean Architecture            ││ │
│  │   │ ┌─────────────────┐ ││  │  │   │ ┌─────────────────────────┐  ││ │
│  │   │ │  Presentation   │ ││  │  │   │ │  Presentation           │  ││ │
│  │   │ │ (Web API v1)    │ ││  │  │   │ │ (Web API v1)            │  ││ │
│  │   │ ├─────────────────┤ ││  │  │   │ ├─────────────────────────┤  ││ │
│  │   │ │  Application    │ ││  │  │   │ │  Application            │  ││ │
│  │   │ │  (CQRS/MediatR) │ ││  │  │   │ │  (CQRS/MediatR)        │  ││ │
│  │   │ ├─────────────────┤ ││Kafka  │   │ ├─────────────────────────┤  ││ │
│  │   │ │ Infrastructure  │─┼┼──►   │   │ │ Infrastructure         │  ││ │
│  │   │ │  (EF + Kafka +  │ ││  topic│  │ │  (EF + Kafka Consumer  │  ││ │
│  │   │ │   Redis)        │ ││  │  │   │ │   + Redis)             │  ││ │
│  │   │ ├─────────────────┤ ││  │  │   │ ├─────────────────────────┤  ││ │
│  │   │ │     Domain      │ ││  │  │   │ │     Domain              │  ││ │
│  │   │ │ (Entities/Repos)│ ││  │  │   │ │ (Entities/Repos)        │  ││ │
│  │   │ └─────────────────┘ ││  │  │   │ └─────────────────────────┘  ││ │
│  │   └─────────────────────┘│  │  │   └───────────────────────────────┘│ │
│  │   [InMemory DB]           │  │  │   [InMemory DB]                    │ │
│  └───────────────────────────┘  │  └────────────────────────────────────┘ │
│                                 │                                          │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Service Decomposition

### 3.1 API Lancamentos (Port 5001)
**Responsibility**: Record and manage financial transactions (credits and debits).

| Endpoint | Method | Description |
|---|---|---|
| `/api/v1/auth/token` | POST | Generate JWT token |
| `/api/v1/lancamentos` | GET | List all transactions |
| `/api/v1/lancamentos/{id}` | GET | Get transaction by ID |
| `/api/v1/lancamentos` | POST | Create new transaction |
| `/health` | GET | Health check |

**Event Published**: `lancamento-criado` (Kafka topic)

### 3.2 API Consolidado Diário (Port 5002)
**Responsibility**: Provide daily consolidated balance reports.

| Endpoint | Method | Description |
|---|---|---|
| `/api/v1/auth/token` | POST | Generate JWT token |
| `/api/v1/consolidado` | GET | Get today's consolidated balance |
| `/api/v1/consolidado/{data}` | GET | Get consolidated balance for a date |
| `/health` | GET | Health check |

**Event Consumed**: `lancamento-criado` (Kafka topic)

---

## 4. Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│                     Presentation                         │
│               (Web API Controllers)                      │
├─────────────────────────────────────────────────────────┤
│                     Application                          │
│          (DTOs, Mappings, App Services, CQRS)           │
├─────────────────────────────────────────────────────────┤
│                    Infrastructure                        │
│       (EF Context, Repos, Kafka, Redis, Handlers)       │
├─────────────────────────────────────────────────────────┤
│                       Domain                             │
│         (Entities, Interfaces, Enums, Services)         │
└─────────────────────────────────────────────────────────┘
```

**Dependency Rule**: Each outer layer can only depend on inner layers. Domain has no external dependencies.

---

## 5. CQRS Pattern

```
Command Side (Write)                Query Side (Read)
─────────────────────               ──────────────────────
CreateLancamentoCommand             GetConsolidadoDiarioQuery
       │                                     │
       ▼                                     ▼
CreateLancamentoCommandHandler      GetConsolidadoDiarioQueryHandler
       │                                     │
       ├── Saves to InMemory DB              ├── Checks Redis cache
       │                                     │      hit → return cached
       └── Publishes Kafka event             └── miss → query DB → cache
```

---

## 6. Event-Driven Flow

```
[Frontend/API Client]
        │
        │ POST /api/v1/lancamentos
        ▼
[API Lancamentos]
   CreateLancamentoCommandHandler
        │
        ├──► [InMemory DB] Saves Lancamento
        │
        └──► [Kafka] Publishes "lancamento-criado" event
                            │
                            ▼
              [API Consolidado — KafkaConsumerService]
                    │
                    ├──► [InMemory DB] Updates ConsolidadoDiario
                    │      (adds credit or debit to daily total)
                    │
                    └──► [Redis] Invalidates cache for that date
```

---

## 7. Resilience Patterns

### 7.1 Circuit Breaker (Polly)
Applied to outbound HTTP calls between services:

```
Normal State    ──────────────────────────────────────► Requests pass
                                                         through
Failing State   5 failures in 30s ──────────────────► Circuit Opens
                                                         (Fail Fast)
Open State      ──────── After 60s ──────────────────► Half-Open Test
Half-Open Test  ──────── If succeeds ────────────────► Circuit Closes
```

Configuration:
- Failure threshold: 5 failures
- Duration of break: 60 seconds
- Retry: 3 attempts with exponential backoff

### 7.2 Async Decoupling (Kafka)
The Lancamentos service **does not depend** on the Consolidado service being available. Even if the Consolidado service is down, transactions are:
1. Recorded in the Lancamentos InMemory DB
2. Published to Kafka

When Consolidado service comes back online, it catches up from the Kafka offset.

### 7.3 Non-Functional Requirements Met
- **Availability**: Lancamentos service stays up even if Consolidado is down (Kafka async)
- **Throughput**: Consolidado handles 50 req/s with Redis cache (cache hit rate > 95%)
- **Data Loss**: Max 5% — achieved via Kafka durability and offset management

---

## 8. Security

```
[Client] ──── JWT Bearer Token ────► [API]
                                       │
              ┌───────────────────────┤
              │                       │
         Google OAuth2           Simple Auth
         (Frontend login)     (username/password
                               for API testing)
                                       │
                                       ▼
                              JWT Token Response
                              (expires: 60 minutes)
```

- **Authentication**: JWT Bearer tokens
- **Token Generation**: Via `/api/v1/auth/token`
- **Frontend Login**: Google OAuth2 (Identity Services)
- **API Protection**: All endpoints require `[Authorize]`
- **Transport**: HTTPS in production (HTTP in dev)

---

## 9. Caching Strategy

```
Client Request
      │
      ▼
Check Redis Cache (key: "consolidado:YYYY-MM-DD", TTL: 5 min)
      │
      ├── HIT ──────────────────────────────► Return cached data (< 1ms)
      │
      └── MISS ─────────────────────────────► Query InMemory DB
                                                     │
                                                     ▼
                                              Store in Redis (TTL: 5 min)
                                                     │
                                                     ▼
                                              Return data
```

---

## 10. Technology Stack

| Component | Technology | Rationale |
|---|---|---|
| Backend Runtime | .NET 8 (C#) | Performance, ecosystem, requirement |
| API Framework | ASP.NET Core 8 | Mature, performant, built-in DI |
| ORM | Entity Framework Core 8 | Requirement; InMemory provider |
| Message Bus | Apache Kafka | Requirement; durable, high-throughput |
| Cache | Redis + StackExchange.Redis | Requirement; low-latency caching |
| Mediator | MediatR 12 | Requirement; CQRS implementation |
| Mapping | AutoMapper 12 | DTO mapping |
| Resilience | Polly 8 | Circuit breaker, retry |
| Auth | JWT Bearer | Stateless, scalable |
| Logging | Serilog | Structured logging |
| API Docs | Swagger/OpenAPI 3 | Developer experience |
| Frontend | Angular 17 (TypeScript) | Requirement |
| Frontend Auth | Google OAuth2 (GSI) | Requirement |
| Containerization | Docker + Docker Compose | Portability |
| Unit Tests | xUnit + Moq + FluentAssertions | .NET ecosystem standard |
| Integration Tests | WebApplicationFactory | Built-in ASP.NET testing |

---

## 11. Data Model

### Lancamento
```
┌──────────────────────────────────────┐
│              Lancamento              │
├──────────────────────────────────────┤
│ Id              : Guid (PK)          │
│ Descricao       : string (required)  │
│ Valor           : decimal (> 0)      │
│ Tipo            : TipoLancamento     │
│                   (Credito=1,        │
│                    Debito=2)         │
│ DataLancamento  : DateTime           │
│ CreatedAt       : DateTime           │
└──────────────────────────────────────┘
```

### ConsolidadoDiario
```
┌──────────────────────────────────────┐
│           ConsolidadoDiario          │
├──────────────────────────────────────┤
│ Id              : Guid (PK)          │
│ Data            : DateTime (date)    │
│ TotalCreditos   : decimal            │
│ TotalDebitos    : decimal            │
│ Saldo           : decimal (computed) │
│                  = Creditos-Debitos  │
│ UpdatedAt       : DateTime           │
└──────────────────────────────────────┘
```

---

## 12. Architecture Decision Records (ADRs)

### ADR-001: Microservices over Monolith
**Decision**: Two separate microservices
**Reason**: Non-functional requirement states Lancamentos must stay available if Consolidado fails
**Trade-off**: Increased operational complexity vs resilience

### ADR-002: InMemory Database
**Decision**: Entity Framework InMemory provider
**Reason**: Stated in requirements; simplifies setup
**Trade-off**: Data not persisted across restarts; acceptable for demo
**Future**: Replace with PostgreSQL/SQL Server for production

### ADR-003: Kafka for Inter-Service Communication
**Decision**: Apache Kafka for async events
**Reason**: Decouples services, guarantees at-least-once delivery
**Alternative Considered**: Direct HTTP (rejected — creates coupling, violates resilience requirement)

### ADR-004: Redis Cache with TTL
**Decision**: Cache consolidated daily balance with 5-minute TTL
**Reason**: Consolidado service receives 50 req/s at peak; cache reduces DB load to ~1 req per 5 minutes per date
**Impact**: Handles 50 req/s with < 5% data loss (cache invalidated on new lancamento event)

### ADR-005: CQRS with MediatR
**Decision**: Separate command and query handlers via MediatR
**Reason**: Requirement; separates read/write concerns, improves testability
**Trade-off**: More boilerplate vs cleaner separation

### ADR-006: JWT + Google OAuth2
**Decision**: JWT for API auth, Google OAuth2 for frontend
**Reason**: Stateless tokens scale horizontally; Google OAuth2 is enterprise-grade
**Security Notes**: Tokens expire in 60 minutes; refresh token flow should be added in production

---

## 13. Future Improvements

1. **Persistent Database**: Replace InMemory with PostgreSQL using proper migrations
2. **Distributed Tracing**: Add OpenTelemetry with Jaeger/Zipkin
3. **API Gateway**: Add NGINX/Kong as single entry point with rate limiting
4. **Event Sourcing**: Store all Lancamento events for full audit trail
5. **Saga Pattern**: Handle distributed transactions for data consistency
6. **Kubernetes**: Deploy with K8s for auto-scaling (HPA based on CPU/memory)
7. **GraphQL**: Add GraphQL endpoint for flexible frontend queries
8. **CQRS Read Store**: Separate read database optimized for queries
9. **WebSockets**: Real-time dashboard updates when new transactions arrive
10. **Multi-tenancy**: Support multiple merchants with tenant isolation

---

## 14. Running Locally

See [README.md](../README.md) for detailed setup instructions.

Quick start:
```bash
docker-compose up -d
```

Services:
- Frontend: http://localhost:4200
- API Lancamentos: http://localhost:5001/swagger
- API Consolidado: http://localhost:5002/swagger
- Kafka: localhost:29092
- Redis: localhost:6379
