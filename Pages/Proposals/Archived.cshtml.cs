using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application.Services;
using SignFlow;

[Authorize(Policy = "OrgMember")]
public class ProposalsArchivedModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICurrentOrganization _org;
    public ProposalsArchivedModel(AppDbContext db, ICurrentOrganization org) { _db = db; _org = org; }

    public List<Proposal> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        if (_org.OrganizationId == null) return;
        Items = await _db.Proposals.IgnoreQueryFilters()
            .Where(p => p.OrganizationId == _org.OrganizationId.Value && (p.IsDeleted || p.Status == ProposalStatus.Archived))
            .OrderByDescending(p => p.DeletedUtc ?? p.SignedUtc ?? p.SentUtc ?? p.PaidUtc ?? p.ExpiresUtc ?? DateTime.UtcNow)
            .Take(200)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostRestoreAsync(Guid id)
    {
        if (_org.OrganizationId == null)
        {
            TempData.Error("No organization context.");
            return RedirectToPage();
        }
        var proposal = await _db.Proposals.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id && p.OrganizationId == _org.OrganizationId.Value);
        if (proposal == null)
        {
            TempData.Error("Proposal not found.");
            return RedirectToPage();
        }
        if (!proposal.IsDeleted)
        {
            TempData.Info("Proposal is not deleted.");
            return RedirectToPage();
        }
        proposal.IsDeleted = false;
        proposal.DeletedUtc = null;
        await _db.SaveChangesAsync();
        TempData.Success("Proposal restored.");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnarchiveAsync(Guid id)
    {
        if (_org.OrganizationId == null)
        {
            TempData.Error("No organization context.");
            return RedirectToPage();
        }
        var proposal = await _db.Proposals.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id && p.OrganizationId == _org.OrganizationId.Value);
        if (proposal == null)
        {
            TempData.Error("Proposal not found.");
            return RedirectToPage();
        }
        if (proposal.Status != ProposalStatus.Archived)
        {
            TempData.Info("Proposal is not archived.");
            return RedirectToPage();
        }
        proposal.Status = ProposalStatus.Draft; // revert to draft or previous status
        await _db.SaveChangesAsync();
        TempData.Success("Proposal unarchived.");
        return RedirectToPage();
    }
}
