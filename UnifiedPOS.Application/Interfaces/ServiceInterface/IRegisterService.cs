using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.RegisterDTO;

namespace UnifiedPOS.Application.Interfaces.ServiceInterface
{
    public interface IRegisterService
    {
        Task<ResponseMessage> OpenShiftAsync(OpenShiftRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> CurrentShiftAsync(Guid registerId, CancellationToken ct);
        Task<ResponseMessage> CloseShiftAsync(Guid id, CloseShiftRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> ReviewShiftAsync(Guid id, ReasonRequest model, string key, CancellationToken ct);
    }
}
