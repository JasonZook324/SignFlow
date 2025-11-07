namespace SignFlow.Domain.Entities;

public class ProposalTemplate
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string JsonDefinition { get; set; } = string.Empty; // future rich template schema
}
