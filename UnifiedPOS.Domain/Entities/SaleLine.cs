
namespace UnifiedPOS.Domain.Entities
{
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
}
