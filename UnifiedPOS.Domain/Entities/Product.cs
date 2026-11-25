
namespace UnifiedPOS.Domain.Entities
{
    // Each sellable variant has its own SKU, barcode and inventory record.
    public class Product : Entity
    {
        public Guid OrganizationId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? ParentProductId { get; set; }
        public string Name { get; set; } = "";
        public string Sku { get; set; } = "";
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public bool IsTaxable { get; set; } = true;
        public int MinimumAge { get; set; }
        public bool IsActive { get; set; } = true;
        public bool PendingReview { get; set; }
        public Guid? SubmittedAtLocationId { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedOn { get; set; }
        public string? ReviewReason { get; set; }
        public Guid? MergedIntoId { get; set; }
    }
}
