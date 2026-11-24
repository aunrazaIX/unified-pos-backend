
namespace UnifiedPOS.Domain.Entities
{
    public class Session : Entity
    {
        public Guid UserId { get; set; }
        public Guid? RegisterId { get; set; }
        public string RefreshTokenHash { get; set; } = "";
        public DateTime ExpiresOn { get; set; }
        public bool Revoked { get; set; }
    }
}
