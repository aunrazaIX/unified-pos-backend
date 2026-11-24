# Unified POS backend

Structure-only scaffold based on igi-health-connect-portal, targeting .NET 10.

## Scope

No modules or business APIs are implemented. Future work is limited to primary POS modules after requirements discussion. Loyalty and Thrive integration are excluded for now. The supplied FSD is requirements input, not an instruction to adopt its architecture.

## Architecture

- UnifiedPOS.API: HTTP controllers, middleware, and API configuration.
- UnifiedPOS.Application: services, DTOs, validators, mapping, and service/repository/helper interfaces.
- UnifiedPOS.Domain: entities and enums.
- UnifiedPOS.Infrastructure: database access, repositories, migrations, and templates.
- UnifiedPOS.BackgroundServices: reserved background service layer; currently a library until worker requirements are agreed.

Project references mirror the source solution: API references Application and Infrastructure; Infrastructure references Application; Application references Domain; BackgroundServices references Application and Infrastructure.

Only folder conventions and project dependencies are reproduced. Healthcare implementations, secrets, database schemas, integrations, and unused NuGet packages are not copied. Empty folders use .gitkeep. Database, authentication, and worker configuration will be added with the relevant implementation; this scaffold is not a deployable POS service.

## Commands

```sh
dotnet build UnifiedPOS.slnx
dotnet run --project UnifiedPOS.API
```

The API host has no endpoints yet. No tests exist because no module behavior has been implemented.
