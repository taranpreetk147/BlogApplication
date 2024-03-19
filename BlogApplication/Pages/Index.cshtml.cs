using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using BlogApplication.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        public List<BlogPostModel> BlogPosts { get; set; } = new List<BlogPostModel>();

        public IActionResult OnGet()
        {
            LoadBlogPosts();
            return Page();
        }
        public IActionResult OnPostLike(int postId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Users/Login");
            }
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == null)
            {
                // Handle the case where userId is not found in the session
                return RedirectToPage("/Users/Login");
            }
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("UpdateLikeStatusAndCount", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PostId", postId);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.ExecuteNonQuery();
                }
            }
            LoadBlogPosts();
            return RedirectToPage();
        }
        private void LoadBlogPosts()
        {
            BlogPosts.Clear(); // Clear existing blog posts before reloading

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("DisplayAllBlogPosts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var blog = new BlogPostModel
                        {
                            PostId = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Content = reader.GetString(2),
                            ImageURL = reader.IsDBNull(3) ? null : (byte[])reader["ImageURL"],
                            Likes = reader.GetInt32(4),
                            IsLiked = reader.GetBoolean(5),
                        };

                        BlogPosts.Add(blog);
                    }
                }
            }
        }
    }

}