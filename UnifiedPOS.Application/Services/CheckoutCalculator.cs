using UnifiedPOS.Application.Common;
using UnifiedPOS.Domain.Entities;
using static UnifiedPOS.Application.DTOs.CheckoutDTO;

namespace UnifiedPOS.Application.Services
{
    public static class CheckoutCalculator
    {
        public static QuoteResponse Calculate(string currency, decimal taxRate, decimal discount, IReadOnlyList<(Product Product, decimal Quantity)> items)
        {
            var ordered = items.OrderBy(x => x.Product.Id).ToList();
            var subtotal = ordered.Sum(x => Money.Round(x.Product.Price * x.Quantity));
            Money.Validate(discount);
            if (discount > subtotal) throw PosException.Invalid("Discount exceeds subtotal.");
            var lines = new List<QuoteLine>();
            var allocated = 0m;
            for (var i = 0; i < ordered.Count; i++)
            {
                var (product, quantity) = ordered[i];
                var gross = Money.Round(product.Price * quantity);
                var lineDiscount = i == ordered.Count - 1 ? discount - allocated : subtotal == 0 ? 0 : Math.Min(gross, Money.Round(discount * gross / subtotal));
                // Cap allocation to keep the remainder non-negative for low-value lines.
                lineDiscount = Math.Min(lineDiscount, discount - allocated);
                allocated += lineDiscount;
                var rate = product.IsTaxable ? taxRate : 0;
                var tax = Money.Round((gross - lineDiscount) * rate);
                lines.Add(new(product.Id, product.CategoryId, product.Name, product.Sku, quantity, product.Price, lineDiscount, rate, tax, gross - lineDiscount + tax));
            }
            if (lines.Any(x => x.Total < 0)) throw PosException.Invalid("Discount cannot be allocated to these items.");
            return new(currency, subtotal, discount, lines.Sum(x => x.Tax), lines.Sum(x => x.Total), lines);
        }
        public static decimal RefundAmount(SaleLine line, decimal quantity)
        {
            if (quantity <= 0 || quantity != decimal.Truncate(quantity) || quantity + line.RefundedQuantity > line.Quantity) throw PosException.Invalid("Refund quantity exceeds remaining units.");
            return quantity + line.RefundedQuantity == line.Quantity ? line.Total - line.RefundedAmount : Math.Min(line.Total - line.RefundedAmount, Money.Round(line.Total * quantity / line.Quantity));
        }
        public static bool IsOldEnough(DateOnly birthDate, int minimumAge, DateOnly today) => birthDate <= today && birthDate.AddYears(minimumAge) <= today;
    }
}
