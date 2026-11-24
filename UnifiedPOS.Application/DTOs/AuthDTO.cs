using System.ComponentModel.DataAnnotations;
using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.Application.DTOs
{
    public static class AuthDTO
    {
        public class LoginRequest
        {
            public Guid OrganizationId { get; set; }
            [Required, StringLength(100)] public string UserName { get; set; } = "";
            [Required, StringLength(128, MinimumLength = 12)] public string Password { get; set; } = "";
        }
        public class PinRequest
        {
            public Guid OrganizationId { get; set; }
            public Guid RegisterId { get; set; }
            [Required, StringLength(100)] public string UserName { get; set; } = "";
            [Required, RegularExpression(@"^\d{6,12}$")] public string Pin { get; set; } = "";
        }
        public class RefreshRequest
        {
            [Required, StringLength(128)] public string RefreshToken { get; set; } = "";
        }
        public class UserRequest
        {
            [Required, StringLength(100, MinimumLength = 3)] public string UserName { get; set; } = "";
            [Required, StringLength(128, MinimumLength = 12)] public string Password { get; set; } = "";
            [EnumDataType(typeof(UserRole))] public UserRole Role { get; set; }
            [MaxLength(100)] public Guid[] LocationIds { get; set; } = [];
        }
        public class LocationsRequest { [MaxLength(100)] public Guid[] LocationIds { get; set; } = []; }
        public class ResetPinRequest
        {
            [Required, RegularExpression(@"^\d{6,12}$")] public string Pin { get; set; } = "";
        }
        public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresOn);
        public record UserResponse(Guid Id, string UserName, UserRole Role, bool IsActive, Guid[] LocationIds);
    }
}
