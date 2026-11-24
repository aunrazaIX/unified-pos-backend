
namespace UnifiedPOS.Domain.Entities
{
    public class Category : Entity
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = "";
    }
}
