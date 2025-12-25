using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using AccountsManagement.Models;

namespace Petroleum_Materials_Transport_Office_System.Pages.Accounts
{
    public class AccountsManagementModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public string ActiveTab { get; set; } = "customers";
        public string SearchText { get; set; }

        public List<AccountRecord> Customers { get; set; } = new List<AccountRecord>();
        public List<AccountRecord> Suppliers { get; set; } = new List<AccountRecord>();
        public List<AccountRecord> DisplayedList { get; set; } = new List<AccountRecord>();

        public AccountsManagementModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet(string tab, string search)
        {
            ActiveTab = string.IsNullOrEmpty(tab) ? "customers" : tab;
            SearchText = search;

            LoadAccounts();

            DisplayedList = (ActiveTab == "customers")
                ? FilterList(Customers)
                : FilterList(Suppliers);
        }

        private void LoadAccounts()
        {
            string connectionString = _configuration.GetConnectionString("PetroleumDB");
            SqlConnection conn = new SqlConnection(connectionString);

            try
            {
                conn.Open();

                // ================== LOAD CUSTOMERS ==================
                string customerSql = @"SELECT 
                                        Company_ID AS ID,
                                        Company_Name AS CompanyName,
                                        Address,
                                        Contact_Info AS ContactInfo,
                                        Net_Balance AS Balance
                                       FROM Company";

                SqlCommand customerCmd = new SqlCommand(customerSql, conn);
                SqlDataReader customerReader = customerCmd.ExecuteReader();

                Customers.Clear();

                while (customerReader.Read())
                {
                    Customers.Add(new AccountRecord
                    {
                        ID = customerReader.GetInt32(customerReader.GetOrdinal("ID")),
                        CompanyName = customerReader.GetString(customerReader.GetOrdinal("CompanyName")),
                        Address = customerReader.GetString(customerReader.GetOrdinal("Address")),
                        ContactInfo = customerReader.GetString(customerReader.GetOrdinal("ContactInfo")),
                        Balance = customerReader.GetDecimal(customerReader.GetOrdinal("Balance"))
                    });
                }

                customerReader.Close();

                // ================== LOAD SUPPLIERS ==================
                string supplierSql = @"SELECT 
                                        Provider_ID AS ID,
                                        Provider_Name AS CompanyName,
                                        '' AS Address,
                                        Contact_Info AS ContactInfo,
                                        Total_Work_Amount AS Balance
                                       FROM Provider";

                SqlCommand supplierCmd = new SqlCommand(supplierSql, conn);
                SqlDataReader supplierReader = supplierCmd.ExecuteReader();

                Suppliers.Clear();

                while (supplierReader.Read())
                {
                    Suppliers.Add(new AccountRecord
                    {
                        ID = supplierReader.GetInt32(supplierReader.GetOrdinal("ID")),
                        CompanyName = supplierReader.GetString(supplierReader.GetOrdinal("CompanyName")),
                        Address = supplierReader.GetString(supplierReader.GetOrdinal("Address")),
                        ContactInfo = supplierReader.GetString(supplierReader.GetOrdinal("ContactInfo")),
                        Balance = supplierReader.GetDecimal(supplierReader.GetOrdinal("Balance"))
                    });
                }

                supplierReader.Close();
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("General Error: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private List<AccountRecord> FilterList(List<AccountRecord> list)
        {
            if (string.IsNullOrEmpty(SearchText))
                return list;

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