using System.Linq.Expressions;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface ISalesRepository
    {
        void AddSale(Sale entity);
        Task<DataListResponse<TResult>> PageSalesAsync<TResult>(Expression<Func<Sale, bool>> filter, Expression<Func<Sale, TResult>> select, Pagination pagination, CancellationToken ct = default);
        Task<bool> AnySalesAsync(Expression<Func<Sale, bool>> filter, CancellationToken ct = default);
        Task<Sale?> FindSaleAsync(Expression<Func<Sale, bool>> filter, CancellationToken ct = default, bool tracked = true);
        void AddRefund(Refund entity);
    }
}
