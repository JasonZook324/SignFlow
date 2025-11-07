using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Domain.Entities;
using SignFlow.Application.Services;

[Authorize(Policy = "OrgOwner")]
public class ProposalDeleteModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICurrentOrganization _org;
    public ProposalDeleteModel(AppDbContext db, ICurrentOrganization org) { _db = db; _org = org; }

    public Proposal? Proposal { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == id);
        if (Proposal == null) return NotFound();
        if (_org.OrganizationId == null || Proposal.OrganizationId != _org.OrganizationId.Value) return Forbid();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == id);
        if (proposal == null) return NotFound();
        if (_org.OrganizationId == null || proposal.OrganizationId != _org.OrganizationId.Value) return Forbid();
        var items = _db.ProposalItems.Where(i => i.ProposalId == id);
        _db.ProposalItems.RemoveRange(items);
        _db.Proposals.Remove(proposal);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
