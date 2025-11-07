namespace SignFlow.Domain.Entities;

public class OrganizationUser
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string UserId { get; set; } = string.Empty; // Identity user id (string)
    public string Role { get; set; } = "Member"; // Owner, Member
}
