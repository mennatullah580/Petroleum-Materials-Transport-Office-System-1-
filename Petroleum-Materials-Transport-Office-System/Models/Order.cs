using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    [Table("Orders")]
    public class Order
    {
        // ==========================================
        // 1. BASIC PROPERTIES (Mapped to Database)
        // ==========================================

        [Key]
        [Column("Order_ID")]
        public int OrderId { get; set; }

        [Column("Order_Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column("Delivery_Date")]
        public DateTime? DeliveryDate { get; set; }

        [Column("Loading_Location")]
        public string LoadingLocation { get; set; } = string.Empty;

        [Column("Unloading_Location")]
        public string UnloadingLocation { get; set; } = string.Empty;

        // "Location_Code" does not exist in DB, so we use NotMapped
        [NotMapped]
        public string LocationCode { get; set; } = string.Empty;

        [Column("Loading_Quantity")]
        public decimal LoadingQuantity { get; set; }

        [Column("Unloading_Quantity")]
        public decimal UnloadingQuantity { get; set; }

        [Column("Shortage")]
        public decimal Shortage { get; set; }

        [Column("Status")]
        public string Status { get; set; } = "Pending";

        // ==========================================
        // 2. RELATIONSHIPS (Foreign Keys)
        // ==========================================

        [Column("Provider_ID")]
        [ForeignKey("Provider")]
        public int? ProviderId { get; set; }
        public virtual Provider? Provider { get; set; }

        [Column("Driver_ID")]
        [ForeignKey("Driver")]
        public int? DriverId { get; set; }
        public virtual Driver? Driver { get; set; }

        [Column("Vehicle_ID")]
        [ForeignKey("Vehicle")]
        public int? VehicleId { get; set; }
        public virtual Vehicle? Vehicle { get; set; }

        [Column("Company_ID")]
        [ForeignKey("Company")]
        public int? CompanyId { get; set; }
        public virtual Company? Company { get; set; }

        [Column("Petroleum_Type")]
        [ForeignKey("FuelType")]
        public int? PetroleumTypeId { get; set; }
        public virtual Fuel_Type? FuelType { get; set; }

        // ==========================================
        // 3. COMPATIBILITY HELPERS (The Fix for CS0200)
        // ==========================================
        // We added 'set {}' to these properties so the compiler allows
        // your Edit pages to assign values to them without crashing.

        [NotMapped]
        public string InvoiceNumber { get; set; } = string.Empty;

        [NotMapped]
        public string ProviderName
        {
            get { return Provider?.Provider_Name ?? ""; }
            set { } // Allows writing without error
        }

        [NotMapped]
        public string VehiclePlateNumber
        {
            get { return Vehicle?.Plate_Number ?? ""; }
            set { } // Allows writing without error
        }

        [NotMapped]
        public string DriverName
        {
            get { return Driver?.Name ?? ""; }
            set { } // Allows writing without error
        }

        [NotMapped]
        public string PetroleumType
        {
            get { return FuelType?.Type_Name ?? ""; }
            set { } // Allows writing without error
        }

        // ==========================================
        // 4. FINANCIAL PROPERTIES
        // ==========================================

        [NotMapped] public decimal CompanyPrice { get; set; }
        [NotMapped] public decimal ProviderPrice { get; set; }
        [NotMapped] public decimal CompanyTotal { get; set; }
        [NotMapped] public decimal ProviderTotal { get; set; }
        [NotMapped] public decimal TaxAmount { get; set; }
        [NotMapped] public decimal StampFee { get; set; } = 50;
        [NotMapped] public decimal GPSFee { get; set; }
        [NotMapped] public decimal TotalDeductions { get; set; }
        [NotMapped] public decimal AdvancePayment { get; set; }
        [NotMapped] public decimal CustodyAmount { get; set; }
        [NotMapped] public decimal NetAmount { get; set; }
        [NotMapped] public decimal Balance { get; set; }
    }
}