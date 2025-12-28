using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Petroleum_Materials_Transport_Office_System.Models;
using System.Data;

namespace Petroleum_Materials_Transport_Office_System.Pages.OrdersManagement
{
    public class IndexModel : PageModel
    {
        private readonly string _connectionString = "Server=DESKTOP-1QHK872;Database=PetroleumTransportDB;Trusted_Connection=True;TrustServerCertificate=True;";

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
        public List<string> LocationCodes { get; set; } = new List<string>
        {
            "LOC001", "LOC002", "LOC003", "LOC004",
            "LOC005", "LOC006", "LOC007", "LOC008"
        };
        public List<string> InvoiceNumbers { get; set; } = new List<string>();
        public Dictionary<int, string> Companies { get; set; } = new Dictionary<int, string>();

        // 🔢 نطاقات الفواتير لكل كود جهة (تم استنتاجها من البيانات)
        private readonly Dictionary<string, (int Min, int Max)> _invoiceRanges = new Dictionary<string, (int, int)>
        {
            { "LOC001", (1, 999) },           // كود 1: من 1 إلى 999
            { "LOC002", (2000, 2999) },       // كود 2: من 2000 إلى 2999
            { "LOC003" , (471000, 471999) },   // كود 3: من 471000 إلى 471999
            { "LOC004", (594000, 597999) },   // كود 4: من 594000 إلى 597999
            { "LOC005", (56000, 56999) },     // كود 6: من 56000 إلى 56999
            { "LOC006", (458000, 460999) },  // كود 11: من 458000 إلى 460999
            { "LOC007", (25000, 25999) },    // كود 14: من 25000 إلى 25999
            { "LOC008", (146000, 146999) }   // كود 15: من 146000 إلى 146999
        };

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

                string companyQuery = "SELECT Company_ID, Company_Name FROM [dbo].[Company] WHERE Status = 'Active'";
                using (SqlCommand cmd = new SqlCommand(companyQuery, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Companies.Add(Convert.ToInt32(reader["Company_ID"]), reader["Company_Name"].ToString());
                    }
                }

                string locationQuery = "SELECT DISTINCT Location_Name, Location_Code FROM [dbo].[Location] WHERE Status = 'Active'";
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

                string fuelQuery = "SELECT Type_name FROM [dbo].[Fuel_Type] WHERE Status = 'Active'";
                using (SqlCommand cmd = new SqlCommand(fuelQuery, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PetroleumTypes.Add(reader["Type_name"].ToString());
                    }
                }

                string providerQuery = "SELECT Provider_Name FROM [dbo].[Provider] WHERE Status = 'Active'";
                using (SqlCommand cmd = new SqlCommand(providerQuery, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Providers.Add(reader["Provider_Name"].ToString());
                    }
                }

                string vehicleQuery = "SELECT Plate_number FROM [dbo].[Vehicle] WHERE Status = 'Active'";
                using (SqlCommand cmd = new SqlCommand(vehicleQuery, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Vehicles.Add(reader["Plate_number"].ToString());
                    }
                }

