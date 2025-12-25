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

        // ==========================================
        // 1. DATA BINDING
        // ==========================================
        [BindProperty]
        public string TransactionType { get; set; } // 'Payment' or 'Receipt'

        [BindProperty]
        public int SelectedTreasuryId { get; set; } // Critical for the new DB

        [BindProperty]
        public decimal Amount { get; set; }

        [BindProperty]
        public DateTime Date { get; set; } = DateTime.Today;

        [BindProperty]
        public int? SelectedProviderId { get; set; } // Nullable (optional if receiving from client)

        [BindProperty]
        public int? SelectedClientId { get; set; } // Nullable (optional if paying provider)

        [BindProperty]
        public string PaymentMethod { get; set; }

        [BindProperty]
        public string InvoiceNumber { get; set; }

        [BindProperty]
        public string Notes { get; set; }

        // Dropdowns
        public List<SelectListItem> TreasuryList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProviderList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ClientList { get; set; } = new List<SelectListItem>();

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        // ==========================================
        // 2. ON GET
        // ==========================================
        public void OnGet()
        {
            LoadDropdowns();
        }

        // ==========================================
        // 3. ON POST
        // ==========================================
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
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // A. Check Treasury Balance (Only if Paying)
                        if (TransactionType == "Payment")
                        {
                            string checkSql = "SELECT Current_Balance, Name FROM Treasury_Bank WHERE Treasury_ID = @TID";
                            using (SqlCommand cmd = new SqlCommand(checkSql, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@TID", SelectedTreasuryId);
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        decimal balance = Convert.ToDecimal(reader["Current_Balance"]);
                                        if (balance < Amount)
                                        {
                                            throw new Exception($"رصيد الخزينة غير كافٍ. الرصيد الحالي: {balance:N2}");
                                        }
                                    }
                                }
                            }
                        }

                        // B. Update Balance (+ for Receipt, - for Payment)
                        string updateBalanceSql = "";
                        if (TransactionType == "Payment")
                            updateBalanceSql = "UPDATE Treasury_Bank SET Current_Balance = Current_Balance - @Amt WHERE Treasury_ID = @TID";
                        else
                            updateBalanceSql = "UPDATE Treasury_Bank SET Current_Balance = Current_Balance + @Amt WHERE Treasury_ID = @TID";

                        using (SqlCommand cmd = new SqlCommand(updateBalanceSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Amt", Amount);
                            cmd.Parameters.AddWithValue("@TID", SelectedTreasuryId);
                            cmd.ExecuteNonQuery();
                        }

                        // C. Insert Transaction Record
                        // We construct the Remarks to include Provider/Client names for easier reading later
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
                            cmd.Parameters.AddWithValue("@Type", TransactionType); // Must match DB Constraint (Payment/Receipt)
                            cmd.Parameters.AddWithValue("@Method", PaymentMethod);
                            cmd.Parameters.AddWithValue("@ProvID", (object)SelectedProviderId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Remarks", fullRemarks);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        SuccessMessage = "تم حفظ العملية وتحديث الرصيد بنجاح!";

                        // Reset Form
                        Amount = 0;
                        InvoiceNumber = "";
                        Notes = "";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ErrorMessage = "خطأ: " + ex.Message;
                    }
                }
            }

            LoadDropdowns();
            return Page();
        }

        // ==========================================
        // HELPERS
        // ==========================================
        private void LoadDropdowns()
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                // Treasuries
                TreasuryList = GetList(conn, "SELECT Treasury_ID, Name FROM Treasury_Bank WHERE Status='Active'");

                // Providers (Contractors)
                ProviderList = GetList(conn, "SELECT Provider_ID, Provider_Name FROM Provider");

                // Clients (Assuming you have a Client table, otherwise remove this)
                // ClientList = GetList(conn, "SELECT Client_ID, Name FROM Clients"); 
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
            string name = "";
            if (SelectedProviderId.HasValue)
            {
                using (SqlCommand cmd = new SqlCommand("SELECT Provider_Name FROM Provider WHERE Provider_ID=@ID", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@ID", SelectedProviderId);
                    name = "مورد: " + cmd.ExecuteScalar()?.ToString();
                }
            }
            // Add Client logic here if you have a Client table
            return name;
        }
    }
}