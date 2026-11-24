
namespace UnifiedPOS.Domain.Entities
{
    public class AuditEvent : Entity
    {
        public Guid OrganizationId { get; set; }
        public Guid? LocationId { get; set; }
        public Guid ActorId { get; set; }
        public string Action { get; set; } = "";
        public Guid EntityId { get; set; }
        public string? Reason { get; set; }
    }
}
