using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.ServiceInterface;
using static UnifiedPOS.Application.DTOs.AuthDTO;

namespace UnifiedPOS.API.Controllers;

[ApiController, Route("api/v1/auth")]
public sealed class AuthController(IAuthService service) : ControllerBase
{
    [AllowAnonymous, EnableRateLimiting("authentication"), HttpPost("login")] public Task<ResponseMessage> Login(LoginRequest model, CancellationToken ct) => service.LoginAsync(model, ct);
    [AllowAnonymous, EnableRateLimiting("authentication"), HttpPost("pin")] public Task<ResponseMessage> Pin(PinRequest model, CancellationToken ct) => service.PinAsync(model, ct);
    [AllowAnonymous, EnableRateLimiting("authentication"), HttpPost("refresh")] public Task<ResponseMessage> Refresh(RefreshRequest model, CancellationToken ct) => service.RefreshAsync(model, ct);
    [Authorize, HttpPost("logout")] public Task<ResponseMessage> Logout(CancellationToken ct) => service.LogoutAsync(ct);
}
