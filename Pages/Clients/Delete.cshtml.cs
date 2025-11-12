using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Application.Services;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using SignFlow;

[Authorize(Policy = "OrgMember")]
public class ClientDeleteModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICurrentOrganization _org;
    public ClientDeleteModel(AppDbContext db, ICurrentOrganization org) { _db = db; _org = org; }

    public Client? Client { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Client = await _db.Clients.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);
        if (Client == null) return NotFound();
        if (_org.OrganizationId == null || Client.OrganizationId != _org.OrganizationId.Value) return Forbid();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var client = await _db.Clients.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);
        if (client == null) return NotFound();
        if (_org.OrganizationId == null || client.OrganizationId != _org.OrganizationId.Value) return Forbid();
        if (client.IsDeleted)
        {
            TempData.Info("Client already deleted.");
            return RedirectToPage("Index");
        }
        // Optional: prevent if proposals exist (soft delete still okay)
        client.IsDeleted = true;
        client.DeletedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData.Success("Client deleted (soft). You can restore it later if needed.");
        return RedirectToPage("Index");
    }
}
