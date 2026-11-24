using System.Linq.Expressions;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;
using static UnifiedPOS.Application.DTOs.ReportDTO;

namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface IRepository<T> where T : Entity
    {
        Task<T?> FindAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default, bool tracked = true);
        Task<List<T>> ListAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default, bool tracked = false, int limit = 1000);
        Task<DataListResponse<TResult>> PageAsync<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> select, Pagination pagination, CancellationToken ct = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>> filter, CancellationToken ct = default);
        void Add(T entity);
        void Remove(T entity);
    }
    public interface IReportRepository
    {
        Task<SummaryResponse> SummaryAsync(Guid organizationId, ReportFilter filter, CancellationToken ct);
        Task<DataListResponse<SaleReportRow>> SalesAsync(Guid organizationId, ReportFilter filter, CancellationToken ct);
        Task<decimal> CashBalanceAsync(Guid shiftId, CancellationToken ct);
        Task<decimal> RefundTotalAsync(Guid saleId, CancellationToken ct);
    }
    public interface IUnitOfWork
    {
        IRepository<Organization> Organizations { get; }
        IRepository<Location> Locations { get; }
        IRepository<Register> Registers { get; }
        IRepository<User> Users { get; }
        IRepository<Session> Sessions { get; }
        IRepository<Category> Categories { get; }
        IRepository<Product> Products { get; }
        IRepository<StoreStock> Stocks { get; }
        IRepository<Customer> Customers { get; }
        IRepository<Shift> Shifts { get; }
        IRepository<Cart> Carts { get; }
        IRepository<Sale> Sales { get; }
        IRepository<Refund> Refunds { get; }
        IRepository<AuditEvent> Audits { get; }
        IRepository<Operation> Operations { get; }
        IRepository<OutboxMessage> Outbox { get; }
        IRepository<DayClose> DayCloses { get; }
        IReportRepository Reports { get; }
        Task SaveChangesAsync(CancellationToken ct = default);
        Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken ct = default);
    }
}
