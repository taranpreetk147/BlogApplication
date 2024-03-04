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
        //public IActionResult OnPost(string action, int postId)
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToPage("/Users/Login");
        //    }
        //    if (action == "like" || action == "unlike")
        //    {
        //        ToggleLikeStatus(postId, action);
        //        // Reload blog posts after updating like status
        //        LoadBlogPosts();
        //    }
        //    return RedirectToPage();
        //}
        public void OnPost(int postId)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("UpdateLikeStatus", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PostId", postId);
                    command.ExecuteNonQuery();
                }
            }

            Response.Redirect("/Index");
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
                        };

                        BlogPosts.Add(blog);
                    }
                }
            }
        }
        //private void ToggleLikeStatus(int postId, string action)
        //{
        //    using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
        //    {
        //        connection.Open();
        //        using (SqlCommand command = new SqlCommand("LikeStatus", connection))
        //        {
        //            command.CommandType = CommandType.StoredProcedure;
        //            command.Parameters.AddWithValue("@PostId", postId);
        //            command.Parameters.AddWithValue("@Action", action);
        //            command.ExecuteNonQuery();
        //        }
        //    }
        //}
    }

}