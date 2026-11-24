using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.AuthDTO;

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
    public interface IJwtService { TokenResponse Create(User user, Session session, string refreshToken); }
}
