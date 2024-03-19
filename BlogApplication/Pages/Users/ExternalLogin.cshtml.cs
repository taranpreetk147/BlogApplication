using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace BlogApplication.Pages.Users
{
    public class ExternalLoginModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public ExternalLoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }       
        public string ReturnMessage { get; set; }
        public async Task<IActionResult> OnGetExternalLoginCallbackAsync(string remoteError = null)
        {
            if (remoteError != null)
            {
                ReturnMessage = $"Error from external provider: {remoteError}";
                return Page();
            }

            var info = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (info == null)
            {
                ReturnMessage = "Error loading external login information.";
                return Page();
            }

            var userName = info.Principal.FindFirstValue(ClaimTypes.Email);
            HttpContext.Session.SetString("UserName", userName);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                // Add more claims as needed
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToPage("/Index");
        }
    }
}
