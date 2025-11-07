using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SignFlow.Infrastructure.Persistence;

namespace SignFlow.Application.Security;

public sealed class OrgMemberRequirement : IAuthorizationRequirement { }
public sealed class OrgOwnerRequirement : IAuthorizationRequirement { }

public sealed class OrgMemberAuthorizationHandler : AuthorizationHandler<OrgMemberRequirement>
{
    private readonly AppDbContext _db;
    public OrgMemberAuthorizationHandler(AppDbContext db) { _db = db; }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrgMemberRequirement requirement)
    {
        var userId = (context.User?.FindFirstValue(ClaimTypes.NameIdentifier)) ?? string.Empty;
        if (string.IsNullOrEmpty(userId)) return;

        // If user belongs to any organization, grant. You can refine by current org via middleware if needed.
        var isMember = await _db.OrganizationUsers.AnyAsync(o => o.UserId == userId);
        if (isMember)
            context.Succeed(requirement);
    }
}

public sealed class OrgOwnerAuthorizationHandler : AuthorizationHandler<OrgOwnerRequirement>
{
    private readonly AppDbContext _db;
    public OrgOwnerAuthorizationHandler(AppDbContext db) { _db = db; }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrgOwnerRequirement requirement)
    {
        var userId = (context.User?.FindFirstValue(ClaimTypes.NameIdentifier)) ?? string.Empty;
        if (string.IsNullOrEmpty(userId)) return;
        var isOwner = await _db.OrganizationUsers.AnyAsync(o => o.UserId == userId && o.Role == "Owner");
        if (isOwner)
            context.Succeed(requirement);
    }
}
