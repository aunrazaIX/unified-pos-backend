using System.ComponentModel.DataAnnotations;
using UnifiedPOS.Domain.Enums;

namespace UnifiedPOS.Application.DTOs
{
    public static class ReportDTO
    {
        public class ReportFilter : Common.Pagination
        {
            public Guid LocationId { get; set; }
            public DateOnly FromDate { get; set; }
            public DateOnly ToDate { get; set; }
            public Guid? RegisterId { get; set; }
            public Guid? CashierId { get; set; }
            public Guid? ProductId { get; set; }
            public Guid? CategoryId { get; set; }
            [EnumDataType(typeof(TenderType))] public TenderType? Tender { get; set; }
            [Range(0, 23)] public int? Hour { get; set; }
        }
        public record SummaryResponse(int SaleCount, decimal GrossSales, decimal Discounts, decimal Tax, decimal Refunds, decimal NetSales, bool OfflineDataMayBePending);
        public class DayCloseRequest { public Guid LocationId { get; set; } public DateOnly BusinessDate { get; set; } }
        public record SaleReportRow(Guid Id, string? ReceiptNumber, DateTime? CompletedOn, DateOnly BusinessDate, Guid RegisterId, Guid CashierId, SaleStatus Status, decimal Total, decimal Discount, decimal Tax);
        public record AuditResponse(Guid Id, DateTime CreatedOn, Guid ActorId, string Action, Guid EntityId, string? Reason);
    }
}
