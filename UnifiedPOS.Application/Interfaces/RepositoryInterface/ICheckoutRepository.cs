using System.Linq.Expressions;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface ICheckoutRepository
    {
        void AddCart(Cart entity);
        Task<DataListResponse<TResult>> PageCartsAsync<TResult>(Expression<Func<Cart, bool>> filter, Expression<Func<Cart, TResult>> select, Pagination pagination, CancellationToken ct = default);
        Task<Cart?> FindCartAsync(Expression<Func<Cart, bool>> filter, CancellationToken ct = default, bool tracked = true);
    }
}
