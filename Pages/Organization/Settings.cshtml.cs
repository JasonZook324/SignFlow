using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Infrastructure.Persistence;
using SignFlow.Application.Services;
using SignFlow.Domain.Entities;

[Authorize(Policy = "OrgOwner")]
public class OrgSettingsModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICurrentOrganization _org;
    public OrgSettingsModel(AppDbContext db, ICurrentOrganization org) { _db = db; _org = org; }

    public Organization? Organization { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Slug { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (_org.OrganizationId == null) return NotFound();
        Organization = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == _org.OrganizationId);
        if (Organization == null) return NotFound();
        Input = new InputModel { Name = Organization.Name, Slug = Organization.Slug };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (_org.OrganizationId == null) return NotFound();
        Organization = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == _org.OrganizationId);
        if (Organization == null) return NotFound();
        if (!ModelState.IsValid) return Page();
        Organization.Name = Input.Name.Trim();
        Organization.Slug = Input.Slug.Trim().ToLowerInvariant();
        await _db.SaveChangesAsync();
        return RedirectToPage("/Dashboard/Index");
    }
}
