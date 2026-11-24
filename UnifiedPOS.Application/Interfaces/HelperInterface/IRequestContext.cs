using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.Application.Interfaces.HelperInterface
{
    public interface IRequestContext
    {
        Guid UserId { get; }
        Guid OrganizationId { get; }
        Guid SessionId { get; }
        Guid? RegisterId { get; }
        UserRole Role { get; }
    }
}
