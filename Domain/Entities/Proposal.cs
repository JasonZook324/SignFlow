namespace SignFlow.Domain.Entities;

public enum ProposalStatus
{
    Draft,
    Sent,
    Viewed,
    Signed,
    Paid,
    Archived
}

public class Proposal
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid ClientId { get; set; }
    public ProposalStatus Status { get; set; } = ProposalStatus.Draft;
    public string Title { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public DateTime? SentUtc { get; set; }
    public DateTime? SignedUtc { get; set; }
    public DateTime? PaidUtc { get; set; }
    public DateTime? ExpiresUtc { get; set; }
}
