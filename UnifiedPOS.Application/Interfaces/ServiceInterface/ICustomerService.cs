using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.CustomerDTO;
using static UnifiedPOS.Application.DTOs.ReportDTO;

namespace UnifiedPOS.Application.Interfaces.ServiceInterface
{
    public interface ICustomerService
    {
        Task<ResponseMessage> GetCustomersAsync(Pagination page, CancellationToken ct);
        Task<ResponseMessage> GetCustomerAsync(Guid id, CancellationToken ct);
        Task<ResponseMessage> SaveCustomerAsync(Guid? id, CustomerRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> GetSalesAsync(Guid id, Guid locationId, Pagination page, CancellationToken ct);
    }
}
