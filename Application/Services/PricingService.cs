namespace SignFlow.Application.Services;

public class PricingService
{
    public (decimal Subtotal, decimal TaxTotal, decimal DiscountTotal, decimal GrandTotal) Calculate(
        IEnumerable<(decimal Quantity, decimal UnitPrice, bool Taxable, decimal? DiscountRate)> items,
        decimal taxRate)
    {
        decimal subtotal = 0m;
        decimal discountTotal = 0m;
        foreach (var item in items)
        {
            var line = item.Quantity * item.UnitPrice;
            if (item.DiscountRate.HasValue && item.DiscountRate.Value > 0)
            {
                var discount = decimal.Round(line * item.DiscountRate.Value, 2, MidpointRounding.AwayFromZero);
                discountTotal += discount;
                line -= discount;
            }
            subtotal += line;
        }
        var taxableBase = items.Where(i => i.Taxable).Sum(i => i.Quantity * i.UnitPrice);
        var taxTotal = decimal.Round(taxableBase * taxRate, 2, MidpointRounding.AwayFromZero);
        var grand = subtotal + taxTotal - discountTotal;
        return (decimal.Round(subtotal, 2), taxTotal, decimal.Round(discountTotal, 2), decimal.Round(grand, 2));
    }
}
