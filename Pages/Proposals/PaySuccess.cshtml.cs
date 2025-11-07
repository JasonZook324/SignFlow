using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Domain.Entities;
using SignFlow.Application.Services;

[Authorize]
public class ProposalPaySuccessModel : PageModel
{
    private readonly AppDbContext _db;

    public ProposalPaySuccessModel(AppDbContext db) { _db = db; }

    public Proposal? Proposal { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        // Mark proposal as paid if any succeeded payment is recorded (webhook should be source of truth later)
        Proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == id);
        if (Proposal != null)
        {
            Proposal.Status = ProposalStatus.Paid;
            Proposal.PaidUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
