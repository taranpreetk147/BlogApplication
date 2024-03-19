using BlogApplication.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;

namespace BlogApplication.Pages.Users
{
    public class ProfileModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public ProfileModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string ReturnMessage { get; set; }
        public IActionResult OnGet()
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToPage("/Users/Login");
                }

                string userName = HttpContext.Session.GetString("UserName") ;

                connection.Open();
                using (SqlCommand command = new SqlCommand("GetUserProfile", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", userName); // Pass the UserId parameter to the stored procedure

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        UserName = reader["UserName"].ToString();
                        EmailAddress = reader["EmailAddress"].ToString();
                        Address = reader["Address"].ToString();                      
                    }
                }
                return Page();
            }
        }

        public IActionResult OnPost(string userName)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToPage("/Users/Login");
                }
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                connection.Open();
                using (SqlCommand command = new SqlCommand("UpdateUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@UserName", userName);
                    //command.Parameters.AddWithValue("@EmailAddress", emailAddress);
                    //command.Parameters.AddWithValue("@Address", address);
                    SqlParameter returnMessageParam = command.Parameters.Add("@ReturnMessage", SqlDbType.NVarChar, 100);
                    returnMessageParam.Direction = ParameterDirection.Output;
                    returnMessageParam.Direction = ParameterDirection.Output;
                    // Add this line to execute the query
                    command.ExecuteNonQuery();
                    ReturnMessage = returnMessageParam.Value.ToString();
                    ViewData["ReturnMessage"] = ReturnMessage;
                }
            }
            return Page();
        }
        public IActionResult OnPostChangePassword(string userName, string currentPassword, string newPassword)
        {
            using (SqlConnection connection =new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                connection.Open();
                using (SqlCommand command=new SqlCommand("ChangePassword", connection))
                {                   
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@UserName", SqlDbType.NVarChar, 50).Value = userName;
                    command.Parameters.Add("@CurrentPassword", SqlDbType.NVarChar, 50).Value = currentPassword;
                    command.Parameters.Add("@NewPassword", SqlDbType.NVarChar, 50).Value = newPassword;

                    SqlParameter returnMessageParam = new SqlParameter("@ReturnMessage", SqlDbType.NVarChar, 100);
                    returnMessageParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(returnMessageParam);
                    command.ExecuteNonQuery();

                    ReturnMessage = returnMessageParam.Value.ToString();
                    ViewData["ReturnMessage"]= ReturnMessage;
                }
            }
            return Page();

        }
    }
    }

