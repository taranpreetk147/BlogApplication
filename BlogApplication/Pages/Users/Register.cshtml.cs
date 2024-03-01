using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Data;

namespace BlogApplication.Pages.Users
{
    public class RegisterModel : PageModel
    {
        private readonly IConfiguration _configuraetion;
        public RegisterModel(IConfiguration configuration)
        {
            _configuraetion = configuration;
        }
        public string ReturnMessage { get; set; }
        public IActionResult OnPost(string userName, string emailAddress, string address, string password)
        {
            using (SqlConnection connection = new SqlConnection(_configuraetion.GetConnectionString("conStr")))
            {
                using (SqlCommand command = new SqlCommand("UserRegister", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", userName);
                    command.Parameters.AddWithValue("@EmailAddress", emailAddress);
                    command.Parameters.AddWithValue("@Address", address);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.Add("@ReturnMessage", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                    connection.Open();
                    command.ExecuteNonQuery();

                    ReturnMessage = command.Parameters["@ReturnMessage"].Value.ToString();
                }

            }
            ViewData["Message"] = ReturnMessage;
            return Page();
        }
    }
}

