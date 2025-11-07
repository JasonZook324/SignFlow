namespace SignFlow.Domain.Entities;

public class Signature
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public string SignerName { get; set; } = string.Empty;
    public string SignerEmail { get; set; } = string.Empty;
    public DateTime? SignedUtc { get; set; }
    public string? IP { get; set; }
    public string? UserAgent { get; set; }
    public string? ImagePath { get; set; }
    public string? VectorJson { get; set; }
    public string? Hash { get; set; }
}
