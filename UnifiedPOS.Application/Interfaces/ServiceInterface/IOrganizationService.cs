using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.OrganizationDTO;

namespace UnifiedPOS.Application.Interfaces.ServiceInterface
{
    public interface IOrganizationService
    {
        Task<ResponseMessage> GetLocationsAsync(Pagination page, CancellationToken ct);
        Task<ResponseMessage> SaveLocationAsync(Guid? id, LocationRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> GetRegistersAsync(Guid locationId, Pagination page, CancellationToken ct);
        Task<ResponseMessage> SaveRegisterAsync(Guid locationId, Guid? id, RegisterRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> GetReadinessAsync(Guid locationId, CancellationToken ct);
    }
}
