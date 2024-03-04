using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogApplication.Pages.Users
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginModel(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string ReturnMessage { get; set; }

        public async Task<IActionResult> OnPostAsync(string userName, string password)
        {
            string returnUrl = Request.Query["ReturnUrl"];

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                using (SqlCommand command = new SqlCommand("UserLogin", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@userName", userName);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.Add("@ReturnMessage", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                    command.Parameters.Add("@UserId", SqlDbType.Int).Direction = ParameterDirection.Output;

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    ReturnMessage = command.Parameters["@ReturnMessage"].Value.ToString();

                    if (ReturnMessage == "User successfully logged in!!")
                    {
                        int userId = Convert.ToInt32(command.Parameters["@UserId"].Value);
                        _httpContextAccessor.HttpContext.Session.SetInt32("UserId", userId);
                        _httpContextAccessor.HttpContext.Session.SetString("UserName", userName);

                        var claims = new[] { new Claim(ClaimTypes.Name, userName) };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true // Make the cookie persistent if needed
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl); // Redirect to the originally requested page
                        }
                        else
                        {
                            return RedirectToPage("/Index"); // Redirect to a default page if no return URL is provided
                        }
                    }
                }
            }

            ViewData["Message"] = ReturnMessage;

            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _httpContextAccessor.HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }
    }
}