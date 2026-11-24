using UnifiedPOS.Domain.Entities;
using static UnifiedPOS.Application.DTOs.AuthDTO;

namespace UnifiedPOS.Application.Interfaces.HelperInterface
{
    public interface IJwtService { TokenResponse Create(User user, Session session, string refreshToken); }
}
