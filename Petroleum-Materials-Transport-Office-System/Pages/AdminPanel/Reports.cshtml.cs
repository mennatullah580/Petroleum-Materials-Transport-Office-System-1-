// Pages/AdminPanel/ReportsModel.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
namespace Petroleum_Materials_Transport_Office_System.Pages.AdminPanel
{
    public class ReportsModel : PageModel
    {
        private readonly string _connectionString =
          "Server=.; Database=PetroleumTransportDB; Integrated Security=True; TrustServerCertificate=True;";
        public List<OrderStat> OrderStats { get; set; } = new();
        public void OnGet(string activeTab = null)
        {
            OrderStats = GetOrderStats();
        }
        private List<OrderStat> GetOrderStats()
        {
            var stats = new List<OrderStat>();
            using var con = new SqlConnection(_connectionString);
            con.Open();
            // ✅ Use YOUR column names: Order_Date (not OrderDate)
            string query = @"
                SELECT
                    CAST(Order_Date AS DATE) as OrderDay,
                    COUNT(*) as OrderCount
                FROM Orders
                WHERE Order_Date >= DATEADD(DAY, -30, GETDATE())
                GROUP BY CAST(Order_Date AS DATE)
                ORDER BY OrderDay";
            using var cmd = new SqlCommand(query, con);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                stats.Add(new OrderStat
                {
                    Date = reader.GetDateTime("OrderDay").ToString("yyyy-MM-dd"),
                    Count = reader.GetInt32("OrderCount")
                });
            }
            return stats;
        }
        public class OrderStat
        {
            public string Date { get; set; } = "";
            public int Count { get; set; }
        }
    }
}