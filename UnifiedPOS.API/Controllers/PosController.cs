using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.ServiceInterface;

namespace UnifiedPOS.API.Controllers;

[ApiController, Authorize, Route("api/v1/pos")]
public sealed class PosController(IPosService service) : ControllerBase
{
    [HttpGet("bootstrap")] public Task<ResponseMessage> Bootstrap([FromQuery] Guid registerId, CancellationToken ct) => service.BootstrapAsync(registerId, ct);
    [HttpGet("changes")] public Task<ResponseMessage> Changes([FromQuery] Guid registerId, [FromQuery] DateTime? cursor, [FromQuery] Guid? afterId, CancellationToken ct) => service.ChangesAsync(registerId, cursor, afterId, ct);
    [HttpPost("sync")] public Task<ResponseMessage> Sync(CancellationToken ct) => service.SyncAsync(ct);
}
