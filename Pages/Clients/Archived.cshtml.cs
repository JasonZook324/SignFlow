using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Application.Services;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using SignFlow;

[Authorize(Policy = "OrgMember")]
public class ClientsArchivedModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICurrentOrganization _org;
    private readonly AuditService _audit;
    public ClientsArchivedModel(AppDbContext db, ICurrentOrganization org, AuditService audit) { _db = db; _org = org; _audit = audit; }

    public List<Client> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        if (_org.OrganizationId == null) return;
        Items = await _db.Clients.IgnoreQueryFilters()
            .Where(c => c.OrganizationId == _org.OrganizationId.Value && c.IsDeleted)
            .OrderByDescending(c => c.DeletedUtc ?? DateTime.UtcNow)
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
        var client = await _db.Clients.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == _org.OrganizationId.Value);
        if (client == null)
        {
            TempData.Error("Client not found.");
            return RedirectToPage();
        }
        if (!client.IsDeleted)
        {
            TempData.Info("Client is already active.");
            return RedirectToPage();
        }
        client.IsDeleted = false;
        client.DeletedUtc = null;
        await _db.SaveChangesAsync();
        await _audit.WriteAsync(_org.OrganizationId.Value, "Client", client.Id, "Restore", new { client.Name, client.Email });
        TempData.Success("Client restored.");
        return RedirectToPage();
    }
}
