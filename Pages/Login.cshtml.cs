using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Supabase;

namespace BudgetTrackingNew.Pages;

public class LoginModel(Client supabaseClient) : PageModel
{
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var session = await supabaseClient.Auth.SignIn(Email, Password);

            if (session?.User != null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, session.User.Id!),
                    new(ClaimTypes.Email, session.User.Email!),
                    new("AccessToken", session.AccessToken!)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToPage("/Index");
            }

            ErrorMessage = "Přihlášení selhalo.";
        }
        catch (Exception ex)
        {
            ErrorMessage = "Chyba: " + ex.Message;
        }

        return Page();
    }
}
