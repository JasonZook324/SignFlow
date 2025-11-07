using System.Text.Json;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;

namespace SignFlow.Application.Services;

public class AuditService
{
    private readonly AppDbContext _db;
    public AuditService(AppDbContext db) { _db = db; }

    public async Task WriteAsync(Guid organizationId, string entityType, Guid entityId, string eventType, object? data = null, CancellationToken ct = default)
    {
        var evt = new AuditEvent
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            EntityType = entityType,
            EntityId = entityId,
            EventType = eventType,
            DataJson = data == null ? null : JsonSerializer.Serialize(data),
            CreatedUtc = DateTime.UtcNow
        };
        _db.AuditEvents.Add(evt);
        await _db.SaveChangesAsync(ct);
    }
}
