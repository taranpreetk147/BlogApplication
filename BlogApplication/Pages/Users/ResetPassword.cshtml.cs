using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Data;
using System.Xml.Linq;

namespace BlogApplication.Pages.Users
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        public ResetPasswordModel(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _contextAccessor = httpContextAccessor;
        }
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            [Required]
            public string Code { get; set; }
            public string ReturnMessage { get; set; }

        }
        public async Task<IActionResult> OnPostAsync()
        {
            string connectionString = _configuration.GetConnectionString("conStr");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("ResetPassword_User", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@email", SqlDbType.NVarChar, 50).Value = Input.Email;
                    command.Parameters.Add("@NewPassword", SqlDbType.NVarChar, 50).Value = Input.Password;
                    command.Parameters.Add("@ConfirmNewPassword", SqlDbType.NVarChar, 50).Value = Input.ConfirmPassword;

                    SqlParameter returnMessageParam = new SqlParameter("@ReturnMessage", SqlDbType.NVarChar, 100);
                    returnMessageParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(returnMessageParam);
                    command.ExecuteNonQuery();

                    Input.ReturnMessage = returnMessageParam.Value.ToString();
                    ViewData["ReturnMessage"] = Input.ReturnMessage;
                }
            }
            return Page();

        }
    }
}
