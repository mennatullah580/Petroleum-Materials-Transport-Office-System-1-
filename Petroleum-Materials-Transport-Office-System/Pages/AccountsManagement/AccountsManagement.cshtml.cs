using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using AccountsManagement.Models;

namespace Petroleum_Materials_Transport_Office_System.Pages.Accounts
{
    public class AccountsManagementModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public string ActiveTab { get; set; } = "customers";
        public string SearchText { get; set; }

        public List<AccountRecord> Customers = new List<AccountRecord>();
        public List<AccountRecord> Suppliers = new List<AccountRecord>();
        public List<AccountRecord> DisplayedList { get; set; } = new List<AccountRecord>();

        public AccountsManagementModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet(string tab, string search)
        {
            ActiveTab = string.IsNullOrEmpty(tab) ? "customers" : tab;
            SearchText = search;

            LoadCustomers();
            LoadSuppliers();

            var targetList = (ActiveTab == "customers") ? Customers : Suppliers;
            DisplayedList = FilterList(targetList);
        }

        private void LoadCustomers()
        {
            string connectionString = _configuration.GetConnectionString("PetroleumTransportDB");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT Company_ID AS ID, Company_Name AS CompanyName, Address, Contact_Info AS ContactInfo, Net_Balance AS Balance FROM Company";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Customers.Clear();
                    while (reader.Read())
                    {
                        Customers.Add(new AccountRecord
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("ID")),
                            CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            ContactInfo = reader.GetString(reader.GetOrdinal("ContactInfo")),
                            Balance = reader.GetDecimal(reader.GetOrdinal("Balance"))
                        });
                    }
                }
            }
        }

        private void LoadSuppliers()
        {
            string connectionString = _configuration.GetConnectionString("PetroleumTransportDB");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT Provider_ID AS ID, Provider_Name AS CompanyName, '' AS Address, Contact_Info AS ContactInfo, Total_Work_Amount AS Balance FROM Provider";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    Suppliers.Clear();
                    while (reader.Read())
                    {
                        Suppliers.Add(new AccountRecord
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("ID")),
                            CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                            Address = reader.GetString(reader.GetOrdinal("Address")), // هيبقى فارغ ''
                            ContactInfo = reader.GetString(reader.GetOrdinal("ContactInfo")),
                            Balance = reader.GetDecimal(reader.GetOrdinal("Balance"))
                        });
                    }
                }
            }
        }

        private List<AccountRecord> FilterList(List<AccountRecord> list)
        {
            if (string.IsNullOrEmpty(SearchText)) return list;

            var result = new List<AccountRecord>();
            foreach (var rec in list)
            {
                if (!string.IsNullOrEmpty(rec.CompanyName) &&
                    rec.CompanyName.ToLower().Contains(SearchText.ToLower()))
                {
                    result.Add(rec);
                }
            }
            return result;
        }
    }

    public class AccountRecord
    {
        public int ID { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string ContactInfo { get; set; }
        public decimal Balance { get; set; }
    }
}
