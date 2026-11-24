using System.Data;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Infrastructure.Databases;
using UnifiedPOS.Infrastructure.Repositories;

namespace UnifiedPOS.Infrastructure
{
    public class UnitOfWork(POSContext context) : IUnitOfWork
    {
        public IRepository<Organization> Organizations { get; } = new Repository<Organization>(context);
        public IRepository<Location> Locations { get; } = new Repository<Location>(context);
        public IRepository<Register> Registers { get; } = new Repository<Register>(context);
        public IRepository<User> Users { get; } = new Repository<User>(context);
        public IRepository<Session> Sessions { get; } = new Repository<Session>(context);
        public IRepository<Category> Categories { get; } = new Repository<Category>(context);
        public IRepository<Product> Products { get; } = new Repository<Product>(context);
        public IRepository<StoreStock> Stocks { get; } = new Repository<StoreStock>(context);
        public IRepository<Customer> Customers { get; } = new Repository<Customer>(context);
        public IRepository<Shift> Shifts { get; } = new Repository<Shift>(context);
        public IRepository<Cart> Carts { get; } = new Repository<Cart>(context);
        public IRepository<Sale> Sales { get; } = new Repository<Sale>(context);
        public IRepository<Refund> Refunds { get; } = new Repository<Refund>(context);
        public IRepository<AuditEvent> Audits { get; } = new Repository<AuditEvent>(context);
        public IRepository<Operation> Operations { get; } = new Repository<Operation>(context);
        public IRepository<OutboxMessage> Outbox { get; } = new Repository<OutboxMessage>(context);
        public IRepository<DayClose> DayCloses { get; } = new Repository<DayClose>(context);
        public IReportRepository Reports { get; } = new ReportRepository(context);
        public async Task SaveChangesAsync(CancellationToken ct = default) => await context.SaveChangesAsync(ct);
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
        {
            var strategy = context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                context.ChangeTracker.Clear();
                await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
                var result = await action();
                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return result;
            });
        }
    }
}
