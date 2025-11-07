using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Application.Services;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;

public class SignIndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly SigningTokenService _tokens;
    public SignIndexModel(AppDbContext db, SigningTokenService tokens) { _db = db; _tokens = tokens; }

    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;

    public Proposal? Proposal { get; set; }
    public string? Error { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        public string SignerName { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string SignerEmail { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = await _tokens.GetValidAsync(Token);
        if (token == null)
        {
            Error = "Invalid or expired link.";
            return Page();
        }
        Proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == token.ProposalId);
        if (Proposal == null)
        {
            Error = "Proposal not found.";
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var token = await _tokens.GetValidAsync(Token);
        if (token == null)
        {
            Error = "Invalid or expired link.";
            return Page();
        }
        var proposal = await _db.Proposals.FirstOrDefaultAsync(p => p.Id == token.ProposalId);
        if (proposal == null)
        {
            Error = "Proposal not found.";
            return Page();
        }
        if (!ModelState.IsValid)
        {
            Proposal = proposal;
            return Page();
        }
        // Minimal signing record for MVP
        _db.Signatures.Add(new Signature
        {
            Id = Guid.NewGuid(),
            ProposalId = proposal.Id,
            SignerName = Input.SignerName,
            SignerEmail = Input.SignerEmail,
            SignedUtc = DateTime.UtcNow
        });
        proposal.Status = ProposalStatus.Signed;
        proposal.SignedUtc = DateTime.UtcNow;
        await _tokens.MarkUsedAsync(token);
        await _db.SaveChangesAsync();
        return RedirectToPage("/Proposals/View", new { id = proposal.Id });
    }
}
