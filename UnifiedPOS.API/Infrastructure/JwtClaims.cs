namespace UnifiedPOS.API.Infrastructure;

public static class JwtClaims
{
    public const string UserId = "sub";
    public const string OrganizationId = "org_id";
    public const string SessionId = "sid";
    public const string RegisterId = "register_id";
    public const string Role = "role";
}
