using System.ComponentModel.DataAnnotations;

namespace UnifiedPOS.Application.DTOs
{
    public static class OrganizationDTO
    {
        public class LocationRequest
        {
            [Required, StringLength(100)] public string Name { get; set; } = "";
            [Required, StringLength(300)] public string Address { get; set; } = "";
            [Required, StringLength(100)] public string TimeZoneId { get; set; } = "UTC";
            [StringLength(500)] public string ReceiptHeader { get; set; } = "";
            [StringLength(1000)] public string Hours { get; set; } = "";
            [Range(typeof(decimal), "0", "1")] public decimal TaxRate { get; set; }
            public bool IsActive { get; set; }
        }
        public class RegisterRequest { [Required, StringLength(100)] public string Name { get; set; } = ""; public bool IsActive { get; set; } = true; }
        public record LocationResponse(Guid Id, string Name, string Address, string TimeZoneId, string ReceiptHeader, string Hours, decimal TaxRate, bool IsActive);
        public record RegisterResponse(Guid Id, Guid LocationId, string Name, bool IsActive);
    }
}
