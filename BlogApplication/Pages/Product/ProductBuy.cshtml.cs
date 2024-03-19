using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;

namespace BlogApplication.Pages.Product
{
    [Authorize]
    public class ProductBuyModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public ProductBuyModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }        
        public IActionResult OnPost(int productId, int quantity, decimal totalAmount)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToPage("/Users/Login");
                }
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                connection.Open();
                using (SqlCommand command = new SqlCommand("AddOrderDetail", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@Quantity", quantity);
                    command.Parameters.AddWithValue("@TotalAmount", totalAmount);

                    command.ExecuteNonQuery();
                }

                return Page();
            }
        }
    }
}

    