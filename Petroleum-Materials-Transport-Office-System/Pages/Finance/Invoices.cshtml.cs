using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Petroleum_Materials_Transport_Office_System.Pages.Finance
{
    public class InvoicesModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public InvoicesModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ViewModel matches the columns in your GUI
        public class InvoiceViewModel
        {
            public string InvoiceNumber { get; set; } // Primary Key in your DB
            public string ClientName { get; set; }    // From Company Table
            public string ProviderName { get; set; }  // From Provider Table
            public string FuelType { get; set; }      // From Fuel_Type Table via Orders
            public DateTime Date { get; set; }        // Issue_Date
            public string Location { get; set; }      // Unloading_Location from Orders
            public decimal CompanyAmount { get; set; }
            public decimal ProviderAmount { get; set; }
            public decimal NetAmount { get; set; }
            public string Status { get; set; }        // 'Paid', 'Unpaid', etc.
        }

        public List<InvoiceViewModel> InvoicesList { get; set; } = new List<InvoiceViewModel>();

        public void OnGet()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Complex Query to join all your tables correctly based on your Schema
            string query = @"
                SELECT 
                    inv.Invoice_Number,
                    inv.Issue_Date,
                    inv.Company_Amount,
                    inv.Provider_Amount,
                    inv.Net_Amount,
                    inv.Payment_Status,
                    c.Company_Name AS ClientName,
                    p.Provider_Name AS ProviderName,
                    ft.Type_Name AS FuelType,
                    loc.Location_Name AS LocationName
                FROM Invoice inv
                LEFT JOIN Orders o ON inv.Order_ID = o.Order_ID
                LEFT JOIN Company c ON inv.Company_ID = c.Company_ID
                LEFT JOIN Provider p ON inv.Provider_ID = p.Provider_ID
                LEFT JOIN Fuel_Type ft ON o.Petroleum_Type = ft.Fuel_ID
                LEFT JOIN Location loc ON o.Unloading_Location = loc.Location_Code
                ORDER BY inv.Issue_Date DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var inv = new InvoiceViewModel
                            {
                                InvoiceNumber = reader["Invoice_Number"].ToString(),
                                Date = Convert.ToDateTime(reader["Issue_Date"]),
                                CompanyAmount = reader["Company_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Company_Amount"]) : 0,
                                ProviderAmount = reader["Provider_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Provider_Amount"]) : 0,
                                NetAmount = reader["Net_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Net_Amount"]) : 0,
                                Status = reader["Payment_Status"].ToString(),

                                // Handling potential NULLs from Joins
                                ClientName = reader["ClientName"] != DBNull.Value ? reader["ClientName"].ToString() : "N/A",
                                ProviderName = reader["ProviderName"] != DBNull.Value ? reader["ProviderName"].ToString() : "N/A",
                                FuelType = reader["FuelType"] != DBNull.Value ? reader["FuelType"].ToString() : "N/A",
                                Location = reader["LocationName"] != DBNull.Value ? reader["LocationName"].ToString() : "N/A"
                            };

                            InvoicesList.Add(inv);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // In a real app, log this error
                Console.WriteLine("Error fetching invoices: " + ex.Message);
            }
        }
    }
}