using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Session;

namespace BlogApplication.Pages.Users
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ReturnMessage { get; set; }

        public IActionResult OnPost(string userName, string password)
        {          
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                using (SqlCommand command = new SqlCommand("UserLogin", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@userName", userName);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.Add("@ReturnMessage", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                    connection.Open();           
                    command.ExecuteNonQuery();
                    ReturnMessage = command.Parameters["@ReturnMessage"].Value.ToString();                 
                }
            }

            ViewData["Message"] = ReturnMessage;

            return Page();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            return RedirectToPage("/Index");
        }
    }
}
