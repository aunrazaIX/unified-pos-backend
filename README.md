# Unified POS backend

POS backend in development, targeting .NET 10 and following the structure of `D:\Aun\igi-health-connect-portal`.

## Scope

The repository contains draft domain, application, and persistence implementations for the primary POS modules. The structural correction described in [the handover](docs/HANDOVER.md) has been implemented: individual entity and enum files, module DTO files, service interfaces, and named module repositories exposed through a unit of work. The API host still has no business endpoints and does not register these layers yet.

Loyalty and Thrive integration are excluded. The supplied FSD is requirements input; the reference project determines code organization. These drafts still require security, financial-policy, database, and API verification before deployment.

## Architecture

- UnifiedPOS.API: HTTP controllers, middleware, and API configuration.
- UnifiedPOS.Application: services, DTOs, validators, mapping, and service/repository/helper interfaces.
- UnifiedPOS.Domain: entities and enums.
- UnifiedPOS.Infrastructure: database access, repositories, migrations, and templates.
- UnifiedPOS.BackgroundServices: reserved background service layer; currently a library until worker requirements are agreed.

Project references mirror the source solution: API references Application and Infrastructure; Infrastructure references Application; Application references Domain; BackgroundServices references Application and Infrastructure.

Each entity and enum has its own file. DTOs are grouped by module. Application services implement interfaces registered in `ApplicationLayer`; infrastructure contains module repositories, shared projection pagination, and lazily initialized repository instances in `UnitOfWork`. Repository contracts expose asynchronous, typed operations without exposing `DbContext` or `IQueryable` to services. The existing transaction boundary, query tracking, navigation loading, and DTO contracts were retained during restructuring.

`UnifiedPOS.Tests` contains architecture tests for service registration, repository instance reuse, and SQL Server model discovery. Its test SDK and xUnit packages are test-only dependencies.

Database configuration expects `ConnectionStrings:POSConnection` when infrastructure is enabled. Supply it through environment variables or local secret configuration. No migration or database has been created or applied. JWT, request context, authorization, provisioning, and worker configuration remain unfinished.

## Commands

```sh
dotnet build UnifiedPOS.slnx
dotnet test UnifiedPOS.slnx
dotnet run --project UnifiedPOS.API
```

The model tests do not open a database connection. They do not replace SQL Server integration tests, migrations, financial workflow tests, or API authorization tests.

Restore currently reports NU1902/NU1903 for transitive `System.Security.Cryptography.Pkcs` 9.0.11. This existing dependency issue must be resolved before release; see the handover for the remaining work.
