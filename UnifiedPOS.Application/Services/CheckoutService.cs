using Microsoft.Extensions.Options;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.CheckoutDTO;

namespace UnifiedPOS.Application.Services
{
    public class CheckoutService(IUnitOfWork uow, IRequestContext request, IOptions<PosSettings> settings) : ServiceBase(uow, request)
    {
        public Task<ResponseMessage> CreateCartAsync(CartRequest model, string key, CancellationToken ct) => MutateAsync("Cart.Create", key, model, async () =>
        {
            var shift = await ShiftAsync(model.ShiftId, ct); await EnsureCustomerAsync(model.CustomerId, ct);
            var cart = new Cart { ShiftId = shift.Id, CustomerId = model.CustomerId };
            uow.Carts.Add(cart); Audit("Cart.Created", cart.Id);
            return Map(cart);
        }, ct);
        public async Task<ResponseMessage> GetCartAsync(Guid id, CancellationToken ct) => ResponseMessage.Ok(Map(await CartAsync(id, ct, false)));
        public async Task<ResponseMessage> GetHeldAsync(Guid shiftId, Pagination page, CancellationToken ct)
        {
            await ShiftAsync(shiftId, ct);
            return ResponseMessage.Ok(await uow.Carts.PageAsync(x => x.ShiftId == shiftId && x.Status == CartStatus.Held,
                x => new { x.Id, x.CustomerId, x.CreatedOn }, page, ct));
        }
        public Task<ResponseMessage> SetLineAsync(Guid id, LineRequest model, string key, CancellationToken ct) => MutateAsync("Cart.Line", key, new { id, model }, async () =>
        {
            var cart = await CartAsync(id, ct);
            var product = await uow.Products.FindAsync(x => x.Id == model.ProductId && x.OrganizationId == OrgId && x.IsActive && x.MergedIntoId == null, ct) ?? throw PosException.Missing();
            var line = cart.Lines.SingleOrDefault(x => x.ProductId == product.Id);
            if (model.Quantity == 0) { if (line != null) cart.Lines.Remove(line); }
            else if (line == null)
            {
                if (cart.Lines.Count >= 100) throw PosException.Invalid("Cart is limited to 100 lines.");
                cart.Lines.Add(new CartLine { CartId = id, ProductId = product.Id, Quantity = model.Quantity });
            }
            else line.Quantity = model.Quantity;
            cart.VerifiedBirthDate = null; cart.AgeVerifiedBy = null;
            Audit("Cart.LineChanged", id);
            return Map(cart);
        }, ct);
        public Task<ResponseMessage> HoldAsync(Guid id, string key, CancellationToken ct) => MutateAsync("Cart.Hold", key, new { id }, async () =>
        {
            var cart = await CartAsync(id, ct); cart.Status = CartStatus.Held; cart.VerifiedBirthDate = null;
            Audit("Cart.Held", id); return Map(cart);
        }, ct);
        public Task<ResponseMessage> RecallAsync(Guid id, string key, CancellationToken ct) => MutateAsync("Cart.Recall", key, new { id }, async () =>
        {
            var cart = await CartAsync(id, ct, false);
            if (cart.Status != CartStatus.Held) throw PosException.Conflict("Only held carts can be recalled.");
            cart.Status = CartStatus.Open; cart.VerifiedBirthDate = null;
            var quote = await QuoteAsync(cart, ct);
            Audit("Cart.Recalled", id); return new { Cart = Map(cart), Quote = quote };
        }, ct);
        public Task<ResponseMessage> DiscountAsync(Guid id, DiscountRequest model, string key, CancellationToken ct) => MutateAsync("Cart.Discount", key, new { id, model }, async () =>
        {
            RequireManager(); Money.Validate(model.Amount);
            var cart = await CartAsync(id, ct); cart.DiscountAmount = model.Amount; cart.DiscountApprovedBy = ActorId; cart.DiscountReason = model.Reason;
            await QuoteAsync(cart, ct); Audit("Cart.DiscountApproved", id, reason: model.Reason); return Map(cart);
        }, ct);
        public Task<ResponseMessage> VerifyAgeAsync(Guid id, AgeRequest model, string key, CancellationToken ct) => MutateAsync("Cart.Age", key, new { id, model }, async () =>
        {
            var cart = await CartAsync(id, ct);
            var shift = await ShiftAsync(cart.ShiftId, ct); var register = await RegisterAsync(shift.RegisterId, ct); var location = await LocationAsync(register.LocationId, ct);
            var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(location.TimeZoneId)));
            var ids = cart.Lines.Select(x => x.ProductId).ToArray();
            var products = await uow.Products.ListAsync(x => ids.Contains(x.Id) && x.OrganizationId == OrgId, ct);
            if (model.BirthDate > today || products.Any(x => !CheckoutCalculator.IsOldEnough(model.BirthDate, x.MinimumAge, today))) throw PosException.Invalid("Age verification failed.");
            cart.VerifiedBirthDate = model.BirthDate; cart.AgeVerifiedBy = ActorId;
            Audit("Cart.AgeVerified", id, location.Id); return Map(cart);
        }, ct);
        public Task<ResponseMessage> AttachCustomerAsync(Guid id, AttachCustomerRequest model, string key, CancellationToken ct) => MutateAsync("Cart.Customer", key, new { id, model }, async () =>
        {
            var cart = await CartAsync(id, ct); await EnsureCustomerAsync(model.CustomerId, ct); cart.CustomerId = model.CustomerId;
            Audit("Cart.CustomerAttached", id); return Map(cart);
        }, ct);
        public async Task<ResponseMessage> GetQuoteAsync(Guid id, CancellationToken ct) => ResponseMessage.Ok(await QuoteAsync(await CartAsync(id, ct), ct));
        public Task<ResponseMessage> CheckoutAsync(Guid id, string key, CancellationToken ct) => MutateAsync("Cart.Checkout", key, new { id }, async () =>
        {
            var cart = await CartAsync(id, ct); var shift = await ShiftAsync(cart.ShiftId, ct); var register = await RegisterAsync(shift.RegisterId, ct); var location = await LocationAsync(register.LocationId, ct);
            var quote = await QuoteAsync(cart, ct, true);
            var businessDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(location.TimeZoneId)));
            if (await uow.DayCloses.AnyAsync(x => x.LocationId == location.Id && x.BusinessDate == businessDate, ct)) throw PosException.Conflict("Business day is closed.");
            var sale = new Sale { OrganizationId = OrgId, LocationId = location.Id, RegisterId = register.Id, CashierId = shift.CashierId, ShiftId = shift.Id, CartId = cart.Id, CustomerId = cart.CustomerId, Currency = quote.Currency, BusinessDate = businessDate, Subtotal = quote.Subtotal, Discount = quote.Discount, Tax = quote.Tax, Total = quote.Total };
            sale.Lines = quote.Lines.Select(x => new SaleLine { SaleId = sale.Id, ProductId = x.ProductId, CategoryId = x.CategoryId, Name = x.Name, Sku = x.Sku, Quantity = x.Quantity, UnitPrice = x.UnitPrice, Discount = x.Discount, TaxRate = x.TaxRate, Tax = x.Tax, Total = x.Total }).ToList();
            var productIds = sale.Lines.Select(x => x.ProductId).ToArray();
            var stocks = await uow.Stocks.ListAsync(x => x.LocationId == location.Id && productIds.Contains(x.ProductId), ct, true);
            foreach (var line in sale.Lines)
            {
                var stock = stocks.SingleOrDefault(x => x.ProductId == line.ProductId);
                if (stock == null) { stock = new StoreStock { LocationId = location.Id, ProductId = line.ProductId }; uow.Stocks.Add(stock); }
                stock.Quantity -= line.Quantity;
            }
            uow.Sales.Add(sale); cart.Status = CartStatus.CheckedOut;
            Audit("Sale.Created", sale.Id, location.Id); return SalesService.Map(sale);
        }, ct);
        private async Task<Cart> CartAsync(Guid id, CancellationToken ct, bool requireOpen = true)
        {
            var cart = await uow.Carts.FindAsync(x => x.Id == id, ct) ?? throw PosException.Missing();
            await ShiftAsync(cart.ShiftId, ct);
            if (requireOpen && cart.Status != CartStatus.Open) throw PosException.Conflict("Cart is not open.");
            return cart;
        }
        private async Task<QuoteResponse> QuoteAsync(Cart cart, CancellationToken ct, bool validateAge = false)
        {
            if (cart.Lines.Count == 0) throw PosException.Invalid("Cart is empty.");
            var shift = await ShiftAsync(cart.ShiftId, ct); var register = await RegisterAsync(shift.RegisterId, ct); var location = await LocationAsync(register.LocationId, ct);
            var organization = await uow.Organizations.FindAsync(x => x.Id == OrgId, ct) ?? throw PosException.Missing();
            var ids = cart.Lines.Select(x => x.ProductId).ToArray();
            var products = await uow.Products.ListAsync(x => x.OrganizationId == OrgId && ids.Contains(x.Id), ct);
            var stocks = await uow.Stocks.ListAsync(x => x.LocationId == location.Id && ids.Contains(x.ProductId), ct);
            var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(location.TimeZoneId)));
            var items = new List<(Product, decimal)>();
            foreach (var line in cart.Lines)
            {
                var product = products.SingleOrDefault(x => x.Id == line.ProductId);
                if (product == null || !product.IsActive || product.MergedIntoId != null) throw PosException.Conflict("A cart product is unavailable; replace it before checkout.");
                var stock = stocks.SingleOrDefault(x => x.ProductId == line.ProductId);
                if (stock?.IsBlocked == true) throw PosException.Invalid("A cart product is blocked at this location.");
                if (!settings.Value.AllowNegativeStock && (stock?.Quantity ?? 0) < line.Quantity) throw PosException.Conflict("Insufficient stock.");
                if (validateAge && product.MinimumAge > 0 && (!cart.VerifiedBirthDate.HasValue || !CheckoutCalculator.IsOldEnough(cart.VerifiedBirthDate.Value, product.MinimumAge, today))) throw PosException.Invalid("Age verification is required.");
                items.Add((product, line.Quantity));
            }
            return CheckoutCalculator.Calculate(organization.Currency, location.TaxRate, cart.DiscountAmount, items);
        }
        private static CartResponse Map(Cart x) => new(x.Id, x.ShiftId, x.CustomerId, x.Status, x.DiscountAmount, x.Lines.Select(l => new CartLineResponse(l.Id, l.ProductId, l.Quantity)).ToArray());
    }
}
