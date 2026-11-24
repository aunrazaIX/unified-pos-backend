using UnifiedPOS.Application.Interfaces.ServiceInterface;
using System.Text.Json;
using UnifiedPOS.Application.Common;
using UnifiedPOS.Application.Interfaces.HelperInterface;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Domain.Entities;
using UnifiedPOS.Domain.Enums;
using static UnifiedPOS.Application.DTOs.CheckoutDTO;
using static UnifiedPOS.Application.DTOs.RegisterDTO;

namespace UnifiedPOS.Application.Services
{
    public class SalesService(IUnitOfWork uow, IRequestContext request) : ServiceBase(uow, request), ISalesService
    {
        public async Task<ResponseMessage> GetSaleAsync(Guid id, CancellationToken ct) => ResponseMessage.Ok(Map(await SaleAsync(id, ct)));
        public Task<ResponseMessage> AddPaymentAsync(Guid id, PaymentRequest model, string key, CancellationToken ct) => MutateAsync("Sale.Payment", key, new { id, model }, async () =>
        {
            var sale = await SaleAsync(id, ct); await ShiftAsync(sale.ShiftId, ct);
            if (sale.Status != SaleStatus.Pending) throw PosException.Conflict("Only pending sales accept payments.");
            if (model.Tender != TenderType.Cash) throw new PosException(503, "Card processor is not configured. Card payments are unavailable.");
            Money.Validate(model.Amount, false); Money.Validate(model.Received, false);
            if (model.Amount > sale.Total - sale.Payments.Sum(x => x.Amount)) throw PosException.Invalid("Payment exceeds balance due.");
            if (model.Received < model.Amount) throw PosException.Invalid("Cash received is less than payment amount.");
            var payment = new Payment { SaleId = id, Tender = TenderType.Cash, Amount = model.Amount, Received = model.Received, Change = model.Received - model.Amount };
            sale.Payments.Add(payment); Audit("Sale.CashPayment", id, sale.LocationId);
            return new PaymentResponse(payment.Id, payment.Tender, payment.Amount, payment.Received, payment.Change);
        }, ct);
        public Task<ResponseMessage> CompleteAsync(Guid id, string key, CancellationToken ct) => MutateAsync("Sale.Complete", key, new { id }, async () =>
        {
            var sale = await SaleAsync(id, ct); await ShiftAsync(sale.ShiftId, ct);
            if (sale.Status == SaleStatus.Completed) return Map(sale);
            if (sale.Status != SaleStatus.Pending) throw PosException.Conflict("Sale is not pending.");
            if (sale.Payments.Sum(x => x.Amount) != sale.Total) throw PosException.Conflict("Sale must be fully paid before completion.");
            sale.Status = SaleStatus.Completed; sale.CompletedOn = DateTime.UtcNow; sale.ReceiptNumber = "POS-" + sale.Id.ToString("N").ToUpperInvariant();
            Event("Sale.Completed", sale, new { sale.Id, sale.LocationId, sale.Total, sale.Currency });
            Audit("Sale.Completed", id, sale.LocationId); return Map(sale);
        }, ct);
        public async Task<ResponseMessage> ReceiptAsync(Guid id, CancellationToken ct)
        {
            var sale = await SaleAsync(id, ct);
            if (sale.Status != SaleStatus.Completed) throw PosException.Conflict("Receipt is available only for completed sales.");
            var location = await LocationAsync(sale.LocationId, ct);
            return ResponseMessage.Ok(new ReceiptResponse(Map(sale), location.Name, location.Address, location.ReceiptHeader));
        }
        public Task<ResponseMessage> RefundAsync(Guid id, RefundRequest model, string key, CancellationToken ct) => MutateAsync("Sale.Refund", key, new { id, model }, async () =>
        {
            RequireManager(); var sale = await SaleAsync(id, ct);
            if (sale.Status != SaleStatus.Completed) throw PosException.Conflict("Only completed sales can be refunded.");
            var shift = await ShiftAsync(model.ShiftId, ct); var register = await RegisterAsync(shift.RegisterId, ct);
            if (register.LocationId != sale.LocationId) throw PosException.Invalid("Refunds must be issued at the original sale location.");
            if (model.Lines.Select(x => x.SaleLineId).Distinct().Count() != model.Lines.Count) throw PosException.Invalid("Duplicate refund lines are not allowed.");
            var refund = new Refund { SaleId = id, ShiftId = shift.Id, ApprovedBy = ActorId, Reason = model.Reason };
            var quantities = new Dictionary<Guid, decimal>();
            foreach (var input in model.Lines)
            {
                var line = sale.Lines.SingleOrDefault(x => x.Id == input.SaleLineId) ?? throw PosException.Invalid("Unknown sale line.");
                var amount = CheckoutCalculator.RefundAmount(line, input.Quantity);
                line.RefundedQuantity += input.Quantity; line.RefundedAmount += amount;
                refund.Lines.Add(new RefundLine { RefundId = refund.Id, SaleLineId = line.Id, Quantity = input.Quantity, Amount = amount });
                refund.Amount += amount; quantities[line.ProductId] = quantities.GetValueOrDefault(line.ProductId) + input.Quantity;
            }
            if (await uow.ReportRepository.RefundTotalAsync(id, ct) + refund.Amount > sale.Payments.Sum(x => x.Amount)) throw PosException.Conflict("Refund exceeds paid amount.");
            await RestoreInventoryAsync(sale.LocationId, quantities, ct);
            uow.SalesRepository.AddRefund(refund); Event("Sale.Refunded", sale, new { refund.Id, SaleId = id, refund.Amount });
            Audit("Sale.Refunded", refund.Id, sale.LocationId, model.Reason);
            return new RefundResponse(refund.Id, id, refund.Amount);
        }, ct);
        public Task<ResponseMessage> VoidAsync(Guid id, ReasonRequest model, string key, CancellationToken ct) => MutateAsync("Sale.Void", key, new { id, model }, async () =>
        {
            RequireManager(); var sale = await SaleAsync(id, ct); await ShiftAsync(sale.ShiftId, ct);
            if (sale.Status != SaleStatus.Pending) throw PosException.Conflict("Only pending sales can be voided. Refund completed sales instead.");
            var paid = sale.Payments.Sum(x => x.Amount);
            if (paid > 0) uow.SalesRepository.AddRefund(new Refund { SaleId = id, ShiftId = sale.ShiftId, ApprovedBy = ActorId, Reason = model.Reason, Amount = paid });
            await RestoreInventoryAsync(sale.LocationId, sale.Lines.GroupBy(x => x.ProductId).ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity)), ct);
            sale.Status = SaleStatus.Voided; Event("Sale.Voided", sale, new { sale.Id, CashToReturn = paid });
            Audit("Sale.Voided", id, sale.LocationId, model.Reason); return new { Sale = Map(sale), CashToReturn = paid };
        }, ct);
        private async Task RestoreInventoryAsync(Guid locationId, Dictionary<Guid, decimal> quantities, CancellationToken ct)
        {
            var ids = quantities.Keys.ToArray();
            var products = await uow.CatalogRepository.ListProductsAsync(x => x.OrganizationId == OrgId && ids.Contains(x.Id), ct);
            var restored = new Dictionary<Guid, decimal>();
            foreach (var product in products)
            {
                var targetId = product.MergedIntoId ?? product.Id;
                restored[targetId] = restored.GetValueOrDefault(targetId) + quantities[product.Id];
            }
            var targetIds = restored.Keys.ToArray();
            var stocks = await uow.CatalogRepository.ListStocksAsync(x => x.LocationId == locationId && targetIds.Contains(x.ProductId), ct, true);
            foreach (var (productId, quantity) in restored)
            {
                var stock = stocks.SingleOrDefault(x => x.ProductId == productId);
                if (stock == null) { stock = new StoreStock { LocationId = locationId, ProductId = productId }; uow.CatalogRepository.AddStoreStock(stock); }
                stock.Quantity += quantity;
            }
        }
        private async Task<Sale> SaleAsync(Guid id, CancellationToken ct)
        {
            var sale = await uow.SalesRepository.FindSaleAsync(x => x.Id == id && x.OrganizationId == OrgId, ct) ?? throw PosException.Missing();
            await LocationAsync(sale.LocationId, ct);
            if (request.RegisterId.HasValue && request.RegisterId != sale.RegisterId) throw PosException.Forbidden();
            if (!IsManager && sale.CashierId != ActorId) throw PosException.Forbidden();
            return sale;
        }
        private void Event<T>(string type, Sale sale, T payload) => uow.OutboxRepository.AddOutboxMessage(new OutboxMessage { OrganizationId = OrgId, AggregateId = type == "Sale.Refunded" ? Guid.NewGuid() : sale.Id, Type = type, Payload = JsonSerializer.Serialize(payload) });
        public static SaleResponse Map(Sale x) => new(x.Id, x.LocationId, x.RegisterId, x.CashierId, x.ShiftId, x.CustomerId, x.Status, x.Currency, x.Subtotal, x.Discount, x.Tax, x.Total, x.Payments.Sum(p => p.Amount), x.ReceiptNumber, x.CompletedOn,
            x.Lines.Select(l => new SaleLineResponse(l.Id, l.ProductId, l.Name, l.Sku, l.Quantity, l.UnitPrice, l.Discount, l.TaxRate, l.Tax, l.Total, l.RefundedQuantity, l.RefundedAmount)).ToArray(),
            x.Payments.Select(p => new PaymentResponse(p.Id, p.Tender, p.Amount, p.Received, p.Change)).ToArray());
    }
}
