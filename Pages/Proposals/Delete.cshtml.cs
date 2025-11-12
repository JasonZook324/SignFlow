using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Domain.Entities;
using SignFlow.Application.Services;
using SignFlow;

[Authorize(Policy = "OrgOwner")]
public class ProposalDeleteModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICurrentOrganization _org;
    private readonly AuditService _audit;
    public ProposalDeleteModel(AppDbContext db, ICurrentOrganization org, AuditService audit) { _db = db; _org = org; _audit = audit; }

    public Proposal? Proposal { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Proposal = await _db.Proposals.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
        if (Proposal == null) return NotFound();
        if (_org.OrganizationId == null || Proposal.OrganizationId != _org.OrganizationId.Value) return Forbid();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var proposal = await _db.Proposals.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
        if (proposal == null) return NotFound();
        if (_org.OrganizationId == null || proposal.OrganizationId != _org.OrganizationId.Value) return Forbid();
        if (proposal.IsDeleted)
        {
            TempData.Info("Proposal already deleted.");
            return RedirectToPage("Index");
        }
        proposal.IsDeleted = true;
        proposal.DeletedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(_org.OrganizationId.Value, "Proposal", proposal.Id, "SoftDelete", new { proposal.Title });
        TempData.Success("Proposal deleted (soft). You can restore it later if needed.");
        return RedirectToPage("Index");
    }
}
