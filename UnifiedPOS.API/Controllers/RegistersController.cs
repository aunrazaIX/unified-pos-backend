using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.ServiceInterface;
using static UnifiedPOS.Application.DTOs.RegisterDTO;

namespace UnifiedPOS.API.Controllers;

[ApiController, Authorize, Route("api/v1/shifts")]
public sealed class RegistersController(IRegisterService service) : ControllerBase
{
    [HttpPost] public Task<ResponseMessage> Open(OpenShiftRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.OpenShiftAsync(model, key, ct);
    [HttpGet("current")] public Task<ResponseMessage> Current([FromQuery] Guid registerId, CancellationToken ct) => service.CurrentShiftAsync(registerId, ct);
    [HttpPost("{id:guid}/close")] public Task<ResponseMessage> Close(Guid id, CloseShiftRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.CloseShiftAsync(id, model, key, ct);
    [Authorize(Roles = Roles.Managers), HttpPost("{id:guid}/review")] public Task<ResponseMessage> Review(Guid id, ReasonRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.ReviewShiftAsync(id, model, key, ct);
}
