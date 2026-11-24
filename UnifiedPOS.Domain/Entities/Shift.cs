
namespace UnifiedPOS.Domain.Entities
{
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
}
