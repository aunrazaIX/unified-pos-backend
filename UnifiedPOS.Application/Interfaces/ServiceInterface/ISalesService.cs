using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.CheckoutDTO;
using static UnifiedPOS.Application.DTOs.RegisterDTO;

namespace UnifiedPOS.Application.Interfaces.ServiceInterface
{
    public interface ISalesService
    {
        Task<ResponseMessage> GetSaleAsync(Guid id, CancellationToken ct);
        Task<ResponseMessage> AddPaymentAsync(Guid id, PaymentRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> CompleteAsync(Guid id, string key, CancellationToken ct);
        Task<ResponseMessage> ReceiptAsync(Guid id, CancellationToken ct);
        Task<ResponseMessage> RefundAsync(Guid id, RefundRequest model, string key, CancellationToken ct);
        Task<ResponseMessage> VoidAsync(Guid id, ReasonRequest model, string key, CancellationToken ct);
    }
}
