namespace SignFlow.Domain.Entities;

public class OrganizationUser
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member"; // Owner, Member
}
