using System.ComponentModel.DataAnnotations;
using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.Application.DTOs
{
    public static class CheckoutDTO
    {
        public class CartRequest { public Guid ShiftId { get; set; } public Guid? CustomerId { get; set; } }
        public class LineRequest { public Guid ProductId { get; set; } [Range(0, 100000)] public int Quantity { get; set; } }
        public class DiscountRequest
        {
            [Range(typeof(decimal), "0", "100000000")] public decimal Amount { get; set; }
            [Required, StringLength(300)] public string Reason { get; set; } = "";
        }
        public class AgeRequest { public DateOnly BirthDate { get; set; } }
        public class AttachCustomerRequest { public Guid? CustomerId { get; set; } }
        public class PaymentRequest
        {
            [EnumDataType(typeof(TenderType))] public TenderType Tender { get; set; }
            [Range(typeof(decimal), "0.01", "100000000")] public decimal Amount { get; set; }
            [Range(typeof(decimal), "0.01", "100000000")] public decimal Received { get; set; }
        }
        public class RefundRequest
        {
            public Guid ShiftId { get; set; }
            [Required, StringLength(300)] public string Reason { get; set; } = "";
            [Required, MinLength(1), MaxLength(100)] public List<RefundLineRequest> Lines { get; set; } = [];
        }
        public class RefundLineRequest { public Guid SaleLineId { get; set; } [Range(1, 100000)] public int Quantity { get; set; } }
        public record CartResponse(Guid Id, Guid ShiftId, Guid? CustomerId, CartStatus Status, decimal Discount, IReadOnlyList<CartLineResponse> Lines);
        public record CartLineResponse(Guid Id, Guid ProductId, decimal Quantity);
        public record QuoteLine(Guid ProductId, Guid CategoryId, string Name, string Sku, decimal Quantity, decimal UnitPrice, decimal Discount, decimal TaxRate, decimal Tax, decimal Total);
        public record QuoteResponse(string Currency, decimal Subtotal, decimal Discount, decimal Tax, decimal Total, IReadOnlyList<QuoteLine> Lines);
        public record SaleLineResponse(Guid Id, Guid ProductId, string Name, string Sku, decimal Quantity, decimal UnitPrice, decimal Discount, decimal TaxRate, decimal Tax, decimal Total, decimal RefundedQuantity, decimal RefundedAmount);
        public record PaymentResponse(Guid Id, TenderType Tender, decimal Amount, decimal Received, decimal Change);
        public record SaleResponse(Guid Id, Guid LocationId, Guid RegisterId, Guid CashierId, Guid ShiftId, Guid? CustomerId, SaleStatus Status, string Currency, decimal Subtotal, decimal Discount, decimal Tax, decimal Total, decimal Paid, string? ReceiptNumber, DateTime? CompletedOn, IReadOnlyList<SaleLineResponse> Lines, IReadOnlyList<PaymentResponse> Payments);
        public record RefundResponse(Guid Id, Guid SaleId, decimal Amount);
        public record ReceiptResponse(SaleResponse Sale, string StoreName, string StoreAddress, string Header);
    }
}
