using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System;
namespace Petroleum_Materials_Transport_Office_System.Pages.Master_Lists
{
    public class Financial_ConfigurationModel : PageModel
    {
        private readonly string _connectionString = "Server=.;Database=PetroleumTransportDB;Trusted_Connection=True;TrustServerCertificate=True;";
        // ==========================================================
        // 1. DATA LISTS (For Displaying in Tables)
        // ==========================================================
        public List<TreasuryModel> Treasuries { get; set; } = new List<TreasuryModel>();
        public List<ExpenseModel> Expenses { get; set; } = new List<ExpenseModel>();
        public List<CostCenterModel> CostCenters { get; set; } = new List<CostCenterModel>();
        // ==========================================================
        // 2. INPUT MODELS (For Add/Edit Forms)
        // ==========================================================
        [BindProperty] public TreasuryModel TreasuryInput { get; set; }
        [BindProperty] public ExpenseModel ExpenseInput { get; set; }
        [BindProperty] public CostCenterModel CostCenterInput { get; set; }
        // For editing specific items
        [BindProperty] public int EditTreasuryId { get; set; }
        [BindProperty] public int EditExpenseId { get; set; }
        [BindProperty] public int EditCostCenterId { get; set; }
        // ==========================================================
        // 3. SEARCH & FILTERS
        // ==========================================================
        [BindProperty(SupportsGet = true)] public string SearchTreasury { get; set; }
        [BindProperty(SupportsGet = true)] public string SearchExpense { get; set; }
        [BindProperty(SupportsGet = true)] public string SearchCostCenter { get; set; }
        [BindProperty(SupportsGet = true)] public string FilterTreasuryType { get; set; }
        [BindProperty(SupportsGet = true)] public string FilterExpenseCategory { get; set; }
        [BindProperty(SupportsGet = true)] public string FilterCostCenterStatus { get; set; }
        // For modal operations
        [BindProperty] public string OperationType { get; set; }
        // Keeps the correct tab open after page reload
        public string ActiveTab { get; set; } = "treasury";
        // ==========================================================
        // 4. GET REQUEST (Load Data)
        // ==========================================================
        public void OnGet(string activeTab = null)
        {
            if (!string.IsNullOrEmpty(activeTab))
            {
                ActiveTab = activeTab;
            }
            else
            {
                // Logic to keep the correct tab open based on search parameters
                if (!string.IsNullOrEmpty(SearchExpense) || !string.IsNullOrEmpty(FilterExpenseCategory)) ActiveTab = "expenses";
                else if (!string.IsNullOrEmpty(SearchCostCenter) || !string.IsNullOrEmpty(FilterCostCenterStatus)) ActiveTab = "cost-centers";
                else ActiveTab = "treasury";
            }
            LoadTreasuries();
            LoadExpenses();
            LoadCostCenters();
        }
        // ==========================================================
        // 5. POST HANDLERS - TREASURY (Add, Update, Delete)
        // ==========================================================
        public IActionResult OnPostSaveTreasury()
        {
            ActiveTab = "treasury";
            if (TreasuryInput == null)
                return RedirectToPage(new { ActiveTab = "treasury" });
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd;
                // IF ID is 0, it's a NEW record (INSERT)
                if (TreasuryInput.ID == 0)
                {
                    string query = @"INSERT INTO Treasury_Bank (Name, Type, Account_Number, Initial_Balance, Current_Balance, Status)
                                     VALUES (@Name, @Type, @Account, @InitBal, @InitBal, @Status)";
                    cmd = new SqlCommand(query, conn);
                }
                // IF ID > 0, it's an EXISTING record (UPDATE)
                else
                {
                    string query = @"UPDATE Treasury_Bank
                                     SET Name = @Name, Type = @Type, Account_Number = @Account,
                                         Initial_Balance = @InitBal, Status = @Status
                                     WHERE Treasury_ID = @ID";
                    cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ID", TreasuryInput.ID);
                }
                cmd.Parameters.AddWithValue("@Name", TreasuryInput.Name);
                cmd.Parameters.AddWithValue("@Type", TreasuryInput.Type);
                cmd.Parameters.AddWithValue("@Account", TreasuryInput.AccountNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@InitBal", TreasuryInput.InitialBalance);
                cmd.Parameters.AddWithValue("@Status", TreasuryInput.Status);
                cmd.ExecuteNonQuery();
            }
            // Clear the input model
            TreasuryInput = new TreasuryModel();
            return RedirectToPage(new { ActiveTab = "treasury" });
        }
        public IActionResult OnPostDeleteTreasury(int id)
        {
            ActiveTab = "treasury";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("DELETE FROM Treasury_Bank WHERE Treasury_ID = @ID", conn);
                cmd.Parameters.AddWithValue("@ID", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToPage(new { ActiveTab = "treasury" });
        }
        // ==========================================================
        // 6. POST HANDLERS - EXPENSES (Add, Update, Delete)
        // ==========================================================
        public IActionResult OnPostSaveExpense()
        {
            ActiveTab = "expenses";
            if (ExpenseInput == null) return RedirectToPage(new { ActiveTab = "expenses" });
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd;
                if (ExpenseInput.ID == 0) // INSERT
                {
                    string query = @"INSERT INTO Expense_Item (Name, Category, Value_Type, Is_Reportable)
                                     VALUES (@Name, @Category, @ValType, @Reportable)";
                    cmd = new SqlCommand(query, conn);
                }
                else // UPDATE
                {
                    string query = @"UPDATE Expense_Item
                                     SET Name = @Name, Category = @Category, Value_Type = @ValType, Is_Reportable = @Reportable
                                     WHERE Expense_ID = @ID";
                    cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ID", ExpenseInput.ID);
                }
                cmd.Parameters.AddWithValue("@Name", ExpenseInput.Name);
                cmd.Parameters.AddWithValue("@Category", ExpenseInput.Category);
                cmd.Parameters.AddWithValue("@ValType", ExpenseInput.ValueType);
                cmd.Parameters.AddWithValue("@Reportable", ExpenseInput.IsReportable);
                cmd.ExecuteNonQuery();
            }
            // Clear the input model
            ExpenseInput = new ExpenseModel();
            return RedirectToPage(new { ActiveTab = "expenses" });
        }
        public IActionResult OnPostDeleteExpense(int id)
        {
            ActiveTab = "expenses";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("DELETE FROM Expense_Item WHERE Expense_ID = @ID", conn);
                cmd.Parameters.AddWithValue("@ID", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToPage(new { ActiveTab = "expenses" });
        }
        // ==========================================================
        // 7. POST HANDLERS - COST CENTERS (Add, Update, Delete)
        // ==========================================================
        public IActionResult OnPostSaveCostCenter()
        {
            ActiveTab = "cost-centers";
            if (CostCenterInput == null) return RedirectToPage(new { ActiveTab = "cost-centers" });
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd;
                if (CostCenterInput.ID == 0) // INSERT
                {
                    string query = @"INSERT INTO Cost_Center (Name, Supervisor, Status, Created_At)
                                     VALUES (@Name, @Supervisor, @Status, @Created)";
                    cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Created", DateTime.Now);
                }
                else // UPDATE
                {
                    string query = @"UPDATE Cost_Center
                                     SET Name = @Name, Supervisor = @Supervisor, Status = @Status
                                     WHERE CostCenter_ID = @ID";
                    cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ID", CostCenterInput.ID);
                }
                cmd.Parameters.AddWithValue("@Name", CostCenterInput.Name);
                cmd.Parameters.AddWithValue("@Supervisor", CostCenterInput.Supervisor ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", CostCenterInput.Status);
                cmd.ExecuteNonQuery();
            }
            // Clear the input model
            CostCenterInput = new CostCenterModel();
            return RedirectToPage(new { ActiveTab = "cost-centers" });
        }
        public IActionResult OnPostDeleteCostCenter(int id)
        {
            ActiveTab = "cost-centers";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("DELETE FROM Cost_Center WHERE CostCenter_ID = @ID", conn);
                cmd.Parameters.AddWithValue("@ID", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToPage(new { ActiveTab = "cost-centers" });
        }
        // ==========================================================
        // 8. PRIVATE LOADING METHODS (Read Data)
        // ==========================================================
        private void LoadTreasuries()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
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
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Type = reader.GetString(2),
                                AccountNumber = reader.IsDBNull(3) ? "-" : reader.GetString(3),
                                InitialBalance = reader.GetDecimal(4),
                                CurrentBalance = reader.GetDecimal(5),
                                Status = reader.GetString(6)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error here if needed
                Console.WriteLine($"Error loading treasuries: {ex.Message}");
            }
        }
        private void LoadExpenses()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
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
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Category = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                ValueType = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                IsReportable = reader.GetBoolean(4)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading expenses: {ex.Message}");
            }
        }
        private void LoadCostCenters()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
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
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Supervisor = reader.IsDBNull(2) ? "-" : reader.GetString(2),
                                Status = reader.GetString(3),
                                CreatedAt = reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading cost centers: {ex.Message}");
            }
        }
        // ==========================================================
        // 9. AJAX HANDLERS (For Populating Edit Modals via JS)
        // ==========================================================
        public JsonResult OnGetTreasuryDetails(int id)
        {
            try
            {
                TreasuryModel result = null;
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
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
                return new JsonResult(result);
            }
            catch (Exception ex) { return new JsonResult(new { error = ex.Message }); }
        }
        public JsonResult OnGetExpenseDetails(int id)
        {
            try
            {
                ExpenseModel result = null;
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
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
                return new JsonResult(result);
            }
            catch (Exception ex) { return new JsonResult(new { error = ex.Message }); }
        }
        public JsonResult OnGetCostCenterDetails(int id)
        {
            try
            {
                CostCenterModel result = null;
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
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
                return new JsonResult(result);
            }
            catch (Exception ex) { return new JsonResult(new { error = ex.Message }); }
        }
    }
    // ==========================================================
    // 10. DATA MODELS
    // ==========================================================
    public class TreasuryModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string AccountNumber { get; set; }
        public decimal InitialBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Status { get; set; }
    }
    public class ExpenseModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string ValueType { get; set; }
        public bool IsReportable { get; set; }
    }
    public class CostCenterModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Supervisor { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}