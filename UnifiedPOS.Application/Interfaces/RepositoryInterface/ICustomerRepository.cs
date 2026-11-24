using System.Linq.Expressions;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface ICustomerRepository
    {
        Task<DataListResponse<TResult>> PageCustomersAsync<TResult>(Expression<Func<Customer, bool>> filter, Expression<Func<Customer, TResult>> select, Pagination pagination, CancellationToken ct = default);
        Task<Customer?> FindCustomerAsync(Expression<Func<Customer, bool>> filter, CancellationToken ct = default, bool tracked = true);
        void AddCustomer(Customer entity);
        Task<bool> AnyCustomersAsync(Expression<Func<Customer, bool>> filter, CancellationToken ct = default);
    }
}
