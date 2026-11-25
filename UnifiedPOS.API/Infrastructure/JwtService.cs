using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Domain.Entities;
using static UnifiedPOS.Application.DTOs.AuthDTO;

namespace UnifiedPOS.API.Infrastructure;

public sealed class JwtService(IOptions<JwtSettings> settings) : IJwtService
{
    public TokenResponse Create(User user, Session session, string refreshToken)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(settings.Value.AccessMinutes);
        var claims = new List<Claim>
        {
            new(JwtClaims.UserId, user.Id.ToString()), new(JwtClaims.OrganizationId, user.OrganizationId.ToString()),
            new(JwtClaims.SessionId, session.Id.ToString()), new(JwtClaims.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (session.RegisterId.HasValue) claims.Add(new Claim(JwtClaims.RegisterId, session.RegisterId.Value.ToString()));
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Value.Key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(settings.Value.Issuer, settings.Value.Audience, claims, now, expires, credentials);
        return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), refreshToken, expires, user.Id, user.OrganizationId, user.Role, session.RegisterId);
    }
}
