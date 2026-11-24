using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.ReportDTO;

namespace UnifiedPOS.Application.Interfaces.ServiceInterface
{
    public interface IReportService
    {
        Task<ResponseMessage> DashboardAsync(ReportFilter filter, CancellationToken ct);
        Task<ResponseMessage> SalesAsync(ReportFilter filter, CancellationToken ct);
        Task<string> ExportAsync(ReportFilter filter, CancellationToken ct);
        Task<ResponseMessage> AuditsAsync(Guid locationId, Pagination page, CancellationToken ct);
        Task<ResponseMessage> CloseDayAsync(DayCloseRequest model, string key, CancellationToken ct);
    }
}
