using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Petroleum_Materials_Transport_Office_System.Models;

namespace Petroleum_Materials_Transport_Office_System.Pages.OrdersManagement
{
    public class IndexModel : PageModel
    {
        private readonly string _connectionString = "Server=DESKTOP-1QHK872;Database=PetroleumTransportDB;Trusted_Connection=True;TrustServerCertificate=True;"; // حط الـ Connection String بتاعك هنا


        public List<Order> Orders { get; set; } = new List<Order>();

        [BindProperty(SupportsGet = true)]
        public string SearchInvoiceNumber { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchLoadingLocation { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchUnloadingLocation { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchProviderName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchPetroleumType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? SearchDateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? SearchDateTo { get; set; }

        [BindProperty]
        public Order NewOrder { get; set; }

        public List<string> LoadingLocations { get; set; } = new List<string>();
        public List<string> UnloadingLocations { get; set; } = new List<string>();
        public List<string> PetroleumTypes { get; set; } = new List<string>();
        public List<string> Providers { get; set; } = new List<string>();
        public List<string> Vehicles { get; set; } = new List<string>();

        public void OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Role")))
            {
                Response.Redirect("/Login");
                return;
            }

            LoadDropdownData();
            LoadOrders();
        }

        private void LoadDropdownData()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // جلب مواقع التحميل والتفريغ
                string locationQuery = "SELECT DISTINCT Location_Name FROM [dbo].[Location] WHERE Status = 'Active'";
                using (SqlCommand cmd = new SqlCommand(locationQuery, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string location = reader["Location_Name"].ToString();
                        LoadingLocations.Add(location);
                        UnloadingLocations.Add(location);
                    }
                }

                // جلب أنواع الوقود
                string fuelQuery = "SELECT Type_name FROM [dbo].[Fuel_Type] WHERE Status = 'Active'";
                using (SqlCommand cmd = new SqlCommand(fuelQuery, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PetroleumTypes.Add(reader["Type_name"].ToString());
                    }
                }

                // جلب المقاولين
                string providerQuery = "SELECT Provider_Name FROM [dbo].[Provider] WHERE Status = 'Active'";
                using (SqlCommand cmd = new SqlCommand(providerQuery, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Providers.Add(reader["Provider_Name"].ToString());
                    }
                }

                // جلب السيارات
                string vehicleQuery = "SELECT Plate_number FROM [dbo].[Vehicle] WHERE Status = 'Active'";
                using (SqlCommand cmd = new SqlCommand(vehicleQuery, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Vehicles.Add(reader["Plate_number"].ToString());
                    }
                }
            }
        }

        private void LoadOrders()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        o.Order_ID,
                        o.Order_Date,
                        o.Delivery_Date,
                        o.Status,
                        o.Loading_Quantity,
                        o.Unloading_Quantity,
                        o.Shortage,
                        p.Provider_Name,
                        v.Plate_number,
                        d.Name as Driver_Name,
                        ft.Type_Name,
                        ll.Location_Name as Loading_Location_Name,
                        ul.Location_Name as Unloading_Location_Name,
                        i.Invoice_Number,
                        f.Company_Price,
                        f.Provider_Price,
                        f.Company_Total,
                        f.Provider_Total,
                        f.Tax_Amount,
                        f.Stamp_Fee,
                        f.GPS_Fee,
                        f.Total_Deduction,
                        f.Advance_Payment,
                        f.Custody_Amount,
                        f.Net_Amount,
                        f.Balance
                    FROM [dbo].[Orders] o
                    LEFT JOIN [dbo].[Provider] p ON o.Provider_ID = p.Provider_ID
                    LEFT JOIN [dbo].[Vehicle] v ON o.Vehicle_ID = v.Vehicle_ID
                    LEFT JOIN [dbo].[Driver] d ON o.Driver_ID = d.Driver_ID
                    LEFT JOIN [dbo].[Fuel_Type] ft ON o.Petroleum_Type = ft.Fuel_ID
                    LEFT JOIN [dbo].[Location] ll ON o.Loading_Location = ll.Location_Code
                    LEFT JOIN [dbo].[Location] ul ON o.Unloading_Location = ul.Location_Code
                    LEFT JOIN [dbo].[Invoice] i ON o.Order_ID = i.Order_ID
                    LEFT JOIN [dbo].[Financials] f ON o.Order_ID = f.Order_ID
                    WHERE 1=1";

