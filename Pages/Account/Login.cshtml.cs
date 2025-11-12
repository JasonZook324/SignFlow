using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SignFlow;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;

    public LoginModel(SignInManager<IdentityUser> signInManager)
    {
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
        public bool RememberMe { get; set; }
    }

    public void OnGet() {}

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            TempData.Error("Please correct the highlighted issues.");
            return Page();
        }
        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            TempData.Success("Signed in successfully.");
            return RedirectToPage("/Index");
        }
        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account is locked. Please try again later.");
            TempData.Error("Account is locked. Please try again later.");
        }
        else if (result.RequiresTwoFactor)
        {
            ModelState.AddModelError(string.Empty, "Two-factor authentication is required.");
            TempData.Info("Two-factor authentication is required.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt. Check your email and password.");
            TempData.Error("Invalid login attempt. Check your email and password.");
        }
        return Page();
    }
}
