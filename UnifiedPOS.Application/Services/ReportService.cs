using UnifiedPOS.Application.Interfaces.ServiceInterface;
using System.Globalization;
using System.Text;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.ReportDTO;

namespace UnifiedPOS.Application.Services
{
    public class ReportService(IUnitOfWork uow, IRequestContext request) : ServiceBase(uow, request), IReportService
    {
        private async Task ValidateAsync(ReportFilter filter, CancellationToken ct)
        {
            await LocationAsync(filter.LocationId, ct);
            if (filter.FromDate == default || filter.ToDate < filter.FromDate || filter.ToDate.DayNumber - filter.FromDate.DayNumber > 366) throw PosException.Invalid("Date range must be valid and at most 366 days.");
            if (!IsManager) filter.CashierId = ActorId;
        }
        public async Task<ResponseMessage> DashboardAsync(ReportFilter filter, CancellationToken ct)
        {
            await ValidateAsync(filter, ct); return ResponseMessage.Ok(await uow.ReportRepository.SummaryAsync(OrgId, filter, ct));
        }
        public async Task<ResponseMessage> SalesAsync(ReportFilter filter, CancellationToken ct)
        {
            await ValidateAsync(filter, ct); return ResponseMessage.Ok(await uow.ReportRepository.SalesAsync(OrgId, filter, ct));
        }
        public async Task<string> ExportAsync(ReportFilter filter, CancellationToken ct)
        {
            await ValidateAsync(filter, ct);
            var rows = await uow.ReportRepository.SalesAsync(OrgId, filter, ct);
            var csv = new StringBuilder("SaleId,ReceiptNumber,CompletedOn,BusinessDate,RegisterId,CashierId,Status,Total,Discount,Tax\r\n");
            foreach (var row in rows.Items)
                csv.AppendLine(string.Join(',', row.Id, row.ReceiptNumber, row.CompletedOn?.ToString("O", CultureInfo.InvariantCulture), row.BusinessDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), row.RegisterId, row.CashierId, row.Status, row.Total.ToString(CultureInfo.InvariantCulture), row.Discount.ToString(CultureInfo.InvariantCulture), row.Tax.ToString(CultureInfo.InvariantCulture)));
            return csv.ToString();
        }
        public async Task<ResponseMessage> AuditsAsync(Guid locationId, Pagination page, CancellationToken ct)
        {
            RequireManager(); await LocationAsync(locationId, ct);
            var orgAdmin = request.Role is UserRole.Owner or UserRole.Admin;
            return ResponseMessage.Ok(await uow.AuditRepository.PageAuditsAsync(x => x.OrganizationId == OrgId && (x.LocationId == locationId || (orgAdmin && x.LocationId == null)),
                x => new AuditResponse(x.Id, x.CreatedOn, x.ActorId, x.Action, x.EntityId, x.Reason), page, ct));
        }
        public Task<ResponseMessage> CloseDayAsync(DayCloseRequest model, string key, CancellationToken ct) => MutateAsync("Day.Close", key, model, async () =>
        {
            RequireManager(); var location = await LocationAsync(model.LocationId, ct);
            var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(location.TimeZoneId)));
            if (model.BusinessDate == default || model.BusinessDate >= today) throw PosException.Invalid("Only a past business day can be closed.");
            if (await uow.ReportRepository.AnyDayClosesAsync(x => x.LocationId == location.Id && x.BusinessDate == model.BusinessDate, ct)) throw PosException.Conflict("Business day already closed.");
            if (await uow.SalesRepository.AnySalesAsync(x => x.LocationId == location.Id && x.BusinessDate == model.BusinessDate && x.Status == SaleStatus.Pending, ct)) throw PosException.Conflict("Pending sales must be resolved before day close.");
            var totals = await uow.ReportRepository.SummaryAsync(OrgId, new ReportFilter { LocationId = location.Id, FromDate = model.BusinessDate, ToDate = model.BusinessDate }, ct);
            var close = new DayClose { LocationId = location.Id, BusinessDate = model.BusinessDate, ClosedBy = ActorId, SalesTotal = totals.GrossSales, RefundTotal = totals.Refunds };
            uow.ReportRepository.AddDayClose(close); Audit("Day.Closed", close.Id, location.Id);
            return new { close.Id, close.BusinessDate, close.SalesTotal, close.RefundTotal, OfflineDataMayBePending = true };
        }, ct);
    }
}
