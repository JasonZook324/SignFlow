namespace SignFlow.Domain.Entities;

public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed,
    Canceled
}

public class Payment
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public string? StripeCheckoutSessionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidUtc { get; set; }
    public string? Method { get; set; }
    public string? ReceiptUrl { get; set; }
}
