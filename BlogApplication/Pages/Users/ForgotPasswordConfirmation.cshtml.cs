using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogApplication.Pages.Users
{
    public class ForgotPasswordConfirmationModel : PageModel
    {
        [AllowAnonymous]
        public class ForgotPasswordConfirmation : PageModel
        {
            public void OnGet()
            {
            }
        }
    }
}
