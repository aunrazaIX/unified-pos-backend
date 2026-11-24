using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.CatalogDTO;

namespace UnifiedPOS.Application.Interfaces.ServiceInterface
{
    public interface IPosService
    {
        Task<ResponseMessage> BootstrapAsync(Guid registerId, CancellationToken ct);
        Task<ResponseMessage> ChangesAsync(Guid registerId, DateTime? cursor, Guid? afterId, CancellationToken ct);
        Task<ResponseMessage> SyncAsync(CancellationToken ct);
    }
}
