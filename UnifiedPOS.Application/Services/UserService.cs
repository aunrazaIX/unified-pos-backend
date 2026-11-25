using UnifiedPOS.Application.Interfaces.ServiceInterface;
using Microsoft.AspNetCore.Identity;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.AuthDTO;

namespace UnifiedPOS.Application.Services
{
    public class UserService(IUnitOfWork uow, IRequestContext request, IPasswordHasher<User> hasher) : ServiceBase(uow, request), IUserService
    {
        public async Task<ResponseMessage> GetAllUsersAsync(Pagination page, CancellationToken ct)
        {
            RequireAdmin();
            return ResponseMessage.Ok(await uow.UserRepository.PageUsersAsync(x => x.OrganizationId == OrgId && (page.SearchString == null || x.UserName.Contains(page.SearchString)),
                x => new UserResponse(x.Id, x.UserName, x.Role, x.IsActive, x.Locations.Select(l => l.LocationId).ToArray()), page, ct));
        }
        public Task<ResponseMessage> AddUserAsync(UserRequest model, string key, CancellationToken ct) => MutateAsync("User.Create", key, model, async () =>
        {
            RequireAdmin();
            if (model.Role == UserRole.Owner && request.Role != UserRole.Owner) throw PosException.Forbidden();
            var ids = model.LocationIds.Distinct().ToArray();
            await ValidateLocationsAsync(ids, ct);
            var user = new User { OrganizationId = OrgId, UserName = model.UserName.Trim(), NormalizedUserName = model.UserName.Trim().ToUpperInvariant(), Role = model.Role };
            if (await uow.UserRepository.AnyUsersAsync(x => x.NormalizedUserName == user.NormalizedUserName, ct)) throw PosException.Conflict("User name already exists.");
            user.PasswordHash = hasher.HashPassword(user, model.Password);
            user.Locations = ids.Select(id => new UserLocation { UserId = user.Id, LocationId = id }).ToList();
            uow.UserRepository.AddUser(user);
            Audit("User.Created", user.Id);
            return new UserResponse(user.Id, user.UserName, user.Role, user.IsActive, ids);
        }, ct);
        private async Task ValidateLocationsAsync(Guid[] ids, CancellationToken ct)
        {
            var locations = await uow.OrganizationRepository.ListLocationsAsync(x => x.OrganizationId == OrgId && ids.Contains(x.Id), ct);
            if (locations.Count != ids.Length) throw PosException.Invalid("One or more locations are unavailable.");
        }
        public Task<ResponseMessage> SetRegistersAsync(Guid id, RegistersRequest model, string key, CancellationToken ct) => MutateAsync("User.Registers", key, new { id, model }, async () =>
        {
            RequireAdmin();
            var user = await uow.UserRepository.FindUserAsync(x => x.Id == id && x.OrganizationId == OrgId && x.IsActive, ct) ?? throw PosException.Missing();
            if (user.Role != UserRole.Cashier) throw PosException.Invalid("Only cashiers receive register assignments.");
            var ids = model.RegisterIds.Distinct().ToArray();
            if (ids.Length != model.RegisterIds.Length) throw PosException.Invalid("Duplicate register assignments are invalid.");
            var locations = user.Locations.Select(x => x.LocationId).ToArray();
            var activeLocations = await uow.OrganizationRepository.ListLocationsAsync(x => x.OrganizationId == OrgId && x.IsActive && locations.Contains(x.Id), ct);
            var locationIds = activeLocations.Select(x => x.Id).ToArray();
            var registers = await uow.OrganizationRepository.ListRegistersAsync(x => ids.Contains(x.Id) && x.IsActive && locationIds.Contains(x.LocationId), ct);
            if (registers.Count != ids.Length) throw PosException.Invalid("Registers must be active and belong to assigned active organization locations.");
            foreach (var removed in user.Registers.Where(x => !ids.Contains(x.RegisterId)).ToArray()) user.Registers.Remove(removed);
            foreach (var registerId in ids.Where(x => !user.Registers.Any(r => r.RegisterId == x))) user.Registers.Add(new UserRegister { UserId = id, RegisterId = registerId });
            Audit("User.RegistersChanged", id);
            return new { UserId = id, RegisterIds = ids };
        }, ct);
        public Task<ResponseMessage> SetLocationsAsync(Guid id, LocationsRequest model, string key, CancellationToken ct) => MutateAsync("User.Locations", key, new { id, model }, async () =>
        {
            RequireAdmin();
            var user = await uow.UserRepository.FindUserAsync(x => x.Id == id && x.OrganizationId == OrgId, ct) ?? throw PosException.Missing();
            if (user.Role == UserRole.Owner && request.Role != UserRole.Owner) throw PosException.Forbidden();
            var ids = model.LocationIds.Distinct().ToArray();
            await ValidateLocationsAsync(ids, ct);
            var removed = user.Locations.Where(x => !ids.Contains(x.LocationId)).ToArray();
            foreach (var item in removed) user.Locations.Remove(item);
            foreach (var locationId in ids.Where(i => !user.Locations.Any(l => l.LocationId == i))) user.Locations.Add(new UserLocation { UserId = id, LocationId = locationId });
            var sessions = await uow.UserRepository.ListSessionsAsync(x => x.UserId == id && !x.Revoked, ct, true);
            foreach (var session in sessions) session.Revoked = true;
            Audit("User.LocationsChanged", id);
            return null;
        }, ct);
        public Task<ResponseMessage> ResetPinAsync(Guid id, ResetPinRequest model, string key, CancellationToken ct) => MutateAsync("User.PinReset", key, new { id, model }, async () =>
        {
            RequireAdmin();
            var user = await uow.UserRepository.FindUserAsync(x => x.Id == id && x.OrganizationId == OrgId, ct) ?? throw PosException.Missing();
            if (request.Role == UserRole.Manager)
            {
                if (user.Role != UserRole.Cashier || user.Locations.Count == 0) throw PosException.Forbidden();
                foreach (var location in user.Locations) await LocationAsync(location.LocationId, ct);
            }
            else if (user.Role == UserRole.Owner && request.Role != UserRole.Owner) throw PosException.Forbidden();
            user.PinHash = hasher.HashPassword(user, model.Pin);
            user.FailedAttempts = 0;
            user.LockedUntil = null;
            var sessions = await uow.UserRepository.ListSessionsAsync(x => x.UserId == id && !x.Revoked, ct, true);
            foreach (var session in sessions) session.Revoked = true;
            Audit("User.PinReset", id);
            return null;
        }, ct);
    }
}
