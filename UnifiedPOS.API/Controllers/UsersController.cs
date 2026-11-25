using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.ServiceInterface;
using static UnifiedPOS.Application.DTOs.AuthDTO;

namespace UnifiedPOS.API.Controllers;

[ApiController, Authorize(Roles = Roles.Administrators), Route("api/v1/users")]
public sealed class UsersController(IUserService service) : ControllerBase
{
    [HttpPut("{id:guid}/registers")] public Task<ResponseMessage> SetRegisters(Guid id, RegistersRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.SetRegistersAsync(id, model, key, ct);
    [HttpGet] public Task<ResponseMessage> GetAll([FromQuery] Pagination page, CancellationToken ct) => service.GetAllUsersAsync(page, ct);
    [HttpPost] public Task<ResponseMessage> Add(UserRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.AddUserAsync(model, key, ct);
    [HttpPut("{id:guid}/locations")] public Task<ResponseMessage> SetLocations(Guid id, LocationsRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.SetLocationsAsync(id, model, key, ct);
    [HttpPut("{id:guid}/pin")] public Task<ResponseMessage> ResetPin(Guid id, ResetPinRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.ResetPinAsync(id, model, key, ct);
}
