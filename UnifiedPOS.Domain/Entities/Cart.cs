using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.Domain.Entities
{
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
}
