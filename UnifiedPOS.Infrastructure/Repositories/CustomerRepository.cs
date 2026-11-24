using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure.Repositories
{
    public class CustomerRepository(POSContext context) : ICustomerRepository
    {
        private IQueryable<Customer> CustomersQuery(bool tracked)
        {
            IQueryable<Customer> query = context.Set<Customer>();
            return tracked ? query : query.AsNoTracking();
        }

        public Task<DataListResponse<TResult>> PageCustomersAsync<TResult>(Expression<Func<Customer, bool>> filter, Expression<Func<Customer, TResult>> select, Pagination pagination, CancellationToken ct = default) => RepositoryQuery.PageAsync(context.Set<Customer>(), filter, select, pagination, ct);

        public Task<Customer?> FindCustomerAsync(Expression<Func<Customer, bool>> filter, CancellationToken ct = default, bool tracked = true) => CustomersQuery(tracked).SingleOrDefaultAsync(filter, ct);

        public void AddCustomer(Customer entity) => context.Set<Customer>().Add(entity);

        public Task<bool> AnyCustomersAsync(Expression<Func<Customer, bool>> filter, CancellationToken ct = default) => context.Set<Customer>().AnyAsync(filter, ct);
    }
}
