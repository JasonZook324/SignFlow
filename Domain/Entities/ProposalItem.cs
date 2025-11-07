namespace SignFlow.Domain.Entities;

public class ProposalItem
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public int SortOrder { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public bool Taxable { get; set; } = true;
    public decimal? DiscountRate { get; set; }
}
