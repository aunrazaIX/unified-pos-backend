
namespace UnifiedPOS.Domain.Entities
{
    public class Location : Entity
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string TimeZoneId { get; set; } = "UTC";
        public string ReceiptHeader { get; set; } = "";
        public string Hours { get; set; } = "";
        public decimal TaxRate { get; set; }
        public bool IsActive { get; set; }
    }
}
