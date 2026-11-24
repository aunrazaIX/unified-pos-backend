
namespace UnifiedPOS.Domain.Entities
{
    public class RefundLine : Entity
    {
        public Guid RefundId { get; set; }
        public Guid SaleLineId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
