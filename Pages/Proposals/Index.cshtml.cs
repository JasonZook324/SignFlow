using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application.Services;

[Authorize(Policy = "OrgMember")]
public class ProposalsIndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICurrentOrganization _org;
    public ProposalsIndexModel(AppDbContext db, ICurrentOrganization org) { _db = db; _org = org; }

    public List<Proposal> Items { get; set; } = new();

    public async Task OnGet()
    {
        if (_org.OrganizationId == null) return;
        Items = await _db.Proposals
            .Where(p => p.OrganizationId == _org.OrganizationId.Value)
            .OrderByDescending(p => p.SentUtc ?? p.PaidUtc ?? p.SignedUtc ?? p.ExpiresUtc ?? DateTime.UtcNow)
            .Take(200)
            .ToListAsync();
    }
}
