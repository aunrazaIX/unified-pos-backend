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
        private IOrganizationRepository? _organizationRepository;
        public IOrganizationRepository OrganizationRepository => _organizationRepository ??= new OrganizationRepository(context);

        private IUserRepository? _userRepository;
        public IUserRepository UserRepository => _userRepository ??= new UserRepository(context);

        private ICatalogRepository? _catalogRepository;
        public ICatalogRepository CatalogRepository => _catalogRepository ??= new CatalogRepository(context);

        private ICustomerRepository? _customerRepository;
        public ICustomerRepository CustomerRepository => _customerRepository ??= new CustomerRepository(context);

        private IRegisterRepository? _registerRepository;
        public IRegisterRepository RegisterRepository => _registerRepository ??= new RegisterRepository(context);

        private ICheckoutRepository? _checkoutRepository;
        public ICheckoutRepository CheckoutRepository => _checkoutRepository ??= new CheckoutRepository(context);

        private ISalesRepository? _salesRepository;
        public ISalesRepository SalesRepository => _salesRepository ??= new SalesRepository(context);

        private IAuditRepository? _auditRepository;
        public IAuditRepository AuditRepository => _auditRepository ??= new AuditRepository(context);

        private IOperationRepository? _operationRepository;
        public IOperationRepository OperationRepository => _operationRepository ??= new OperationRepository(context);

        private IOutboxRepository? _outboxRepository;
        public IOutboxRepository OutboxRepository => _outboxRepository ??= new OutboxRepository(context);

        private IReportRepository? _reportRepository;
        public IReportRepository ReportRepository => _reportRepository ??= new ReportRepository(context);

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
