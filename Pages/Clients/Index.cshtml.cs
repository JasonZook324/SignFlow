using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;

[Authorize]
public class ClientsIndexModel : PageModel
{
    private readonly AppDbContext _db;
    public ClientsIndexModel(AppDbContext db) { _db = db; }

    public List<Client> Clients { get; set; } = new();

    public async Task OnGet()
    {
        Clients = await _db.Clients.OrderBy(c => c.Name).Take(200).ToListAsync();
    }
}
