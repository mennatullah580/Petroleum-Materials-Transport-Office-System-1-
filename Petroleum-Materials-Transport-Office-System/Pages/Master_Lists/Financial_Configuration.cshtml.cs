using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System;

namespace Petroleum_Materials_Transport_Office_System.Pages.Master_Lists
{
    public class Financial_ConfigurationModel : PageModel
    {
        private readonly string _connectionString = "Server=DESKTOP-1QHK872; Database=PetroleumTransportDB; Integrated Security=True; TrustServerCertificate=True;";

        public List<TreasuryModel> Treasuries { get; set; } = new List<TreasuryModel>();
        public List<ExpenseModel> Expenses { get; set; } = new List<ExpenseModel>();
        public List<CostCenterModel> CostCenters { get; set; } = new List<CostCenterModel>();

        // --- 1. SEARCH VARIABLES (TEXT) ---
        [BindProperty(SupportsGet = true)] public string SearchTreasury { get; set; }
        [BindProperty(SupportsGet = true)] public string SearchExpense { get; set; }
        [BindProperty(SupportsGet = true)] public string SearchCostCenter { get; set; }

        // --- 2. NEW FILTER VARIABLES (DROPDOWNS) ---
        // These match the 'name' attributes in your HTML select tags
        [BindProperty(SupportsGet = true)] public string FilterTreasuryType { get; set; }
        [BindProperty(SupportsGet = true)] public string FilterExpenseCategory { get; set; }
        [BindProperty(SupportsGet = true)] public string FilterCostCenterStatus { get; set; }

        // This variable remembers which tab should be open
        public string ActiveTab { get; set; } = "treasury";

        public void OnGet()
        {
            // --- 3. TAB LOGIC FIX ---
            // We now check if the user typed text OR selected a dropdown item

            // Check Expenses Tab
            if (!string.IsNullOrEmpty(SearchExpense) || !string.IsNullOrEmpty(FilterExpenseCategory))
            {
                ActiveTab = "expenses";
            }
            // Check Cost Centers Tab
            else if (!string.IsNullOrEmpty(SearchCostCenter) || !string.IsNullOrEmpty(FilterCostCenterStatus))
            {
                ActiveTab = "cost-centers";
            }
            // Default to Treasury Tab
            else
            {
                ActiveTab = "treasury";
            }

            // 4. Load the data
            LoadTreasuries();
            LoadExpenses();
            LoadCostCenters();
        }

        private void LoadTreasuries()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Use WHERE 1=1 so we can easily append AND conditions
                string query = "SELECT * FROM Treasury_Bank WHERE 1=1";

                if (!string.IsNullOrEmpty(SearchTreasury))
                    query += " AND Name LIKE @Search";

                if (!string.IsNullOrEmpty(FilterTreasuryType))
                    query += " AND Type = @Type";

                SqlCommand cmd = new SqlCommand(query, conn);

                if (!string.IsNullOrEmpty(SearchTreasury))
                    cmd.Parameters.AddWithValue("@Search", "%" + SearchTreasury + "%");

                if (!string.IsNullOrEmpty(FilterTreasuryType))
                    cmd.Parameters.AddWithValue("@Type", FilterTreasuryType);

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

        private void LoadExpenses()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Expense_Item WHERE 1=1";

                if (!string.IsNullOrEmpty(SearchExpense))
                    query += " AND Name LIKE @Search";

                // IMPORTANT: Ensure your DB column name is actually 'Category' or whatever you named it
                if (!string.IsNullOrEmpty(FilterExpenseCategory))
                    query += " AND Category = @Category";

                SqlCommand cmd = new SqlCommand(query, conn);

                if (!string.IsNullOrEmpty(SearchExpense))
                    cmd.Parameters.AddWithValue("@Search", "%" + SearchExpense + "%");

                if (!string.IsNullOrEmpty(FilterExpenseCategory))
                    cmd.Parameters.AddWithValue("@Category", FilterExpenseCategory);

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

        private void LoadCostCenters()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Cost_Center WHERE 1=1";

                if (!string.IsNullOrEmpty(SearchCostCenter))
                    query += " AND Name LIKE @Search";

                if (!string.IsNullOrEmpty(FilterCostCenterStatus))
                    query += " AND Status = @Status";

                SqlCommand cmd = new SqlCommand(query, conn);

                if (!string.IsNullOrEmpty(SearchCostCenter))
                    cmd.Parameters.AddWithValue("@Search", "%" + SearchCostCenter + "%");

                if (!string.IsNullOrEmpty(FilterCostCenterStatus))
                    cmd.Parameters.AddWithValue("@Status", FilterCostCenterStatus);

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
    }

    public class TreasuryModel { public int ID { get; set; } public string Name { get; set; } public string Type { get; set; } public string AccountNumber { get; set; } public decimal InitialBalance { get; set; } public decimal CurrentBalance { get; set; } public string Status { get; set; } }
    public class ExpenseModel { public int ID { get; set; } public string Name { get; set; } public string Category { get; set; } public string ValueType { get; set; } public bool IsReportable { get; set; } }
    public class CostCenterModel { public int ID { get; set; } public string Name { get; set; } public string Supervisor { get; set; } public string Status { get; set; } public DateTime CreatedAt { get; set; } }
}