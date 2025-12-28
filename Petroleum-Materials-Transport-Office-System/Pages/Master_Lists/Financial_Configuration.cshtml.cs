using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System;

namespace Petroleum_Materials_Transport_Office_System.Pages.Master_Lists
{
    public class Financial_ConfigurationModel : PageModel
    {
        // YOUR EXACT CONNECTION STRING
        private readonly string _connectionString = "Server=DESKTOP-1QHK872;Database=PetroleumTransportDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public List<TreasuryModel> Treasuries { get; set; } = new List<TreasuryModel>();
        public List<ExpenseModel> Expenses { get; set; } = new List<ExpenseModel>();
        public List<CostCenterModel> CostCenters { get; set; } = new List<CostCenterModel>();

        [BindProperty(SupportsGet = true)] public string SearchTreasury { get; set; }
        [BindProperty(SupportsGet = true)] public string SearchExpense { get; set; }
        [BindProperty(SupportsGet = true)] public string SearchCostCenter { get; set; }

        [BindProperty(SupportsGet = true)] public string FilterTreasuryType { get; set; }
        [BindProperty(SupportsGet = true)] public string FilterExpenseCategory { get; set; }
        [BindProperty(SupportsGet = true)] public string FilterCostCenterStatus { get; set; }

        public string ActiveTab { get; set; } = "treasury";

        public void OnGet()
        {
            if (!string.IsNullOrEmpty(SearchExpense) || !string.IsNullOrEmpty(FilterExpenseCategory)) ActiveTab = "expenses";
            else if (!string.IsNullOrEmpty(SearchCostCenter) || !string.IsNullOrEmpty(FilterCostCenterStatus)) ActiveTab = "cost-centers";
            else ActiveTab = "treasury";

            try { LoadTreasuries(); } catch { }
            try { LoadExpenses(); } catch { }
            try { LoadCostCenters(); } catch { }
        }

        private void LoadTreasuries()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Updated Column Names: Treasury_ID, Account_Number
                string query = "SELECT Treasury_ID, Name, Type, Account_Number, Initial_Balance, Current_Balance, Status FROM Treasury_Bank WHERE 1=1";

                if (!string.IsNullOrEmpty(SearchTreasury)) query += " AND Name LIKE @Search";
                if (!string.IsNullOrEmpty(FilterTreasuryType)) query += " AND Type = @Type";

