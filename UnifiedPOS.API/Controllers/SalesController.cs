using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.ServiceInterface;
using static UnifiedPOS.Application.DTOs.CheckoutDTO;
using static UnifiedPOS.Application.DTOs.RegisterDTO;

namespace UnifiedPOS.API.Controllers;

[ApiController, Authorize, Route("api/v1/sales")]
public sealed class SalesController(ISalesService service) : ControllerBase
{
    [HttpGet("{id:guid}")] public Task<ResponseMessage> Get(Guid id, CancellationToken ct) => service.GetSaleAsync(id, ct);
    [HttpPost("{id:guid}/payments")] public Task<ResponseMessage> Payment(Guid id, PaymentRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.AddPaymentAsync(id, model, key, ct);
    [HttpPost("{id:guid}/complete")] public Task<ResponseMessage> Complete(Guid id, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.CompleteAsync(id, key, ct);
    [HttpGet("{id:guid}/receipt")] public Task<ResponseMessage> Receipt(Guid id, CancellationToken ct) => service.ReceiptAsync(id, ct);
    [Authorize(Roles = Roles.Managers), HttpPost("{id:guid}/refunds")] public Task<ResponseMessage> Refund(Guid id, RefundRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.RefundAsync(id, model, key, ct);
    [Authorize(Roles = Roles.Managers), HttpPost("{id:guid}/void")] public Task<ResponseMessage> Void(Guid id, ReasonRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.VoidAsync(id, model, key, ct);
}
