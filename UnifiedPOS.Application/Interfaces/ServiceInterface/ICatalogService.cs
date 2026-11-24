using UnifiedPOS.Application.Common;
using static UnifiedPOS.Application.DTOs.CatalogDTO;

namespace UnifiedPOS.Application.Interfaces.ServiceInterface
{
    public interface ICatalogService
    {
        Task<ResponseMessage> AddCategoryAsync(CategoryRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> GetCategoriesAsync(Pagination page, CancellationToken ct);
        Task<ResponseMessage> GetProductsAsync(Pagination page, CancellationToken ct);
        Task<ResponseMessage> LookupAsync(string code, CancellationToken ct);
        Task<ResponseMessage> SaveProductAsync(Guid? id, ProductRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> GetStockAsync(Guid locationId, Guid productId, CancellationToken ct);
        Task<ResponseMessage> AdjustStockAsync(Guid locationId, Guid productId, StockRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> MergeAsync(MergeRequest model, string key, CancellationToken ct);
    }
}