                // تطبيق الفلاتر
                if (!string.IsNullOrEmpty(SearchInvoiceNumber))
                    query += " AND i.Invoice_Number LIKE @InvoiceNumber";
                if (!string.IsNullOrEmpty(SearchLoadingLocation))
                    query += " AND ll.Location_Name = @LoadingLocation";
                if (!string.IsNullOrEmpty(SearchUnloadingLocation))
                    query += " AND ul.Location_Name = @UnloadingLocation";
                if (!string.IsNullOrEmpty(SearchProviderName))
                    query += " AND p.Provider_Name = @ProviderName";
                if (!string.IsNullOrEmpty(SearchPetroleumType))
                    query += " AND ft.Type_Name = @PetroleumType";
                if (!string.IsNullOrEmpty(SearchStatus))
                    query += " AND o.Status = @Status";
                if (SearchDateFrom.HasValue)
                    query += " AND o.Order_date >= @DateFrom";
                if (SearchDateTo.HasValue)
                    query += " AND o.Order_date <= @DateTo";

                query += " ORDER BY o.Order_date DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(SearchInvoiceNumber))
                        cmd.Parameters.AddWithValue("@InvoiceNumber", "%" + SearchInvoiceNumber + "%");
                    if (!string.IsNullOrEmpty(SearchLoadingLocation))
                        cmd.Parameters.AddWithValue("@LoadingLocation", SearchLoadingLocation);
                    if (!string.IsNullOrEmpty(SearchUnloadingLocation))
                        cmd.Parameters.AddWithValue("@UnloadingLocation", SearchUnloadingLocation);
                    if (!string.IsNullOrEmpty(SearchProviderName))
                        cmd.Parameters.AddWithValue("@ProviderName", SearchProviderName);
                    if (!string.IsNullOrEmpty(SearchPetroleumType))
                        cmd.Parameters.AddWithValue("@PetroleumType", SearchPetroleumType);
                    if (!string.IsNullOrEmpty(SearchStatus))
                        cmd.Parameters.AddWithValue("@Status", SearchStatus);
                    if (SearchDateFrom.HasValue)
                        cmd.Parameters.AddWithValue("@DateFrom", SearchDateFrom.Value);
                    if (SearchDateTo.HasValue)
                        cmd.Parameters.AddWithValue("@DateTo", SearchDateTo.Value);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new Order
                            {
                                OrderId = Convert.ToInt32(reader["Order_ID"]),
                                InvoiceNumber = reader["Invoice_Number"]?.ToString() ?? "",
                                OrderDate = reader["Order_date"] != DBNull.Value ? Convert.ToDateTime(reader["Order_date"]) : DateTime.Now,
                                LoadingLocation = reader["Loading_Location_Name"]?.ToString() ?? "",
                                UnloadingLocation = reader["Unloading_Location_Name"]?.ToString() ?? "",
                                PetroleumType = reader["Type_Name"]?.ToString() ?? "",
                                LoadingQuantity = reader["Loading_Quantity"] != DBNull.Value ? Convert.ToDecimal(reader["Loading_Quantity"]) : 0,
                                UnloadingQuantity = reader["Unloading_Quantity"] != DBNull.Value ? Convert.ToDecimal(reader["Unloading_Quantity"]) : 0,
                                Shortage = reader["Shortage"] != DBNull.Value ? Convert.ToDecimal(reader["Shortage"]) : 0,
                                ProviderName = reader["Provider_Name"]?.ToString() ?? "",
                                VehiclePlateNumber = reader["Plate_number"]?.ToString() ?? "",
                                DriverName = reader["Driver_Name"]?.ToString() ?? "",
                                Status = reader["Status"]?.ToString() ?? "",
                                DeliveryDate = reader["Delivery_Date"] != DBNull.Value ? Convert.ToDateTime(reader["Delivery_Date"]) : DateTime.Now,

                                CompanyPrice = reader["Company_Price"] != DBNull.Value ? Convert.ToDecimal(reader["Company_Price"]) : 0,
                                ProviderPrice = reader["Provider_Price"] != DBNull.Value ? Convert.ToDecimal(reader["Provider_Price"]) : 0,
                                CompanyTotal = reader["Company_Total"] != DBNull.Value ? Convert.ToDecimal(reader["Company_Total"]) : 0,
                                ProviderTotal = reader["Provider_Total"] != DBNull.Value ? Convert.ToDecimal(reader["Provider_Total"]) : 0,
                                TaxAmount = reader["Tax_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Tax_Amount"]) : 0,
                                StampFee = reader["Stamp_Fee"] != DBNull.Value ? Convert.ToDecimal(reader["Stamp_Fee"]) : 0,
                                GPSFee = reader["GPS_Fee"] != DBNull.Value ? Convert.ToDecimal(reader["GPS_Fee"]) : 0,
                                TotalDeductions = reader["Total_Deduction"] != DBNull.Value ? Convert.ToDecimal(reader["Total_Deduction"]) : 0,
                                AdvancePayment = reader["Advance_Payment"] != DBNull.Value ? Convert.ToDecimal(reader["Advance_Payment"]) : 0,
                                CustodyAmount = reader["Custody_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Custody_Amount"]) : 0,
                                NetAmount = reader["Net_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Net_Amount"]) : 0,
                                Balance = reader["Balance"] != DBNull.Value ? Convert.ToDecimal(reader["Balance"]) : 0
                            };

                            Orders.Add(order);
                        }
                    }
                }
            }
        }

        public IActionResult OnPostCreateOrder()
        {
            if (NewOrder == null)
            {
                TempData["Error"] = "يرجى إدخال بيانات الطلب";
                return RedirectToPage();
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // الحصول على IDs
                            int providerId = GetProviderIdByName(NewOrder.ProviderName, conn, transaction);
                            int vehicleId = GetVehicleIdByPlate(NewOrder.VehiclePlateNumber, conn, transaction);
                            int fuelTypeId = GetFuelTypeIdByName(NewOrder.PetroleumType, conn, transaction);
                            string loadingLocationCode = GetLocationCodeByName(NewOrder.LoadingLocation, conn, transaction);
                            string unloadingLocationCode = GetLocationCodeByName(NewOrder.UnloadingLocation, conn, transaction);

                            // حساب العجز
                            NewOrder.Shortage = NewOrder.LoadingQuantity - NewOrder.UnloadingQuantity;

                            // إدخال الطلب
                            string insertOrderQuery = @"
                                INSERT INTO [dbo].[Orders] (
                                    Company_ID, Provider_ID, Vehicle_ID, Petroleum_Type,
                                    Order_Date, Loading_Location, Unloading_Location,
                                    Loading_Quantity, Unloading_Quantity, Shortage,
                                    Status, Delivery_Date
                                )
                                VALUES (
                                    @CompanyID, @ProviderID, @VehicleID, @PetroleumType,
                                    @OrderDate, @LoadingLocation, @UnloadingLocation,
                                    @LoadingQuantity, @UnloadingQuantity, @Shortage,
                                    @Status, @DeliveryDate
                                );
                                SELECT SCOPE_IDENTITY();";

                            int orderId;
                            using (SqlCommand cmd = new SqlCommand(insertOrderQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@CompanyID", 1);
                                cmd.Parameters.AddWithValue("@ProviderID", providerId);
                                cmd.Parameters.AddWithValue("@VehicleID", vehicleId);
                                cmd.Parameters.AddWithValue("@PetroleumType", fuelTypeId);
                                cmd.Parameters.AddWithValue("@OrderDate", NewOrder.OrderDate);
                                cmd.Parameters.AddWithValue("@LoadingLocation", loadingLocationCode);
                                cmd.Parameters.AddWithValue("@UnloadingLocation", unloadingLocationCode);
                                cmd.Parameters.AddWithValue("@LoadingQuantity", NewOrder.LoadingQuantity);
                                cmd.Parameters.AddWithValue("@UnloadingQuantity", NewOrder.UnloadingQuantity);
                                cmd.Parameters.AddWithValue("@Shortage", NewOrder.Shortage);
                                cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(NewOrder.Status) ? "Pending" : NewOrder.Status);
                                cmd.Parameters.AddWithValue("@DeliveryDate", NewOrder.DeliveryDate);

                                orderId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            // حساب المبالغ
                            NewOrder.CompanyTotal = NewOrder.UnloadingQuantity * NewOrder.CompanyPrice;
                            NewOrder.ProviderTotal = NewOrder.UnloadingQuantity * NewOrder.ProviderPrice;
                            NewOrder.TaxAmount = NewOrder.ProviderTotal * 0.05m;
                            NewOrder.TotalDeductions = NewOrder.TaxAmount + NewOrder.StampFee + NewOrder.GPSFee;
                            NewOrder.NetAmount = NewOrder.ProviderTotal - NewOrder.TotalDeductions - NewOrder.AdvancePayment - NewOrder.CustodyAmount;
                            NewOrder.Balance = NewOrder.NetAmount;

                            // إدخال البيانات المالية
                            string insertFinancialsQuery = @"
                                INSERT INTO [dbo].[Financials] (
                                    Order_ID, Company_Price, Provider_Price, Company_Total,
                                    Provider_Total, Tax_Amount, Stamp_Fee, GPS_Fee,
                                    Total_Deduction, Advance_Payment, Custody_Amount,
                                    Net_Amount, Balance
                                )
                                VALUES (
                                    @OrderID, @CompanyPrice, @ProviderPrice, @CompanyTotal,
                                    @ProviderTotal, @TaxAmount, @StampFee, @GPSFee,
                                    @TotalDeduction, @AdvancePayment, @CustodyAmount,
                                    @NetAmount, @Balance
                                )";

                            using (SqlCommand cmd = new SqlCommand(insertFinancialsQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                cmd.Parameters.AddWithValue("@CompanyPrice", NewOrder.CompanyPrice);
                                cmd.Parameters.AddWithValue("@ProviderPrice", NewOrder.ProviderPrice);
                                cmd.Parameters.AddWithValue("@CompanyTotal", NewOrder.CompanyTotal);
                                cmd.Parameters.AddWithValue("@ProviderTotal", NewOrder.ProviderTotal);
                                cmd.Parameters.AddWithValue("@TaxAmount", NewOrder.TaxAmount);
                                cmd.Parameters.AddWithValue("@StampFee", NewOrder.StampFee);
                                cmd.Parameters.AddWithValue("@GPSFee", NewOrder.GPSFee);
                                cmd.Parameters.AddWithValue("@TotalDeduction", NewOrder.TotalDeductions);
                                cmd.Parameters.AddWithValue("@AdvancePayment", NewOrder.AdvancePayment);
                                cmd.Parameters.AddWithValue("@CustodyAmount", NewOrder.CustodyAmount);
                                cmd.Parameters.AddWithValue("@NetAmount", NewOrder.NetAmount);
                                cmd.Parameters.AddWithValue("@Balance", NewOrder.Balance);

                                cmd.ExecuteNonQuery();
                            }

                            // إنشاء فاتورة
                            if (!string.IsNullOrEmpty(NewOrder.InvoiceNumber))
                            {
                                string insertInvoiceQuery = @"
                                    INSERT INTO [dbo].[Invoice] (
                                        Invoice_Number, Order_ID, Company_ID, Provider_ID, Company_Amount, Provider_Amount,
                                        Issue_Date, Payment_Status, Net_Amount, Deductions, Advance_Payment, Custody_Amount
                                    )
                                    VALUES (
                                        @InvoiceNumber, @OrderID, @CompanyID, @ProviderID, @CompanyAmount, @ProviderAmount,
                                        @IssueDate, @PaymentStatus, @NetAmount, @Deductions, @AdvancePayment, @CustodyAmount
                                    )";

                                using (SqlCommand cmd = new SqlCommand(insertInvoiceQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@InvoiceNumber", NewOrder.InvoiceNumber);
                                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                                    cmd.Parameters.AddWithValue("@CompanyID", 1);
                                    cmd.Parameters.AddWithValue("@ProviderID", providerId);
                                    cmd.Parameters.AddWithValue("@CompanyAmount", NewOrder.CompanyTotal);
                                    cmd.Parameters.AddWithValue("@ProviderAmount", NewOrder.ProviderTotal);
                                    cmd.Parameters.AddWithValue("@IssueDate", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@PaymentStatus", "Unpaid");
                                    cmd.Parameters.AddWithValue("@NetAmount", NewOrder.NetAmount);
                                    cmd.Parameters.AddWithValue("@Deductions", NewOrder.TotalDeductions);
                                    cmd.Parameters.AddWithValue("@AdvancePayment", NewOrder.AdvancePayment);
                                    cmd.Parameters.AddWithValue("@CustodyAmount", NewOrder.CustodyAmount);

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            TempData["Success"] = "تم إنشاء الطلب بنجاح";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception("فشل إنشاء الطلب: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"حدث خطأ: {ex.Message}";
            }

            return RedirectToPage();
        }

        // دوال مساعدة
        private int GetProviderIdByName(string providerName, SqlConnection conn, SqlTransaction transaction)
        {
            string query = "SELECT Provider_ID FROM [dbo].[Provider] WHERE Provider_Name = @Name";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Name", providerName);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private int GetVehicleIdByPlate(string plateNumber, SqlConnection conn, SqlTransaction transaction)
        {
            string query = "SELECT Vehicle_ID FROM [dbo].[Vehicle] WHERE Plate_number = @Plate";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Plate", plateNumber);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private int GetFuelTypeIdByName(string typeName, SqlConnection conn, SqlTransaction transaction)
        {
            string query = "SELECT Fuel_ID FROM [dbo].[Fuel_Type] WHERE Type_Name = @Name";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Name", typeName);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private string GetLocationCodeByName(string locationName, SqlConnection conn, SqlTransaction transaction)
        {
            string query = "SELECT Location_Code FROM [dbo].[Location] WHERE Location_Name = @Name";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Name", locationName);
                var result = cmd.ExecuteScalar();
                return result?.ToString() ?? "";
            }
        }

        public IActionResult OnPostDeleteOrder(int orderId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // حذف الفواتير
                            string deleteInvoiceQuery = "DELETE FROM [dbo].[Invoice] WHERE Order_ID = @OrderID";
                            using (SqlCommand cmd = new SqlCommand(deleteInvoiceQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                cmd.ExecuteNonQuery();
                            }

                            // حذف البيانات المالية
                            string deleteFinancialsQuery = "DELETE FROM [dbo].[Financials] WHERE Order_ID = @OrderID";
                            using (SqlCommand cmd = new SqlCommand(deleteFinancialsQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                cmd.ExecuteNonQuery();
                            }

                            // حذف الطلب
                            string deleteOrderQuery = "DELETE FROM [dbo].[Orders] WHERE Order_ID = @OrderID";
                            using (SqlCommand cmd = new SqlCommand(deleteOrderQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            TempData["Success"] = "تم حذف الطلب بنجاح";
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"حدث خطأ: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}