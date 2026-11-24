using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.Domain.Entities
{
    public class User : Entity
    {
        public Guid OrganizationId { get; set; }
        public string UserName { get; set; } = "";
        public string NormalizedUserName { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string? PinHash { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public ICollection<UserLocation> Locations { get; set; } = [];
    }
}
