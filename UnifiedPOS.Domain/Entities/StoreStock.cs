
namespace UnifiedPOS.Domain.Entities
{
    public class StoreStock : Entity
    {
        public Guid LocationId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public bool IsBlocked { get; set; }
    }
}
