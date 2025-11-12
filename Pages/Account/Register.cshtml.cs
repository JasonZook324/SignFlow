using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SignFlow;

public class RegisterModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public RegisterModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet() {}

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            TempData.Error("Please correct the highlighted issues.");
            return Page();
        }
        var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
        var result = await _userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData.Success("Account created successfully. You are now signed in.");
            return RedirectToPage("/Index");
        }
        bool passwordIssue = false;
        foreach (var error in result.Errors)
        {
            if (error.Code.StartsWith("Password", System.StringComparison.OrdinalIgnoreCase)) passwordIssue = true;
            ModelState.AddModelError(string.Empty, error.Description);
        }
        TempData.Error(passwordIssue
            ? "Password requirements: at least 8 characters, include a number."
            : "Registration failed. Please review errors.");
        return Page();
    }
}
