using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.ServiceInterface;
using static UnifiedPOS.Application.DTOs.CheckoutDTO;

namespace UnifiedPOS.API.Controllers;

[ApiController, Authorize, Route("api/v1/carts")]
public sealed class CartsController(ICheckoutService service) : ControllerBase
{
    [HttpPost] public Task<ResponseMessage> Create(CartRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.CreateCartAsync(model, key, ct);
    [HttpGet("{id:guid}")] public Task<ResponseMessage> Get(Guid id, CancellationToken ct) => service.GetCartAsync(id, ct);
    [HttpGet("held")] public Task<ResponseMessage> Held([FromQuery] Guid shiftId, [FromQuery] Pagination page, CancellationToken ct) => service.GetHeldAsync(shiftId, page, ct);
    [HttpPut("{id:guid}/lines")] public Task<ResponseMessage> SetLine(Guid id, LineRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.SetLineAsync(id, model, key, ct);
    [HttpPost("{id:guid}/hold")] public Task<ResponseMessage> Hold(Guid id, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.HoldAsync(id, key, ct);
    [HttpPost("{id:guid}/recall")] public Task<ResponseMessage> Recall(Guid id, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.RecallAsync(id, key, ct);
    [Authorize(Roles = Roles.Managers), HttpPost("{id:guid}/discount")] public Task<ResponseMessage> Discount(Guid id, DiscountRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.DiscountAsync(id, model, key, ct);
    [HttpPost("{id:guid}/age-verification")] public Task<ResponseMessage> VerifyAge(Guid id, AgeRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.VerifyAgeAsync(id, model, key, ct);
    [HttpPut("{id:guid}/customer")] public Task<ResponseMessage> AttachCustomer(Guid id, AttachCustomerRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.AttachCustomerAsync(id, model, key, ct);
    [HttpGet("{id:guid}/quote")] public Task<ResponseMessage> Quote(Guid id, CancellationToken ct) => service.GetQuoteAsync(id, ct);
    [HttpPost("{id:guid}/checkout")] public Task<ResponseMessage> Checkout(Guid id, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.CheckoutAsync(id, key, ct);
}
