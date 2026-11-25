using UnifiedPOS.Application.Common;
using static UnifiedPOS.Application.DTOs.AuthDTO;

namespace UnifiedPOS.Application.Interfaces.ServiceInterface
{
    public interface IAuthService
    {
        Task<ResponseMessage> LoginAsync(LoginRequest model, CancellationToken ct);
        Task<ResponseMessage> PinAsync(PinRequest model, CancellationToken ct);
        Task<ResponseMessage> RefreshAsync(RefreshRequest model, CancellationToken ct);
        Task<ResponseMessage> LogoutAsync(CancellationToken ct);
        Task<ResponseMessage> AvailableRegistersAsync(CancellationToken ct);
        Task<ResponseMessage> SelectRegisterAsync(SelectRegisterRequest model, CancellationToken ct);
    }
}