                string invQuery = "SELECT DISTINCT Invoice_Number FROM [dbo].[Invoice] WHERE Invoice_Number IS NOT NULL";
                using (SqlCommand cmd = new SqlCommand(invQuery, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Invoice_Number"] != DBNull.Value)
                        {
                            InvoiceNumbers.Add(reader["Invoice_Number"].ToString());
                        }
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
                        f.Balance,
                        c.Company_Name 
                    FROM [dbo].[Orders] o
                    LEFT JOIN [dbo].[Company] c ON o.Company_ID = c.Company_ID
                    LEFT JOIN [dbo].[Provider] p ON o.Provider_ID = p.Provider_ID
                    LEFT JOIN [dbo].[Vehicle] v ON o.Vehicle_ID = v.Vehicle_ID
                    LEFT JOIN [dbo].[Driver] d ON o.Driver_ID = d.Driver_ID
                    LEFT JOIN [dbo].[Fuel_Type] ft ON o.Petroleum_Type = ft.Fuel_ID
                    LEFT JOIN [dbo].[Location] ll ON o.Loading_Location = ll.Location_Code
                    LEFT JOIN [dbo].[Location] ul ON o.Unloading_Location = ul.Location_Code
                    LEFT JOIN [dbo].[Invoice] i ON o.Order_ID = i.Order_ID
                    LEFT JOIN [dbo].[Financials] f ON o.Order_ID = f.Order_ID
                    WHERE 1=1";

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
                    if (!string.IsNullOrEmpty(SearchInvoiceNumber)) cmd.Parameters.AddWithValue("@InvoiceNumber", "%" + SearchInvoiceNumber + "%");
                    if (!string.IsNullOrEmpty(SearchLoadingLocation)) cmd.Parameters.AddWithValue("@LoadingLocation", SearchLoadingLocation);
                    if (!string.IsNullOrEmpty(SearchUnloadingLocation)) cmd.Parameters.AddWithValue("@UnloadingLocation", SearchUnloadingLocation);
                    if (!string.IsNullOrEmpty(SearchProviderName)) cmd.Parameters.AddWithValue("@ProviderName", SearchProviderName);
                    if (!string.IsNullOrEmpty(SearchPetroleumType)) cmd.Parameters.AddWithValue("@PetroleumType", SearchPetroleumType);
                    if (!string.IsNullOrEmpty(SearchStatus)) cmd.Parameters.AddWithValue("@Status", SearchStatus);
                    if (SearchDateFrom.HasValue) cmd.Parameters.AddWithValue("@DateFrom", SearchDateFrom.Value);
                    if (SearchDateTo.HasValue) cmd.Parameters.AddWithValue("@DateTo", SearchDateTo.Value);

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
                                Balance = reader["Balance"] != DBNull.Value ? Convert.ToDecimal(reader["Balance"]) : 0,
                            };

                            Orders.Add(order);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 🎯 توليد رقم فاتورة عشوائي بناءً على كود الجهة
        /// </summary>
        private string GenerateInvoiceNumber(string locationCode, SqlConnection conn, SqlTransaction transaction)
        {
            // التحقق من وجود نطاق لكود الجهة
            if (!_invoiceRanges.ContainsKey(locationCode))
            {
                throw new Exception($"كود الجهة {locationCode} غير موجود في النطاقات المحددة");
            }

            var range = _invoiceRanges[locationCode];
            var random = new Random();
            string invoiceNumber;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                // توليد رقم عشوائي ضمن النطاق (أرقام فقط)
                int randomNumber = random.Next(range.Min, range.Max + 1);
                invoiceNumber = randomNumber.ToString();

                // التحقق من عدم وجود الرقم مسبقاً في قاعدة البيانات
                string checkQuery = "SELECT COUNT(*) FROM [dbo].[Invoice] WHERE Invoice_Number = @InvoiceNumber";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                {
                    checkCmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count == 0)
                    {
                        // الرقم غير مستخدم، يمكن استخدامه
                        return invoiceNumber;
                    }
                }

                attempts++;

                if (attempts >= maxAttempts)
                {
                    throw new Exception($"فشل في توليد رقم فاتورة فريد بعد {maxAttempts} محاولة. قد يكون النطاق ممتلئاً");
                }

            } while (true);
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
                            // 1. الحصول على IDs من الأسماء
                            int providerId = GetProviderIdByName(NewOrder.ProviderName, conn, transaction);
                            int driverId = GetDriverIdByName(NewOrder.DriverName, conn, transaction);
                            int vehicleId = GetVehicleIdByPlate(NewOrder.VehiclePlateNumber, conn, transaction);
                            int petroleumTypeId = GetFuelTypeIdByName(NewOrder.PetroleumType, conn, transaction);
                            string loadingLocationCode = GetLocationCodeByName(NewOrder.LoadingLocation, conn, transaction);
                            string unloadingLocationCode = GetLocationCodeByName(NewOrder.UnloadingLocation, conn, transaction);

                            // 2. التحقق من البيانات الأساسية
                            if (providerId == 0 || vehicleId == 0 || petroleumTypeId == 0 ||
                                string.IsNullOrEmpty(loadingLocationCode) || string.IsNullOrEmpty(unloadingLocationCode))
                            {
                                throw new Exception("تأكد من صحة البيانات المدخلة (المقاول، السيارة، نوع الوقود، المواقع)");
                            }

                            // 2.5 التحقق من كود الجهة
                            if (string.IsNullOrEmpty(NewOrder.LocationCode))
                            {
                                throw new Exception("يرجى اختيار كود الجهة");
                            }

