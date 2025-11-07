namespace SignFlow.Domain.Entities;

public class SigningToken
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public string Token { get; set; } = string.Empty; // public link token
    public DateTime ExpiresUtc { get; set; }
    public DateTime? UsedUtc { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
