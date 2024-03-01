using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using BlogApplication.Model;

namespace BlogApplication.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
          
        }
        
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageURL { get; set; }
        public List<BlogPostModel> BlogPosts { get; set; } = new List<BlogPostModel>();

        public IActionResult OnGet()
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("DisplayAllBlogPosts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    SqlDataReader reader = command.ExecuteReader();
                    BlogPosts = new List<BlogPostModel>();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var blog = new BlogPostModel
                                {
                                    Title = reader.GetString(1),
                                    Content = reader.GetString(2),
                                    ImageURL = reader.GetString(3)
                                };
                                BlogPosts.Add(blog);
                            }
                        }                   
                }

                return Page();
            }
        }
    }
}
