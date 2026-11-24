using System.Linq.Expressions;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;
using static UnifiedPOS.Application.DTOs.ReportDTO;

namespace UnifiedPOS.Application.Interfaces.RepositoryInterface
{
    public interface IReportRepository
    {
        Task<bool> AnyDayClosesAsync(System.Linq.Expressions.Expression<Func<DayClose, bool>> filter, CancellationToken ct = default);
        void AddDayClose(DayClose entity);
        Task<SummaryResponse> SummaryAsync(Guid organizationId, ReportFilter filter, CancellationToken ct);
        Task<DataListResponse<SaleReportRow>> SalesAsync(Guid organizationId, ReportFilter filter, CancellationToken ct);
        Task<decimal> CashBalanceAsync(Guid shiftId, CancellationToken ct);
        Task<decimal> RefundTotalAsync(Guid saleId, CancellationToken ct);
    }
}
