using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace Petroleum_Materials_Transport_Office_System.Pages.Master_Lists
{
    public class ResourcesModel : PageModel
    {
        private readonly string _connString = "Server=.; Database=PetroleumTransportDB; Integrated Security=True; TrustServerCertificate=True;";

        public List<VehicleDto> Vehicles { get; set; } = new List<VehicleDto>();
        public List<DriverDto> Drivers { get; set; } = new List<DriverDto>();
        public List<WarehouseDto> Warehouses { get; set; } = new List<WarehouseDto>();
        public List<MappingDto> Mappings { get; set; } = new List<MappingDto>();

        // Dropdown for Mapping Filter
        public List<SelectListItem> ContractorsList { get; set; } = new List<SelectListItem>();

        [BindProperty(SupportsGet = true)] public string ActiveTab { get; set; } = "vehicles"; // To keep tab open

        // Vehicle Filters
        [BindProperty(SupportsGet = true)] public string VehicleSearch { get; set; }
        [BindProperty(SupportsGet = true)] public string VehicleStatus { get; set; }

        // Driver Filters
        [BindProperty(SupportsGet = true)] public string DriverSearch { get; set; }

        // Warehouse Filters
        [BindProperty(SupportsGet = true)] public string WarehouseSearch { get; set; }

        // Mapping Filters
        [BindProperty(SupportsGet = true)] public string MappingContractor { get; set; }

        public void OnGet()
        {
            try
            {
                // Load Dropdown Data
                LoadContractorsList();

                // Load Tab Data
                LoadVehicles();
                LoadDrivers();
                LoadWarehouses();
                LoadMappings();
            }
            catch (SqlException ex)
            {
                // In a real app, log this error
                Console.WriteLine("Database Error: " + ex.Message);
            }
        }

        // ---------------------------------------------------------
        // 1. Load Vehicles
        // ---------------------------------------------------------
        private void LoadVehicles()
        {
            Vehicles.Clear();
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                string sql = @"
                    SELECT Vehicle_ID, Plate_Number, Model, Capacity, Status 
                    FROM Vehicle 
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(VehicleSearch))
                    sql += " AND (Plate_Number LIKE @Search OR CAST(Vehicle_ID AS NVARCHAR) LIKE @Search)";

                if (!string.IsNullOrEmpty(VehicleStatus) && VehicleStatus != "الكل")
                {
                    string dbStatus = VehicleStatus == "نشط" ? "Active" : "Maintenance";
                    sql += " AND Status = @Status";
                }

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(VehicleSearch)) cmd.Parameters.AddWithValue("@Search", "%" + VehicleSearch + "%");
                if (!string.IsNullOrEmpty(VehicleStatus) && VehicleStatus != "الكل")
                    cmd.Parameters.AddWithValue("@Status", VehicleStatus == "نشط" ? "Active" : "Maintenance");

                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Vehicles.Add(new VehicleDto
                        {
                            VehicleCode = r["Vehicle_ID"].ToString(),
                            PlateNumber = r["Plate_Number"].ToString(),
                            Model = r["Model"].ToString(),
                            Capacity = Convert.ToDouble(r["Capacity"]),
                            Status = r["Status"].ToString()
                        });
                    }
                }
            }
        }

        // ---------------------------------------------------------
        // 2. Load Drivers
        // ---------------------------------------------------------
        private void LoadDrivers()
        {
            Drivers.Clear();
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                string sql = "SELECT * FROM Driver WHERE 1=1";

                if (!string.IsNullOrEmpty(DriverSearch))
                    sql += " AND Name LIKE @Name";

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(DriverSearch)) cmd.Parameters.AddWithValue("@Name", "%" + DriverSearch + "%");

                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Drivers.Add(new DriverDto
                        {
                            DriverCode = r["Driver_ID"].ToString(),
                            Name = r["Name"].ToString(),
                            LicenseNumber = r["License_Number"].ToString(),
                            Phone = r["Phone_Number"].ToString(),
                            Status = r["Availability_Status"].ToString()
                        });
                    }
                }
            }
        }

        // ---------------------------------------------------------
        // 3. Load Warehouses
        // ---------------------------------------------------------
        private void LoadWarehouses()
        {
            Warehouses.Clear();
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                string sql = "SELECT * FROM Warehouse WHERE 1=1";

                if (!string.IsNullOrEmpty(WarehouseSearch))
                    sql += " AND Location LIKE @Search"; // Using Location column as Name based on schema

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(WarehouseSearch)) cmd.Parameters.AddWithValue("@Search", "%" + WarehouseSearch + "%");

                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Warehouses.Add(new WarehouseDto
                        {
                            Code = r["Warehouse_ID"].ToString(),
                            Name = "مستودع " + r["Location"].ToString(), // Formatting name
                            Location = r["Location"].ToString(),
                            Supervisor = r["Supervisor_Name"].ToString()
                        });
                    }
                }
            }
        }

        private void LoadMappings()
        {
            Mappings.Clear();
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                string sql = @"
                    SELECT v.Vehicle_ID, p.Provider_Name, v.Plate_Number, v.Status
                    FROM Vehicle v
                    INNER JOIN Provider p ON v.Assigned_Provider = p.Provider_ID
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(MappingContractor) && MappingContractor != "اختر المقاول...")
                    sql += " AND p.Provider_ID = @ProvID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(MappingContractor) && MappingContractor != "اختر المقاول...")
                    cmd.Parameters.AddWithValue("@ProvID", MappingContractor);

                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Mappings.Add(new MappingDto
                        {
                            LinkID = r["Vehicle_ID"].ToString(), // Using VehicleID as Link Reference
                            ContractorName = r["Provider_Name"].ToString(),
                            VehicleNumber = r["Plate_Number"].ToString(),
                            StartDate = "-", // Schema doesn't track specific mapping start date, utilizing placeholder
                            EndDate = "-",   // Schema doesn't track specific mapping end date
                            Status = r["Status"].ToString()
                        });
                    }
                }
            }
        }

        private void LoadContractorsList()
        {
            ContractorsList.Add(new SelectListItem("اختر المقاول...", ""));
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Provider_ID, Provider_Name FROM Provider ORDER BY Provider_Name", conn);
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        ContractorsList.Add(new SelectListItem(r["Provider_Name"].ToString(), r["Provider_ID"].ToString()));
                    }
                }
            }
        }

        // =========================================================
        // Data Transfer Objects (DTOs)
        // =========================================================
        public class VehicleDto
        {
            public string VehicleCode { get; set; }
            public string PlateNumber { get; set; }
            public string Model { get; set; }
            public double Capacity { get; set; }
            public string Status { get; set; }
        }

        public class DriverDto
        {
            public string DriverCode { get; set; }
            public string Name { get; set; }
            public string LicenseNumber { get; set; }
            public string Phone { get; set; }
            public string Status { get; set; }
        }

        public class WarehouseDto
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }
            public string Supervisor { get; set; }
        }

        public class MappingDto
        {
            public string LinkID { get; set; }
            public string ContractorName { get; set; }
            public string VehicleNumber { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string Status { get; set; }
        }
    }
}