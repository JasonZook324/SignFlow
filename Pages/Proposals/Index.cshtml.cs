using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;

[Authorize]
public class ProposalsIndexModel : PageModel
{
    private readonly AppDbContext _db;
    public ProposalsIndexModel(AppDbContext db) { _db = db; }

    public List<Proposal> Items { get; set; } = new();

    public async Task OnGet()
    {
        Items = await _db.Proposals
            .OrderByDescending(p => p.SentUtc ?? p.PaidUtc ?? p.SignedUtc ?? p.ExpiresUtc ?? DateTime.UtcNow)
            .Take(200)
            .ToListAsync();
    }
}
