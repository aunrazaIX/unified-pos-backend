namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface IUnitOfWork
    {
        IOrganizationRepository OrganizationRepository { get; }
        IUserRepository UserRepository { get; }
        ICatalogRepository CatalogRepository { get; }
        ICustomerRepository CustomerRepository { get; }
        IRegisterRepository RegisterRepository { get; }
        ICheckoutRepository CheckoutRepository { get; }
        ISalesRepository SalesRepository { get; }
        IAuditRepository AuditRepository { get; }
        IOperationRepository OperationRepository { get; }
        IOutboxRepository OutboxRepository { get; }
        IReportRepository ReportRepository { get; }
        Task SaveChangesAsync(CancellationToken ct = default);
        Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken ct = default);
    }
}
