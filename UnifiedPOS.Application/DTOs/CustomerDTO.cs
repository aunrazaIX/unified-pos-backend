using System.ComponentModel.DataAnnotations;

namespace UnifiedPOS.Application.DTOs
{
    public static class CustomerDTO
    {
        public class CustomerRequest
        {
            [Required, StringLength(200)] public string Name { get; set; } = "";
            [StringLength(20), RegularExpression(@"^\+[1-9]\d{6,14}$")] public string? Phone { get; set; }
        }
        public record CustomerResponse(Guid Id, string Name, string? Phone);
    }
}
