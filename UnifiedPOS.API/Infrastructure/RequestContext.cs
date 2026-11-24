using System.Security.Claims;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.API.Infrastructure;

public sealed class RequestContext(IHttpContextAccessor accessor) : IRequestContext
{
    private ClaimsPrincipal Principal => accessor.HttpContext?.User ?? throw new InvalidOperationException("No active HTTP request.");
    public Guid UserId => ReadGuid(JwtClaims.UserId);
    public Guid OrganizationId => ReadGuid(JwtClaims.OrganizationId);
    public Guid SessionId => ReadGuid(JwtClaims.SessionId);
    public Guid? RegisterId => Guid.TryParse(Principal.FindFirstValue(JwtClaims.RegisterId), out var value) ? value : null;
    public UserRole Role => Enum.TryParse<UserRole>(Principal.FindFirstValue(JwtClaims.Role), out var value) ? value : throw new InvalidOperationException("The authenticated identity has no valid role.");
    private Guid ReadGuid(string claim) => Guid.TryParse(Principal.FindFirstValue(claim), out var value) && value != Guid.Empty ? value : throw new InvalidOperationException($"The authenticated identity has no valid '{claim}' claim.");
}
