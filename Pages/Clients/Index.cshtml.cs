using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application.Services;
using Microsoft.AspNetCore.Mvc;
using SignFlow;

[Authorize(Policy = "OrgMember")]
public class ClientsIndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICurrentOrganization _org;
    public ClientsIndexModel(AppDbContext db, ICurrentOrganization org) { _db = db; _org = org; }

    public List<Client> Clients { get; set; } = new();

    public async Task OnGet()
    {
        if (_org.OrganizationId == null) return;
        Clients = await _db.Clients
            .Where(c => c.OrganizationId == _org.OrganizationId.Value)
            .OrderBy(c => c.Name)
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
        TempData.Success("Client restored.");
        return RedirectToPage();
    }
}
