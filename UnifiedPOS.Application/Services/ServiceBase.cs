using System.Text.Json;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.Application.Services
{
    public abstract class ServiceBase(IUnitOfWork uow, IRequestContext request)
    {
        protected readonly IUnitOfWork uow = uow;
        protected readonly IRequestContext request = request;
        protected Guid OrgId => request.OrganizationId;
        protected Guid ActorId => request.UserId;
        protected bool IsManager => request.Role != UserRole.Cashier;
        protected void RequireManager() { if (!IsManager) throw PosException.Forbidden(); }
        protected void RequireAdmin() { if (request.Role is not (UserRole.Owner or UserRole.Admin)) throw PosException.Forbidden(); }
        protected async Task<Location> LocationAsync(Guid id, CancellationToken ct)
        {
            var location = await uow.OrganizationRepository.FindLocationAsync(x => x.Id == id && x.OrganizationId == OrgId, ct) ?? throw PosException.Missing();
            if (request.Role is UserRole.Owner or UserRole.Admin) return location;
            var user = await uow.UserRepository.FindUserAsync(x => x.Id == ActorId && x.OrganizationId == OrgId && x.IsActive, ct) ?? throw PosException.Forbidden();
            if (!user.Locations.Any(x => x.LocationId == id)) throw PosException.Forbidden();
            return location;
        }
        protected async Task<Register> RegisterAsync(Guid id, CancellationToken ct)
        {
            var register = await uow.OrganizationRepository.FindRegisterAsync(x => x.Id == id, ct) ?? throw PosException.Missing();
            await LocationAsync(register.LocationId, ct);
            if (request.Role == UserRole.Cashier)
            {
                var user = await uow.UserRepository.FindUserAsync(x => x.Id == ActorId && x.OrganizationId == OrgId && x.IsActive, ct) ?? throw PosException.Forbidden();
                if (request.RegisterId != id || !user.Registers.Any(x => x.RegisterId == id) || !register.IsActive)
                    throw PosException.Forbidden();
            }
            if (request.RegisterId.HasValue && request.RegisterId != id) throw PosException.Forbidden();
            return register;
        }
        protected async Task<Shift> ShiftAsync(Guid id, CancellationToken ct, bool requireOpen = true)
        {
            var shift = await uow.RegisterRepository.FindShiftAsync(x => x.Id == id, ct) ?? throw PosException.Missing();
            var register = await RegisterAsync(shift.RegisterId, ct);
            var location = await LocationAsync(register.LocationId, ct);
            if (!IsManager && shift.CashierId != ActorId) throw PosException.Forbidden();
            if (requireOpen && (shift.ClosedOn.HasValue || !register.IsActive || !location.IsActive))
                throw PosException.Conflict("Shift, register and location must be active.");
            return shift;
        }
        protected async Task EnsureCustomerAsync(Guid? id, CancellationToken ct)
        {
            if (id.HasValue && !await uow.CustomerRepository.AnyCustomersAsync(x => x.Id == id && x.OrganizationId == OrgId, ct)) throw PosException.Missing();
        }
        protected void Audit(string action, Guid id, Guid? locationId = null, string? reason = null) => uow.AuditRepository.AddAuditEvent(new AuditEvent
        {
            OrganizationId = OrgId, LocationId = locationId, ActorId = ActorId, Action = action, EntityId = id, Reason = reason
        });
        protected Task<ResponseMessage> MutateAsync<T>(string action, string key, T payload, Func<Task<object?>> execute, CancellationToken ct)
        {
            if (!Guid.TryParse(key, out var operationId) || operationId == Guid.Empty) throw PosException.Invalid("Idempotency-Key must be a non-empty UUID.");
            var operationKey = action + ":" + operationId.ToString("N");
            var hash = OperationHash.Of(payload);
            return uow.ExecuteAsync(async () =>
            {
                var existing = await uow.OperationRepository.FindOperationAsync(x => x.OrganizationId == OrgId && x.ActorId == ActorId && x.Key == operationKey, ct);
                if (existing != null)
                {
                    if (existing.PayloadHash != hash) throw PosException.Conflict("Idempotency key was already used with a different payload.");
                    return JsonSerializer.Deserialize<ResponseMessage>(existing.ResponseJson)!;
                }
                var result = ResponseMessage.Ok(await execute());
                uow.OperationRepository.AddOperation(new Operation { OrganizationId = OrgId, ActorId = ActorId, Key = operationKey, PayloadHash = hash, ResponseJson = JsonSerializer.Serialize(result) });
                return result;
            }, ct);
        }
    }
}
