using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace Petroleum_Materials_Transport_Office_System.Pages.Master_Lists
{
    public class FuelManagementModel : PageModel
    {
        private readonly string _connString = "Server=.; Database=PetroleumTransportDB; Integrated Security=True; TrustServerCertificate=True;";

        // --- Data Properties ---
        public List<OrderDto> Orders { get; set; } = new List<OrderDto>();
        public List<FuelDto> Fuels { get; set; } = new List<FuelDto>();
        public List<SelectListItem> Customers { get; set; } = new List<SelectListItem>();

        // --- Search Filters ---
        [BindProperty(SupportsGet = true)] public string InvoiceNum { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
        [BindProperty(SupportsGet = true)] public string CustomerFilter { get; set; }
        [BindProperty(SupportsGet = true)] public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)] public string FuelNameSearch { get; set; }
        [BindProperty(SupportsGet = true)] public string FuelStatus { get; set; }

        [BindProperty(SupportsGet = true)] public string ActiveTab { get; set; } = "orders";

        public void OnGet()
        {
            try
            {
                // 1. Determine which tab to show based on search parameters
                if (!string.IsNullOrEmpty(FuelNameSearch) || !string.IsNullOrEmpty(FuelStatus))
                    ActiveTab = "fuel";

                // 2. Load Data
                LoadCustomers(); // Loads from 'Company' table
                LoadOrders();    // Loads from 'Orders', joins Company, Provider, Location, Invoice
                LoadFuel();      // Loads from 'Fuel_Type' table
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database Error: {ex.Message}");
            }
        }

        private void LoadOrders()
        {
            Orders.Clear();
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                // This query joins all the necessary tables defined in your new schema
                string sql = @"
                    SELECT 
                        i.Invoice_Number, 
                        o.Order_Date, 
                        c.Company_Name, 
                        p.Provider_Name, 
                        l1.Location_Name AS LoadLoc, 
                        l2.Location_Name AS UnloadLoc,
                        o.Loading_Quantity, 
                        o.Shortage, 
                        o.Status
                    FROM Orders o
                    LEFT JOIN Invoice i ON o.Order_ID = i.Order_ID
                    LEFT JOIN Company c ON o.Company_ID = c.Company_ID
                    LEFT JOIN Provider p ON o.Provider_ID = p.Provider_ID
                    LEFT JOIN Location l1 ON o.Loading_Location = l1.Location_Code
                    LEFT JOIN Location l2 ON o.Unloading_Location = l2.Location_Code
                    WHERE 1=1";

                // --- Apply Filters ---
                if (!string.IsNullOrEmpty(InvoiceNum)) sql += " AND i.Invoice_Number LIKE @Inv";
                if (DateFrom.HasValue) sql += " AND o.Order_Date >= @D1";
                if (DateTo.HasValue) sql += " AND o.Order_Date <= @D2";
                if (!string.IsNullOrEmpty(CustomerFilter) && CustomerFilter != "الكل") sql += " AND c.Company_ID = @Cust";

                // Status Filter Logic
                if (!string.IsNullOrEmpty(StatusFilter) && StatusFilter != "الكل")
                {
                    // Map Arabic UI status to English DB status if necessary
                    // Assuming DB uses English: Pending, Delivered, etc.
                    sql += " AND o.Status = @Stat";
                }

                sql += " ORDER BY o.Order_Date DESC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(InvoiceNum)) cmd.Parameters.AddWithValue("@Inv", "%" + InvoiceNum + "%");
                if (DateFrom.HasValue) cmd.Parameters.AddWithValue("@D1", DateFrom.Value);
                if (DateTo.HasValue) cmd.Parameters.AddWithValue("@D2", DateTo.Value);
                if (!string.IsNullOrEmpty(CustomerFilter) && CustomerFilter != "الكل") cmd.Parameters.AddWithValue("@Cust", CustomerFilter);
                if (!string.IsNullOrEmpty(StatusFilter) && StatusFilter != "الكل") cmd.Parameters.AddWithValue("@Stat", StatusFilter);

                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        double qty = r["Loading_Quantity"] != DBNull.Value ? Convert.ToDouble(r["Loading_Quantity"]) : 0;
                        double shortage = r["Shortage"] != DBNull.Value ? Convert.ToDouble(r["Shortage"]) : 0;

                        Orders.Add(new OrderDto
                        {
                            InvoiceNumber = r["Invoice_Number"] != DBNull.Value ? r["Invoice_Number"].ToString() : "-",
                            Date = r["Order_Date"] != DBNull.Value ? Convert.ToDateTime(r["Order_Date"]) : DateTime.MinValue,
                            Customer = r["Company_Name"] != DBNull.Value ? r["Company_Name"].ToString() : "-",
                            Contractor = r["Provider_Name"] != DBNull.Value ? r["Provider_Name"].ToString() : "-",
                            Loading = r["LoadLoc"] != DBNull.Value ? r["LoadLoc"].ToString() : "-",
                            Unloading = r["UnloadLoc"] != DBNull.Value ? r["UnloadLoc"].ToString() : "-",
                            Quantity = qty,
                            Shortage = shortage,
                            Net = qty - shortage,
                            Status = r["Status"] != DBNull.Value ? r["Status"].ToString() : "Pending"
                        });
                    }
                }
            }
        }

        private void LoadFuel()
        {
            Fuels.Clear();
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                // Load from Fuel_Type table
                string sql = "SELECT * FROM Fuel_Type WHERE 1=1";

                if (!string.IsNullOrEmpty(FuelNameSearch)) sql += " AND Type_Name LIKE @Name";

                if (!string.IsNullOrEmpty(FuelStatus) && FuelStatus != "الكل")
                {
                    // Map UI "Active" (Arabic: نشط) to DB "Active"
                    sql += " AND Status = @Stat";
                }

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(FuelNameSearch)) cmd.Parameters.AddWithValue("@Name", "%" + FuelNameSearch + "%");

                if (!string.IsNullOrEmpty(FuelStatus) && FuelStatus != "الكل")
                {
                    string statusParam = (FuelStatus == "نشط") ? "Active" : "Inactive";
                    cmd.Parameters.AddWithValue("@Stat", statusParam);
                }

                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Fuels.Add(new FuelDto
                        {
                            Name = r["Type_Name"].ToString(),
                            CompanyPrice = r["Company_Price_Per_Unit"] != DBNull.Value ? Convert.ToDecimal(r["Company_Price_Per_Unit"]) : 0,
                            ContractorPrice = r["Provider_Price_Per_Unit"] != DBNull.Value ? Convert.ToDecimal(r["Provider_Price_Per_Unit"]) : 0,
                            Tax = r["Tax_Percentage"] != DBNull.Value ? Convert.ToDouble(r["Tax_Percentage"]) : 0,
                            Stamp = r["Additional_Stamp_Fee"] != DBNull.Value ? Convert.ToDouble(r["Additional_Stamp_Fee"]) : 0,
                            // GPS fees are on the Provider table in your schema, not Fuel_Type, so we set this to 0 for this view
                            GPS = 0,
                            IsActive = r["Status"].ToString() == "Active"
                        });
                    }
                }
            }
        }

        private void LoadCustomers()
        {
            Customers.Add(new SelectListItem("الكل", "الكل"));
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                // Selects from Company Table
                SqlCommand cmd = new SqlCommand("SELECT Company_ID, Company_Name FROM Company ORDER BY Company_Name", conn);
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Customers.Add(new SelectListItem(r["Company_Name"].ToString(), r["Company_ID"].ToString()));
                    }
                }
            }
        }

        // --- DTO Classes ---
        public class OrderDto
        {
            public string InvoiceNumber { get; set; }
            public DateTime Date { get; set; }
            public string Customer { get; set; }
            public string Contractor { get; set; }
            public string Loading { get; set; }
            public string Unloading { get; set; }
            public double Quantity { get; set; }
            public double Shortage { get; set; }
            public double Net { get; set; }
            public string Status { get; set; }
        }

        public class FuelDto
        {
            public string Name { get; set; }
            public decimal CompanyPrice { get; set; }
            public decimal ContractorPrice { get; set; }
            public double Tax { get; set; }
            public double Stamp { get; set; }
            public decimal GPS { get; set; }
            public bool IsActive { get; set; }
        }
    }
}