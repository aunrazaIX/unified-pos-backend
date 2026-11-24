using Microsoft.Extensions.Options;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.CatalogDTO;

namespace UnifiedPOS.Application.Services
{
    public class PosService(IUnitOfWork uow, IRequestContext request, IOptions<PosSettings> settings) : ServiceBase(uow, request)
    {
        public async Task<ResponseMessage> BootstrapAsync(Guid registerId, CancellationToken ct)
        {
            var register = await RegisterAsync(registerId, ct); var location = await LocationAsync(register.LocationId, ct);
            var organization = await uow.Organizations.FindAsync(x => x.Id == OrgId, ct) ?? throw PosException.Missing();
            return ResponseMessage.Ok(new { OrganizationId = OrgId, organization.Currency, Location = new { location.Id, location.Name, location.Address, location.TimeZoneId, location.TaxRate, location.ReceiptHeader }, Register = new { register.Id, register.Name, register.IsActive }, AllowNegativeStock = settings.Value.AllowNegativeStock, OfflineAuthenticationEnabled = false, OfflineSaleReplayEnabled = false, CardPaymentsEnabled = false, CatalogChangesUrl = "/api/v1/pos/changes", StockIsApproximateOffline = true });
        }
        public async Task<ResponseMessage> ChangesAsync(Guid registerId, DateTime? cursor, Guid? afterId, CancellationToken ct)
        {
            await RegisterAsync(registerId, ct);
            var since = cursor?.ToUniversalTime() ?? DateTime.UnixEpoch;
            var after = afterId ?? Guid.Empty;
            if (since > DateTime.UtcNow) throw PosException.Invalid("Cursor cannot be in the future.");
            // A lexicographic (timestamp, UUID) cursor prevents equal-timestamp rows being lost.
            var rows = await uow.Products.PageAsync(x => x.OrganizationId == OrgId && (x.ModifiedOn > since || (x.ModifiedOn == since && x.Id.CompareTo(after) > 0)),
                x => new { Product = new ProductResponse(x.Id, x.CategoryId, x.ParentProductId, x.Name, x.Sku, x.Barcode, x.Price, x.MinimumAge, x.IsTaxable, x.IsActive, x.MergedIntoId), x.ModifiedOn }, new Pagination { PageSize = 100 }, ct);
            return ResponseMessage.Ok(new { rows.Items, rows.TotalCount, RequiresFullCatalogRefresh = true, OfflineSaleReplayEnabled = false });
        }
        public Task<ResponseMessage> SyncAsync(CancellationToken ct) => throw new PosException(503, "Offline sale replay is disabled pending approved authentication, pricing and conflict-resolution policies. No sale was accepted.");
    }
}