                SqlCommand cmd = new SqlCommand(query, conn);
                if (!string.IsNullOrEmpty(SearchTreasury)) cmd.Parameters.AddWithValue("@Search", "%" + SearchTreasury + "%");
                if (!string.IsNullOrEmpty(FilterTreasuryType)) cmd.Parameters.AddWithValue("@Type", FilterTreasuryType);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Treasuries.Add(new TreasuryModel
                        {
                            ID = reader.GetInt32(0), // Maps to Treasury_ID
                            Name = reader.GetString(1),
                            Type = reader.GetString(2),
                            AccountNumber = reader.IsDBNull(3) ? "-" : reader.GetString(3), // Maps to Account_Number
                            InitialBalance = reader.GetDecimal(4),
                            CurrentBalance = reader.GetDecimal(5),
                            Status = reader.GetString(6)
                        });
                    }
                }
            }
        }

        private void LoadExpenses()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Updated Column Names: Expense_ID, Value_Type, Is_Reportable
                string query = "SELECT Expense_ID, Name, Category, Value_Type, Is_Reportable FROM Expense_Item WHERE 1=1";

                if (!string.IsNullOrEmpty(SearchExpense)) query += " AND Name LIKE @Search";
                if (!string.IsNullOrEmpty(FilterExpenseCategory)) query += " AND Category = @Category";

                SqlCommand cmd = new SqlCommand(query, conn);
                if (!string.IsNullOrEmpty(SearchExpense)) cmd.Parameters.AddWithValue("@Search", "%" + SearchExpense + "%");
                if (!string.IsNullOrEmpty(FilterExpenseCategory)) cmd.Parameters.AddWithValue("@Category", FilterExpenseCategory);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Expenses.Add(new ExpenseModel
                        {
                            ID = reader.GetInt32(0), // Maps to Expense_ID
                            Name = reader.GetString(1),
                            Category = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            ValueType = reader.IsDBNull(3) ? "" : reader.GetString(3), // Maps to Value_Type
                            IsReportable = reader.GetBoolean(4) // Maps to Is_Reportable
                        });
                    }
                }
            }
        }

        private void LoadCostCenters()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Updated Column Names: CostCenter_ID
                string query = "SELECT CostCenter_ID, Name, Supervisor, Status, Created_At FROM Cost_Center WHERE 1=1";

                if (!string.IsNullOrEmpty(SearchCostCenter)) query += " AND Name LIKE @Search";
                if (!string.IsNullOrEmpty(FilterCostCenterStatus)) query += " AND Status = @Status";

                SqlCommand cmd = new SqlCommand(query, conn);
                if (!string.IsNullOrEmpty(SearchCostCenter)) cmd.Parameters.AddWithValue("@Search", "%" + SearchCostCenter + "%");
                if (!string.IsNullOrEmpty(FilterCostCenterStatus)) cmd.Parameters.AddWithValue("@Status", FilterCostCenterStatus);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CostCenters.Add(new CostCenterModel
                        {
                            ID = reader.GetInt32(0), // Maps to CostCenter_ID
                            Name = reader.GetString(1),
                            Supervisor = reader.IsDBNull(2) ? "-" : reader.GetString(2),
                            Status = reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4)
                        });
                    }
                }
            }
        }

        // ==========================================================
        //  AJAX HANDLERS - UPDATED WITH EXACT COLUMN NAMES
        // ==========================================================

        public JsonResult OnGetTreasuryDetails(int id)
        {
            try
            {
                TreasuryModel result = null;
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // FIX: Changed WHERE ID to WHERE Treasury_ID
                    string query = "SELECT Treasury_ID, Name, Type, Account_Number, Initial_Balance, Current_Balance, Status FROM Treasury_Bank WHERE Treasury_ID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = new TreasuryModel
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Type = reader.GetString(2),
                                AccountNumber = reader.IsDBNull(3) ? "-" : reader.GetString(3),
                                InitialBalance = reader.GetDecimal(4),
                                CurrentBalance = reader.GetDecimal(5),
                                Status = reader.GetString(6)
                            };
                        }
                    }
                }
                if (result == null) return new JsonResult(new { error = "Record not found" });
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        public JsonResult OnGetExpenseDetails(int id)
        {
            try
            {
                ExpenseModel result = null;
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // FIX: Changed WHERE ID to WHERE Expense_ID
                    string query = "SELECT Expense_ID, Name, Category, Value_Type, Is_Reportable FROM Expense_Item WHERE Expense_ID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = new ExpenseModel
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Category = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                ValueType = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                IsReportable = reader.GetBoolean(4)
                            };
                        }
                    }
                }
                if (result == null) return new JsonResult(new { error = "Record not found" });
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        public JsonResult OnGetCostCenterDetails(int id)
        {
            try
            {
                CostCenterModel result = null;
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // FIX: Changed WHERE ID to WHERE CostCenter_ID
                    string query = "SELECT CostCenter_ID, Name, Supervisor, Status, Created_At FROM Cost_Center WHERE CostCenter_ID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = new CostCenterModel
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Supervisor = reader.IsDBNull(2) ? "-" : reader.GetString(2),
                                Status = reader.GetString(3),
                                CreatedAt = reader.GetDateTime(4)
                            };
                        }
                    }
                }
                if (result == null) return new JsonResult(new { error = "Record not found" });
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }
    }

    public class TreasuryModel { public int ID { get; set; } public string Name { get; set; } public string Type { get; set; } public string AccountNumber { get; set; } public decimal InitialBalance { get; set; } public decimal CurrentBalance { get; set; } public string Status { get; set; } }
    public class ExpenseModel { public int ID { get; set; } public string Name { get; set; } public string Category { get; set; } public string ValueType { get; set; } public bool IsReportable { get; set; } }
    public class CostCenterModel { public int ID { get; set; } public string Name { get; set; } public string Supervisor { get; set; } public string Status { get; set; } public DateTime CreatedAt { get; set; } }
}