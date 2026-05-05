# TicketFlow Backend Best Practices - Deep Dive

This document provides a detailed technical specification for building microservices in the TicketFlow ecosystem, following the patterns established in `EventService` and `BuildingBlocks`.

## 0. Meta-Rules for AI Development

These rules govern how I (Antigravity) interact with this project to ensure stability and consistency.

### Documentation Integrity:

- **Additive Updates**: When adding new best practices to this file, **never** remove or shorten existing detailed sections unless they are explicitly deprecated. The more detailed, the better. The file can grow to 1000+ lines to capture every essential detail.
- **Strict Scope**: Do not modify unrelated sections of the markdown or KI when adding new rules.

### Code Modification Standards:

- **Open-Closed Principle (OCP)**: Always consider if a change can be implemented by extending existing code rather than modifying it. Avoid breaking changes to shared logic in `BuildingBlocks`.
- **Flow Impact Analysis**: Before editing any existing service logic, perform a thorough scan of dependencies to ensure the current flow remains intact.
- **Minimal Invasive Surgery**: If a bug fix is needed, aim for the most precise fix possible without refactoring unrelated parts of the file.

## 1. Core Coding & Design Principles

### SOLID Principles:

- **S - Single Responsibility**: Each class should do one thing.
- **O - Open-Closed**: Open for extension, closed for modification.
- **L - Liskov Substitution**: Derived classes must be usable through base classes.
- **I - Interface Segregation**: Split large interfaces.
- **D - Dependency Inversion**: Depend on abstractions.

### Object-Oriented Programming (OOP) & SoC:

