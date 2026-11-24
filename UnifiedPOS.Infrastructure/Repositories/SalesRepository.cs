using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure.Repositories
{
    public class SalesRepository(POSContext context) : ISalesRepository
    {
        private IQueryable<Sale> SalesQuery(bool tracked)
        {
            IQueryable<Sale> query = context.Set<Sale>().Include(x => x.Lines).Include(x => x.Payments).AsSplitQuery();
            return tracked ? query : query.AsNoTracking();
        }

        public void AddSale(Sale entity) => context.Set<Sale>().Add(entity);

        public Task<DataListResponse<TResult>> PageSalesAsync<TResult>(Expression<Func<Sale, bool>> filter, Expression<Func<Sale, TResult>> select, Pagination pagination, CancellationToken ct = default) => RepositoryQuery.PageAsync(context.Set<Sale>(), filter, select, pagination, ct);

        public Task<bool> AnySalesAsync(Expression<Func<Sale, bool>> filter, CancellationToken ct = default) => context.Set<Sale>().AnyAsync(filter, ct);

        public Task<Sale?> FindSaleAsync(Expression<Func<Sale, bool>> filter, CancellationToken ct = default, bool tracked = true) => SalesQuery(tracked).SingleOrDefaultAsync(filter, ct);

        public void AddRefund(Refund entity) => context.Set<Refund>().Add(entity);
    }
}
