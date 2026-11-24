using Microsoft.AspNetCore.Mvc;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;

namespace UnifiedPOS.API.Infrastructure;

public sealed class SessionValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IUnitOfWork uow)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = Guid.Empty;
            var organizationId = Guid.Empty;
            var sessionId = Guid.Empty;
            var valid = Guid.TryParse(context.User.FindFirst(JwtClaims.UserId)?.Value, out userId)
                && Guid.TryParse(context.User.FindFirst(JwtClaims.OrganizationId)?.Value, out organizationId)
                && Guid.TryParse(context.User.FindFirst(JwtClaims.SessionId)?.Value, out sessionId);
            if (!valid) { await RejectAsync(context); return; }
            var session = await uow.UserRepository.FindSessionAsync(x => x.Id == sessionId && x.UserId == userId && !x.Revoked && x.ExpiresOn > DateTime.UtcNow, context.RequestAborted, false);
            var user = session == null ? null : await uow.UserRepository.FindUserAsync(x => x.Id == userId && x.OrganizationId == organizationId && x.IsActive, context.RequestAborted, false);
            if (session == null || user == null
                || session.RegisterId?.ToString() != context.User.FindFirst(JwtClaims.RegisterId)?.Value
                || user.Role.ToString() != context.User.FindFirst(JwtClaims.Role)?.Value)
            { await RejectAsync(context); return; }
        }
        await next(context);
    }

    private static Task RejectAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return context.Response.WriteAsJsonAsync(new ProblemDetails { Status = 401, Title = "Unauthorized", Detail = "The session is expired, revoked, or invalid." });
    }
}