                            // 3. حساب القيم
                            decimal shortage = NewOrder.LoadingQuantity - NewOrder.UnloadingQuantity;
                            decimal companyTotal = NewOrder.UnloadingQuantity * NewOrder.CompanyPrice;
                            decimal providerTotal = NewOrder.UnloadingQuantity * NewOrder.ProviderPrice;
                            decimal taxAmount = providerTotal * 0.05m;
                            decimal totalDeductions = taxAmount + NewOrder.StampFee + NewOrder.GPSFee;
                            decimal netAmount = providerTotal - totalDeductions - NewOrder.AdvancePayment - NewOrder.CustodyAmount;

                            // 4. تحديد الحالة
                            string status = string.IsNullOrEmpty(NewOrder.Status) ? "Pending" : NewOrder.Status;

                            // 5. إدراج Order
                            string insertOrderQuery = @"
                                INSERT INTO [dbo].[Orders] (
                                    Company_ID, Provider_ID, Driver_ID, Vehicle_ID, Petroleum_Type,
                                    Order_Date, Loading_Location, Unloading_Location, 
                                    Loading_Quantity, Unloading_Quantity, Shortage, Status, Delivery_Date
                                )
                                VALUES (
                                    @CompanyID, @ProviderID, @DriverID, @VehicleID, @PetroleumType,
                                    @OrderDate, @LoadingLocation, @UnloadingLocation,
                                    @LoadingQuantity, @UnloadingQuantity, @Shortage, @Status, @DeliveryDate
                                );
                                SELECT SCOPE_IDENTITY();";

