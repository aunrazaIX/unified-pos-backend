using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.CheckoutDTO;

namespace UnifiedPOS.Application.Interfaces.ServiceInterface
{
    public interface ICheckoutService
    {
        Task<ResponseMessage> CreateCartAsync(CartRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> GetCartAsync(Guid id, CancellationToken ct);
        Task<ResponseMessage> GetHeldAsync(Guid shiftId, Pagination page, CancellationToken ct);
        Task<ResponseMessage> SetLineAsync(Guid id, LineRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> HoldAsync(Guid id, string key, CancellationToken ct);
        Task<ResponseMessage> RecallAsync(Guid id, string key, CancellationToken ct);
        Task<ResponseMessage> DiscountAsync(Guid id, DiscountRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> VerifyAgeAsync(Guid id, AgeRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> AttachCustomerAsync(Guid id, AttachCustomerRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> GetQuoteAsync(Guid id, CancellationToken ct);
        Task<ResponseMessage> CheckoutAsync(Guid id, string key, CancellationToken ct);
    }
}
