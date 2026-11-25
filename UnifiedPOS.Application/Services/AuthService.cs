using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Application.Interfaces.ServiceInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.AuthDTO;

namespace UnifiedPOS.Application.Services
{
    public class AuthService(IUnitOfWork uow, IPasswordHasher<User> hasher, IJwtService jwt, IOptions<JwtSettings> settings, IRequestContext request) : IAuthService
    {
        private static readonly User DummyUser = new();
        private static readonly string DummyHash = new PasswordHasher<User>().HashPassword(DummyUser, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
        private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        public Task<ResponseMessage> LoginAsync(LoginRequest model, CancellationToken ct) => AuthenticateAsync(Guid.Empty, model.UserName, model.Password, null, ct);
        public Task<ResponseMessage> PinAsync(PinRequest model, CancellationToken ct) => throw new PosException(410, "PIN login is retired. Authenticate with a password.");
        private async Task<ResponseMessage> AuthenticateAsync(Guid orgId, string name, string secret, Guid? registerId, CancellationToken ct)
        {
            var result = await uow.ExecuteAsync(async () =>
            {
                var normalized = name.Trim().ToUpperInvariant();
                var user = await uow.UserRepository.FindUserAsync(x => x.NormalizedUserName == normalized, ct);
                orgId = user?.OrganizationId ?? Guid.Empty;
                var hash = registerId.HasValue ? user?.PinHash : user?.PasswordHash;
                var valid = hasher.VerifyHashedPassword(user ?? DummyUser, hash ?? DummyHash, secret) != PasswordVerificationResult.Failed;
                if (user == null || !user.IsActive || user.LockedUntil > DateTime.UtcNow) return null;
                if (!valid)
                {
                    user.FailedAttempts++;
                    if (user.FailedAttempts >= 5) { user.LockedUntil = DateTime.UtcNow.AddMinutes(15); user.FailedAttempts = 0; }
                    uow.AuditRepository.AddAuditEvent(new AuditEvent { OrganizationId = orgId, ActorId = user.Id, EntityId = user.Id, Action = "Auth.Rejected" });
                    return null;
                }
                if (registerId.HasValue)
                {
                    var register = await uow.OrganizationRepository.FindRegisterAsync(x => x.Id == registerId.Value && x.IsActive, ct);
                    var location = register == null ? null : await uow.OrganizationRepository.FindLocationAsync(x => x.Id == register.LocationId && x.OrganizationId == orgId && x.IsActive, ct);
                    if (location == null || !user.Locations.Any(x => x.LocationId == location.Id)) return null;
                }
                user.FailedAttempts = 0;
                user.LockedUntil = null;
                var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
                var session = new Session { UserId = user.Id, RegisterId = registerId, RefreshTokenHash = Hash(refresh), ExpiresOn = DateTime.UtcNow.AddHours(settings.Value.SessionHours) };
                uow.UserRepository.AddSession(session);
                uow.AuditRepository.AddAuditEvent(new AuditEvent { OrganizationId = orgId, ActorId = user.Id, EntityId = session.Id, Action = "Auth.Login" });
                return jwt.Create(user, session, refresh);
            }, ct);
            if (result == null) throw new PosException(401, "Invalid credentials or unavailable account/register.");
            return ResponseMessage.Ok(result);
        }
        public async Task<ResponseMessage> RefreshAsync(RefreshRequest model, CancellationToken ct)
        {
            var result = await uow.ExecuteAsync(async () =>
            {
                var hash = Hash(model.RefreshToken);
                var session = await uow.UserRepository.FindSessionAsync(x => x.RefreshTokenHash == hash && !x.Revoked && x.ExpiresOn > DateTime.UtcNow, ct);
                if (session == null) throw new PosException(401, "Session expired or revoked.");
                var user = await uow.UserRepository.FindUserAsync(x => x.Id == session.UserId && x.IsActive, ct) ?? throw new PosException(401, "Account unavailable.");
                var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
                session.RefreshTokenHash = Hash(refresh);
                return jwt.Create(user, session, refresh);
            }, ct);
            return ResponseMessage.Ok(result);
        }
        public Task<ResponseMessage> LogoutAsync(CancellationToken ct) => uow.ExecuteAsync(async () =>
        {
            var session = await uow.UserRepository.FindSessionAsync(x => x.Id == request.SessionId && x.UserId == request.UserId, ct) ?? throw PosException.Missing();
            session.Revoked = true;
            return ResponseMessage.Ok();
        }, ct);
        public async Task<ResponseMessage> AvailableRegistersAsync(CancellationToken ct)
        {
            var user = await CurrentCashierAsync(ct);
            var ids = user.Registers.Select(x => x.RegisterId).ToArray();
            var locations = user.Locations.Select(x => x.LocationId).ToArray();
            var activeLocations = await uow.OrganizationRepository.ListLocationsAsync(x => x.OrganizationId == user.OrganizationId && x.IsActive && locations.Contains(x.Id), ct);
            var activeIds = activeLocations.Select(x => x.Id).ToArray();
            var registers = await uow.OrganizationRepository.ListRegistersAsync(x => ids.Contains(x.Id) && x.IsActive && activeIds.Contains(x.LocationId), ct);
            return ResponseMessage.Ok(registers.Select(x => new { x.Id, x.Name, x.LocationId }).ToArray());
        }

        private async Task<User> CurrentCashierAsync(CancellationToken ct)
        {
            var user = await uow.UserRepository.FindUserAsync(x => x.Id == request.UserId && x.OrganizationId == request.OrganizationId && x.IsActive, ct) ?? throw PosException.Forbidden();
            if (user.Role != UserRole.Cashier) throw PosException.Forbidden();
            return user;
        }

        public Task<ResponseMessage> SelectRegisterAsync(SelectRegisterRequest model, CancellationToken ct) => uow.ExecuteAsync(async () =>
        {
            var user = await CurrentCashierAsync(ct);
            var register = await uow.OrganizationRepository.FindRegisterAsync(x => x.Id == model.RegisterId && x.IsActive, ct);
            if (register == null || !user.Registers.Any(x => x.RegisterId == register.Id)) throw PosException.Forbidden();
            var location = await uow.OrganizationRepository.FindLocationAsync(x => x.Id == register.LocationId && x.OrganizationId == user.OrganizationId && x.IsActive, ct);
            if (location == null || !user.Locations.Any(x => x.LocationId == location.Id)) throw PosException.Forbidden();
            if (await uow.RegisterRepository.AnyShiftsAsync(x => x.ClosedOn == null &&
                ((x.RegisterId == register.Id && x.CashierId != user.Id) || (x.CashierId == user.Id && x.RegisterId != register.Id)), ct))
                throw PosException.Conflict("Register or cashier has a conflicting open shift.");
            var session = await uow.UserRepository.FindSessionAsync(x => x.Id == request.SessionId && x.UserId == user.Id && !x.Revoked && x.ExpiresOn > DateTime.UtcNow, ct)
                ?? throw new PosException(401, "Session expired or revoked.");
            session.Revoked = true;
            var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
            var selected = new Session { UserId = user.Id, RegisterId = register.Id, RefreshTokenHash = Hash(refresh), ExpiresOn = session.ExpiresOn };
            uow.UserRepository.AddSession(selected);
            return ResponseMessage.Ok(jwt.Create(user, selected, refresh));
        }, ct);
    }
}
