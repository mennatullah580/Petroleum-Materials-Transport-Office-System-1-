using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Petroleum_Materials_Transport_Office_System.Models;

namespace Petroleum_Materials_Transport_Office_System.Pages.OrdersManagement
{
    public class ViewOrderModel : PageModel
    {
        private readonly string _connectionString =
            @"Server=DESKTOP-1QHK872;Database=PetroleumTransportDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public Order? Order { get; set; }

        public IActionResult OnGet(int orderId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string query = @"
            SELECT 
                o.Order_ID,
                o.Order_Date,
                o.Delivery_Date,
                o.Status,
                o.Loading_Location,
                o.Unloading_Location,

                ll.Location_Name AS Loading_Location_Name,
                ll.Location_Code AS Loading_Location_Code,
                ul.Location_Name AS Unloading_Location_Name,
                ul.Location_Code AS Unloading_Location_Code,

                ft.Type_Name,
                i.Invoice_Number,

                o.Loading_Quantity,
                o.Unloading_Quantity,

                p.Provider_Name,
                v.Plate_Number,
                d.Name AS Driver_Name,

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

            FROM Orders o
            LEFT JOIN Location ll ON o.Loading_Location = ll.Location_Code
            LEFT JOIN Location ul ON o.Unloading_Location = ul.Location_Code
            LEFT JOIN Fuel_Type ft ON o.Petroleum_Type = ft.Fuel_ID
            LEFT JOIN Invoice i ON o.Order_ID = i.Order_ID
            LEFT JOIN Provider p ON o.Provider_ID = p.Provider_ID
            LEFT JOIN Vehicle v ON o.Vehicle_ID = v.Vehicle_ID
            LEFT JOIN Driver d ON o.Driver_ID = d.Driver_ID
            LEFT JOIN Financials f ON o.Order_ID = f.Order_ID
            WHERE o.Order_ID = @ID";

            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ID", orderId);

            using SqlDataReader r = cmd.ExecuteReader();
            if (!r.Read())
                return RedirectToPage("/OrdersManagement");

            decimal loadingQty = r["Loading_Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Loading_Quantity"]);
            decimal unloadingQty = r["Unloading_Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Unloading_Quantity"]);

            // 🔥 جلب كود الجهة من o.Loading_Location مباشرة (هو الكود الأصلي)
            string locationCode = r["Loading_Location"]?.ToString() ?? "";

            Order = new Order
            {
                OrderId = orderId,
                InvoiceNumber = r["Invoice_Number"]?.ToString() ?? "",
                OrderDate = Convert.ToDateTime(r["Order_Date"]),
                DeliveryDate = r["Delivery_Date"] == DBNull.Value
                                ? DateTime.MinValue
                                : Convert.ToDateTime(r["Delivery_Date"]),
                Status = r["Status"]?.ToString() ?? "",

                LoadingLocation = r["Loading_Location_Name"]?.ToString() ?? "",
                UnloadingLocation = r["Unloading_Location_Name"]?.ToString() ?? "",
                PetroleumType = r["Type_Name"]?.ToString() ?? "",

                // 🔥 كود الجهة الصحيح من العمود Loading_Location
                LocationCode = locationCode,

                LoadingQuantity = loadingQty,
                UnloadingQuantity = unloadingQty,
                Shortage = loadingQty - unloadingQty,

                ProviderName = r["Provider_Name"]?.ToString() ?? "",
                VehiclePlateNumber = r["Plate_Number"]?.ToString() ?? "",
                DriverName = r["Driver_Name"]?.ToString() ?? "",

                CompanyPrice = r["Company_Price"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Company_Price"]),
                ProviderPrice = r["Provider_Price"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Provider_Price"]),
                CompanyTotal = r["Company_Total"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Company_Total"]),
                ProviderTotal = r["Provider_Total"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Provider_Total"]),
                TaxAmount = r["Tax_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Tax_Amount"]),
                StampFee = r["Stamp_Fee"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Stamp_Fee"]),
                GPSFee = r["GPS_Fee"] == DBNull.Value ? 0 : Convert.ToDecimal(r["GPS_Fee"]),
                TotalDeductions = r["Total_Deduction"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Total_Deduction"]),
                AdvancePayment = r["Advance_Payment"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Advance_Payment"]),
                CustodyAmount = r["Custody_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Custody_Amount"]),
                NetAmount = r["Net_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Net_Amount"]),
                Balance = r["Balance"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Balance"])
            };

            return Page();
        }

        public IActionResult OnPostDelete(int orderId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            using SqlTransaction t = conn.BeginTransaction();

            try
            {
                // حذف الفاتورة الأول
                new SqlCommand("DELETE FROM Invoice WHERE Order_ID=@ID", conn, t)
                { Parameters = { new SqlParameter("@ID", orderId) } }.ExecuteNonQuery();

                // حذف المعاملات المالية
                new SqlCommand("DELETE FROM Financials WHERE Order_ID=@ID", conn, t)
                { Parameters = { new SqlParameter("@ID", orderId) } }.ExecuteNonQuery();

                // حذف الطلب نفسه
                new SqlCommand("DELETE FROM Orders WHERE Order_ID=@ID", conn, t)
                { Parameters = { new SqlParameter("@ID", orderId) } }.ExecuteNonQuery();

                t.Commit();
            }
            catch
            {
                t.Rollback();
                throw;
            }

            // ✅ الرجوع للصفحة الرئيسية - المسار الصحيح
            return RedirectToPage("/OrdersManagement");
        }
    }
}