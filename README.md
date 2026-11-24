# Unified POS backend

POS backend in development, targeting .NET 10 and following the structure of `D:\Aun\igi-health-connect-portal`.

## Scope

The repository contains domain, application, persistence, and HTTP API implementations for the primary POS modules. The structural correction described in [the handover](docs/HANDOVER.md) has been implemented: individual entity and enum files, module DTO files, service interfaces, and named module repositories exposed through a unit of work. The API host exposes versioned endpoints for authentication, users, organizations, catalog, customers, shifts, carts, sales, reports, and POS bootstrap/change operations.

Loyalty and Thrive integration are excluded. The supplied FSD is requirements input; the reference project determines code organization. The business workflows still require financial-policy and real-database verification before deployment.

## Architecture

- UnifiedPOS.API: HTTP controllers, middleware, and API configuration.
- UnifiedPOS.Application: services, DTOs, validators, mapping, and service/repository/helper interfaces.
- UnifiedPOS.Domain: entities and enums.
- UnifiedPOS.Infrastructure: database access, repositories, migrations, and templates.
- UnifiedPOS.BackgroundServices: reserved background service layer; currently a library until worker requirements are agreed.

Project references mirror the source solution: API references Application and Infrastructure; Infrastructure references Application; Application references Domain; BackgroundServices references Application and Infrastructure.

Each entity and enum has its own file. DTOs are grouped by module. Application services implement interfaces registered in `ApplicationLayer`; infrastructure contains module repositories, shared projection pagination, and lazily initialized repository instances in `UnitOfWork`. Repository contracts expose asynchronous, typed operations without exposing `DbContext` or `IQueryable` to services. The existing transaction boundary, query tracking, navigation loading, and DTO contracts were retained during restructuring.

`UnifiedPOS.Tests` contains architecture tests for service registration, repository instance reuse, and SQL Server model discovery. Its test SDK and xUnit packages are test-only dependencies.

Database configuration expects `ConnectionStrings:POSConnection`. JWT configuration requires `Jwt:Key` (at least 32 characters), `Jwt:Issuer`, and `Jwt:Audience`. Supply secrets through environment variables or a secret store; do not add them to tracked settings. No migration or database has been created or applied. Initial owner provisioning and worker configuration remain unfinished.

The API validates request models, JWT signature/audience/issuer/lifetime, backing session state, active-user state, register binding, and current role on protected requests. It applies role authorization, global and authentication-specific rate limits, correlation IDs, safe problem-detail errors, cancellation propagation, and idempotency keys for state-changing business operations. OpenAPI JSON is available at `/openapi/v1.json`, and the anonymous liveness endpoint is `/health`.

Required environment keys:

```text
ConnectionStrings__POSConnection
Jwt__Key
Jwt__Issuer
Jwt__Audience
```

## Commands

```sh
dotnet build UnifiedPOS.slnx
dotnet test UnifiedPOS.slnx
dotnet run --project UnifiedPOS.API
```

The tests do not open a database connection. They cover architecture and API security/contract metadata, but do not replace SQL Server integration tests, migrations, financial workflow tests, or end-to-end API authorization tests against persisted sessions.

Restore currently reports NU1902/NU1903 for transitive `System.Security.Cryptography.Pkcs` 9.0.11. This existing dependency issue must be resolved before release; see the handover for the remaining work.
