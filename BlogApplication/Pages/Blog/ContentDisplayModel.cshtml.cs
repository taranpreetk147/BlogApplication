using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BlogApplication.Pages.Blog
{
    public class ContentDisplayModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ContentDisplayModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Content { get; set; }
        public byte[] ImageData { get; set; }
        public List<Comment> Comments { get; set; }

        public class Comment
        {
            public string UserName { get; set; }
            public string CommentText { get; set; }
            public DateTime CommentDate { get; set; }
        }

        public IActionResult OnGet(int postId)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                connection.Open();

                // Retrieve blog post content and image data
                using (SqlCommand command = new SqlCommand("SELECT Content, ImageURL FROM BlogPosts WHERE PostId = @PostId", connection))
                {
                    command.Parameters.AddWithValue("@PostId", postId);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        Content = reader["Content"].ToString();
                        ImageData = (byte[])reader["ImageURL"];
                    }
                    reader.Close(); // Close the SqlDataReader after reading the data
                }             
                // Retrieve comments for the specific blog post
                Comments = new List<Comment>();
                using (SqlCommand commentCommand = new SqlCommand("DisplayComments", connection))
                {              
                    commentCommand.CommandType = CommandType.StoredProcedure;
                    commentCommand.Parameters.AddWithValue("@PostId", postId);

                    using (SqlDataReader commentReader = commentCommand.ExecuteReader())
                    {
                        while (commentReader.Read())
                        {
                            Comments.Add(new Comment
                            {
                                UserName = commentReader["UserName"].ToString(),
                                CommentText = commentReader["CommentText"].ToString(),
                                CommentDate = (DateTime)commentReader["CommentDate"]
                            });
                        }
                    }
                }

                return Page();
            }
        }

        public IActionResult OnPost(string userName, string commentText, int postId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Users/Login");
            }
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("AddComment", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", userName);
                    command.Parameters.AddWithValue("@CommentText", commentText);
                    command.Parameters.AddWithValue("@PostId", postId);
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToPage(new { postId });
        }
    }
}
