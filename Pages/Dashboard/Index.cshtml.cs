using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly AppDbContext _db;

    public DashboardModel(AppDbContext db) { _db = db; }

    public int ClientCount { get; set; }
    public int DraftCount { get; set; }
    public int SentCount { get; set; }
    public int SignedCount { get; set; }

    public async Task OnGet()
    {
        // For now aggregate across all orgs. Later: scope to current organization.
        ClientCount = await _db.Clients.CountAsync();
        DraftCount = await _db.Proposals.CountAsync(p => p.Status == ProposalStatus.Draft);
        SentCount = await _db.Proposals.CountAsync(p => p.Status == ProposalStatus.Sent);
        SignedCount = await _db.Proposals.CountAsync(p => p.Status == ProposalStatus.Signed);
    }
}
