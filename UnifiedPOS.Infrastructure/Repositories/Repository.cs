using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure.Repositories
{
    public class Repository<T>(POSContext context) : IRepository<T> where T : Entity
    {
        private IQueryable<T> Query(bool tracked)
        {
            IQueryable<T> query = context.Set<T>();
            if (typeof(T) == typeof(User)) query = (IQueryable<T>)context.Set<User>().Include(x => x.Locations);
            if (typeof(T) == typeof(Cart)) query = (IQueryable<T>)context.Set<Cart>().Include(x => x.Lines);
            if (typeof(T) == typeof(Sale)) query = (IQueryable<T>)context.Set<Sale>().Include(x => x.Lines).Include(x => x.Payments).AsSplitQuery();
            return tracked ? query : query.AsNoTracking();
        }
        public Task<T?> FindAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default, bool tracked = true) => Query(tracked).SingleOrDefaultAsync(filter, ct);
        public Task<List<T>> ListAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default, bool tracked = false, int limit = 1000) => Query(tracked).Where(filter).OrderBy(x => x.Id).Take(limit).ToListAsync(ct);
        public Task<bool> AnyAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default) => context.Set<T>().AnyAsync(filter, ct);
        public async Task<DataListResponse<TResult>> PageAsync<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> select, Pagination pagination, CancellationToken ct = default)
        {
            var query = context.Set<T>().AsNoTracking().Where(filter);
            var count = await query.CountAsync(ct);
            var items = await query.OrderByDescending(x => x.CreatedOn).ThenBy(x => x.Id)
                .Skip((pagination.CurrentPage - 1) * pagination.PageSize).Take(pagination.PageSize).Select(select).ToListAsync(ct);
            return new(items, count, pagination.CurrentPage, pagination.PageSize);
        }
        public void Add(T entity) => context.Set<T>().Add(entity);
        public void Remove(T entity) => context.Set<T>().Remove(entity);
    }
}
