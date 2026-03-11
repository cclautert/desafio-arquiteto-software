# Non-Functional Requirements Analysis

## Requirement: Service Independence

> *"O serviço de controle de lançamento não deve ficar indisponível se o sistema de consolidado diário cair."*

### Analysis
The transaction recording service must continue operating regardless of the consolidated daily service status.

### Solution: Asynchronous Kafka Communication

```
Without Kafka (tightly coupled — BAD):
┌──────────────┐   HTTP sync call   ┌──────────────────┐
│  Lancamentos │ ──────────────────► │   Consolidado    │
│   Service    │ ◄────────────────── │   Service        │
└──────────────┘  waits for response └──────────────────┘
  ↑ If Consolidado is down, Lancamentos also fails!

With Kafka (loosely coupled — GOOD):
┌──────────────┐   Kafka event   ┌──────────┐   Consumer   ┌──────────────────┐
│  Lancamentos │ ───────────────► │  Kafka   │ ────────────► │   Consolidado    │
│   Service    │                  │  Broker  │  (when alive) │   Service        │
└──────────────┘                  └──────────┘               └──────────────────┘
  ↑ Lancamentos records transaction and returns success
    regardless of Consolidado state.

    When Consolidado comes back online:
    - Reads pending messages from Kafka offset
    - Catches up on missed transactions
    - No data loss
```

### Behavior Matrix

| Lancamentos | Consolidado | Kafka | System Behavior |
|---|---|---|---|
| ✅ Up | ✅ Up | ✅ Up | Full functionality |
| ✅ Up | ❌ Down | ✅ Up | Lancamentos works. Events queued in Kafka. Consolidado catches up when it restarts. |
| ✅ Up | ✅ Up | ❌ Down | Lancamentos works (with error logged). Consolidado might miss events. |
| ❌ Down | ✅ Up | ✅ Up | Frontend shows error for creating lancamentos. Consolidado still serves cached/existing data. |

---

## Requirement: Peak Load — 50 req/s with Max 5% Loss

> *"Em dias de picos, o serviço de consolidado diário recebe 50 requisições por segundo, com no máximo 5% de perda de requisições."*

### Analysis
At 50 req/s, the Consolidado service would hit the database 50 times per second without caching — clearly unsustainable for any real database.

### Solution: Redis Cache with TTL

```
50 req/s to GET /api/v1/consolidado/2024-01-15
                │
                ▼
         Check Redis Cache
         key: "consolidado:2024-01-15"
                │
    ┌───────────┴───────────┐
    │                       │
  HIT (~95%)              MISS (~5%)
  < 1ms latency          ~10ms latency
    │                       │
    └──────────┬────────────┘
               │
               ▼
         Response: 200 OK

Cache Invalidation:
  When Kafka consumer processes a new "lancamento-criado" event
  for date X → invalidates cache for "consolidado:X"
  Next request rebuilds cache from DB
```

### Capacity Calculation

With TTL = 5 minutes:
- Requests at peak: 50 req/s × 300 seconds = 15,000 requests per 5-minute window
- DB queries needed: 1 (initial cache miss) + ~1/5min for invalidation = ~2 total
- Cache hit rate: (15,000 - 2) / 15,000 = **99.98%**
- Data staleness: max 5 minutes (acceptable for daily consolidation)

### Polly Configuration for Resilience

```csharp
// Retry policy (handles transient Redis failures)
var retryPolicy = Policy
    .Handle<RedisException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt))
    );

// Circuit breaker (prevents cascade failures)
var circuitBreakerPolicy = Policy
    .Handle<Exception>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(60)
    );
```

---

## Scalability Analysis

### Horizontal Scaling

Both services are **stateless** — no session state in memory. JWT tokens are validated cryptographically (no server-side session store needed).

```
                    ┌──────────────────────┐
                    │    Load Balancer      │
                    │    (NGINX/AWS ALB)    │
                    └──────────┬───────────┘
                               │
              ┌────────────────┼────────────────┐
              │                │                │
    ┌─────────▼──────┐ ┌───────▼───────┐ ┌──────▼──────────┐
    │ Consolidado    │ │ Consolidado   │ │ Consolidado     │
    │ Instance 1     │ │ Instance 2    │ │ Instance 3      │
    └────────────────┘ └───────────────┘ └─────────────────┘
              │                │                │
              └────────────────┼────────────────┘
                               │
                    ┌──────────▼───────────┐
                    │      Redis           │
                    │    (Shared Cache)    │
                    └──────────────────────┘
```

All instances share the same Redis cache — a cache miss on one instance populates the cache for all.

---

## Reliability Metrics

### Target SLAs

| Metric | Target | Mechanism |
|---|---|---|
| **Availability** | 99.9% | Stateless services + health checks + restart policies |
| **Durability** | 99.99% | Kafka at-least-once delivery |
| **Latency (P99)** | < 200ms | Redis cache + async processing |
| **Throughput** | 50 req/s | Horizontal scaling + caching |
| **Data Loss** | < 5% | Kafka consumer group offset management |

### Kafka Consumer Group Configuration

```
Group ID: consolidado-group
Auto Offset Reset: earliest (don't lose messages)
Enable Auto Commit: false (manual commit after processing)
Max Poll Records: 10 (batch processing)
Session Timeout: 30s
```

Manual commit ensures no message is lost — if processing fails, the message is reprocessed after timeout.
