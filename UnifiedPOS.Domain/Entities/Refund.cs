
namespace UnifiedPOS.Domain.Entities
{
    public class Refund : Entity
    {
        public Guid SaleId { get; set; }
        public Guid ShiftId { get; set; }
        public Guid ApprovedBy { get; set; }
        public string Reason { get; set; } = "";
        public decimal Amount { get; set; }
        public ICollection<RefundLine> Lines { get; set; } = [];
    }
}
