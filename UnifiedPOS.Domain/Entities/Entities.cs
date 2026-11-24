using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.Domain.Entities
{
    public abstract class Entity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;
        public byte[] Version { get; set; } = [];
    }

    public class Organization : Entity
    {
        public string Name { get; set; } = "";
        public string Currency { get; set; } = "";
    }

    public class Location : Entity
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string TimeZoneId { get; set; } = "UTC";
        public string ReceiptHeader { get; set; } = "";
        public string Hours { get; set; } = "";
        public decimal TaxRate { get; set; }
        public bool IsActive { get; set; }
    }

    public class Register : Entity
    {
        public Guid LocationId { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public class User : Entity
    {
        public Guid OrganizationId { get; set; }
        public string UserName { get; set; } = "";
        public string NormalizedUserName { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string? PinHash { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public ICollection<UserLocation> Locations { get; set; } = [];
    }

    public class UserLocation
    {
        public Guid UserId { get; set; }
        public Guid LocationId { get; set; }
    }

    public class Session : Entity
    {
        public Guid UserId { get; set; }
        public Guid? RegisterId { get; set; }
        public string RefreshTokenHash { get; set; } = "";
        public DateTime ExpiresOn { get; set; }
        public bool Revoked { get; set; }
    }

    public class Category : Entity
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = "";
    }

    // Each sellable variant has its own SKU, barcode and inventory record.
    public class Product : Entity
    {
        public Guid OrganizationId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? ParentProductId { get; set; }
        public string Name { get; set; } = "";
        public string Sku { get; set; } = "";
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public bool IsTaxable { get; set; } = true;
        public int MinimumAge { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? MergedIntoId { get; set; }
    }

    public class StoreStock : Entity
    {
        public Guid LocationId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsBlocked { get; set; }
    }

    public class Customer : Entity
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = "";
        public string? Phone { get; set; }
    }

    public class Shift : Entity
    {
        public Guid RegisterId { get; set; }
        public Guid CashierId { get; set; }
        public decimal StartingCash { get; set; }
        public decimal? ExpectedCash { get; set; }
        public decimal? CountedCash { get; set; }
        public DateTime? ClosedOn { get; set; }
        public Guid? ReviewedBy { get; set; }
        public string? ReviewReason { get; set; }
    }

    public class Cart : Entity
    {
        public Guid ShiftId { get; set; }
        public Guid? CustomerId { get; set; }
        public CartStatus Status { get; set; }
        public decimal DiscountAmount { get; set; }
        public Guid? DiscountApprovedBy { get; set; }
        public string? DiscountReason { get; set; }
        public DateOnly? VerifiedBirthDate { get; set; }
        public Guid? AgeVerifiedBy { get; set; }
        public ICollection<CartLine> Lines { get; set; } = [];
    }

    public class CartLine : Entity
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class Sale : Entity
    {
        public Guid OrganizationId { get; set; }
        public Guid LocationId { get; set; }
        public Guid RegisterId { get; set; }
        public Guid CashierId { get; set; }
        public Guid ShiftId { get; set; }
        public Guid CartId { get; set; }
        public Guid? CustomerId { get; set; }
        public string Currency { get; set; } = "";
        public DateOnly BusinessDate { get; set; }
        public SaleStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string? ReceiptNumber { get; set; }
        public DateTime? CompletedOn { get; set; }
        public ICollection<SaleLine> Lines { get; set; } = [];
        public ICollection<Payment> Payments { get; set; } = [];
    }

    public class SaleLine : Entity
    {
        public Guid SaleId { get; set; }
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = "";
        public string Sku { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal RefundedQuantity { get; set; }
        public decimal RefundedAmount { get; set; }
    }

    public class Payment : Entity
    {
        public Guid SaleId { get; set; }
        public TenderType Tender { get; set; }
        public decimal Amount { get; set; }
        public decimal Received { get; set; }
        public decimal Change { get; set; }
    }

    public class Refund : Entity
    {
        public Guid SaleId { get; set; }
        public Guid ShiftId { get; set; }
        public Guid ApprovedBy { get; set; }
        public string Reason { get; set; } = "";
        public decimal Amount { get; set; }
        public ICollection<RefundLine> Lines { get; set; } = [];
    }

    public class RefundLine : Entity
    {
        public Guid RefundId { get; set; }
        public Guid SaleLineId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }

    public class AuditEvent : Entity
    {
        public Guid OrganizationId { get; set; }
        public Guid? LocationId { get; set; }
        public Guid ActorId { get; set; }
        public string Action { get; set; } = "";
        public Guid EntityId { get; set; }
        public string? Reason { get; set; }
    }

    public class Operation : Entity
    {
        public Guid OrganizationId { get; set; }
        public Guid ActorId { get; set; }
        public string Key { get; set; } = "";
        public string PayloadHash { get; set; } = "";
        public string ResponseJson { get; set; } = "";
    }

    public class OutboxMessage : Entity
    {
        public Guid OrganizationId { get; set; }
        public string Type { get; set; } = "";
        public Guid AggregateId { get; set; }
        public string Payload { get; set; } = "";
        public DateTime? ProcessedOn { get; set; }
    }

    public class DayClose : Entity
    {
        public Guid LocationId { get; set; }
        public DateOnly BusinessDate { get; set; }
        public Guid ClosedBy { get; set; }
        public decimal SalesTotal { get; set; }
        public decimal RefundTotal { get; set; }
    }
}
