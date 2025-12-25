using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Petroleum_Materials_Transport_Office_System.Models;

namespace Petroleum_Materials_Transport_Office_System.Pages.OrdersManagement
{
    public class EditOrderModel : PageModel
    {
        private readonly string _connectionString = @"Server=DESKTOP-1QHK872;Database=PetroleumTransportDB;Trusted_Connection=True;TrustServerCertificate=True;";


        [BindProperty]
        public Order Order { get; set; }

        public List<string> LoadingLocations { get; set; } = new List<string>();
        public List<string> UnloadingLocations { get; set; } = new List<string>();
        public List<string> PetroleumTypes { get; set; } = new List<string>();
        public List<string> Providers { get; set; } = new List<string>();
        public List<string> Vehicles { get; set; } = new List<string>();

        public IActionResult OnGet(int orderId)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Role")))
            {
                return RedirectToPage("/Login");
            }

            LoadDropdownData();
            LoadOrderData(orderId);

            if (Order == null)
            {
                TempData["Error"] = "لم يتم العثور على الطلب";
                return Redirect("/OrdersManagement");  // ✅ مُصلح هنا
            }

            return Page();
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

        private void LoadOrderData(int orderId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT 
                        o.Order_ID,
                        o.Company_ID,
                        o.Provider_ID,
                        o.Driver_ID,
                        o.Vehicle_ID,
                        o.Petroleum_Type,
                        o.Order_Date,
                        o.Loading_Location,
                        o.Unloading_Location,
                        o.Loading_Quantity,
                        o.Unloading_Quantity,
                        o.Shortage,
                        o.Status,
                        o.Delivery_Date,
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
                    WHERE o.Order_ID = @OrderID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Order = new Order
                            {
                                OrderId = orderId,
                                InvoiceNumber = reader["Invoice_Number"]?.ToString() ?? "",
                                OrderDate = reader["Order_Date"] != DBNull.Value ? Convert.ToDateTime(reader["Order_Date"]) : DateTime.Now,
                                DeliveryDate = reader["Delivery_Date"] != DBNull.Value ? Convert.ToDateTime(reader["Delivery_Date"]) : DateTime.Now,
                                Status = reader["Status"]?.ToString() ?? "Pending",
                                LoadingLocation = reader["Loading_Location_Name"]?.ToString() ?? "",
                                UnloadingLocation = reader["Unloading_Location_Name"]?.ToString() ?? "",
                                PetroleumType = reader["Type_Name"]?.ToString() ?? "",
                                LoadingQuantity = reader["Loading_Quantity"] != DBNull.Value ? Convert.ToDecimal(reader["Loading_Quantity"]) : 0,
                                UnloadingQuantity = reader["Unloading_Quantity"] != DBNull.Value ? Convert.ToDecimal(reader["Unloading_Quantity"]) : 0,
                                Shortage = reader["Shortage"] != DBNull.Value ? Convert.ToDecimal(reader["Shortage"]) : 0,
                                ProviderName = reader["Provider_Name"]?.ToString() ?? "",
                                VehiclePlateNumber = reader["Plate_number"]?.ToString() ?? "",
                                DriverName = reader["Driver_Name"]?.ToString() ?? "",

                                // البيانات المالية
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
                        }
                    }
                }
            }
        }

        public IActionResult OnPost()
        {
            if (Order == null)
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
                            // الحصول على IDs من الأسماء
                            int providerId = GetProviderIdByName(Order.ProviderName, conn, transaction);
                            int vehicleId = GetVehicleIdByPlate(Order.VehiclePlateNumber, conn, transaction);
                            int fuelTypeId = GetFuelTypeIdByName(Order.PetroleumType, conn, transaction);
                            string loadingLocationCode = GetLocationCodeByName(Order.LoadingLocation, conn, transaction);
                            string unloadingLocationCode = GetLocationCodeByName(Order.UnloadingLocation, conn, transaction);

                            // حساب العجز
                            Order.Shortage = Order.LoadingQuantity - Order.UnloadingQuantity;

                            // حساب المبالغ
                            Order.CompanyTotal = Order.UnloadingQuantity * Order.CompanyPrice;
                            Order.ProviderTotal = Order.UnloadingQuantity * Order.ProviderPrice;
                            Order.TaxAmount = Order.ProviderTotal * 0.05m;
                            Order.TotalDeductions = Order.TaxAmount + Order.StampFee + Order.GPSFee;
                            Order.NetAmount = Order.ProviderTotal - Order.TotalDeductions - Order.AdvancePayment - Order.CustodyAmount;
                            Order.Balance = Order.NetAmount;

                            // التحقق من الحالة
                            string validStatus = string.IsNullOrEmpty(Order.Status) ? "Pending" : Order.Status;
                            if (validStatus != "Pending" && validStatus != "In Transit" &&
                                validStatus != "Delivered" && validStatus != "Cancelled")
                            {
                                validStatus = "Pending";
                            }

                            // تحديث جدول Orders
                            string updateOrderQuery = @"
                                UPDATE [dbo].[Orders]
                                SET 
                                    Provider_ID = @ProviderID,
                                    Vehicle_ID = @VehicleID,
                                    Petroleum_Type = @PetroleumType,
                                    Order_Date = @OrderDate,
                                    Loading_Location = @LoadingLocation,
                                    Unloading_Location = @UnloadingLocation,
                                    Loading_Quantity = @LoadingQuantity,
                                    Unloading_Quantity = @UnloadingQuantity,
                                    Shortage = @Shortage,
                                    Status = @Status,
                                    Delivery_Date = @DeliveryDate
                                WHERE Order_ID = @OrderID";

                            using (SqlCommand cmd = new SqlCommand(updateOrderQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", Order.OrderId);
                                cmd.Parameters.AddWithValue("@ProviderID", providerId);
                                cmd.Parameters.AddWithValue("@VehicleID", vehicleId);
                                cmd.Parameters.AddWithValue("@PetroleumType", fuelTypeId);
                                cmd.Parameters.AddWithValue("@OrderDate", Order.OrderDate);
                                cmd.Parameters.AddWithValue("@LoadingLocation", loadingLocationCode);
                                cmd.Parameters.AddWithValue("@UnloadingLocation", unloadingLocationCode);
                                cmd.Parameters.AddWithValue("@LoadingQuantity", Order.LoadingQuantity);
                                cmd.Parameters.AddWithValue("@UnloadingQuantity", Order.UnloadingQuantity);
                                cmd.Parameters.AddWithValue("@Shortage", Order.Shortage);
                                cmd.Parameters.AddWithValue("@Status", validStatus);
                                cmd.Parameters.AddWithValue("@DeliveryDate", Order.DeliveryDate);

                                cmd.ExecuteNonQuery();
                            }

                            // تحديث جدول Financials
                            string updateFinancialsQuery = @"
                                UPDATE [dbo].[Financials]
                                SET 
                                    Company_Price = @CompanyPrice,
                                    Provider_Price = @ProviderPrice,
                                    Company_Total = @CompanyTotal,
                                    Provider_Total = @ProviderTotal,
                                    Tax_Amount = @TaxAmount,
                                    Stamp_Fee = @StampFee,
                                    GPS_Fee = @GPSFee,
                                    Total_Deduction = @TotalDeduction,
                                    Advance_Payment = @AdvancePayment,
                                    Custody_Amount = @CustodyAmount,
                                    Net_Amount = @NetAmount,
                                    Balance = @Balance
                                WHERE Order_ID = @OrderID";

                            using (SqlCommand cmd = new SqlCommand(updateFinancialsQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", Order.OrderId);
                                cmd.Parameters.AddWithValue("@CompanyPrice", Order.CompanyPrice);
                                cmd.Parameters.AddWithValue("@ProviderPrice", Order.ProviderPrice);
                                cmd.Parameters.AddWithValue("@CompanyTotal", Order.CompanyTotal);
                                cmd.Parameters.AddWithValue("@ProviderTotal", Order.ProviderTotal);
                                cmd.Parameters.AddWithValue("@TaxAmount", Order.TaxAmount);
                                cmd.Parameters.AddWithValue("@StampFee", Order.StampFee);
                                cmd.Parameters.AddWithValue("@GPSFee", Order.GPSFee);
                                cmd.Parameters.AddWithValue("@TotalDeduction", Order.TotalDeductions);
                                cmd.Parameters.AddWithValue("@AdvancePayment", Order.AdvancePayment);
                                cmd.Parameters.AddWithValue("@CustodyAmount", Order.CustodyAmount);
                                cmd.Parameters.AddWithValue("@NetAmount", Order.NetAmount);
                                cmd.Parameters.AddWithValue("@Balance", Order.Balance);

                                cmd.ExecuteNonQuery();
                            }

                            // تحديث Invoice
                            if (!string.IsNullOrEmpty(Order.InvoiceNumber))
                            {
                                // التحقق من وجود فاتورة
                                string checkInvoiceQuery = "SELECT COUNT(*) FROM [dbo].[Invoice] WHERE Order_ID = @OrderID";
                                int invoiceExists = 0;
                                using (SqlCommand cmd = new SqlCommand(checkInvoiceQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@OrderID", Order.OrderId);
                                    invoiceExists = Convert.ToInt32(cmd.ExecuteScalar());
                                }

                                if (invoiceExists > 0)
                                {
                                    // تحديث الفاتورة الموجودة
                                    string updateInvoiceQuery = @"
                                        UPDATE [dbo].[Invoice]
                                        SET 
                                            Invoice_Number = @InvoiceNumber,
                                            Company_Amount = @CompanyAmount,
                                            Provider_Amount = @ProviderAmount,
                                            Net_Amount = @NetAmount,
                                            Deductions = @Deductions,
                                            Advance_Payment = @AdvancePayment,
                                            Custody_Amount = @CustodyAmount
                                        WHERE Order_ID = @OrderID";

                                    using (SqlCommand cmd = new SqlCommand(updateInvoiceQuery, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@OrderID", Order.OrderId);
                                        cmd.Parameters.AddWithValue("@InvoiceNumber", Order.InvoiceNumber);
                                        cmd.Parameters.AddWithValue("@CompanyAmount", Order.CompanyTotal);
                                        cmd.Parameters.AddWithValue("@ProviderAmount", Order.ProviderTotal);
                                        cmd.Parameters.AddWithValue("@NetAmount", Order.NetAmount);
                                        cmd.Parameters.AddWithValue("@Deductions", Order.TotalDeductions);
                                        cmd.Parameters.AddWithValue("@AdvancePayment", Order.AdvancePayment);
                                        cmd.Parameters.AddWithValue("@CustodyAmount", Order.CustodyAmount);

                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    // إنشاء فاتورة جديدة
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
                                        cmd.Parameters.AddWithValue("@InvoiceNumber", Order.InvoiceNumber);
                                        cmd.Parameters.AddWithValue("@OrderID", Order.OrderId);
                                        cmd.Parameters.AddWithValue("@CompanyID", 1);
                                        cmd.Parameters.AddWithValue("@ProviderID", providerId);
                                        cmd.Parameters.AddWithValue("@CompanyAmount", Order.CompanyTotal);
                                        cmd.Parameters.AddWithValue("@ProviderAmount", Order.ProviderTotal);
                                        cmd.Parameters.AddWithValue("@IssueDate", DateTime.Now);
                                        cmd.Parameters.AddWithValue("@PaymentStatus", "Unpaid");
                                        cmd.Parameters.AddWithValue("@NetAmount", Order.NetAmount);
                                        cmd.Parameters.AddWithValue("@Deductions", Order.TotalDeductions);
                                        cmd.Parameters.AddWithValue("@AdvancePayment", Order.AdvancePayment);
                                        cmd.Parameters.AddWithValue("@CustodyAmount", Order.CustodyAmount);

                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            transaction.Commit();
                            TempData["Success"] = "تم تحديث الطلب بنجاح";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception("فشل تحديث الطلب: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"حدث خطأ: {ex.Message}";
                return Page();
            }

            return Redirect("/OrdersManagement");  // ✅ مُصلح هنا
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
    }
}