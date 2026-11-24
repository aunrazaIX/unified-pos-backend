using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure.Repositories
{
    public class CatalogRepository(POSContext context) : ICatalogRepository
    {
        public void AddCategory(Category entity) => context.Set<Category>().Add(entity);

        public Task<DataListResponse<TResult>> PageCategoriesAsync<TResult>(Expression<Func<Category, bool>> filter, Expression<Func<Category, TResult>> select, Pagination pagination, CancellationToken ct = default) => RepositoryQuery.PageAsync(context.Set<Category>(), filter, select, pagination, ct);

        public Task<bool> AnyCategoriesAsync(Expression<Func<Category, bool>> filter, CancellationToken ct = default) => context.Set<Category>().AnyAsync(filter, ct);

        private IQueryable<Product> ProductsQuery(bool tracked)
        {
            IQueryable<Product> query = context.Set<Product>();
            return tracked ? query : query.AsNoTracking();
        }

        public Task<DataListResponse<TResult>> PageProductsAsync<TResult>(Expression<Func<Product, bool>> filter, Expression<Func<Product, TResult>> select, Pagination pagination, CancellationToken ct = default) => RepositoryQuery.PageAsync(context.Set<Product>(), filter, select, pagination, ct);

        public Task<Product?> FindProductAsync(Expression<Func<Product, bool>> filter, CancellationToken ct = default, bool tracked = true) => ProductsQuery(tracked).SingleOrDefaultAsync(filter, ct);

        public Task<bool> AnyProductsAsync(Expression<Func<Product, bool>> filter, CancellationToken ct = default) => context.Set<Product>().AnyAsync(filter, ct);

        public void AddProduct(Product entity) => context.Set<Product>().Add(entity);

        public Task<List<Product>> ListProductsAsync(Expression<Func<Product, bool>> filter, CancellationToken ct = default, bool tracked = false, int limit = 1000) => ProductsQuery(tracked).Where(filter).OrderBy(x => x.Id).Take(limit).ToListAsync(ct);

        private IQueryable<StoreStock> StocksQuery(bool tracked)
        {
            IQueryable<StoreStock> query = context.Set<StoreStock>();
            return tracked ? query : query.AsNoTracking();
        }

        public Task<StoreStock?> FindStoreStockAsync(Expression<Func<StoreStock, bool>> filter, CancellationToken ct = default, bool tracked = true) => StocksQuery(tracked).SingleOrDefaultAsync(filter, ct);

        public void AddStoreStock(StoreStock entity) => context.Set<StoreStock>().Add(entity);

        public Task<List<StoreStock>> ListStocksAsync(Expression<Func<StoreStock, bool>> filter, CancellationToken ct = default, bool tracked = false, int limit = 1000) => StocksQuery(tracked).Where(filter).OrderBy(x => x.Id).Take(limit).ToListAsync(ct);
    }
}
