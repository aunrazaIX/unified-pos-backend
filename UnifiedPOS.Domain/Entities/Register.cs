
namespace UnifiedPOS.Domain.Entities
{
    public class Register : Entity
    {
        public Guid LocationId { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
