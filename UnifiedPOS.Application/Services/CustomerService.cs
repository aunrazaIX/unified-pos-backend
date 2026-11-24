using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.CustomerDTO;
using static UnifiedPOS.Application.DTOs.ReportDTO;

namespace UnifiedPOS.Application.Services
{
    public class CustomerService(IUnitOfWork uow, IRequestContext request) : ServiceBase(uow, request)
    {
        public async Task<ResponseMessage> GetCustomersAsync(Pagination page, CancellationToken ct) => ResponseMessage.Ok(await uow.Customers.PageAsync(x => x.OrganizationId == OrgId &&
            (page.SearchString == null || x.Name.Contains(page.SearchString) || (x.Phone != null && x.Phone.Contains(page.SearchString))), x => new CustomerResponse(x.Id, x.Name, x.Phone), page, ct));
        public async Task<ResponseMessage> GetCustomerAsync(Guid id, CancellationToken ct)
        {
            var customer = await uow.Customers.FindAsync(x => x.Id == id && x.OrganizationId == OrgId, ct, false) ?? throw PosException.Missing();
            return ResponseMessage.Ok(new CustomerResponse(customer.Id, customer.Name, customer.Phone));
        }
        public Task<ResponseMessage> SaveCustomerAsync(Guid? id, CustomerRequest model, string key, CancellationToken ct) => MutateAsync("Customer.Save", key, new { id, model }, async () =>
        {
            var customer = id.HasValue ? await uow.Customers.FindAsync(x => x.Id == id && x.OrganizationId == OrgId, ct) ?? throw PosException.Missing() : new Customer { OrganizationId = OrgId };
            customer.Name = model.Name.Trim(); customer.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone;
            if (!id.HasValue) uow.Customers.Add(customer);
            Audit("Customer.Saved", customer.Id);
            return new CustomerResponse(customer.Id, customer.Name, customer.Phone);
        }, ct);
        public async Task<ResponseMessage> GetSalesAsync(Guid id, Guid locationId, Pagination page, CancellationToken ct)
        {
            await EnsureCustomerAsync(id, ct); await LocationAsync(locationId, ct);
            var cashier = request.Role == UserRole.Cashier;
            return ResponseMessage.Ok(await uow.Sales.PageAsync(x => x.OrganizationId == OrgId && x.CustomerId == id && x.LocationId == locationId && (!cashier || x.CashierId == ActorId),
                x => new SaleReportRow(x.Id, x.ReceiptNumber, x.CompletedOn, x.BusinessDate, x.RegisterId, x.CashierId, x.Status, x.Total, x.Discount, x.Tax), page, ct));
        }
    }
}
