using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace Petroleum_Materials_Transport_Office_System.Pages.Finance
{
    public class ExpensesModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ExpensesModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ==========================================
        // 1. DATA BINDING
        // ==========================================
        [BindProperty]
        public int SelectedTreasuryId { get; set; }

        [BindProperty]
        public int SelectedExpenseId { get; set; }

        [BindProperty]
        public int SelectedCostCenterId { get; set; }

        [BindProperty]
        public decimal Amount { get; set; }

        [BindProperty]
        public DateTime Date { get; set; } = DateTime.Today;

        [BindProperty]
        public string Description { get; set; }

        // Dropdown Lists
        public List<SelectListItem> TreasuryList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ExpenseList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CostCenterList { get; set; } = new List<SelectListItem>();

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        // ==========================================
        // 2. ON GET (Load Data)
        // ==========================================
        public void OnGet()
        {
            LoadDropdowns();
        }

        // ==========================================
        // 3. ON POST (Save Transaction)
        // ==========================================
        public IActionResult OnPost()
        {
            // 1. Basic Validation
            if (Amount <= 0)
            {
                ErrorMessage = "يجب إدخال مبلغ أكبر من صفر.";
                LoadDropdowns();
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // 2. Get Treasury Details (Check Balance)
                string treasuryName = "";
                string treasuryType = "";
                decimal currentBalance = 0;

                string checkSql = "SELECT Name, Type, Current_Balance FROM Treasury_Bank WHERE Treasury_ID = @TID";
                using (SqlCommand cmd = new SqlCommand(checkSql, connection))
                {
                    cmd.Parameters.AddWithValue("@TID", SelectedTreasuryId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            treasuryName = reader["Name"].ToString();
                            treasuryType = reader["Type"].ToString();
                            currentBalance = Convert.ToDecimal(reader["Current_Balance"]);
                        }
                        else
                        {
                            ErrorMessage = "الخزينة المختارة غير موجودة.";
                            LoadDropdowns();
                            return Page();
                        }
                    }
                }

                // 3. Check Sufficient Funds
                if (currentBalance < Amount)
                {
                    ErrorMessage = $"عفواً، رصيد {treasuryName} غير كافٍ. الرصيد الحالي: {currentBalance:N2} ج.م";
                    LoadDropdowns();
                    return Page();
                }

                // 4. Get Helper Names (for Remarks)
                string expenseName = GetNameById(connection, "Expense_Item", "Expense_ID", SelectedExpenseId);
                string costCenterName = GetNameById(connection, "Cost_Center", "CostCenter_ID", SelectedCostCenterId);

                // 5. EXECUTE TRANSACTION (ACID Compliant)
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // A. Deduct Money from Treasury
                        string updateSql = "UPDATE Treasury_Bank SET Current_Balance = Current_Balance - @Amt WHERE Treasury_ID = @TID";
                        using (SqlCommand cmd = new SqlCommand(updateSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Amt", Amount);
                            cmd.Parameters.AddWithValue("@TID", SelectedTreasuryId);
                            cmd.ExecuteNonQuery();
                        }

                        // B. Insert into Payment_Transaction
                        // Note: We format the Description to include the Cost Center/Expense Type since the original table might not have those FK columns.
                        string fullRemarks = $"[مصروفات] بند: {expenseName} | مشروع: {costCenterName} | {Description}";
                        string method = (treasuryType == "Bank") ? "Bank Transfer" : "Cash";

                        string insertSql = @"
                            INSERT INTO Payment_Transaction 
                            (Amount, Date, Type, Method, Status, Remarks, Created_At)
                            VALUES (@Amt, @Date, N'Payment', @Method, N'Completed', @Remarks, GETDATE())";

                        using (SqlCommand cmd = new SqlCommand(insertSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Amt", Amount);
                            cmd.Parameters.AddWithValue("@Date", Date);
                            cmd.Parameters.AddWithValue("@Method", method);
                            cmd.Parameters.AddWithValue("@Remarks", fullRemarks);
                            cmd.ExecuteNonQuery();
                        }

                        // C. Commit Changes
                        transaction.Commit();
                        SuccessMessage = "تم تسجيل المصروف وخصم المبلغ بنجاح.";

                        // Clear Form
                        Amount = 0;
                        Description = "";
                        SelectedTreasuryId = 0;
                        SelectedExpenseId = 0;
                        SelectedCostCenterId = 0;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ErrorMessage = "حدث خطأ أثناء الحفظ: " + ex.Message;
                    }
                }
            }

            LoadDropdowns();
            return Page();
        }

        // ==========================================
        // HELPER METHODS
        // ==========================================
        private void LoadDropdowns()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Load Treasuries
                    TreasuryList = GetDropdownList(connection, "SELECT Treasury_ID, Name FROM Treasury_Bank WHERE Status='Active'");

                    // Load Expenses
                    ExpenseList = GetDropdownList(connection, "SELECT Expense_ID, Name FROM Expense_Item");

                    // Load Cost Centers
                    CostCenterList = GetDropdownList(connection, "SELECT CostCenter_ID, Name FROM Cost_Center WHERE Status='Active'");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error loading lists: " + ex.Message;
            }
        }

        private List<SelectListItem> GetDropdownList(SqlConnection conn, string query)
        {
            var list = new List<SelectListItem>();
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    list.Add(new SelectListItem { Value = r[0].ToString(), Text = r[1].ToString() });
                }
            }
            return list;
        }

        private string GetNameById(SqlConnection conn, string table, string idCol, int idVal)
        {
            string sql = $"SELECT Name FROM {table} WHERE {idCol} = @ID";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", idVal);
                return cmd.ExecuteScalar()?.ToString() ?? "Unknown";
            }
        }
    }
}