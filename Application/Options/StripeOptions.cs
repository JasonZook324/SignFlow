namespace SignFlow.Application.Options;

public class StripeOptions
{
    public const string SectionName = "Stripe";
    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string PriceStarter { get; set; } = string.Empty;
    public string PricePro { get; set; } = string.Empty;
}
