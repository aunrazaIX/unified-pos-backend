using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.ServiceInterface;

namespace UnifiedPOS.API.Controllers;

[ApiController, Authorize, Route("api/v1/pos")]
public sealed class PosController(IPosService service) : ControllerBase
{
    [Authorize(Roles = "Cashier"), HttpGet("available-registers")]
    public Task<ResponseMessage> AvailableRegisters([FromServices] IAuthService auth, CancellationToken ct) => auth.AvailableRegistersAsync(ct);
    [Authorize(Roles = "Cashier"), HttpPost("select-register")]
    public Task<ResponseMessage> SelectRegister([FromServices] IAuthService auth, UnifiedPOS.Application.DTOs.AuthDTO.SelectRegisterRequest model, CancellationToken ct) => auth.SelectRegisterAsync(model, ct);
    [HttpGet("bootstrap")] public Task<ResponseMessage> Bootstrap([FromQuery] Guid registerId, CancellationToken ct) => service.BootstrapAsync(registerId, ct);
    [HttpGet("changes")] public Task<ResponseMessage> Changes([FromQuery] Guid registerId, [FromQuery] DateTime? cursor, [FromQuery] Guid? afterId, CancellationToken ct) => service.ChangesAsync(registerId, cursor, afterId, ct);
    [HttpPost("sync")] public Task<ResponseMessage> Sync(CancellationToken ct) => service.SyncAsync(ct);
}
