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
        // Data Properties
        public List<VehicleDto> Vehicles { get; set; } = new List<VehicleDto>();
        public List<DriverDto> Drivers { get; set; } = new List<DriverDto>();
        public List<WarehouseDto> Warehouses { get; set; } = new List<WarehouseDto>();
        public List<MappingDto> Mappings { get; set; } = new List<MappingDto>();
        // Input Models for Forms
        [BindProperty] public VehicleInputModel VehicleInput { get; set; }
        [BindProperty] public DriverInputModel DriverInput { get; set; }
        [BindProperty] public WarehouseInputModel WarehouseInput { get; set; }
        [BindProperty] public MappingInputModel MappingInput { get; set; }
        // Dropdown Lists
        public List<SelectListItem> ContractorsList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> VehicleList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProviderList { get; set; } = new List<SelectListItem>();
        // Active Tab
        [BindProperty] public string ActiveTab { get; set; } = "vehicles";
        // Search Filters
        [BindProperty(SupportsGet = true)] public string VehicleSearch { get; set; }
        [BindProperty(SupportsGet = true)] public string VehicleStatus { get; set; }
        [BindProperty(SupportsGet = true)] public string DriverSearch { get; set; }
        [BindProperty(SupportsGet = true)] public string WarehouseSearch { get; set; }
        [BindProperty(SupportsGet = true)] public string MappingContractor { get; set; }
        public void OnGet(string activeTab = null)
        {
            if (!string.IsNullOrEmpty(activeTab))
            {
                ActiveTab = activeTab;
            }
            // Load Dropdown Data
            LoadContractorsList();
            LoadVehicleList();
            LoadProviderList();
            // Load Tab Data
            LoadVehicles();
            LoadDrivers();
            LoadWarehouses();
            LoadMappings();
        }
        // =========================================================
        // VEHICLE CRUD OPERATIONS
        // =========================================================
        public IActionResult OnPostSaveVehicle()
        {
            if (VehicleInput == null)
                return RedirectToPage(new { activeTab = "vehicles" });
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                SqlCommand cmd;
                if (VehicleInput.VehicleId == 0) // INSERT
                {
                    string query = @"INSERT INTO Vehicle
                                    (Plate_Number, Model, Capacity, Status, Assigned_Provider)
                                    VALUES (@Plate, @Model, @Capacity, @Status, @Provider)";
                    cmd = new SqlCommand(query, conn);
                }
                else // UPDATE
                {
                    string query = @"UPDATE Vehicle
                                    SET Plate_Number = @Plate,
                                        Model = @Model,
                                        Capacity = @Capacity,
                                        Status = @Status,
                                        Assigned_Provider = @Provider
                                    WHERE Vehicle_ID = @Id";
                    cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", VehicleInput.VehicleId);
                }
                cmd.Parameters.AddWithValue("@Plate", VehicleInput.PlateNumber);
                cmd.Parameters.AddWithValue("@Model", VehicleInput.Model);
                cmd.Parameters.AddWithValue("@Capacity", VehicleInput.Capacity);
                cmd.Parameters.AddWithValue("@Status", VehicleInput.Status);
                cmd.Parameters.AddWithValue("@Provider", VehicleInput.AssignedProvider ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            VehicleInput = new VehicleInputModel();
            return RedirectToPage(new { activeTab = "vehicles" });
        }
        public IActionResult OnPostDeleteVehicle(int id, string activeTab = "vehicles")
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                string query = "DELETE FROM Vehicle WHERE Vehicle_ID = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToPage(new { activeTab });
        }
        // =========================================================
        // DRIVER CRUD OPERATIONS
        // =========================================================
        public IActionResult OnPostSaveDriver()
        {
            if (DriverInput == null)
                return RedirectToPage(new { activeTab = "drivers" });
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                SqlCommand cmd;
                if (DriverInput.DriverId == 0) // INSERT
                {
                    string query = @"INSERT INTO Driver
                                    (Name, License_Number, Phone_Number, Availability_Status)
                                    VALUES (@Name, @License, @Phone, @Status)";
                    cmd = new SqlCommand(query, conn);
                }
                else // UPDATE
                {
                    string query = @"UPDATE Driver
                                    SET Name = @Name,
                                        License_Number = @License,
                                        Phone_Number = @Phone,
                                        Availability_Status = @Status
                                    WHERE Driver_ID = @Id";
                    cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", DriverInput.DriverId);
                }
                cmd.CommandTimeout = 120;
                cmd.Parameters.AddWithValue("@Name", DriverInput.Name);
                cmd.Parameters.AddWithValue("@License", DriverInput.LicenseNumber);
                cmd.Parameters.AddWithValue("@Phone", DriverInput.PhoneNumber);
                cmd.Parameters.AddWithValue("@Status", DriverInput.Status);
                cmd.ExecuteNonQuery();
            }
            DriverInput = new DriverInputModel();
            return RedirectToPage(new { activeTab = "drivers" });
        }
        public IActionResult OnPostDeleteDriver(int id, string activeTab = "drivers")
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                string query = "DELETE FROM Driver WHERE Driver_ID = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToPage(new { activeTab });
        }
        // =========================================================
        // WAREHOUSE CRUD OPERATIONS
        // =========================================================
        public IActionResult OnPostSaveWarehouse()
        {
            if (WarehouseInput == null)
                return RedirectToPage(new { activeTab = "warehouses" });
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                SqlCommand cmd;
                if (WarehouseInput.WarehouseId == 0) // INSERT
                {
                    string query = @"INSERT INTO Warehouse
                                    (Location, Supervisor_Name, Capacity)
                                    VALUES (@Location, @Supervisor, @Capacity)";
                    cmd = new SqlCommand(query, conn);
                }
                else // UPDATE
                {
                    string query = @"UPDATE Warehouse
                                    SET Location = @Location,
                                        Supervisor_Name = @Supervisor,
                                        Capacity = @Capacity
                                    WHERE Warehouse_ID = @Id";
                    cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", WarehouseInput.WarehouseId);
                }
                cmd.Parameters.AddWithValue("@Location", WarehouseInput.Location);
                cmd.Parameters.AddWithValue("@Supervisor", WarehouseInput.SupervisorName);
                cmd.Parameters.AddWithValue("@Capacity", WarehouseInput.Capacity);
                cmd.ExecuteNonQuery();
            }
            WarehouseInput = new WarehouseInputModel();
            return RedirectToPage(new { activeTab = "warehouses" });
        }
        public IActionResult OnPostDeleteWarehouse(int id, string activeTab = "warehouses")
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                string query = "DELETE FROM Warehouse WHERE Warehouse_ID = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToPage(new { activeTab });
        }
        // =========================================================
        // MAPPING CRUD OPERATIONS
        // =========================================================
        public IActionResult OnPostSaveMapping()
        {
            if (MappingInput == null)
                return RedirectToPage(new { activeTab = "mapping" });
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                // First update the vehicle to assign it to the provider
                string query = @"UPDATE Vehicle
                                SET Assigned_Provider = @ProviderId
                                WHERE Vehicle_ID = @VehicleId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@VehicleId", MappingInput.VehicleId);
                cmd.Parameters.AddWithValue("@ProviderId", MappingInput.ProviderId);
                cmd.ExecuteNonQuery();
            }
            MappingInput = new MappingInputModel();
            return RedirectToPage(new { activeTab = "mapping" });
        }
        public IActionResult OnPostDeleteMapping(int vehicleId, string activeTab = "mapping")
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                // Remove the assignment by setting Assigned_Provider to NULL
                string query = "UPDATE Vehicle SET Assigned_Provider = NULL WHERE Vehicle_ID = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", vehicleId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToPage(new { activeTab });
        }
        // =========================================================
        // AJAX HANDLERS FOR EDIT MODALS
        // =========================================================
        public JsonResult OnGetVehicleDetails(int id)
        {
            try
            {
                VehicleInputModel result = null;
                using (SqlConnection conn = new SqlConnection(_connString))
                {
                    string query = @"SELECT Vehicle_ID, Plate_Number, Model, Capacity, Status, Assigned_Provider
                                        FROM dbo.Vehicle WHERE Vehicle_ID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = new VehicleInputModel
                            {
                                VehicleId = reader.GetInt32(0),
                                PlateNumber = reader.GetString(1),
                                Model = reader.GetString(2),
                                Capacity = reader.GetDouble(3),
                                Status = reader.GetString(4),
                                AssignedProvider = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                            };
                        }
                    }
                }
                return new JsonResult(result);
            }
            catch (Exception ex) { return new JsonResult(new { error = ex.Message }); }
        }
        public JsonResult OnGetDriverDetails(int id)
        {
            try
            {
                DriverInputModel result = null;
                using (SqlConnection conn = new SqlConnection(_connString))
                {
                    string query = @"SELECT Driver_ID, Name, License_Number, Phone_Number, Availability_Status
                                    FROM Driver WHERE Driver_ID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = new DriverInputModel
                            {
                                DriverId = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                LicenseNumber = reader.GetString(2),
                                PhoneNumber = reader.GetString(3),
                                Status = reader.GetString(4)
                            };
                        }
                    }
                }
                return new JsonResult(result);
            }
            catch (Exception ex) { return new JsonResult(new { error = ex.Message }); }
        }
        public JsonResult OnGetWarehouseDetails(int id)
        {
            try
            {
                WarehouseInputModel result = null;
                using (SqlConnection conn = new SqlConnection(_connString))
                {
                    string query = @"SELECT Warehouse_ID, Location, Supervisor_Name, Capacity
                                    FROM Warehouse WHERE Warehouse_ID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = new WarehouseInputModel
                            {
                                WarehouseId = reader.GetInt32(0),
                                Location = reader.GetString(1),
                                SupervisorName = reader.GetString(2),
                                Capacity = reader.GetDouble(3)
                            };
                        }
                    }
                }
                return new JsonResult(result);
            }
            catch (Exception ex) { return new JsonResult(new { error = ex.Message }); }
        }
        public JsonResult OnGetMappingDetails(int vehicleId)
        {
            try
            {
                MappingInputModel result = null;
                using (SqlConnection conn = new SqlConnection(_connString))
                {
                    string query = @"SELECT v.Vehicle_ID, v.Assigned_Provider, p.Provider_Name
                                    FROM Vehicle v
                                    LEFT JOIN Provider p ON v.Assigned_Provider = p.Provider_ID
                                    WHERE v.Vehicle_ID = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", vehicleId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = new MappingInputModel
                            {
                                VehicleId = reader.GetInt32(0),
                                ProviderId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                                ProviderName = reader.IsDBNull(2) ? "" : reader.GetString(2)
                            };
                        }
                    }
                }
                return new JsonResult(result);
            }
            catch (Exception ex) { return new JsonResult(new { error = ex.Message }); }
        }
        // =========================================================
        // PRIVATE LOADING METHODS
        // =========================================================
        private void LoadVehicles()
        {
            Vehicles.Clear();
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                string sql = @"SELECT Vehicle_ID, Plate_Number, Model, Capacity, Status
                              FROM Vehicle WHERE 1=1";
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
                            VehicleId = Convert.ToInt32(r["Vehicle_ID"]),
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
                            DriverId = Convert.ToInt32(r["Driver_ID"]),
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
        private void LoadWarehouses()
        {
            Warehouses.Clear();
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                string sql = "SELECT * FROM Warehouse WHERE 1=1";
                if (!string.IsNullOrEmpty(WarehouseSearch))
                    sql += " AND Location LIKE @Search";
                SqlCommand cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(WarehouseSearch)) cmd.Parameters.AddWithValue("@Search", "%" + WarehouseSearch + "%");
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Warehouses.Add(new WarehouseDto
                        {
                            WarehouseId = Convert.ToInt32(r["Warehouse_ID"]),
                            Code = r["Warehouse_ID"].ToString(),
                            Name = "مستودع " + r["Location"].ToString(),
                            Location = r["Location"].ToString(),
                            Supervisor = r["Supervisor_Name"].ToString(),
                            Capacity = Convert.ToDouble(r["Capacity"])
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
                string sql = @"SELECT v.Vehicle_ID, p.Provider_Name, v.Plate_Number, v.Status
                              FROM Vehicle v
                              INNER JOIN Provider p ON v.Assigned_Provider = p.Provider_ID
                              WHERE v.Assigned_Provider IS NOT NULL";
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
                            VehicleId = Convert.ToInt32(r["Vehicle_ID"]),
                            LinkID = r["Vehicle_ID"].ToString(),
                            ContractorName = r["Provider_Name"].ToString(),
                            VehicleNumber = r["Plate_Number"].ToString(),
                            StartDate = "-",
                            EndDate = "-",
                            Status = r["Status"].ToString()
                        });
                    }
                }
            }
        }
        private void LoadContractorsList()
        {
            ContractorsList.Clear();
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
        private void LoadVehicleList()
        {
            VehicleList.Clear();
            VehicleList.Add(new SelectListItem("اختر المركبة...", ""));
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Vehicle_ID, Plate_Number FROM Vehicle WHERE Assigned_Provider IS NULL ORDER BY Plate_Number", conn);
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        VehicleList.Add(new SelectListItem(r["Plate_Number"].ToString(), r["Vehicle_ID"].ToString()));
                    }
                }
            }
        }
        private void LoadProviderList()
        {
            ProviderList.Clear();
            ProviderList.Add(new SelectListItem("اختر المقاول...", ""));
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Provider_ID, Provider_Name FROM Provider ORDER BY Provider_Name", conn);
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        ProviderList.Add(new SelectListItem(r["Provider_Name"].ToString(), r["Provider_ID"].ToString()));
                    }
                }
            }
        }
        // =========================================================
        // DATA TRANSFER OBJECTS (DTOs)
        // =========================================================
        public class VehicleDto
        {
            public int VehicleId { get; set; }
            public string VehicleCode { get; set; }
            public string PlateNumber { get; set; }
            public string Model { get; set; }
            public double Capacity { get; set; }
            public string Status { get; set; }
        }
        public class DriverDto
        {
            public int DriverId { get; set; }
            public string DriverCode { get; set; }
            public string Name { get; set; }
            public string LicenseNumber { get; set; }
            public string Phone { get; set; }
            public string Status { get; set; }
        }
        public class WarehouseDto
        {
            public int WarehouseId { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }
            public string Supervisor { get; set; }
            public double Capacity { get; set; }
        }
        public class MappingDto
        {
            public int VehicleId { get; set; }
            public string LinkID { get; set; }
            public string ContractorName { get; set; }
            public string VehicleNumber { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string Status { get; set; }
        }
        // Input Models for Forms
        public class VehicleInputModel
        {
            public int VehicleId { get; set; }
            public string PlateNumber { get; set; }
            public string Model { get; set; }
            public double Capacity { get; set; }
            public string Status { get; set; } = "Active";
            public int? AssignedProvider { get; set; }
        }
        public class DriverInputModel
        {
            public int DriverId { get; set; }
            public string Name { get; set; }
            public string LicenseNumber { get; set; }
            public string PhoneNumber { get; set; }
            public string Status { get; set; } = "Available";
        }
        public class WarehouseInputModel
        {
            public int WarehouseId { get; set; }
            public string Location { get; set; }
            public string SupervisorName { get; set; }
            public double Capacity { get; set; }
        }
        public class MappingInputModel
        {
            public int VehicleId { get; set; }
            public int? ProviderId { get; set; }
            public string ProviderName { get; set; }
        }
    }
}