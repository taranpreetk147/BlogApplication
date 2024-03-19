using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace BlogApplication.Pages.Users
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        public ForgotPasswordModel(IConfiguration configuration, IEmailSender emailSender)
        {
            _configuration = configuration;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                string connectionString = _configuration.GetConnectionString("conStr");
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string verificationLink = $"{Request.Scheme}://{Request.Host}/Users/ResetPassword?email={Input.Email}";
                    string message = $"Please Reset your password by clicking <a href='{verificationLink}'>here</a>.";
                    string subject = "Reset Password";

                    await _emailSender.SendEmailConfirmationAsync(Input.Email, message, subject);
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }
            }

            return Page();
        }
    }
}
