using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using BlogApplication.Model;

namespace BlogApplication.Pages.Product
{
    [ValidateAntiForgeryToken]
    public class ProductDetailModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ProductDetailModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<DisplayProducts> Products { get; set; } = new List<DisplayProducts>();
        public int Rating { get; set; } // Add the Rating property here
        public bool IsUserRated { get; set; }
        public string isUserAuthenticated { get; set; } = "false";

        public IActionResult OnGet(int productId)
        {
            isUserAuthenticated = User.Identity.IsAuthenticated ? "true" : "false";
            IsUserRated = CheckIfUserRatedProduct(productId);
            Products.Clear();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                connection.Open();

                // Fetch product details
                using (SqlCommand command = new SqlCommand("productDetail", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", productId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = new DisplayProducts
                            {
                                Id = productId,
                                Category = reader["Category"].ToString(),
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Amount = Convert.ToDecimal(reader["Amount"]),
                                ImageURL = (byte[])reader["ImageURL"]
                            };

                            Products.Add(product);
                        }
                    }
                }

                // Fetch product rating
                using (SqlCommand command = new SqlCommand("GetProductRating", connection))
                {
                    int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@UserId", userId);
                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        Rating = Convert.ToInt32(result);
                    }
                    else
                    {
                        // Handle the case where no rating is found
                        Rating = 0;
                    }
                }
            }

            return Page();
        }


        public IActionResult OnPostRateProduct(int productId, int rating)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {                
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                connection.Open();
                using (SqlCommand command = new SqlCommand("AddProductRating", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@Rating", rating);
                    command.ExecuteNonQuery();
                }
           
            }
            return RedirectToPage(new { productId = productId, ratingAdded = true });
            //return RedirectToAction("OnGet", new { productId });
        }
        private bool CheckIfUserRatedProduct(int productId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return false;
            }

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("CheckUserProductRating", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ProductId", productId);

                    // Assuming the stored procedure returns a single value indicating whether the user has rated the product
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }
    }
}
