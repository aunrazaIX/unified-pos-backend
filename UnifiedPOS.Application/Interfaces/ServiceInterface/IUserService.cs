using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.AuthDTO;

namespace UnifiedPOS.Application.Interfaces.ServiceInterface
{
    public interface IUserService
    {
        Task<ResponseMessage> GetAllUsersAsync(Pagination page, CancellationToken ct);
        Task<ResponseMessage> AddUserAsync(UserRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> SetLocationsAsync(Guid id, LocationsRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> ResetPinAsync(Guid id, ResetPinRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> SetRegistersAsync(Guid id, RegistersRequest model, string key, CancellationToken ct);
    }
}
