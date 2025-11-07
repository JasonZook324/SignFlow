using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application.Services;

[Authorize(Policy = "OrgMember")]
public class ClientCreateModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICurrentOrganization _org;
    public ClientCreateModel(AppDbContext db, ICurrentOrganization org) { _db = db; _org = org; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public void OnGet() {}

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        if (_org.OrganizationId == null)
        {
            ModelState.AddModelError(string.Empty, "No organization context.");
            return Page();
        }
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = Input.Name,
            Email = Input.Email,
            Phone = Input.Phone,
            OrganizationId = _org.OrganizationId.Value
        };
        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
