using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Application.Services;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application.Services;

[Authorize(Policy = "OrgMember")]
public class ProposalPayModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly PaymentService _payments;
    private readonly ICurrentOrganization _org;

    public ProposalPayModel(AppDbContext db, PaymentService payments, ICurrentOrganization org)
    {
        _db = db; _payments = payments; _org = org;
    }

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

        var successUrl = Url.Page(pageName: "/Proposals/PaySuccess", pageHandler: null, values: new { id }, protocol: Request.Scheme, host: Request.Host.Value, fragment: null);
        var cancelUrl = Url.Page(pageName: "/Proposals/View", pageHandler: null, values: new { id }, protocol: Request.Scheme, host: Request.Host.Value, fragment: null);
        var session = await _payments.CreateCheckoutSessionAsync(proposal, successUrl!, cancelUrl!);
        return Redirect(session.Url!);
    }
}
