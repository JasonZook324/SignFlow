using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Application.Services;
using SignFlow.Infrastructure.Persistence;

namespace SignFlow.Pages.Proposals;

[Authorize]
public class PdfModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly PdfService _pdf;
    public PdfModel(AppDbContext db, PdfService pdf) { _db = db; _pdf = pdf; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == id);
        if (proposal == null) return NotFound();
        var items = await _db.ProposalItems.Where(i => i.ProposalId == id).OrderBy(i => i.SortOrder).ToListAsync();
        var bytes = _pdf.RenderProposal(proposal, items);
        return File(bytes, "application/pdf", $"proposal-{proposal.Id}.pdf");
    }
}
