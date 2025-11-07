using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SignFlow.Application.Options;
using SignFlow.Application.Services;
using SignFlow.Domain.Entities;
using SignFlow.Infrastructure.Email;
using SignFlow.Infrastructure.Persistence;

[Authorize(Policy = "OrgOwner")]
public class OrgMembersModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ICurrentOrganization _org;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;

    public OrgMembersModel(AppDbContext db, ICurrentOrganization org, UserManager<IdentityUser> userManager, IEmailSender emailSender)
    {
        _db = db; _org = org; _userManager = userManager; _emailSender = emailSender;
    }

    public record MemberRow(string UserId, string Email, string Role);

    public List<MemberRow> Members { get; set; } = new();

    [BindProperty]
    public InviteModel Invite { get; set; } = new();

    public string? Message { get; set; }

    public class InviteModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = "Member";
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (_org.OrganizationId == null) return NotFound();
        var orgId = _org.OrganizationId.Value;
        Members = await (from ou in _db.OrganizationUsers
                         join u in _db.Set<IdentityUser>() on ou.UserId equals u.Id
                         where ou.OrganizationId == orgId
                         select new MemberRow(ou.UserId, u.Email!, ou.Role)).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostInviteAsync()
    {
        if (_org.OrganizationId == null) return NotFound();
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }
        var email = Invite.Email.Trim().ToLowerInvariant();
        var role = Invite.Role;
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = false };
            var pwd = Guid.NewGuid().ToString("N").Substring(0, 12) + "!a1";
            var create = await _userManager.CreateAsync(user, pwd);
            if (!create.Succeeded)
            {
                foreach (var e in create.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                await OnGetAsync();
                return Page();
            }
            // Email user with temp password
            await _emailSender.SendAsync(email, "You've been invited to SignFlow", $"Your temporary password is: {pwd}");
        }
        var exists = await _db.OrganizationUsers.AnyAsync(x => x.OrganizationId == _org.OrganizationId && x.UserId == user.Id);
        if (!exists)
        {
            _db.OrganizationUsers.Add(new OrganizationUser
            {
                Id = Guid.NewGuid(),
                OrganizationId = _org.OrganizationId.Value,
                UserId = user.Id,
                Role = role
            });
            await _db.SaveChangesAsync();
        }
        Message = $"Invited {email} ({role}).";
        return await OnGetAsync();
    }

    public async Task<IActionResult> OnPostRemoveAsync(string userId)
    {
        if (_org.OrganizationId == null) return NotFound();
        var member = await _db.OrganizationUsers.FirstOrDefaultAsync(x => x.OrganizationId == _org.OrganizationId && x.UserId == userId);
        if (member == null) return NotFound();
        if (member.Role == "Owner")
        {
            ModelState.AddModelError(string.Empty, "Cannot remove an Owner. Transfer ownership first.");
            await OnGetAsync();
            return Page();
        }
        _db.OrganizationUsers.Remove(member);
        await _db.SaveChangesAsync();
        Message = "Member removed.";
        return await OnGetAsync();
    }
}
