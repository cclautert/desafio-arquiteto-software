# Future Improvements & Evolution Roadmap

This document describes architectural evolutions and improvements that would be implemented given more time or in a production context.

---

## 1. Production-Grade Database

### Current State
Using Entity Framework InMemory provider (as required by the challenge). Data is lost on service restart.

### Target State
```
PostgreSQL (Primary) ──── Read Replica ──── Analytics DB
      │                        │                  │
  [Write path]          [Read path]         [Reporting]
  Commands              Queries             BI/Dashboards
```

**Implementation Steps:**
1. Replace `UseInMemoryDatabase` with `UseNpgsql` or `UseSqlServer`
2. Add proper EF Core migrations
3. Implement connection pooling (PgBouncer)
4. Add read replicas for the Consolidado query service

---

## 2. Event Sourcing

### Concept
Instead of storing only the current state, store all events that led to the current state.

```
Traditional:
  Lancamento { Valor: 100, Tipo: Credito }

Event Sourcing:
  LancamentoCriadoEvent { Valor: 100, Tipo: Credito, Timestamp: T1 }
  LancamentoCanceladoEvent { LancamentoId: X, Timestamp: T2 }
  LancamentoCriadoEvent { Valor: 150, Tipo: Debito, Timestamp: T3 }
```

**Benefits:**
- Complete audit trail — know exactly what happened and when
- Replay events to rebuild any point-in-time state
- Natural fit with CQRS read models

**Tools:** Marten (PostgreSQL event store), EventStoreDB

---

## 3. Saga Pattern for Distributed Transactions

### Problem
When creating a Lancamento, we need to:
1. Save to Lancamentos DB
2. Update ConsolidadoDiario in Consolidado service

What if step 2 fails? We'd have an inconsistent state.

### Solution: Choreography-based Saga

```
[API Lancamentos]
  1. Publish LancamentoCriadoEvent
  2. If Consolidado ACKs → success
  3. If no ACK after timeout → publish LancamentoCompensationEvent

[API Consolidado]
  1. Consume LancamentoCriadoEvent
  2. Update ConsolidadoDiario
  3. Publish LancamentoProcessadoEvent (ACK)
```

---

## 4. CQRS Read Store Separation

### Current State
Both reads and writes use the same InMemory DB.

### Target State
```
Write Side                    Read Side
──────────────────            ─────────────────────────
InMemory/PostgreSQL           Elasticsearch (full-text)
                              Redis (hot cache)
                              ClickHouse (analytics)
```

**Benefits:**
- Scale reads independently from writes
- Optimize each store for its specific access pattern
- GraphQL on top of the read store for flexible queries

---

## 5. Kubernetes Deployment

```yaml
# Horizontal Pod Autoscaler for Consolidado (handles 50 req/s)
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
spec:
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        averageUtilization: 70
```

**Benefits:**
- Auto-scale based on actual load
- Self-healing (restart failed pods)
- Zero-downtime deployments (rolling updates)
- Resource efficiency (scale down at night)

---

## 6. Distributed Tracing (OpenTelemetry)

```
Request enters API Lancamentos
  │
  ├── TraceID: abc-123, SpanID: span-1
  │   [POST /api/v1/lancamentos]
  │
  ├── SpanID: span-2 [EF Core: INSERT Lancamento]
  │
  └── SpanID: span-3 [Kafka: Publish lancamento-criado]
              │
              └── SpanID: span-4 [API Consolidado: Kafka consumer]
                          │
                          ├── SpanID: span-5 [Redis: cache invalidate]
                          └── SpanID: span-6 [EF Core: UPDATE ConsolidadoDiario]
```

**Tools:** OpenTelemetry SDK + Jaeger or Zipkin for visualization

---

## 7. API Gateway

```
Internet
   │
[API Gateway — Kong/NGINX]
   │
   ├── Rate Limiting (100 req/min per IP)
   ├── JWT Validation (centralized)
   ├── Request Logging
   ├── SSL Termination
   │
   ├── /api/lancamentos/* ──► API Lancamentos
   └── /api/consolidado/* ──► API Consolidado
```

---

## 8. Real-Time Dashboard (WebSockets)

```
[API Lancamentos]
   │ Creates Lancamento
   └── Broadcasts via SignalR Hub
              │
              ▼
   [Frontend — WebSocket]
      Dashboard updates in real-time:
      - New transaction appears in list
      - Balance counter updates
      - Chart re-renders
```

---

## 9. Multi-Tenancy

For supporting multiple merchants:

```
Request: POST /api/v1/lancamentos
Header: X-Tenant-Id: merchant-abc

TenantMiddleware extracts tenant ID
  │
  ▼
Repository uses tenant-filtered queries:
  context.Lancamentos.Where(l => l.TenantId == tenantId)
```

**Isolation Strategies:**
- Database per tenant (max isolation)
- Schema per tenant (medium isolation)
- Row-level isolation (min overhead)

---

## 10. Feature Flags

Allow gradual rollout of features:

```csharp
if (await featureFlags.IsEnabledAsync("new-consolidado-algorithm"))
{
    return await newAlgorithm.Calculate(data);
}
return await legacyAlgorithm.Calculate(data);
```

**Tools:** Microsoft.FeatureManagement, LaunchDarkly

---

## 11. Refresh Token Flow

```
Initial Login → JWT (60 min) + Refresh Token (7 days)
              │
              ▼ After 60 min
Client sends expired JWT + Refresh Token
              │
              ▼
Server validates Refresh Token → Issues new JWT
              │
              ▼ If Refresh Token expired
Client must re-authenticate
```

---

## 12. Performance Targets (Production SLAs)

| Metric | Target | Current (Dev) |
|---|---|---|
| API P99 Latency | < 200ms | < 50ms (InMemory) |
| Consolidado Cache Hit Rate | > 95% | N/A (demo) |
| API Availability | 99.9% (43.8 min/month downtime) | N/A |
| Max Throughput (Consolidado) | 50 req/s | N/A |
| Data Loss (max) | < 5% | 0% (Kafka offset) |

---

## 13. Security Hardening

1. **Secrets Management**: Use HashiCorp Vault or Azure Key Vault instead of appsettings.json
2. **mTLS**: Mutual TLS between microservices
3. **RBAC**: Role-based access control (Admin, Merchant, ReadOnly)
4. **Audit Log**: Every API call logged with user identity
5. **Input Sanitization**: FluentValidation on all DTOs
6. **CORS Policy**: Restrict to known frontend origins
7. **Content Security Policy**: Headers to prevent XSS
8. **Rate Limiting**: Per-user and per-IP limits
