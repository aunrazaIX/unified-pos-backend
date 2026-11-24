using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.ServiceInterface;
using static UnifiedPOS.Application.DTOs.OrganizationDTO;

namespace UnifiedPOS.API.Controllers;

[ApiController, Authorize, Route("api/v1")]
public sealed class OrganizationsController(IOrganizationService service) : ControllerBase
{
    [HttpGet("locations")] public Task<ResponseMessage> Locations([FromQuery] Pagination page, CancellationToken ct) => service.GetLocationsAsync(page, ct);
    [Authorize(Roles = Roles.Administrators), HttpPost("locations")] public Task<ResponseMessage> AddLocation(LocationRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.SaveLocationAsync(null, model, key, ct);
    [Authorize(Roles = Roles.Administrators), HttpPut("locations/{id:guid}")] public Task<ResponseMessage> UpdateLocation(Guid id, LocationRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.SaveLocationAsync(id, model, key, ct);
    [HttpGet("locations/{locationId:guid}/registers")] public Task<ResponseMessage> Registers(Guid locationId, [FromQuery] Pagination page, CancellationToken ct) => service.GetRegistersAsync(locationId, page, ct);
    [Authorize(Roles = Roles.Administrators), HttpPost("locations/{locationId:guid}/registers")] public Task<ResponseMessage> AddRegister(Guid locationId, RegisterRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.SaveRegisterAsync(locationId, null, model, key, ct);
    [Authorize(Roles = Roles.Administrators), HttpPut("locations/{locationId:guid}/registers/{id:guid}")] public Task<ResponseMessage> UpdateRegister(Guid locationId, Guid id, RegisterRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.SaveRegisterAsync(locationId, id, model, key, ct);
    [HttpGet("locations/{locationId:guid}/readiness")] public Task<ResponseMessage> Readiness(Guid locationId, CancellationToken ct) => service.GetReadinessAsync(locationId, ct);
}
