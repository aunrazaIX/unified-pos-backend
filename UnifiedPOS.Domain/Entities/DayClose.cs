
namespace UnifiedPOS.Domain.Entities
{
    public class DayClose : Entity
    {
        public Guid LocationId { get; set; }
        public DateOnly BusinessDate { get; set; }
        public Guid ClosedBy { get; set; }
        public decimal SalesTotal { get; set; }
        public decimal RefundTotal { get; set; }
    }
}
