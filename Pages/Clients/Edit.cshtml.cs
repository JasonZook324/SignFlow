using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;

[Authorize]
public class ClientEditModel : PageModel
{
    private readonly AppDbContext _db;
    public ClientEditModel(AppDbContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var c = await _db.Clients.FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return NotFound();
        Input = new InputModel { Id = c.Id, Name = c.Name, Email = c.Email, Phone = c.Phone };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var c = await _db.Clients.FirstOrDefaultAsync(x => x.Id == Input.Id);
        if (c == null) return NotFound();
        c.Name = Input.Name; c.Email = Input.Email; c.Phone = Input.Phone;
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
