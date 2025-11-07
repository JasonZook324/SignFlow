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
    private readonly ICurrentOrganization _org;

    public ProposalPaySuccessModel(AppDbContext db, ICurrentOrganization org) { _db = db; _org = org; }

    public Proposal? Proposal { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        Proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == id);
        if (Proposal == null) return;
        if (_org.OrganizationId == null || Proposal.OrganizationId != _org.OrganizationId.Value) { Proposal = null; return; }
        Proposal.Status = ProposalStatus.Paid;
        Proposal.PaidUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
