# Unified POS backend

## Unified password login and cashier registers

`POST /api/v1/auth/login` accepts `userName` and `password` for Owner, Admin, Manager, and Cashier. Organization is resolved server-side from the globally unique normalized username. Apply the `UnifiedPasswordAndRegisterAssignments` migration before running this version. Existing duplicate normalized usernames must be resolved explicitly before that migration can succeed; it never merges accounts.

Token responses include userId, organizationId, role, and optional registerId alongside access/refresh tokens. PIN login is retired (HTTP 410).

Owner/Admin assign cashiers to locations, then replace register assignments using `PUT /api/v1/users/{id}/registers` with `{ "registerIds": [...] }` and an Idempotency-Key header. Assignments require active cashiers, registers, and organization locations. Duplicate pairs are prohibited by the join-table primary key; foreign keys reject missing users/registers.

After password login, cashiers call `GET /api/v1/pos/available-registers`, then `POST /api/v1/pos/select-register` with `{ "registerId": "..." }`. Selection rejects unauthorized/inactive resources and conflicting open shifts. It revokes the old session and returns new tokens, which clients must replace together. Register operations require the selected register, an existing assignment, and the cashier's own shift. Bound sessions are checked against current assignments and active location/register state on every authenticated HTTP request.

## Initial Owner provisioning

Owner is the super-admin persona within the current single organization. Owner and Admin may create/update locations and registers and manage users and assignments. Manager and Cashier cannot perform these mutations, including PIN resets. Owner location access is role-based and automatically includes future locations; assignment rows are not required.

Bootstrap is disabled by default. In Development configure the following with environment variables (`:` becomes `__`) or API project user secrets:

```powershell
dotnet user-secrets set "Bootstrap:Enabled" "true" --project UnifiedPOS.API
dotnet user-secrets set "Bootstrap:OrganizationName" "Your organization" --project UnifiedPOS.API
dotnet user-secrets set "Bootstrap:Currency" "PKR" --project UnifiedPOS.API
dotnet user-secrets set "Bootstrap:OwnerUserName" "aun@mailinator.com" --project UnifiedPOS.API
```

Set `Bootstrap__OwnerPassword` through your secure environment/secret manager, or use Visual Studio Manage User Secrets to add `Bootstrap:OwnerPassword`. Use a unique 12–128 character password. The requested password is eight characters; the suggested POS suffix produces eleven characters, so neither satisfies the existing policy. Do not commit credentials or reuse the SQL administrator password.

On startup a serializable transaction and SQL application lock protect initial creation. Bootstrap creates the organization and a hashed Owner password only if absent. It never resets an existing password, promotes an existing non-Owner, or creates a second initial Owner. Configuration mismatches fail startup. No bootstrap endpoint or secret-bearing response is exposed.

For production, apply migrations using a deployment identity, supply bootstrap values from a secret manager, and explicitly set `Bootstrap__AllowProduction=true` together with `Bootstrap__Enabled=true` for the controlled provisioning run. Disable bootstrap and remove its password afterward. Use a restricted SQL identity for the running API.

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
