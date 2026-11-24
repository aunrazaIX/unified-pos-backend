
namespace UnifiedPOS.Domain.Entities
{
    public class OutboxMessage : Entity
    {
        public Guid OrganizationId { get; set; }
        public string Type { get; set; } = "";
        public Guid AggregateId { get; set; }
        public string Payload { get; set; } = "";
        public DateTime? ProcessedOn { get; set; }
    }
}
