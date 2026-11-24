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
        public Task<ResponseMessage> LoginAsync(LoginRequest model, CancellationToken ct) => AuthenticateAsync(model.OrganizationId, model.UserName, model.Password, null, ct);
        public Task<ResponseMessage> PinAsync(PinRequest model, CancellationToken ct) => AuthenticateAsync(model.OrganizationId, model.UserName, model.Pin, model.RegisterId, ct);
        private async Task<ResponseMessage> AuthenticateAsync(Guid orgId, string name, string secret, Guid? registerId, CancellationToken ct)
        {
            var result = await uow.ExecuteAsync(async () =>
            {
                var normalized = name.Trim().ToUpperInvariant();
                var user = await uow.UserRepository.FindUserAsync(x => x.OrganizationId == orgId && x.NormalizedUserName == normalized, ct);
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
                else if (user.Role == UserRole.Cashier) return null;
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
    }
}
