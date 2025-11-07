namespace SignFlow.Domain.Entities;

public enum SubscriptionStatus
{
    Inactive,
    Active,
    PastDue,
    Canceled
}

public class Subscription
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string StripeSubscriptionId { get; set; } = string.Empty;
    public string PlanCode { get; set; } = string.Empty; // Starter, Pro
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Inactive;
    public DateTime? CurrentPeriodEndUtc { get; set; }
}
