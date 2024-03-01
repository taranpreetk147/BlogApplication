using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;

namespace BlogApplication.Pages.Blog
{
    public class CreateblogModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public CreateblogModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [BindProperty]
        public string Title { get; set; }

        [BindProperty]
        public string Content { get; set; }

        [BindProperty]
        public string ImageURL { get; set; }
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("InsertBlogPost", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Title", Title);
                    command.Parameters.AddWithValue("@Content", Content);
                    command.Parameters.AddWithValue("@ImageURL", ImageURL);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        ViewData["Message"] = "Blog post successfully inserted.";
                        return RedirectToPage("/Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to insert blog post.");
                        return Page();
                    }
                }
            }
        }
    }
}


