using System.ComponentModel.DataAnnotations;

namespace UnifiedPOS.Application.Common
{
    public class JwtSettings
    {
        [Required, MinLength(32)] public string Key { get; set; } = "";
        [Required] public string Issuer { get; set; } = "";
        [Required] public string Audience { get; set; } = "";
        [Range(1, 30)] public int AccessMinutes { get; set; } = 15;
        [Range(1, 24)] public int SessionHours { get; set; } = 8;
    }
}
