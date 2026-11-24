
namespace UnifiedPOS.Domain.Entities
{
    public class Customer : Entity
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = "";
        public string? Phone { get; set; }
    }
}
