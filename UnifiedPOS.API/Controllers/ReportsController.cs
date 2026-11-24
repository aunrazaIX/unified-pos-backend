using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.ServiceInterface;
using static UnifiedPOS.Application.DTOs.ReportDTO;

namespace UnifiedPOS.API.Controllers;

[ApiController, Authorize(Roles = Roles.Managers), Route("api/v1/reports")]
public sealed class ReportsController(IReportService service) : ControllerBase
{
    [HttpGet("dashboard")] public Task<ResponseMessage> Dashboard([FromQuery] ReportFilter filter, CancellationToken ct) => service.DashboardAsync(filter, ct);
    [HttpGet("sales")] public Task<ResponseMessage> Sales([FromQuery] ReportFilter filter, CancellationToken ct) => service.SalesAsync(filter, ct);
    [HttpGet("sales/export")] public async Task<IActionResult> Export([FromQuery] ReportFilter filter, CancellationToken ct) => File(Encoding.UTF8.GetBytes(await service.ExportAsync(filter, ct)), "text/csv; charset=utf-8", "sales.csv");
    [HttpGet("audits")] public Task<ResponseMessage> Audits([FromQuery] Guid locationId, [FromQuery] Pagination page, CancellationToken ct) => service.AuditsAsync(locationId, page, ct);
    [HttpPost("day-close")] public Task<ResponseMessage> CloseDay(DayCloseRequest model, [FromHeader(Name = "Idempotency-Key")] string key, CancellationToken ct) => service.CloseDayAsync(model, key, ct);
}