                            int newOrderId;
                            using (SqlCommand cmd = new SqlCommand(insertOrderQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@CompanyID", 1);
                                cmd.Parameters.AddWithValue("@ProviderID", providerId);
                                cmd.Parameters.AddWithValue("@DriverID", driverId == 0 ? (object)DBNull.Value : driverId);
                                cmd.Parameters.AddWithValue("@VehicleID", vehicleId);
                                cmd.Parameters.AddWithValue("@PetroleumType", petroleumTypeId);
                                cmd.Parameters.AddWithValue("@OrderDate", NewOrder.OrderDate);
                                cmd.Parameters.AddWithValue("@LoadingLocation", loadingLocationCode);
                                cmd.Parameters.AddWithValue("@UnloadingLocation", unloadingLocationCode);
                                cmd.Parameters.AddWithValue("@LoadingQuantity", NewOrder.LoadingQuantity);
                                cmd.Parameters.AddWithValue("@UnloadingQuantity", NewOrder.UnloadingQuantity);
                                cmd.Parameters.AddWithValue("@Shortage", shortage);
                                cmd.Parameters.AddWithValue("@Status", status);
                                cmd.Parameters.AddWithValue("@DeliveryDate", status == "Pending" ? (object)DBNull.Value : NewOrder.DeliveryDate);

                                newOrderId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            // 6. توليد رقم الفاتورة بناءً على كود الجهة (أرقام فقط)
                            string invoiceNumber = GenerateInvoiceNumber(NewOrder.LocationCode, conn, transaction);

                            // 7. إدراج Invoice
                            string insertInvoiceQuery = @"
                                INSERT INTO [dbo].[Invoice] (
                                    Invoice_Number, Order_ID, Company_ID, Provider_ID,
                                    Company_Amount, Provider_Amount, Issue_Date, Payment_Status,
                                    Net_Amount, Deductions, Advance_Payment, Custody_Amount
                                )
                                VALUES (
                                    @InvoiceNumber, @OrderID, @CompanyID, @ProviderID,
                                    @CompanyAmount, @ProviderAmount, @IssueDate, @PaymentStatus,
                                    @NetAmount, @Deductions, @AdvancePayment, @CustodyAmount
                                )";

                            using (SqlCommand cmd = new SqlCommand(insertInvoiceQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                                cmd.Parameters.AddWithValue("@OrderID", newOrderId);
                                cmd.Parameters.AddWithValue("@CompanyID", 1);
                                cmd.Parameters.AddWithValue("@ProviderID", providerId);
                                cmd.Parameters.AddWithValue("@CompanyAmount", companyTotal);
                                cmd.Parameters.AddWithValue("@ProviderAmount", providerTotal);
                                cmd.Parameters.AddWithValue("@IssueDate", NewOrder.OrderDate);
                                cmd.Parameters.AddWithValue("@PaymentStatus", "Unpaid");
                                cmd.Parameters.AddWithValue("@NetAmount", netAmount);
                                cmd.Parameters.AddWithValue("@Deductions", totalDeductions);
                                cmd.Parameters.AddWithValue("@AdvancePayment", NewOrder.AdvancePayment);
                                cmd.Parameters.AddWithValue("@CustodyAmount", NewOrder.CustodyAmount);

                                cmd.ExecuteNonQuery();
                            }

                            // 8. إدراج Financials
                            string insertFinancialsQuery = @"
                                INSERT INTO [dbo].[Financials] (
                                    Order_ID, Company_Price, Provider_Price, Company_Total, Provider_Total,
                                    Tax_Amount, Stamp_Fee, GPS_Fee, Total_Deduction,
                                    Advance_Payment, Custody_Amount, Net_Amount, Balance
                                )
                                VALUES (
                                    @OrderID, @CompanyPrice, @ProviderPrice, @CompanyTotal, @ProviderTotal,
                                    @TaxAmount, @StampFee, @GPSFee, @TotalDeduction,
                                    @AdvancePayment, @CustodyAmount, @NetAmount, @Balance
                                )";

                            using (SqlCommand cmd = new SqlCommand(insertFinancialsQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", newOrderId);
                                cmd.Parameters.AddWithValue("@CompanyPrice", NewOrder.CompanyPrice);
                                cmd.Parameters.AddWithValue("@ProviderPrice", NewOrder.ProviderPrice);
                                cmd.Parameters.AddWithValue("@CompanyTotal", companyTotal);
                                cmd.Parameters.AddWithValue("@ProviderTotal", providerTotal);
                                cmd.Parameters.AddWithValue("@TaxAmount", taxAmount);
                                cmd.Parameters.AddWithValue("@StampFee", NewOrder.StampFee);
                                cmd.Parameters.AddWithValue("@GPSFee", NewOrder.GPSFee);
                                cmd.Parameters.AddWithValue("@TotalDeduction", totalDeductions);
                                cmd.Parameters.AddWithValue("@AdvancePayment", NewOrder.AdvancePayment);
                                cmd.Parameters.AddWithValue("@CustodyAmount", NewOrder.CustodyAmount);
                                cmd.Parameters.AddWithValue("@NetAmount", netAmount);
                                cmd.Parameters.AddWithValue("@Balance", netAmount);

                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            TempData["Success"] = $"✅ تم إنشاء الطلب بنجاح! رقم الفاتورة: {invoiceNumber} (كود الجهة: {NewOrder.LocationCode})";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception("فشل في إنشاء الطلب: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ خطأ: {ex.Message}";
            }

            return RedirectToPage();
        }

        // Helper Functions
        private int GetProviderIdByName(string name, SqlConnection conn, SqlTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(name)) return 0;
            string query = "SELECT Provider_ID FROM [dbo].[Provider] WHERE Provider_Name = @Name";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Name", name);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private int GetDriverIdByName(string name, SqlConnection conn, SqlTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(name)) return 0;
            string query = "SELECT Driver_ID FROM [dbo].[Driver] WHERE Name = @Name";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Name", name);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private int GetVehicleIdByPlate(string plate, SqlConnection conn, SqlTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(plate)) return 0;
            string query = "SELECT Vehicle_ID FROM [dbo].[Vehicle] WHERE Plate_Number = @Plate";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Plate", plate);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private int GetFuelTypeIdByName(string type, SqlConnection conn, SqlTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(type)) return 0;
            string query = "SELECT Fuel_ID FROM [dbo].[Fuel_Type] WHERE Type_Name = @Name";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Name", type);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private string GetLocationCodeByName(string name, SqlConnection conn, SqlTransaction transaction = null)
        {
            if (string.IsNullOrEmpty(name)) return "";
            string query = "SELECT Location_Code FROM [dbo].[Location] WHERE Location_Name = @Name";
            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Name", name);
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
                            using (SqlCommand cmd = new SqlCommand("DELETE FROM [dbo].[Invoice] WHERE Order_ID = @ID", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ID", orderId);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand("DELETE FROM [dbo].[Financials] WHERE Order_ID = @ID", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ID", orderId);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand("DELETE FROM [dbo].[Orders] WHERE Order_ID = @ID", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ID", orderId);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            TempData["Success"] = "✅ تم حذف الطلب بنجاح";
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ خطأ في الحذف: {ex.Message}";
            }
            return RedirectToPage();
        }
    }
}