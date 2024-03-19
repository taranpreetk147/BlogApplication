using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;

namespace BlogApplication.Pages.Product
{
    public class AddProductModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public AddProductModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [BindProperty]
        public string name { get; set; }
        [BindProperty]
        public string category { get; set; }

        [BindProperty]
        public string description { get; set; }
        [BindProperty]
        public int quantity { get; set; }
        [BindProperty]
        public decimal amount { get; set; }
        [BindProperty]
        public IFormFile ImageFile { get; set; }        
            
        public IActionResult OnPost()
        {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                byte[] imageBytes = null;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        ImageFile.CopyTo(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }
                }
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("AddProduct", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Category", category);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Quantity", quantity);
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@ImageURL", imageBytes);
                    int rowsEffected= command.ExecuteNonQuery();
                    if(rowsEffected > 0)
                    {
                        ViewData["Message"] = "Blog post successfully inserted.";
                        return RedirectToPage("/Product/ViewProduct");
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
