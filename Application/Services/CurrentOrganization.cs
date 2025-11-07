using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SignFlow.Infrastructure.Persistence;

namespace SignFlow.Application.Services;

public interface ICurrentOrganization
{
    Guid? OrganizationId { get; }
}

public class CurrentOrganization : ICurrentOrganization
{
    public Guid? OrganizationId { get; internal set; }
}

public class CurrentOrganizationMiddleware
{
    private readonly RequestDelegate _next;
    public CurrentOrganizationMiddleware(RequestDelegate next) { _next = next; }

    public async Task InvokeAsync(HttpContext context, AppDbContext db, CurrentOrganization currentOrg)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var orgId = await db.OrganizationUsers
                    .Where(o => o.UserId == userId)
                    .OrderBy(o => o.Role == "Owner" ? 0 : 1)
                    .Select(o => (Guid?)o.OrganizationId)
                    .FirstOrDefaultAsync();
                currentOrg.OrganizationId = orgId;
            }
        }
        await _next(context);
    }
}

public static class CurrentOrganizationExtensions
{
    public static IApplicationBuilder UseCurrentOrganization(this IApplicationBuilder app)
        => app.UseMiddleware<CurrentOrganizationMiddleware>();
}
