using BlogApplication.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Policy;

namespace BlogApplication.Pages.Users
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailSender _emailSender;
        public LoginModel(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IEmailSender emailSender)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
        }

        public string ReturnMessage { get; set; }
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync(string userName, string password)
        {
            string returnUrl = Request.Query["ReturnUrl"];
            int userId = 0;
            string userRole = "";
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                using (SqlCommand command = new SqlCommand("UserLogin", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@userName", userName);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.Add("@ReturnMessage", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                    command.Parameters.Add("@UserId", SqlDbType.Int).Direction = ParameterDirection.Output;
                    command.Parameters.Add("@UserRole", SqlDbType.NVarChar, 50).Direction = ParameterDirection.Output;
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    ReturnMessage = command.Parameters["@ReturnMessage"].Value.ToString();
                    if (ReturnMessage == "You have successfully logged in!!")
                    {
                        userId = Convert.ToInt32(command.Parameters["@UserId"].Value);
                        userRole = command.Parameters["@UserRole"].Value.ToString();
                    }
                    else
                    {
                        ViewData["Message"] = ReturnMessage;
                    }

                    if (ReturnMessage == "You have successfully logged in!!")
                    {
                        // Store user ID and username in session
                        HttpContext.Session.SetInt32("UserId", userId);
                        HttpContext.Session.SetString("UserName", userName);
                        HttpContext.Session.SetString("UserRole", userRole);

                        var claims = new List<Claim>
                        {
                        new Claim(ClaimTypes.Name, userName),
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim(ClaimTypes.Role,userRole),
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20)
                        };

                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            ViewData["Message"] = ReturnMessage;
                            return Redirect(returnUrl); // Redirect to the originally requested page
                        }
                        else
                        {
                            ViewData["Message"] = ReturnMessage;
                            if(userRole == "Admin")
                            {
                                return RedirectToPage("/Product/AddProduct");
                            }
                            else 
                            {
                                return RedirectToPage("/Index");
                            }           
                        }
                    }
                }
            }
            return Page();
        }
        public async Task<IActionResult> OnPostSendVerificationEmail()
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Please Login first!";
                return RedirectToPage("/Users/Login"); // Redirect to the login page
            }

            // Get the current user's email address
            string userName = User.Identity.Name;

            // Construct the verification link
            string verificationLink = $"{Request.Scheme}://{Request.Host}/ConfirmEmail?email={userName}";
            string message = $"Please confirm your email address by clicking <a href='{verificationLink}'>here</a>.";
            string subject = "Confirm your email address";

            // Send the verification email
            await _emailSender.SendEmailConfirmationAsync(userName, message,subject);

            return RedirectToPage("/Index");
        }
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _httpContextAccessor.HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
        public IActionResult OnPostGoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Page("./ExternalLogin", pageHandler: "ExternalLoginCallback") };
            return Challenge(properties, "Google");
        }      
    }
}