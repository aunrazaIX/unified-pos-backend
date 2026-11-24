using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure.Repositories
{
    public class CheckoutRepository(POSContext context) : ICheckoutRepository
    {
        private IQueryable<Cart> CartsQuery(bool tracked)
        {
            IQueryable<Cart> query = context.Set<Cart>().Include(x => x.Lines);
            return tracked ? query : query.AsNoTracking();
        }

        public void AddCart(Cart entity) => context.Set<Cart>().Add(entity);

        public Task<DataListResponse<TResult>> PageCartsAsync<TResult>(Expression<Func<Cart, bool>> filter, Expression<Func<Cart, TResult>> select, Pagination pagination, CancellationToken ct = default) => RepositoryQuery.PageAsync(context.Set<Cart>(), filter, select, pagination, ct);

        public Task<Cart?> FindCartAsync(Expression<Func<Cart, bool>> filter, CancellationToken ct = default, bool tracked = true) => CartsQuery(tracked).SingleOrDefaultAsync(filter, ct);
    }
}
