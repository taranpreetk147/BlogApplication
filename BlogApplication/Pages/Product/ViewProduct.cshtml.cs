using BlogApplication.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Data;

namespace BlogApplication.Pages.Product
{
    public class ViewProductModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public ViewProductModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public List<DisplayProducts>Products { get; set; } = new List<DisplayProducts>();
        public void OnGet()
        {
            Products.Clear();
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("conStr")))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("DisplayProduct", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var product = new DisplayProducts
                        {
                            Id= reader.GetInt32("Id"),
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
        }
    }
}
