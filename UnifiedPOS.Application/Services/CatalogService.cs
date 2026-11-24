using UnifiedPOS.Application.Interfaces.ServiceInterface;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using static UnifiedPOS.Application.DTOs.CatalogDTO;

namespace UnifiedPOS.Application.Services
{
    public class CatalogService(IUnitOfWork uow, IRequestContext request) : ServiceBase(uow, request), ICatalogService
    {
        public Task<ResponseMessage> AddCategoryAsync(CategoryRequest model, string key, CancellationToken ct) => MutateAsync("Category.Create", key, model, () =>
        {
            RequireManager();
            var category = new Category { OrganizationId = OrgId, Name = model.Name.Trim() };
            uow.CatalogRepository.AddCategory(category); Audit("Category.Created", category.Id);
            return Task.FromResult<object?>(new { category.Id, category.Name });
        }, ct);
        public async Task<ResponseMessage> GetCategoriesAsync(Pagination page, CancellationToken ct) => ResponseMessage.Ok(await uow.CatalogRepository.PageCategoriesAsync(x => x.OrganizationId == OrgId,
            x => new { x.Id, x.Name }, page, ct));
        public async Task<ResponseMessage> GetProductsAsync(Pagination page, CancellationToken ct) => ResponseMessage.Ok(await uow.CatalogRepository.PageProductsAsync(x => x.OrganizationId == OrgId && x.MergedIntoId == null &&
            (page.SearchString == null || x.Name.Contains(page.SearchString) || x.Sku.Contains(page.SearchString) || x.Barcode == page.SearchString),
            x => new ProductResponse(x.Id, x.CategoryId, x.ParentProductId, x.Name, x.Sku, x.Barcode, x.Price, x.MinimumAge, x.IsTaxable, x.IsActive, x.MergedIntoId), page, ct));
        public async Task<ResponseMessage> LookupAsync(string code, CancellationToken ct)
        {
            code = code.Trim().ToUpperInvariant();
            var product = await uow.CatalogRepository.FindProductAsync(x => x.OrganizationId == OrgId && (x.Sku == code || x.Barcode == code), ct, false) ?? throw PosException.Missing();
            if (product.MergedIntoId.HasValue) product = await uow.CatalogRepository.FindProductAsync(x => x.Id == product.MergedIntoId && x.OrganizationId == OrgId, ct, false) ?? throw PosException.Missing();
            return ResponseMessage.Ok(Map(product));
        }
        public Task<ResponseMessage> SaveProductAsync(Guid? id, ProductRequest model, string key, CancellationToken ct) => MutateAsync("Product.Save", key, new { id, model }, async () =>
        {
            if (id.HasValue) RequireManager();
            Money.Validate(model.Price);
            if (!await uow.CatalogRepository.AnyCategoriesAsync(x => x.Id == model.CategoryId && x.OrganizationId == OrgId, ct)) throw PosException.Invalid("Unknown category.");
            if (model.ParentProductId.HasValue && (!await uow.CatalogRepository.AnyProductsAsync(x => x.Id == model.ParentProductId && x.OrganizationId == OrgId && x.ParentProductId == null && x.MergedIntoId == null, ct) || model.ParentProductId == id)) throw PosException.Invalid("Invalid variant parent.");
            var sku = model.Sku.Trim().ToUpperInvariant();
            var barcode = string.IsNullOrWhiteSpace(model.Barcode) ? null : model.Barcode.Trim().ToUpperInvariant();
            if (await uow.CatalogRepository.AnyProductsAsync(x => x.OrganizationId == OrgId && x.Id != id && (x.Sku == sku || (barcode != null && x.Barcode == barcode)), ct)) throw PosException.Conflict("SKU or barcode already exists.");
            var name = model.Name.Trim();
            if (!model.ConfirmSimilarName && await uow.CatalogRepository.AnyProductsAsync(x => x.OrganizationId == OrgId && x.Id != id && x.MergedIntoId == null && (x.Name.Contains(name) || name.Contains(x.Name)), ct)) throw PosException.Conflict("Similar product name exists. Select it or explicitly confirm creation.");
            var product = id.HasValue ? await uow.CatalogRepository.FindProductAsync(x => x.Id == id && x.OrganizationId == OrgId, ct) ?? throw PosException.Missing() : new Product { OrganizationId = OrgId };
            if (product.MergedIntoId.HasValue) throw PosException.Conflict("Merged products cannot be edited.");
            product.Name = name; product.Sku = sku; product.Barcode = barcode; product.Price = model.Price; product.CategoryId = model.CategoryId;
            product.ParentProductId = model.ParentProductId; product.MinimumAge = model.MinimumAge; product.IsTaxable = model.IsTaxable; product.IsActive = model.IsActive;
            if (!id.HasValue) uow.CatalogRepository.AddProduct(product);
            Audit("Product.Saved", product.Id);
            return Map(product);
        }, ct);
        public async Task<ResponseMessage> GetStockAsync(Guid locationId, Guid productId, CancellationToken ct)
        {
            await LocationAsync(locationId, ct); await ProductAsync(productId, ct);
            var stock = await uow.CatalogRepository.FindStoreStockAsync(x => x.LocationId == locationId && x.ProductId == productId, ct, false);
            return ResponseMessage.Ok(new StockResponse(productId, locationId, stock?.Quantity ?? 0, stock?.IsBlocked ?? false));
        }
        public Task<ResponseMessage> AdjustStockAsync(Guid locationId, Guid productId, StockRequest model, string key, CancellationToken ct) => MutateAsync("Stock.Adjust", key, new { locationId, productId, model }, async () =>
        {
            RequireManager(); await LocationAsync(locationId, ct); var product = await ProductAsync(productId, ct);
            if (product.MergedIntoId.HasValue) throw PosException.Conflict("Adjust the merged target product.");
            if (decimal.Truncate(model.Adjustment) != model.Adjustment) throw PosException.Invalid("Stock adjustments require whole units.");
            var stock = await uow.CatalogRepository.FindStoreStockAsync(x => x.LocationId == locationId && x.ProductId == productId, ct);
            if (stock == null) { stock = new StoreStock { LocationId = locationId, ProductId = productId }; uow.CatalogRepository.AddStoreStock(stock); }
            if (stock.Quantity + model.Adjustment < 0) throw PosException.Invalid("Stock adjustment would make quantity negative.");
            stock.Quantity += model.Adjustment; stock.IsBlocked = model.IsBlocked;
            Audit("Stock.Adjusted", stock.Id, locationId, model.Reason);
            return new StockResponse(productId, locationId, stock.Quantity, stock.IsBlocked);
        }, ct);
        public Task<ResponseMessage> MergeAsync(MergeRequest model, string key, CancellationToken ct) => MutateAsync("Product.Merge", key, model, async () =>
        {
            RequireAdmin();
            if (model.SourceId == model.TargetId) throw PosException.Invalid("Source and target must differ.");
            var source = await ProductAsync(model.SourceId, ct); var target = await ProductAsync(model.TargetId, ct);
            if (source.MergedIntoId.HasValue || target.MergedIntoId.HasValue || !target.IsActive) throw PosException.Conflict("Only unmerged products and active targets are allowed.");
            if (source.MinimumAge != target.MinimumAge || source.IsTaxable != target.IsTaxable) throw PosException.Conflict("Products with different compliance or tax rules cannot be merged.");
            if (await uow.CatalogRepository.AnyProductsAsync(x => x.ParentProductId == source.Id, ct)) throw PosException.Conflict("Merge variants individually before their parent.");
            // Preserve immutable sale references. Inventory is transferred under the same transaction.
            var stocks = await uow.CatalogRepository.ListStocksAsync(x => x.ProductId == source.Id || x.ProductId == target.Id, ct, true, 10001);
            if (stocks.Count > 10000) throw PosException.Conflict("Merge requires a scheduled bulk operation for this catalog size.");
            foreach (var stock in stocks.Where(x => x.ProductId == source.Id))
            {
                var destination = stocks.SingleOrDefault(x => x.ProductId == target.Id && x.LocationId == stock.LocationId);
                if (destination == null) { destination = new StoreStock { LocationId = stock.LocationId, ProductId = target.Id }; uow.CatalogRepository.AddStoreStock(destination); }
                destination.Quantity += stock.Quantity; destination.IsBlocked |= stock.IsBlocked; stock.Quantity = 0;
            }
            var aliases = await uow.CatalogRepository.ListProductsAsync(x => x.OrganizationId == OrgId && x.MergedIntoId == source.Id, ct, true, 10001);
            if (aliases.Count > 10000) throw PosException.Conflict("Merge alias limit exceeded.");
            foreach (var alias in aliases) alias.MergedIntoId = target.Id;
            source.IsActive = false; source.MergedIntoId = target.Id;
            Audit("Product.Merged", source.Id, reason: model.Reason);
            return new { SourceId = source.Id, TargetId = target.Id };
        }, ct);
        private async Task<Product> ProductAsync(Guid id, CancellationToken ct) => await uow.CatalogRepository.FindProductAsync(x => x.Id == id && x.OrganizationId == OrgId, ct) ?? throw PosException.Missing();
        private static ProductResponse Map(Product x) => new(x.Id, x.CategoryId, x.ParentProductId, x.Name, x.Sku, x.Barcode, x.Price, x.MinimumAge, x.IsTaxable, x.IsActive, x.MergedIntoId);
    }
}
