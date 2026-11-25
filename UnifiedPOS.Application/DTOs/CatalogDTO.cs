using System.ComponentModel.DataAnnotations;

namespace UnifiedPOS.Application.DTOs
{
    public static class CatalogDTO
    {
        public class CategoryRequest { [Required, StringLength(100)] public string Name { get; set; } = ""; }
        public class ReviewRequest
        {
            public bool Approved { get; set; }
            [Required, StringLength(300)] public string Reason { get; set; } = "";
        }
        public class ProductRequest
        {
            public Guid CategoryId { get; set; }
            public Guid? ParentProductId { get; set; }
            [Required, StringLength(200)] public string Name { get; set; } = "";
            [Required, StringLength(100)] public string Sku { get; set; } = "";
            [StringLength(100)] public string? Barcode { get; set; }
            [Range(typeof(decimal), "0", "1000000")] public decimal Price { get; set; }
            [Range(0, 100)] public int MinimumAge { get; set; }
            public bool IsTaxable { get; set; } = true;
            public bool ConfirmSimilarName { get; set; }
            public bool IsActive { get; set; } = true;
        }
        public class StockRequest
        {
            [Range(typeof(decimal), "-1000000", "1000000")] public decimal Adjustment { get; set; }
            public bool IsBlocked { get; set; }
            [Required, StringLength(300)] public string Reason { get; set; } = "";
        }
        public class MergeRequest
        {
            public Guid SourceId { get; set; }
            public Guid TargetId { get; set; }
            [Required, StringLength(300)] public string Reason { get; set; } = "";
        }
        public record ProductResponse(Guid Id, Guid CategoryId, Guid? ParentProductId, string Name, string Sku, string? Barcode, decimal Price, int MinimumAge, bool IsTaxable, bool IsActive, Guid? MergedIntoId);
        public record StockResponse(Guid ProductId, Guid LocationId, decimal Quantity, bool IsBlocked);
    }
}
