using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;
using UnifiedPOS.Infrastructure.Databases;
using static UnifiedPOS.Application.DTOs.ReportDTO;

namespace UnifiedPOS.Infrastructure.Repositories
{
    public class ReportRepository(POSContext context) : IReportRepository
    {
        public Task<bool> AnyDayClosesAsync(System.Linq.Expressions.Expression<Func<DayClose, bool>> filter, CancellationToken ct = default) => context.Set<DayClose>().AnyAsync(filter, ct);
        public void AddDayClose(DayClose entity) => context.Set<DayClose>().Add(entity);
        private IQueryable<Sale> Filter(Guid organizationId, ReportFilter filter) => context.Set<Sale>().AsNoTracking().Where(x =>
            x.OrganizationId == organizationId && x.LocationId == filter.LocationId &&
            x.BusinessDate >= filter.FromDate && x.BusinessDate <= filter.ToDate &&
            (!filter.RegisterId.HasValue || x.RegisterId == filter.RegisterId) &&
            (!filter.CashierId.HasValue || x.CashierId == filter.CashierId) &&
            (!filter.ProductId.HasValue || x.Lines.Any(l => l.ProductId == filter.ProductId)) &&
            (!filter.CategoryId.HasValue || x.Lines.Any(l => l.CategoryId == filter.CategoryId)) &&
            (!filter.Tender.HasValue || x.Payments.Any(p => p.Tender == filter.Tender)));

        public async Task<SummaryResponse> SummaryAsync(Guid organizationId, ReportFilter filter, CancellationToken ct)
        {
            var query = Filter(organizationId, filter).Where(x => x.Status == SaleStatus.Completed);
            var totals = await query.GroupBy(x => 1).Select(g => new { Count = g.Count(), Sales = g.Sum(x => x.Total), Discounts = g.Sum(x => x.Discount), Tax = g.Sum(x => x.Tax) }).SingleOrDefaultAsync(ct);
            var refunds = await context.Set<Refund>().Where(r => query.Any(s => s.Id == r.SaleId)).SumAsync(r => (decimal?)r.Amount, ct) ?? 0;
            return new(totals?.Count ?? 0, totals?.Sales ?? 0, totals?.Discounts ?? 0, totals?.Tax ?? 0, refunds, (totals?.Sales ?? 0) - refunds, true);
        }
        public async Task<DataListResponse<SaleReportRow>> SalesAsync(Guid organizationId, ReportFilter filter, CancellationToken ct)
        {
            var query = Filter(organizationId, filter);
            var count = await query.CountAsync(ct);
            var rows = await query.OrderByDescending(x => x.CreatedOn).ThenBy(x => x.Id).Skip((filter.CurrentPage - 1) * filter.PageSize).Take(filter.PageSize)
                .Select(x => new SaleReportRow(x.Id, x.ReceiptNumber, x.CompletedOn, x.BusinessDate, x.RegisterId, x.CashierId, x.Status, x.Total, x.Discount, x.Tax)).ToListAsync(ct);
            return new(rows, count, filter.CurrentPage, filter.PageSize);
        }
        public async Task<decimal> CashBalanceAsync(Guid shiftId, CancellationToken ct)
        {
            var payments = await context.Set<Payment>().Where(p => p.Tender == TenderType.Cash && context.Set<Sale>().Any(s => s.Id == p.SaleId && s.ShiftId == shiftId)).SumAsync(x => (decimal?)x.Amount, ct) ?? 0;
            var refunds = await context.Set<Refund>().Where(x => x.ShiftId == shiftId).SumAsync(x => (decimal?)x.Amount, ct) ?? 0;
            return payments - refunds;
        }
        public async Task<decimal> RefundTotalAsync(Guid saleId, CancellationToken ct) => await context.Set<Refund>().Where(x => x.SaleId == saleId).SumAsync(x => (decimal?)x.Amount, ct) ?? 0;
    }
}
