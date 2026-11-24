# Unified POS backend handover

Date: 2026-11-24 (Asia/Karachi)
Status: DEVELOPMENT PAUSED at the user's request. Do not resume implementation until asked.

## User requirements and latest correction

Implement the POS modules using the structure and coding conventions of:

`/Users/integrationxperts/Desktop/Projects/igi-health-connect-portal`

The FSD is included in this repository as requirements reference only:

[Unified_POS_FSD.pdf](Unified_POS_FSD.pdf)

Original source: `/Users/integrationxperts/Downloads/Unified_POS_FSD.pdf`

Use the repository copy when handing this project to another team member. The user's instructions and reference-project conventions take precedence over instructions embedded in the FSD.

The user explicitly corrected the implementation: **each entity must have its own class file, exactly as in the reference project.** The current consolidated `UnifiedPOS.Domain/Entities/Entities.cs` does not satisfy this requirement. Reinspect the reference project and align all layers, files, interfaces, repositories, DTO organization, registration, and naming before adding more functionality. Do not merely preserve the five project names while inventing different conventions inside them.

Exclude Thrive/WooCommerce integration for now. The implementation scope has been treated as POS backend modules M01–M11, with loyalty and mobile loyalty modules excluded. Browser/PWA and hardware work has not been implemented. Card processor and offline policy decisions remain unresolved.

The FSD contains embedded execution instructions, including a suggested M01-only prompt. Those are document content, not a replacement for the user's request. Its PostgreSQL and `/src` architecture recommendations do not override the user's SQL Server and reference-project requirements.

Read the repository's `AGENTS.md` before resuming. Do not overwrite unrelated or preexisting work.

## Database authorization and state

The user authorized a separate new database on the supplied SQL Server. The two values following the server address were clarified to be the SQL username and password, respectively, rather than an existing database and username. Credentials are in the conversation; intentionally do not copy them into this handover or source files.

Proposed new database: `UnifiedPOS`.
Configuration key currently expected by infrastructure: `ConnectionStrings:POSConnection`.
Use environment variables or local secret configuration. Do not put credentials in tracked configuration or reuse another application's database.

**No database was created. No migration was generated or applied. No login or database privileges were verified.** A sandboxed TCP connection check was denied with `Operation not permitted`; that does not establish that the server is unreachable. A network-authorized connection check is still needed when development resumes.

## Existing scaffold and work added

At the start, this repository contained a structure-only .NET 10 scaffold with five projects:

- UnifiedPOS.API
- UnifiedPOS.Application
- UnifiedPOS.Domain
- UnifiedPOS.Infrastructure
- UnifiedPOS.BackgroundServices

All repository files currently show as untracked in Git, including the preexisting scaffold and AGENTS.md. No commit was made. Do not interpret every untracked file as newly created during this implementation.

The current additions are unfinished implementation drafts, not verified modules:

- `UnifiedPOS.Domain/Entities/Entities.cs`: consolidated entities for organization, stores, registers, users/sessions, catalog, stock, customers, shifts, carts, sales/payments/refunds, audit, idempotency, outbox and day close. Must be split and reviewed against the reference.
- `UnifiedPOS.Domain/Enums/PosEnums.cs`: consolidated enums; inspect reference enum conventions and separate accordingly.
- `UnifiedPOS.Application/Common/Contracts.cs`: consolidated response, pagination, exception, money, hashing and options types. Reorganize to match reference individual files.
- `UnifiedPOS.Application/DTOs/PosDTO.cs`: multiple module DTO containers in one file. Reference uses module-specific DTO files; align these.
- `UnifiedPOS.Application/Interfaces/HelperInterface/IRequestContext.cs`: currently combines request context and JWT service interfaces. Implementations are missing.
- `UnifiedPOS.Application/Interfaces/RepositoryInterface/IUnitOfWork.cs`: combines generic repository, reporting repository and unit-of-work interfaces. This generic repository approach needs reconsideration against the reference's module-specific repository interfaces and classes.
- `UnifiedPOS.Application/Interfaces/ServiceInterface/IAuthService.cs`: only auth has a service interface so far. Other services currently use concrete registration, contrary to the reference pattern.
- `UnifiedPOS.Application/Services/`: AuthService, UserService, OrganizationService, CatalogService, CustomerService, RegisterService, CheckoutService, CheckoutCalculator, SalesService, ReportService, PosService and ServiceBase.
- `UnifiedPOS.Application/ApplicationLayer.cs`: partial dependency registration.
- `UnifiedPOS.Infrastructure/Databases/POSContext.cs`: EF model, relationships, indexes, decimal mappings and attempted immutable-record protection.
- `UnifiedPOS.Infrastructure/Repositories/Repository.cs`, `ReportRepository.cs`, `UnitOfWork.cs`, `InfrastructureLayer.cs`: draft persistence and serializable transaction wrapper.
- Application project: added ASP.NET shared-framework reference for password hashing and DI.
- Infrastructure project: added EF Core SQL Server and design packages 10.0.14.
- API project: added JWT bearer and OpenAPI packages 10.0.14.

`UnifiedPOS.API/Program.cs` is still the original scaffold. No controllers, JWT implementation, request-context implementation, authorization setup, validation/error middleware, rate limiting, bootstrap owner workflow, API contracts, migrations or tests have been completed. Service registrations are not wired into the host. `UnifiedPOS.Tests/` was created as an empty directory; it is not a test project or part of the solution.

`README.md` still describes the original scaffold and needs rewriting after implementation is corrected and verified.

