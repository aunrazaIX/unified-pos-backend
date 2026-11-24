using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.Domain.Entities
{
    public class Payment : Entity
    {
        public Guid SaleId { get; set; }
        public TenderType Tender { get; set; }
        public decimal Amount { get; set; }
        public decimal Received { get; set; }
        public decimal Change { get; set; }
    }
}
