using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Petroleum_Materials_Transport_Office_System.Models;

namespace Petroleum_Materials_Transport_Office_System.Pages.OrdersManagement
{
    public class ViewOrderModel : PageModel
    {
        private readonly string _connectionString = @"Server=DESKTOP-1QHK872;Database=PetroleumTransportDB;Trusted_Connection=True;TrustServerCertificate=True;"; // حط الـ Connection String بتاعك هنا

        public Order Order { get; set; }

        public void OnGet(int orderId)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Role")))
            {
                Response.Redirect("/Login");
                return;
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // استعلام صحيح مع JOINs
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
                                Status = reader["Status"]?.ToString() ?? "",
                                LoadingLocation = reader["Loading_Location_Name"]?.ToString() ?? "",
                                UnloadingLocation = reader["Unloading_Location_Name"]?.ToString() ?? "",
                                PetroleumType = reader["Type_Name"]?.ToString() ?? "",
                                LoadingQuantity = reader["Loading_Quantity"] != DBNull.Value ? Convert.ToDecimal(reader["Loading_Quantity"]) : 0,
                                UnloadingQuantity = reader["Unloading_Quantity"] != DBNull.Value ? Convert.ToDecimal(reader["Unloading_Quantity"]) : 0,
                                Shortage = reader["Shortage"] != DBNull.Value ? Convert.ToDecimal(reader["Shortage"]) : 0,
                                ProviderName = reader["Provider_Name"]?.ToString() ?? "",
                                VehiclePlateNumber = reader["Plate_number"]?.ToString() ?? "",
                                DriverName = reader["Driver_Name"]?.ToString() ?? "",

                                // البيانات المالية من جدول Financials
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
                        else
                        {
                            Response.Redirect("/OrdersManagement");
                        }
                    }
                }
            }
        }
    }
}