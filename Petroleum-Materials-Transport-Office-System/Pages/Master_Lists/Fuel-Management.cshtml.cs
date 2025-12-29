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

        // --- Input Models for Add/Edit ---
        [BindProperty] public OrderInputDto OrderInput { get; set; }
        [BindProperty] public FuelInputDto FuelInput { get; set; }

        public void OnGet()
        {
            try
            {
                // Initialize input models
                OrderInput = new OrderInputDto();
                FuelInput = new FuelInputDto();

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

        // ==========================================================
        //  POST HANDLERS - ORDERS (Add, Update, Delete)
        // ==========================================================
        public IActionResult OnPostSaveOrder()
        {
            ActiveTab = "orders";

            if (OrderInput == null)
                return RedirectToPage(new { ActiveTab = "orders" });

            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                SqlCommand cmd;

                // Check if this is an update (invoice number exists)
                bool isUpdate = !string.IsNullOrEmpty(OrderInput.InvoiceNumber);

                if (!isUpdate) // INSERT
                {
                    string query = @"
                        INSERT INTO Orders (Company_ID, Provider_ID, Order_Date, Loading_Location, Unloading_Location, 
                                          Loading_Quantity, Shortage, Status, Created_At)
                        VALUES (@CompanyId, @ProviderId, @Date, @Loading, @Unloading, @Quantity, @Shortage, @Status, @Created);
                        
                        DECLARE @OrderId INT = SCOPE_IDENTITY();
                        
                        INSERT INTO Invoice (Order_ID, Company_ID, Issue_Date, Invoice_Number, Payment_Status, Created_At)
                        VALUES (@OrderId, @CompanyId, @Date, @InvoiceNum, 'Pending', @Created)";

                    cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Created", DateTime.Now);
                    cmd.Parameters.AddWithValue("@InvoiceNum", OrderInput.InvoiceNumber ?? "INV-" + DateTime.Now.Ticks);
                }
                else // UPDATE
                {
                    string query = @"
                        UPDATE Orders 
                        SET Company_ID = @CompanyId, Order_Date = @Date, Loading_Location = @Loading, 
                            Unloading_Location = @Unloading, Loading_Quantity = @Quantity, 
                            Shortage = @Shortage, Status = @Status
                        FROM Orders o
                        INNER JOIN Invoice i ON o.Order_ID = i.Order_ID
                        WHERE i.Invoice_Number = @InvoiceNum";

                    cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@InvoiceNum", OrderInput.InvoiceNumber);
                }

                cmd.Parameters.AddWithValue("@CompanyId", OrderInput.CompanyId);
                cmd.Parameters.AddWithValue("@ProviderId", 1); // Default provider, adjust as needed
                cmd.Parameters.AddWithValue("@Date", OrderInput.Date);
                cmd.Parameters.AddWithValue("@Loading", OrderInput.Loading ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Unloading", OrderInput.Unloading ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", OrderInput.Quantity);
                cmd.Parameters.AddWithValue("@Shortage", OrderInput.Shortage);
                cmd.Parameters.AddWithValue("@Status", OrderInput.Status ?? "Pending");

                cmd.ExecuteNonQuery();
            }

            // Clear the input model
            OrderInput = new OrderInputDto();

            return RedirectToPage(new { ActiveTab = "orders" });
        }

        public IActionResult OnPostDeleteOrder(string id)
        {
            ActiveTab = "orders";
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                // First get the Order_ID
                string getOrderQuery = "SELECT Order_ID FROM Invoice WHERE Invoice_Number = @InvoiceNum";
                SqlCommand getCmd = new SqlCommand(getOrderQuery, conn);
                getCmd.Parameters.AddWithValue("@InvoiceNum", id);

                object orderId = getCmd.ExecuteScalar();

                if (orderId != null)
                {
                    // Delete invoice first (FK constraint)
                    string deleteInvoiceQuery = "DELETE FROM Invoice WHERE Invoice_Number = @InvoiceNum";
                    SqlCommand delInvCmd = new SqlCommand(deleteInvoiceQuery, conn);
                    delInvCmd.Parameters.AddWithValue("@InvoiceNum", id);
                    delInvCmd.ExecuteNonQuery();

                    // Then delete order
                    string deleteOrderQuery = "DELETE FROM Orders WHERE Order_ID = @OrderId";
                    SqlCommand delOrdCmd = new SqlCommand(deleteOrderQuery, conn);
                    delOrdCmd.Parameters.AddWithValue("@OrderId", orderId);
                    delOrdCmd.ExecuteNonQuery();
                }
            }
            return RedirectToPage(new { ActiveTab = "orders" });
        }

        // ==========================================================
        //  POST HANDLERS - FUEL (Add, Update, Delete)
        // ==========================================================
        public IActionResult OnPostSaveFuel()
        {
            ActiveTab = "fuel";

            if (FuelInput == null || string.IsNullOrWhiteSpace(FuelInput.Name))
            {
                return RedirectToPage(new { ActiveTab = "fuel" });
            }

            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();

                // Check if fuel type exists
                int exists = 0;
                string checkQuery = "SELECT COUNT(*) FROM Fuel_Type WHERE Type_Name = @Name";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Name", FuelInput.Name);
                    exists = (int)checkCmd.ExecuteScalar();
                }

                // Now perform INSERT or UPDATE
                string query;
                if (exists == 0) // INSERT
                {
                    query = @"INSERT INTO Fuel_Type (Type_Name, Company_Price_Per_Unit, Provider_Price_Per_Unit, 
                                                   Tax_Percentage, Additional_Stamp_Fee, Status, Created_At) 
                             VALUES (@Name, @CompPrice, @ProvPrice, @Tax, @Stamp, @Status, @Created)";
                }
                else // UPDATE
                {
                    query = @"UPDATE Fuel_Type 
                             SET Company_Price_Per_Unit = @CompPrice, Provider_Price_Per_Unit = @ProvPrice, 
                                 Tax_Percentage = @Tax, Additional_Stamp_Fee = @Stamp, Status = @Status 
                             WHERE Type_Name = @Name";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", FuelInput.Name);
                    cmd.Parameters.AddWithValue("@CompPrice", FuelInput.CompanyPrice);
                    cmd.Parameters.AddWithValue("@ProvPrice", FuelInput.ContractorPrice);
                    cmd.Parameters.AddWithValue("@Tax", FuelInput.Tax);
                    cmd.Parameters.AddWithValue("@Stamp", FuelInput.Stamp);
                    cmd.Parameters.AddWithValue("@Status", FuelInput.IsActive ? "Active" : "Inactive");

                    if (exists == 0) // Only add Created_At for INSERT
                    {
                        cmd.Parameters.AddWithValue("@Created", DateTime.Now);
                    }

                    cmd.ExecuteNonQuery();
                }
            }

            // Clear the input model
            FuelInput = new FuelInputDto();

            return RedirectToPage(new { ActiveTab = "fuel" });
        }

        public IActionResult OnPostDeleteFuel(string id)
        {
            ActiveTab = "fuel";
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                string query = "DELETE FROM Fuel_Type WHERE Type_Name = @Name";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToPage(new { ActiveTab = "fuel" });
        }

        // ==========================================================
        //  AJAX HANDLERS (For Details Modal)
        // ==========================================================
        public JsonResult OnGetOrderDetails(string id)
        {
            try
            {
                OrderDto result = null;
                using (SqlConnection conn = new SqlConnection(_connString))
                {
                    string sql = @"
                        SELECT 
                            i.Invoice_Number, 
                            o.Order_Date, 
                            c.Company_Name,
                            c.Company_ID,
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
                        WHERE i.Invoice_Number = @Id";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            double qty = r["Loading_Quantity"] != DBNull.Value ? Convert.ToDouble(r["Loading_Quantity"]) : 0;
                            double shortage = r["Shortage"] != DBNull.Value ? Convert.ToDouble(r["Shortage"]) : 0;

                            result = new OrderDto
                            {
                                InvoiceNumber = r["Invoice_Number"].ToString(),
                                Date = Convert.ToDateTime(r["Order_Date"]),
                                Customer = r["Company_Name"].ToString(),
                                Contractor = r["Provider_Name"] != DBNull.Value ? r["Provider_Name"].ToString() : "-",
                                Loading = r["LoadLoc"] != DBNull.Value ? r["LoadLoc"].ToString() : "-",
                                Unloading = r["UnloadLoc"] != DBNull.Value ? r["UnloadLoc"].ToString() : "-",
                                Quantity = qty,
                                Shortage = shortage,
                                Net = qty - shortage,
                                Status = r["Status"].ToString(),
                                CompanyId = r["Company_ID"] != DBNull.Value ? Convert.ToInt32(r["Company_ID"]) : 0
                            };
                        }
                    }
                }
                return new JsonResult(result);
            }
            catch (Exception ex) { return new JsonResult(new { error = ex.Message }); }
        }

        public JsonResult OnGetFuelDetails(string id)
        {
            try
            {
                FuelDto result = null;
                using (SqlConnection conn = new SqlConnection(_connString))
                {
                    string sql = "SELECT * FROM Fuel_Type WHERE Type_Name = @Name";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Name", id);

                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            result = new FuelDto
                            {
                                Name = r["Type_Name"].ToString(),
                                CompanyPrice = r["Company_Price_Per_Unit"] != DBNull.Value ? Convert.ToDecimal(r["Company_Price_Per_Unit"]) : 0,
                                ContractorPrice = r["Provider_Price_Per_Unit"] != DBNull.Value ? Convert.ToDecimal(r["Provider_Price_Per_Unit"]) : 0,
                                Tax = r["Tax_Percentage"] != DBNull.Value ? Convert.ToDouble(r["Tax_Percentage"]) : 0,
                                Stamp = r["Additional_Stamp_Fee"] != DBNull.Value ? Convert.ToDouble(r["Additional_Stamp_Fee"]) : 0,
                                GPS = 0,
                                IsActive = r["Status"].ToString() == "Active"
                            };
                        }
                    }
                }
                return new JsonResult(result);
            }
            catch (Exception ex) { return new JsonResult(new { error = ex.Message }); }
        }

        // ==========================================================
        //  PRIVATE LOADING METHODS
        // ==========================================================
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

        // ==========================================================
        //  DTO CLASSES
        // ==========================================================
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
            public int CompanyId { get; set; }
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

        // ==========================================================
        //  INPUT DTO CLASSES (For Add/Edit Forms)
        // ==========================================================
        public class OrderInputDto
        {
            public string InvoiceNumber { get; set; }
            public DateTime Date { get; set; }
            public int CompanyId { get; set; }
            public string Contractor { get; set; }
            public string Loading { get; set; }
            public string Unloading { get; set; }
            public double Quantity { get; set; }
            public double Shortage { get; set; }
            public string Status { get; set; }
        }

        public class FuelInputDto
        {
            public string Name { get; set; }
            public decimal CompanyPrice { get; set; }
            public decimal ContractorPrice { get; set; }
            public double Tax { get; set; }
            public double Stamp { get; set; }
            public bool IsActive { get; set; }
        }
    }
}