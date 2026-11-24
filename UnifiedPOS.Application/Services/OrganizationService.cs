using UnifiedPOS.Application.Interfaces.ServiceInterface;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.OrganizationDTO;

namespace UnifiedPOS.Application.Services
{
    public class OrganizationService(IUnitOfWork uow, IRequestContext request) : ServiceBase(uow, request), IOrganizationService
    {
        public async Task<ResponseMessage> GetLocationsAsync(Pagination page, CancellationToken ct)
        {
            var user = await uow.UserRepository.FindUserAsync(x => x.Id == ActorId && x.OrganizationId == OrgId, ct) ?? throw PosException.Forbidden();
            var ids = user.Locations.Select(x => x.LocationId).ToArray();
            var all = request.Role is UserRole.Owner or UserRole.Admin;
            return ResponseMessage.Ok(await uow.OrganizationRepository.PageLocationsAsync(x => x.OrganizationId == OrgId && (all || ids.Contains(x.Id)) && (page.SearchString == null || x.Name.Contains(page.SearchString)),
                x => new LocationResponse(x.Id, x.Name, x.Address, x.TimeZoneId, x.ReceiptHeader, x.Hours, x.TaxRate, x.IsActive), page, ct));
        }
        public Task<ResponseMessage> SaveLocationAsync(Guid? id, LocationRequest model, string key, CancellationToken ct) => MutateAsync("Location.Save", key, new { id, model }, async () =>
        {
            RequireAdmin();
            try { TimeZoneInfo.FindSystemTimeZoneById(model.TimeZoneId); }
            catch (TimeZoneNotFoundException) { throw PosException.Invalid("Unknown time zone."); }
            catch (InvalidTimeZoneException) { throw PosException.Invalid("Invalid time zone."); }
            if (decimal.Round(model.TaxRate, 4) != model.TaxRate) throw PosException.Invalid("Tax rate supports four decimal places.");
            var location = id.HasValue ? await LocationAsync(id.Value, ct) : new Location { OrganizationId = OrgId };
            if (!id.HasValue) uow.OrganizationRepository.AddLocation(location);
            location.Name = model.Name.Trim(); location.Address = model.Address.Trim(); location.TimeZoneId = model.TimeZoneId;
            location.ReceiptHeader = model.ReceiptHeader; location.Hours = model.Hours; location.TaxRate = model.TaxRate;
            if (id.HasValue && !model.IsActive)
            {
                var registers = await uow.OrganizationRepository.ListRegistersAsync(x => x.LocationId == location.Id, ct);
                var registerIds = registers.Select(x => x.Id).ToArray();
                if (await uow.RegisterRepository.AnyShiftsAsync(x => registerIds.Contains(x.RegisterId) && x.ClosedOn == null, ct))
                    throw PosException.Conflict("Close active shifts before disabling a location.");
            }
            location.IsActive = model.IsActive;
            Audit("Location.Saved", location.Id, location.Id);
            return new LocationResponse(location.Id, location.Name, location.Address, location.TimeZoneId, location.ReceiptHeader, location.Hours, location.TaxRate, location.IsActive);
        }, ct);
        public async Task<ResponseMessage> GetRegistersAsync(Guid locationId, Pagination page, CancellationToken ct)
        {
            await LocationAsync(locationId, ct);
            return ResponseMessage.Ok(await uow.OrganizationRepository.PageRegistersAsync(x => x.LocationId == locationId, x => new RegisterResponse(x.Id, x.LocationId, x.Name, x.IsActive), page, ct));
        }
        public Task<ResponseMessage> SaveRegisterAsync(Guid locationId, Guid? id, RegisterRequest model, string key, CancellationToken ct) => MutateAsync("Register.Save", key, new { locationId, id, model }, async () =>
        {
            RequireManager(); await LocationAsync(locationId, ct);
            var register = id.HasValue ? await RegisterAsync(id.Value, ct) : new Register { LocationId = locationId };
            if (register.LocationId != locationId) throw PosException.Missing();
            if (!model.IsActive && await uow.RegisterRepository.AnyShiftsAsync(x => x.RegisterId == register.Id && x.ClosedOn == null, ct)) throw PosException.Conflict("Close the active shift before disabling a register.");
            register.Name = model.Name.Trim(); register.IsActive = model.IsActive;
            if (!id.HasValue) uow.OrganizationRepository.AddRegister(register);
            Audit("Register.Saved", register.Id, locationId);
            return new RegisterResponse(register.Id, locationId, register.Name, register.IsActive);
        }, ct);
        public async Task<ResponseMessage> GetReadinessAsync(Guid locationId, CancellationToken ct)
        {
            var location = await LocationAsync(locationId, ct);
            var hasRegister = await uow.OrganizationRepository.AnyRegistersAsync(x => x.LocationId == locationId && x.IsActive, ct);
            return ResponseMessage.Ok(new { locationId, location.IsActive, HasActiveRegister = hasRegister, HasAddress = !string.IsNullOrWhiteSpace(location.Address), HasReceiptHeader = !string.IsNullOrWhiteSpace(location.ReceiptHeader), ThriveEnabled = false, CardPaymentsEnabled = false, ReadyForCash = location.IsActive && hasRegister && !string.IsNullOrWhiteSpace(location.ReceiptHeader) });
        }
    }
}
