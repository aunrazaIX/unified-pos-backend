using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.ServiceInterface;
using static UnifiedPOS.Application.DTOs.CustomerDTO;

namespace UnifiedPOS.API.Controllers;

[ApiController, Authorize, Route("api/v1/customers")]
public sealed class CustomersController(ICustomerService service) : ControllerBase
{
    [HttpGet] public Task<ResponseMessage> GetAll([FromQuery] Pagination page, CancellationToken ct) => service.GetCustomersAsync(page, ct);
    [HttpGet("{id:guid}")] public Task<ResponseMessage> Get(Guid id, CancellationToken ct) => service.GetCustomerAsync(id, ct);
    [HttpPost] public Task<ResponseMessage> Add(CustomerRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.SaveCustomerAsync(null, model, key, ct);
    [HttpPut("{id:guid}")] public Task<ResponseMessage> Update(Guid id, CustomerRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.SaveCustomerAsync(id, model, key, ct);
    [HttpGet("{id:guid}/sales")] public Task<ResponseMessage> Sales(Guid id, [FromQuery] Guid locationId, [FromQuery] Pagination page, CancellationToken ct) => service.GetSalesAsync(id, locationId, page, ct);
}
