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

    public ProposalViewModel(AppDbContext db, SigningTokenService tokens, IEmailSender emailSender, IConfiguration config)
    {
        _db = db; _tokens = tokens; _emailSender = emailSender; _config = config;
    }

    public Proposal? Proposal { get; set; }
    public List<ProposalItem> Items { get; set; } = new();
    public string? SigningLink { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        Proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == id);
        if (Proposal != null)
            Items = await _db.ProposalItems.Where(i => i.ProposalId == id).OrderBy(i => i.SortOrder).ToListAsync();
    }

    public async Task<IActionResult> OnPostSendAsync(Guid id)
    {
        Proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == id);
        if (Proposal == null) return NotFound();
        Items = await _db.ProposalItems.Where(i => i.ProposalId == id).OrderBy(i => i.SortOrder).ToListAsync();

        // For MVP assume client email available from first client with proposal.ClientId
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == Proposal.ClientId);
        if (client == null)
        {
            ModelState.AddModelError(string.Empty, "Client not found for proposal.");
            return Page();
        }

        // Generate signing token (24h expiry)
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

        // Update status -> Sent if Draft
        if (Proposal.Status == ProposalStatus.Draft)
        {
            Proposal.Status = ProposalStatus.Sent;
            Proposal.SentUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return Page();
    }
}
