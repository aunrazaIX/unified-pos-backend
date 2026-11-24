
namespace UnifiedPOS.Domain.Entities
{
    public class CartLine : Entity
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
    }
}
