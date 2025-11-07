using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application.Services;
using SignFlow.Infrastructure.Email;

[Authorize]
public class ProposalViewModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly SigningTokenService _tokens;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;
    private readonly AuditService _audit;
    private readonly ICurrentOrganization _org;

    public ProposalViewModel(AppDbContext db, SigningTokenService tokens, IEmailSender emailSender, IConfiguration config, AuditService audit, ICurrentOrganization org)
    {
        _db = db; _tokens = tokens; _emailSender = emailSender; _config = config; _audit = audit; _org = org;
    }

    public Proposal? Proposal { get; set; }
    public List<ProposalItem> Items { get; set; } = new();
    public string? SigningLink { get; set; }
    public List<AuditEvent> AuditEvents { get; set; } = new();
    public Payment? LatestPayment { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == id);
        if (Proposal == null) return NotFound();
        if (_org.OrganizationId == null || Proposal.OrganizationId != _org.OrganizationId.Value) return Forbid();
        Items = await _db.ProposalItems.Where(i => i.ProposalId == id).OrderBy(i => i.SortOrder).ToListAsync();
        AuditEvents = await _db.AuditEvents
            .Where(a => a.EntityType == nameof(Proposal) && a.EntityId == id)
            .OrderByDescending(a => a.CreatedUtc)
            .Take(50)
            .ToListAsync();
        LatestPayment = await _db.Payments.Where(p => p.ProposalId == id)
            .OrderByDescending(p => p.PaidUtc ?? DateTime.MinValue)
            .ThenByDescending(p => p.Id)
            .FirstOrDefaultAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSendAsync(Guid id)
    {
        Proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == id);
        if (Proposal == null) return NotFound();
        if (_org.OrganizationId == null || Proposal.OrganizationId != _org.OrganizationId.Value) return Forbid();
        Items = await _db.ProposalItems.Where(i => i.ProposalId == id).OrderBy(i => i.SortOrder).ToListAsync();

        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == Proposal.ClientId && c.OrganizationId == _org.OrganizationId.Value);
        if (client == null)
        {
            ModelState.AddModelError(string.Empty, "Client not found for proposal.");
            await LoadAuditAsync(id);
            return Page();
        }

        var token = await _tokens.CreateAsync(Proposal.Id, TimeSpan.FromHours(24));
        SigningLink = Url.Page(
            pageName: "/Sign/Index",
            pageHandler: null,
            values: new { token = token.Token },
            protocol: Request.Scheme,
            host: Request.Host.Value,
            fragment: null);

        var subject = $"Proposal: {Proposal.Title}";
        var plain = $"Hello {client.Name},\n\nPlease review and sign the proposal: {SigningLink}\n\nTotal: {Proposal.GrandTotal:C}";
        await _emailSender.SendAsync(client.Email, subject, plain);

        if (Proposal.Status == ProposalStatus.Draft)
        {
            Proposal.Status = ProposalStatus.Sent;
            Proposal.SentUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        await _audit.WriteAsync(Proposal.OrganizationId, nameof(Proposal), Proposal.Id, "Sent", new
        {
            client.Name,
            client.Email,
            Link = SigningLink
        });

        await LoadAuditAsync(id);
        LatestPayment = await _db.Payments.Where(p => p.ProposalId == id)
            .OrderByDescending(p => p.PaidUtc ?? DateTime.MinValue)
            .ThenByDescending(p => p.Id)
            .FirstOrDefaultAsync();
        return Page();
    }

    private async Task LoadAuditAsync(Guid id)
    {
        AuditEvents = await _db.AuditEvents
            .Where(a => a.EntityType == nameof(Proposal) && a.EntityId == id)
            .OrderByDescending(a => a.CreatedUtc)
            .Take(50)
            .ToListAsync();
    }
}
