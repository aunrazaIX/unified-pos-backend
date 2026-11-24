using System.ComponentModel.DataAnnotations;
using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.Application.DTOs
{
    public static class AuthDTO
    {
        public class LoginRequest
        {
            public Guid OrganizationId { get; set; }
            [Required, StringLength(100)] public string UserName { get; set; } = "";
            [Required, StringLength(128, MinimumLength = 12)] public string Password { get; set; } = "";
        }
        public class PinRequest
        {
            public Guid OrganizationId { get; set; }
            public Guid RegisterId { get; set; }
            [Required, StringLength(100)] public string UserName { get; set; } = "";
            [Required, RegularExpression(@"^\d{6,12}$")] public string Pin { get; set; } = "";
        }
        public class RefreshRequest
        {
            [Required, StringLength(128)] public string RefreshToken { get; set; } = "";
        }
        public class UserRequest
        {
            [Required, StringLength(100, MinimumLength = 3)] public string UserName { get; set; } = "";
            [Required, StringLength(128, MinimumLength = 12)] public string Password { get; set; } = "";
            [EnumDataType(typeof(UserRole))] public UserRole Role { get; set; }
            [MaxLength(100)] public Guid[] LocationIds { get; set; } = [];
        }
        public class LocationsRequest { [MaxLength(100)] public Guid[] LocationIds { get; set; } = []; }
        public class ResetPinRequest
        {
            [Required, RegularExpression(@"^\d{6,12}$")] public string Pin { get; set; } = "";
        }
        public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresOn);
        public record UserResponse(Guid Id, string UserName, UserRole Role, bool IsActive, Guid[] LocationIds);
    }
    public static class OrganizationDTO
    {
        public class LocationRequest
        {
            [Required, StringLength(100)] public string Name { get; set; } = "";
            [Required, StringLength(300)] public string Address { get; set; } = "";
            [Required, StringLength(100)] public string TimeZoneId { get; set; } = "UTC";
            [StringLength(500)] public string ReceiptHeader { get; set; } = "";
            [StringLength(1000)] public string Hours { get; set; } = "";
            [Range(typeof(decimal), "0", "1")] public decimal TaxRate { get; set; }
            public bool IsActive { get; set; }
        }
        public class RegisterRequest { [Required, StringLength(100)] public string Name { get; set; } = ""; public bool IsActive { get; set; } = true; }
        public record LocationResponse(Guid Id, string Name, string Address, string TimeZoneId, string ReceiptHeader, string Hours, decimal TaxRate, bool IsActive);
        public record RegisterResponse(Guid Id, Guid LocationId, string Name, bool IsActive);
    }
    public static class CatalogDTO
    {
        public class CategoryRequest { [Required, StringLength(100)] public string Name { get; set; } = ""; }
        public class ProductRequest
        {
            public Guid CategoryId { get; set; }
            public Guid? ParentProductId { get; set; }
            [Required, StringLength(200)] public string Name { get; set; } = "";
            [Required, StringLength(100)] public string Sku { get; set; } = "";
            [StringLength(100)] public string? Barcode { get; set; }
            [Range(typeof(decimal), "0", "1000000")] public decimal Price { get; set; }
            [Range(0, 100)] public int MinimumAge { get; set; }
            public bool IsTaxable { get; set; } = true;
            public bool ConfirmSimilarName { get; set; }
            public bool IsActive { get; set; } = true;
        }
        public class StockRequest
        {
            [Range(typeof(decimal), "-1000000", "1000000")] public decimal Adjustment { get; set; }
            public bool IsBlocked { get; set; }
            [Required, StringLength(300)] public string Reason { get; set; } = "";
        }
        public class MergeRequest
        {
            public Guid SourceId { get; set; }
            public Guid TargetId { get; set; }
            [Required, StringLength(300)] public string Reason { get; set; } = "";
        }
        public record ProductResponse(Guid Id, Guid CategoryId, Guid? ParentProductId, string Name, string Sku, string? Barcode, decimal Price, int MinimumAge, bool IsTaxable, bool IsActive, Guid? MergedIntoId);
        public record StockResponse(Guid ProductId, Guid LocationId, decimal Quantity, bool IsBlocked);
    }
    public static class CustomerDTO
    {
        public class CustomerRequest
        {
            [Required, StringLength(200)] public string Name { get; set; } = "";
            [StringLength(20), RegularExpression(@"^\+[1-9]\d{6,14}$")] public string? Phone { get; set; }
        }
        public record CustomerResponse(Guid Id, string Name, string? Phone);
    }
    public static class RegisterDTO
    {
        public class OpenShiftRequest { public Guid RegisterId { get; set; } [Range(typeof(decimal), "0", "100000000")] public decimal StartingCash { get; set; } }
        public class CloseShiftRequest { [Range(typeof(decimal), "0", "100000000")] public decimal CountedCash { get; set; } }
        public class ReasonRequest { [Required, StringLength(300)] public string Reason { get; set; } = ""; }
        public record ShiftResponse(Guid Id, Guid RegisterId, Guid CashierId, decimal StartingCash, decimal? ExpectedCash, decimal? CountedCash, decimal? Discrepancy, DateTime? ClosedOn, Guid? ReviewedBy);
    }
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
    public static class ReportDTO
    {
        public class ReportFilter : Common.Pagination
        {
            public Guid LocationId { get; set; }
            public DateOnly FromDate { get; set; }
            public DateOnly ToDate { get; set; }
            public Guid? RegisterId { get; set; }
            public Guid? CashierId { get; set; }
            public Guid? ProductId { get; set; }
            public Guid? CategoryId { get; set; }
            [EnumDataType(typeof(TenderType))] public TenderType? Tender { get; set; }
            [Range(0, 23)] public int? Hour { get; set; }
        }
        public record SummaryResponse(int SaleCount, decimal GrossSales, decimal Discounts, decimal Tax, decimal Refunds, decimal NetSales, bool OfflineDataMayBePending);
        public class DayCloseRequest { public Guid LocationId { get; set; } public DateOnly BusinessDate { get; set; } }
        public record SaleReportRow(Guid Id, string? ReceiptNumber, DateTime? CompletedOn, DateOnly BusinessDate, Guid RegisterId, Guid CashierId, SaleStatus Status, decimal Total, decimal Discount, decimal Tax);
        public record AuditResponse(Guid Id, DateTime CreatedOn, Guid ActorId, string Action, Guid EntityId, string? Reason);
    }
}
