namespace SignFlow.Domain.Entities;

public class AuditEvent
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? DataJson { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
