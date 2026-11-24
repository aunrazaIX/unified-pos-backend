using System.ComponentModel.DataAnnotations;

namespace UnifiedPOS.Application.DTOs
{
    public static class RegisterDTO
    {
        public class OpenShiftRequest { public Guid RegisterId { get; set; } [Range(typeof(decimal), "0", "100000000")] public decimal StartingCash { get; set; } }
        public class CloseShiftRequest { [Range(typeof(decimal), "0", "100000000")] public decimal CountedCash { get; set; } }
        public class ReasonRequest { [Required, StringLength(300)] public string Reason { get; set; } = ""; }
        public record ShiftResponse(Guid Id, Guid RegisterId, Guid CashierId, decimal StartingCash, decimal? ExpectedCash, decimal? CountedCash, decimal? Discrepancy, DateTime? ClosedOn, Guid? ReviewedBy);
    }
}
