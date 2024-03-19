using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;

namespace BlogApplication.Pages
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public bool EmailConfirmed { get; set; }

        public ConfirmEmailModel(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor= httpContextAccessor;
        }

        public async Task<IActionResult> OnGetAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Index");
            }

            // string email = HttpContext.Session.GetString("Email");
            string connectionString = _configuration.GetConnectionString("conStr");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("ConfirmEmail", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@EmailAddress", email);
                    command.ExecuteNonQuery();
                }
            }
            return Page();
        }
    }
}