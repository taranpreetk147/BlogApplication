using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace BlogApplication.Pages.Users
{
    public class RegisterModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public RegisterModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string ReturnMessage { get; set; }
        [BindProperty]
        public string UserName { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public string ReturnUrl { get; set; }

        public IActionResult OnPostRegister(string userName, string password)
        {
            string userRole = "User";
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                using (SqlCommand command = new SqlCommand("UserRegister", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", userName);
                    //command.Parameters.AddWithValue("@EmailAddress", emailAddress);
                    //command.Parameters.AddWithValue("@Address", address);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@UserRole", userRole);
                    command.Parameters.Add("@ReturnMessage", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                    command.Parameters.Add("@UserExists", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    connection.Open();
                    command.ExecuteNonQuery();

                    ReturnMessage = command.Parameters["@ReturnMessage"].Value.ToString();
                }

            }
            ViewData["Message"] = ReturnMessage;
            return Page();
        }

        public IActionResult OnGetCheckUserNameAvailability(string userName)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                using (SqlCommand command = new SqlCommand("CheckUserName", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", userName);
                    command.Parameters.Add("@ReturnMessage", SqlDbType.NVarChar, 100).Direction = ParameterDirection.Output;
                    command.Parameters.Add("@UserExists", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    connection.Open();
                    command.ExecuteNonQuery();

                    bool userExists = (bool)command.Parameters["@UserExists"].Value;

                    if (userExists)
                    {
                        // User already registered
                        return new JsonResult(new { ReturnMessage = command.Parameters["@ReturnMessage"].Value.ToString() });
                    }
                    else
                    {
                        // User is available
                        return new JsonResult(new { ReturnMessage = command.Parameters["@ReturnMessage"].Value.ToString() });
                    }
                }
            }
        }
        public IActionResult OnPostGoogleRegister()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Page("./Register", pageHandler: "ExternalRegisterCallback") };
            return Challenge(properties, "Google");
        }
        public async Task<IActionResult> OnGetExternalRegisterCallbackAsync(string email, string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    UserName = info.Principal.FindFirstValue(ClaimTypes.Email);
                }
                return RedirectToAction("OnPost");
            }
        }
    }
}

//hrjikgh