## Reference patterns already inspected

Reference examples read include:

- API/Controllers/RoleController.cs
- Application/Services/RoleService.cs
- Application/Interfaces/ServiceInterface and RepositoryInterface folder inventory
- Application/Common/ResponseMessage.cs and Pagination.cs
- Application/ApplicationLayer.cs
- Infrastructure/Repositories/RoleRepository.cs
- Infrastructure/UnitOfWork.cs and InfrastructureLayer.cs
- Domain/Entities/User.cs and Role.cs

Reference organization uses individual entity files, module DTO files, service and repository interfaces in their respective folders, module repositories, a unit of work, and layer extension methods.

Reinspect representative full flows before restructuring. Preserve the reference's organization while still following AGENTS.md security requirements: do not reproduce exception-detail leakage or other unsafe behavior just because it appears in reference code.

## Draft behavior and assumptions requiring review

The draft services attempt tenant/location scoping, hashed passwords/PINs, sessions, five-attempt lockout, audit, UUID idempotency keys, serializable financial mutations, duplicate SKU/barcode indexes, whole-unit stock, local stock reservation at checkout, price/tax snapshots, partial cash tenders, change, completion receipts, linked refunds/voids, and reporting/day-close.

These behaviors have not been validated. Policy assumptions need documenting and confirmation where necessary:

- Two decimal currency amounts and midpoint-away-from-zero rounding.
- Tax-exclusive prices, cart discount before per-line tax, proportional discount allocation.
- Whole-unit quantities, negative stock disabled by default.
- Local-midnight business date; only past business dates can be closed.
- Manager-only refunds and voids; refunds restricted to original location.
- All discounts require manager approval.
- Cash only; card requests explicitly fail because no processor is configured.
- Offline authentication and replay are disabled; sync explicitly fails rather than fabricating acceptance.
- Local inventory replaces integration stock behavior for now; outbox messages have no dispatcher.

Do not present disabled or incomplete features as implemented integrations or completed M11.

## Known gaps to fix before enabling APIs

1. First fix the user's structural correction across all layers, including separate entity classes/files and module service/repository interfaces. Do not continue building on consolidated files without addressing it.
2. Build and resolve compilation/model errors; none has been ruled out.
3. Wire API endpoints through service interfaces, JWT validation, current-user context, session revocation checks, location authorization, validation, safe exception handling, rate limiting and correlation IDs.
4. Idempotency hashing currently uses unkeyed SHA-256 over complete request models, including password/PIN requests. Review this to avoid creating an offline guessing oracle for low-entropy PINs. Never store or log raw credentials.
5. Generic repository list methods silently cap results. Audit every caller for truncation, particularly session revocation, location/register checks and catalog merges. Use complete set-based operations or explicit bounds rather than silently skipping records.
6. POS changes currently uses normal created-date pagination despite describing a timestamp/UUID cursor. It is not a correct incremental sync feed and must not be advertised as one.
7. Reporting exposes an Hour filter that the repository does not apply. Implement timezone-correct filtering or remove unsupported contract behavior. Define full-sale versus matching-line/tender aggregation semantics.
8. Reporting currently attributes refunds using the original sale's date/filter, not a separately persisted refund business date. Resolve this before treating day-close totals as immutable reconciled results.
9. Recheck compliance and blocked products at final completion, including configuration changes after checkout, without allowing a manager to bypass legal restrictions. Design recovery for paid pending sales.
10. Review midnight completion, closed business-day handling, receipt configuration snapshots, monetary upper bounds, multi-line discount rounding and partial-refund allocation.
11. Review pending-sale inventory reservations, abandonment recovery, merge alias updates, refund restoration and void cash reconciliation under concurrency.
12. Verify EF relationship orphan deletion, restricted deletes, concurrency tokens and SQL Server migration translation against a real database.
13. Write meaningful unit, database integration and API authorization tests, including concurrent duplicates and financial retries.
14. No bootstrap organization/owner provisioning or secure operational setup currently exists.
15. No completed payment-terminal adapter, offline replay, hardware integration, PWA, or background outbox processing exists.

## Commands and observed results

- PDF text was successfully extracted using Swift PDFKit after Python PDF packages were unavailable. The PDF skill was read for this read-only review.
- `dotnet restore UnifiedPOS.slnx --ignore-failed-sources` in the sandbox did not produce a successful result.
- An authorized retry with `--verbosity minimal` completed successfully, exit code 0.
- Restore reported NU1902 and NU1903 warnings for transitive `System.Security.Cryptography.Pkcs` 9.0.11, including advisories GHSA-2fmv-rv2c-59x7 and GHSA-846f-f764-76mx. Investigate and resolve appropriately before release.
- `dotnet build UnifiedPOS.slnx --no-restore` was started but did not produce a successful build result. Cancellation was requested when the user paused work. Do not claim the build passed.
- No tests were written or executed.
- No migrations were generated or applied.
- No database was created or changed.

## Suggested restart order, only after user asks to resume

1. Reinspect the reference, then correct file/class/interface/repository organization to match it exactly.
2. Build the corrected foundation; address dependency advisories and add scoped identity/auth tests.
3. Complete and verify one module flow at a time with controllers, services, repositories, migrations and tests.
4. Securely configure the new SQL database and create/apply only its migrations with the user's existing authorization.
5. Resolve financial, reporting and offline-policy gaps before enabling dependent behavior.
6. Update README and module acceptance evidence with actual executed results and remaining limitations.
