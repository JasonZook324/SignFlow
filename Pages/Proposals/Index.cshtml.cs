using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    // Filtering / paging inputs
    [BindProperty(SupportsGet = true)] public ProposalStatus? Status { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? ClientId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;
    public int PageSize { get; } = 25;

    // Output metadata
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public List<Client> Clients { get; set; } = new();

    public async Task OnGetAsync()
    {
        if (_org.OrganizationId == null) return;

        // Base query
        var query = _db.Proposals
            .Where(p => p.OrganizationId == _org.OrganizationId.Value);

        if (Status.HasValue)
            query = query.Where(p => p.Status == Status.Value);
        if (ClientId.HasValue)
            query = query.Where(p => p.ClientId == ClientId.Value);
        if (!string.IsNullOrWhiteSpace(Search))
            query = query.Where(p => p.Title.Contains(Search));

        TotalCount = await query.CountAsync();
        if (Page < 1) Page = 1;
        var skip = (Page - 1) * PageSize;

        Items = await query
            .OrderByDescending(p => p.SentUtc ?? p.PaidUtc ?? p.SignedUtc ?? p.ExpiresUtc ?? DateTime.UtcNow)
            .Skip(skip)
            .Take(PageSize)
            .ToListAsync();

        Clients = await _db.Clients
            .Where(c => c.OrganizationId == _org.OrganizationId.Value)
            .OrderBy(c => c.Name)
            .Take(500)
            .ToListAsync();
    }
}
