using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.ServiceInterface;
using static UnifiedPOS.Application.DTOs.CatalogDTO;

namespace UnifiedPOS.API.Controllers;

[ApiController, Authorize, Route("api/v1/catalog")]
public sealed class CatalogController(ICatalogService service) : ControllerBase
{
    [Authorize(Roles = Roles.Managers), HttpGet("review-queue")] public Task<ResponseMessage> ReviewQueue([FromQuery] Pagination page, CancellationToken ct) => service.GetReviewQueueAsync(page, ct);
    [Authorize(Roles = Roles.Managers), HttpPost("products/{id:guid}/review")] public Task<ResponseMessage> Review(Guid id, ReviewRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.ReviewProductAsync(id, model, key, ct);
    [HttpGet("categories")] public Task<ResponseMessage> Categories([FromQuery] Pagination page, CancellationToken ct) => service.GetCategoriesAsync(page, ct);
    [Authorize(Roles = Roles.Managers), HttpPost("categories")] public Task<ResponseMessage> AddCategory(CategoryRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.AddCategoryAsync(model, key, ct);
    [HttpGet("products")] public Task<ResponseMessage> Products([FromQuery] Pagination page, CancellationToken ct) => service.GetProductsAsync(page, ct);
    [HttpGet("lookup/{code}")] public Task<ResponseMessage> Lookup(string code, CancellationToken ct) => service.LookupAsync(code, ct);
    [HttpPost("products")] public Task<ResponseMessage> AddProduct(ProductRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.SaveProductAsync(null, model, key, ct);
    [Authorize(Roles = Roles.Managers), HttpPut("products/{id:guid}")] public Task<ResponseMessage> UpdateProduct(Guid id, ProductRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.SaveProductAsync(id, model, key, ct);
    [HttpGet("locations/{locationId:guid}/products/{productId:guid}/stock")] public Task<ResponseMessage> Stock(Guid locationId, Guid productId, CancellationToken ct) => service.GetStockAsync(locationId, productId, ct);
    [Authorize(Roles = Roles.Managers), HttpPost("locations/{locationId:guid}/products/{productId:guid}/stock-adjustments")] public Task<ResponseMessage> Adjust(Guid locationId, Guid productId, StockRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.AdjustStockAsync(locationId, productId, model, key, ct);
    [Authorize(Roles = Roles.Administrators), HttpPost("products/merge")] public Task<ResponseMessage> Merge(MergeRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.MergeAsync(model, key, ct);
}
