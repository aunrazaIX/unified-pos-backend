using System.Linq.Expressions;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface ICatalogRepository
    {
        void AddCategory(Category entity);
        Task<DataListResponse<TResult>> PageCategoriesAsync<TResult>(Expression<Func<Category, bool>> filter, Expression<Func<Category, TResult>> select, Pagination pagination, CancellationToken ct = default);
        Task<bool> AnyCategoriesAsync(Expression<Func<Category, bool>> filter, CancellationToken ct = default);
        Task<DataListResponse<TResult>> PageProductsAsync<TResult>(Expression<Func<Product, bool>> filter, Expression<Func<Product, TResult>> select, Pagination pagination, CancellationToken ct = default);
        Task<Product?> FindProductAsync(Expression<Func<Product, bool>> filter, CancellationToken ct = default, bool tracked = true);
        Task<bool> AnyProductsAsync(Expression<Func<Product, bool>> filter, CancellationToken ct = default);
        void AddProduct(Product entity);
        Task<List<Product>> ListProductsAsync(Expression<Func<Product, bool>> filter, CancellationToken ct = default, bool tracked = false, int limit = 1000);
        Task<StoreStock?> FindStoreStockAsync(Expression<Func<StoreStock, bool>> filter, CancellationToken ct = default, bool tracked = true);
        void AddStoreStock(StoreStock entity);
        Task<List<StoreStock>> ListStocksAsync(Expression<Func<StoreStock, bool>> filter, CancellationToken ct = default, bool tracked = false, int limit = 1000);
    }
}
