using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application.Services;

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
}
