
namespace UnifiedPOS.Domain.Entities
{
    public class Operation : Entity
    {
        public Guid OrganizationId { get; set; }
        public Guid ActorId { get; set; }
        public string Key { get; set; } = "";
        public string PayloadHash { get; set; } = "";
        public string ResponseJson { get; set; } = "";
    }
}