- **Encapsulation**: Hide internal state.
- **Separation of Concerns (SoC)**: Keep layers separate.
- **DRY (Don't Repeat Yourself)**: Shared logic in `BuildingBlocks`.
- **KISS & YAGNI**: Do not over-engineer.

## 2. Microservices Architecture & Folder Structure

### Layered Responsibility:

- **Domain**: Entities, Enums, Value Objects.
- **Application**: Handlers, DTOs, Mapper interfaces.
- **Persistence**: DbContext, Configurations.
- **Infrastructure**: External service implementations.
- **Api**: Controllers, Middleware.

### Folder Organization:

Use **Features** folder (Vertical Slices).

## 3. Domain Entities (DDD Core)

- **Inheritance**: Must inherit from `BaseEntity`.
- **Encapsulation**: `private set` for properties.
- **Logic**: Use domain methods for state changes.

## 4. API Controllers & Routing

- **URL Style**: kebab-case.
- **Resource Naming**: Plural nouns.
- **Documentation**: Mandatory XML Comments and every possible case for `[ProducesResponseType]`.

## 5. Application Layer: MediatR & LINQ

### Command & Query Handlers:

- **Coordination**: Handler only coordinates; Entity handles logic.
- **Performance**: Use `.AsNoTracking()`, Projection, and Pagination.
- **Mapperly**: Use for source-generated mapping. Place in `Mappers` subfolder.

### Technical Optimizations:

- **LINQ**: Projection (`.Select`) does not need `.AsNoTracking()`.
- **CancellationToken**: Mandatory propagation through all layers.

## 6. Middleware & Filters

- **Middleware**: Global concerns (Auth, Exceptions). Lightweight and order-dependent.
- **Filters**: Action-specific concerns. Use **MediatR Validation Behaviors** for validation.

## 7. Exception Handling & Resilience

- **No-Swallow Rule**: Never catch and ignore.
- **Strategic Try-Catch**: Use for boundary operations and specific exceptions.
- **Data Consistency**: Use Transactions and the Outbox pattern.

## 8. Avoiding Hardcoding

- **Constants/Enums**: For statuses, roles, and magic numbers.
- **IOptions**: For strongly-typed configuration.

## 9. Persistence (EF Core)

- **Soft Delete Index**: Unique indexes must include `.HasFilter("\"IsDeleted\" = false")`.
- **Decimal Precision**: Use `decimal(12,2)`.

## 10. Messaging

- **Outbox Pattern**: Use MassTransit Outbox to ensure atomicity.

## 11. Logging & Observability

- **Structured Logging**: No interpolation in templates.
- **Correlation**: Attach TraceId to every log.

## 12. Testing Strategy

- **Naming**: `Feature_Action_Should_ExpectedResult_When_Condition`.
- **Integration Tests**: Use Testcontainers for real DB/Redis.

## 13. Security Standards

- **Claims-based**: Check specific permissions over roles.
- **Authorize Attribute**: Apply at Controller level by default.

## 14. Distributed Caching

- **Pattern**: Cache-aside.
- **Consistency**: Event-driven invalidation.

## 15. Concurrency Control (Optimistic & Distributed)

- **Optimistic**: Use RowVersion/Timestamp.
- **Distributed Lock**: Use Redis (RedLock) for cross-instance locking.

## 16. Message Queue Reliability (MassTransit)

- **Retry**: Exponential backoff.
- **DLQ**: Monitor `_error` queues.
- **Idempotency**: Consumers must handle duplicate messages.

## 17. Background Tasks & Periodic Jobs

- **Graceful Shutdown**: Respect CancellationToken.
- **Scope Management**: Use `IServiceScopeFactory`.

## 18. API Versioning & Breaking Changes

- **Versioning**: Path-based (`/v1`, `/v2`).
- **Compatibility**: Never break existing versions.

## 19. Git & Commit Convention (GitHub)

- **Conventional Commits**: `type(scope): description`.
- **Branching**: `feature/`, `bugfix/`, `hotfix/`.

## 20. API Rate Limiting & Protection

- **Standard**: Use `Microsoft.AspNetCore.RateLimiting`.
- **Policies**: Fixed Window, Sliding Window, Concurrency.

## 21. Security Headers & API Hardening

- **Headers**: HSTS, CSP, X-Frame-Options.
- **CORS**: Strict whitelist, no `*`.

## 22. Advanced Health Checks (Kubernetes/Docker Probes)

- **Probes**: Liveness, Readiness, Startup.
- **Checks**: Db, Redis, MQ, External APIs.

## 23. Advanced Distributed Tracing (OpenTelemetry)

- **Manual Spans**: Use `ActivitySource`.
- **Tagging**: Business-relevant tags.

## 24. EF Core Performance & Advanced Features

- **Bulk Operations**: ExecuteUpdate/Delete.
- **Query Splitting**: `.AsSplitQuery()`.

## 25. Microservices Consistency - Saga Choreography Pattern

- **Compensating Transactions**: Undo on failure.
- **Outbox Dependency**: Mandatory.

## 26. Static Analysis & Code Quality (Linting)

- **EditorConfig**: Mandatory naming/formatting.
- **Git Hooks**: Pre-commit validation.

## 27. Advanced DDD - Value Objects & Aggregates

- **Value Objects**: Immutable, no identity.
- **Aggregate Root**: Boundary for consistency and changes.

## 28. API Response - Standard Filtering, Sorting & Paging

- **Standardized Params**: page, sort, filter.

## 29. External Service Resilience (Polly Deep-dive)

- **Retry with Jitter**: Prevent thundering herds.
- **Circuit Breaker**: Stop requests to failing services.

## 30. Inbox Pattern (Message Idempotency)

- **Deduplication**: Consumer-side tracking.

## 31. Database Migration Standards

- **No Manual Edits**: Idempotent CLI-managed migrations.
- **Testing**: Up/Down local validation.

## 32. Internal Error Codes Standard

- **Format**: `PREFIX_ENTITY_CODE`.

## 33. Environment & Secret Management

- **User Secrets**: Local development.
- **Key Vault**: Production secrets.

## 34. High-Performance Engineering

- **Response Compression**: Brotli/Gzip.
- **Client-Side Caching**: ETags support.
- **Read-Write Splitting**: DB Level routing.
- **Async Efficiency**: `ConfigureAwait(false)`.

## 35. Advanced Security & Hardening

- **Encryption at Rest**: AES-256 for PII.
- **JWT Revocation**: Secure refresh token lifecycle.
- **Password Hashing**: Argon2id/BCrypt.
- **Audit Logging**: Security events monitoring.

## 36. Distributed Data Consistency & Integrity

- **Idempotency Key**: `X-Idempotency-Key` for write operations.
- **Reconciliation**: Background workers to fix eventual consistency gaps.
- **Dual Writes**: Always via Outbox/Events.

## 37. System Reliability & Self-Healing

Ensuring the system remains operational under stress and automatically recovers from failures.

### Graceful Degradation:
- **Rule**: If a non-critical feature fails (e.g., "Related Events" recommendation), the main business flow (Booking) must remain unaffected.
- **Fallback UI**: The UI should handle `null` or empty responses from secondary services gracefully without crashing.

### Mandatory Service Timeouts:
- **Global Rule**: NEVER allow a network call (HTTP, DB, Cache, MQ) to wait indefinitely.
- **Implementation**: Every client must have an explicit timeout (e.g., 5-10 seconds). This prevents a slow downstream service from causing a "Thread Pool Starvation" in your service.

### Bulkheading (Resource Isolation):
- **Purpose**: Isolate resources so that a failure in one area doesn't starve the whole system.
- **Example**: Use separate HttpClient instances or separate thread pools for different external integrations. If the Payment service is slow, it shouldn't block threads used for the Event catalog.

### Database Deadlock Handling:
- **Retry Logic**: Implement specific Polly policies to catch SQL Deadlock exceptions and retry the transaction. Deadlocks are expected in high-traffic ticket booking scenarios.

### Web Server Graceful Shutdown:
- **Kestrel Standard**: Configure the web host to wait for active requests to complete (e.g., `HostOptions.ShutdownTimeout = TimeSpan.FromSeconds(30)`).
- **Rule**: When a shutdown signal is received, stop accepting new requests but allow existing ones to finish within the timeout period.

### Resource Limits & Quotas:
- **Monitoring**: Set alerts for CPU/Memory usage above 80%.
- **OOM Prevention**: For memory-intensive tasks (e.g., report generation), use limits to prevent the service from being killed by the OS (OOM Killer).

### Self-Healing Connections:
- **Reconnection Logic**: Services must proactively monitor the health of their connections to SQL, Redis, and RabbitMQ. If a connection is dropped, use an automated exponential backoff strategy to reconnect without manual intervention.

---

_Last Updated: 2026-04-22_
