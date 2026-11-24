using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.Domain.Entities
{
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
}
