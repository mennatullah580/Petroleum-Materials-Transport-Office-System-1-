using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Petroleum_Materials_Transport_Office_System.Pages.Finance
{
    public class PaymentsModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public PaymentsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // --- Bind Properties ---
        [BindProperty]
        public string TransactionType { get; set; } // 'Payment' or 'Receipt'

        [BindProperty]
        public int SelectedTreasuryId { get; set; }

        [BindProperty]
        public decimal Amount { get; set; }

        [BindProperty]
        public DateTime Date { get; set; } = DateTime.Today;

        [BindProperty]
        public int? SelectedProviderId { get; set; }

        // ✅ ADDED: This was missing and caused the error
        [BindProperty]
        public int? SelectedClientId { get; set; }

        [BindProperty]
        public string PaymentMethod { get; set; }

        [BindProperty]
        public string InvoiceNumber { get; set; }

        [BindProperty]
        public string Notes { get; set; }

        // --- Dropdown Lists ---
        public List<SelectListItem> TreasuryList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProviderList { get; set; } = new List<SelectListItem>();

        // ✅ ADDED: This was missing
        public List<SelectListItem> ClientList { get; set; } = new List<SelectListItem>();

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            // Restore messages after page reload
            if (TempData["SuccessMessage"] != null) SuccessMessage = TempData["SuccessMessage"].ToString();
            if (TempData["ErrorMessage"] != null) ErrorMessage = TempData["ErrorMessage"].ToString();

            LoadDropdowns();
        }

        public IActionResult OnPost()
        {
            if (Amount <= 0)
            {
                ErrorMessage = "يجب إدخال مبلغ صحيح.";
                LoadDropdowns();
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // 1. Get Treasury details
                string treasuryName = "";
                decimal currentBalance = 0;
                string checkSql = "SELECT Name, Current_Balance FROM Treasury_Bank WHERE Treasury_ID = @TID";

                using (SqlCommand cmd = new SqlCommand(checkSql, connection))
                {
                    cmd.Parameters.AddWithValue("@TID", SelectedTreasuryId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            treasuryName = reader["Name"].ToString();
                            currentBalance = reader["Current_Balance"] != DBNull.Value ? Convert.ToDecimal(reader["Current_Balance"]) : 0;
                        }
                        else
                        {
                            ErrorMessage = "الخزينة غير موجودة.";
                            LoadDropdowns();
                            return Page();
                        }
                    }
                }

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 2. Check Balance (Only if Paying)
                        if (TransactionType == "Payment" && currentBalance < Amount)
                        {
                            throw new Exception($"رصيد '{treasuryName}' غير كافٍ. الرصيد الحالي: {currentBalance:N0}");
                        }

                        // 3. Update Treasury Balance
                        string updateSql = TransactionType == "Payment"
                            ? "UPDATE Treasury_Bank SET Current_Balance = Current_Balance - @Amt WHERE Treasury_ID = @TID"
                            : "UPDATE Treasury_Bank SET Current_Balance = Current_Balance + @Amt WHERE Treasury_ID = @TID";

                        using (SqlCommand cmd = new SqlCommand(updateSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Amt", Amount);
                            cmd.Parameters.AddWithValue("@TID", SelectedTreasuryId);
                            cmd.ExecuteNonQuery();
                        }

                        // 4. Record Transaction
                        string entityName = GetEntityName(connection, transaction);
                        string fullRemarks = $"[{TransactionType}] {entityName} | فاتورة: {InvoiceNumber} | {Notes}";

                        string insertSql = @"
                            INSERT INTO Payment_Transaction 
                            (Amount, Date, Type, Method, Status, Provider_ID, Remarks, Created_At)
                            VALUES 
                            (@Amt, @Date, @Type, @Method, N'Completed', @ProvID, @Remarks, GETDATE())";

                        using (SqlCommand cmd = new SqlCommand(insertSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Amt", Amount);
                            cmd.Parameters.AddWithValue("@Date", Date);
                            cmd.Parameters.AddWithValue("@Type", TransactionType);
                            cmd.Parameters.AddWithValue("@Method", PaymentMethod ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ProvID", (object)SelectedProviderId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Remarks", fullRemarks);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();

                        // 5. Success Message & Reload
                        if (TransactionType == "Payment")
                            TempData["SuccessMessage"] = $"تم خصم {Amount:N0} من {treasuryName}. المتبقي: {currentBalance - Amount:N0}";
                        else
                            TempData["SuccessMessage"] = $"تم إيداع {Amount:N0} في {treasuryName}. الجديد: {currentBalance + Amount:N0}";

                        return RedirectToPage();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ErrorMessage = "خطأ: " + ex.Message;
                        LoadDropdowns();
                        return Page();
                    }
                }
            }
        }

        private void LoadDropdowns()
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    TreasuryList = GetList(conn, "SELECT Treasury_ID, Name FROM Treasury_Bank WHERE Status='Active'");
                    ProviderList = GetList(conn, "SELECT Provider_ID, Provider_Name FROM Provider");

                    // Note: If you don't have a 'Clients' table yet, this line might fail or return nothing.
                    // If it fails, you can comment it out temporarily.
                    try
                    {
                        ClientList = GetList(conn, "SELECT Client_ID, Name FROM Clients");
                    }
                    catch { /* Ignore if Clients table doesn't exist yet */ }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error loading lists: " + ex.Message;
            }
        }

        private List<SelectListItem> GetList(SqlConnection conn, string query)
        {
            var list = new List<SelectListItem>();
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read()) list.Add(new SelectListItem { Value = r[0].ToString(), Text = r[1].ToString() });
            }
            return list;
        }

        private string GetEntityName(SqlConnection conn, SqlTransaction trans)
        {
            // Logic to get name of Provider OR Client
            if (SelectedProviderId.HasValue)
            {
                string sql = "SELECT Provider_Name FROM Provider WHERE Provider_ID = @ID";
                using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@ID", SelectedProviderId);
                    return "مورد: " + cmd.ExecuteScalar()?.ToString();
                }
            }
            else if (SelectedClientId.HasValue)
            {
                try
                {
                    string sql = "SELECT Name FROM Clients WHERE Client_ID = @ID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ID", SelectedClientId);
                        return "عميل: " + cmd.ExecuteScalar()?.ToString();
                    }
                }
                catch { return "عميل: (غير محدد)"; }
            }
            return "عام";
        }
    }
}