using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace Petroleum_Materials_Transport_Office_System.Pages.Master_Lists
{
    public class StakeholdersModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly string _connString = "Server=.; Database=PetroleumTransportDB; Integrated Security=True; TrustServerCertificate=True;";

        public StakeholdersModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<ClientDto> Clients { get; set; } = new List<ClientDto>();
        public List<SupplierDto> Suppliers { get; set; } = new List<SupplierDto>();
        public List<ContractorDto> Contractors { get; set; } = new List<ContractorDto>();
        public List<string> AvailableFuelTypes { get; set; } = new List<string>();
        public string ErrorMessage { get; set; } = "";

        // FIX: Add this property to track the active tab (Default is 'clients')
        [BindProperty(SupportsGet = true)]
        public string ActiveTab { get; set; } = "clients";

        public void OnGet()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connString))
                {
                    conn.Open();

                    // ==========================================
                    // 1. CLIENTS
                    // ==========================================
                    string clientQuery = @"
                        SELECT 
                            c.Company_ID, c.Company_Name, c.Phone_Number, c.Address, 
                            (ISNULL(c.Initial_Balance, 0) + ISNULL((SELECT SUM(f.Company_Total) FROM Financials f JOIN Orders o ON f.Order_ID = o.Order_ID WHERE o.Company_ID = c.Company_ID), 0) - ISNULL((SELECT SUM(pt.Amount) FROM Payment_Transaction pt WHERE pt.Company_ID = c.Company_ID AND pt.Type = 'Payment'), 0)) as Current_Balance
                        FROM Company c WHERE 1=1";

                    if (!string.IsNullOrEmpty(Request.Query["clientName"]))
                        clientQuery += " AND c.Company_Name LIKE N'%" + Request.Query["clientName"] + "%'";

                    using (SqlCommand cmd = new SqlCommand(clientQuery, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Clients.Add(new ClientDto
                            {
                                ClientId = reader["Company_ID"].ToString(),
                                Name = reader["Company_Name"].ToString(),
                                Phone = reader["Phone_Number"] != DBNull.Value ? reader["Phone_Number"].ToString() : "-",
                                Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : "-",
                                CurrentBalance = Convert.ToDecimal(reader["Current_Balance"])
                            });
                        }
                    }

                    // ==========================================
                    // 2. CONTRACTORS
                    // ==========================================
                    string contractorQuery = @"
                        SELECT Provider_ID, Provider_Name, Phone_Number, Contact_Info, Total_Work_Amount, Total_Advances, Total_Custody, Status 
                        FROM Provider WHERE 1=1";

                    if (!string.IsNullOrEmpty(Request.Query["contractorName"]))
                        contractorQuery += " AND Provider_Name LIKE N'%" + Request.Query["contractorName"] + "%'";

                    if (!string.IsNullOrEmpty(Request.Query["contractorStatus"]) && Request.Query["contractorStatus"] != "الكل")
                        contractorQuery += " AND Status = '" + Request.Query["contractorStatus"] + "'";

                    using (SqlCommand cmd = new SqlCommand(contractorQuery, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal totalWork = reader["Total_Work_Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Total_Work_Amount"]) : 0;
                            decimal totalAdv = reader["Total_Advances"] != DBNull.Value ? Convert.ToDecimal(reader["Total_Advances"]) : 0;
                            decimal totalCustody = reader["Total_Custody"] != DBNull.Value ? Convert.ToDecimal(reader["Total_Custody"]) : 0;

                            Contractors.Add(new ContractorDto
                            {
                                ContractorId = reader["Provider_ID"].ToString(),
                                Name = reader["Provider_Name"].ToString(),
                                Phone = reader["Phone_Number"] != DBNull.Value ? reader["Phone_Number"].ToString() : "-",
                                Address = reader["Contact_Info"] != DBNull.Value ? reader["Contact_Info"].ToString() : "-",
                                OrderCount = 0,
                                TotalWork = totalWork,
                                TotalAdvances = totalAdv,
                                TotalCustody = totalCustody,
                                NetBalance = totalWork - totalAdv - totalCustody,
                                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "Active"
                            });
                        }
                    }

                    // ==========================================
                    // 3. SUPPLIERS
                    // ==========================================
                    string supplierQuery = @"
                        SELECT L.Location_Code, L.Location_Name, FT.Type_Name
                        FROM Location L
                        LEFT JOIN Orders O ON L.Location_Code = O.Loading_Location
                        LEFT JOIN Fuel_Type FT ON O.Petroleum_Type = FT.Fuel_ID";

                    using (SqlCommand cmd = new SqlCommand(supplierQuery, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string locId = reader["Location_Code"].ToString();
                            string locName = reader["Location_Name"].ToString();
                            string fuelName = reader["Type_Name"] != DBNull.Value ? reader["Type_Name"].ToString() : "غير محدد";

                            string searchName = Request.Query["supplierName"];
                            if (!string.IsNullOrEmpty(searchName) && !locName.Contains(searchName)) continue;

                            string searchType = Request.Query["materialType"];
                            if (!string.IsNullOrEmpty(searchType) && fuelName != searchType) continue;

                            var existing = Suppliers.FirstOrDefault(s => s.SupplierId == locId);
                            if (existing == null)
                            {
                                Suppliers.Add(new SupplierDto { SupplierId = locId, Name = locName, FuelType = fuelName });
                            }
                            else
                            {
                                if (fuelName != "غير محدد" && !existing.FuelType.Contains(fuelName))
                                {
                                    if (existing.FuelType == "غير محدد") existing.FuelType = fuelName;
                                    else existing.FuelType += ", " + fuelName;
                                }
                            }

                            if (fuelName != "غير محدد" && !AvailableFuelTypes.Contains(fuelName))
                                AvailableFuelTypes.Add(fuelName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "FATAL ERROR: " + ex.Message;
            }
        }
    }

    public class ClientDto
    {
        public string ClientId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal CurrentBalance { get; set; }
    }

    public class SupplierDto
    {
        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string FuelType { get; set; } = "";
        public string ResponsiblePerson { get; set; } = "-";
        public string Phone { get; set; } = "-";
    }

    public class ContractorDto
    {
        public string ContractorId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalWork { get; set; }
        public decimal TotalAdvances { get; set; }
        public decimal TotalCustody { get; set; }
        public decimal NetBalance { get; set; }
        public string Status { get; set; }
    }
}