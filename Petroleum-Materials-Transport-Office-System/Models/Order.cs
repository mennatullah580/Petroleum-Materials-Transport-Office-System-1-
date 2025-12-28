namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime DeliveryDate { get; set; } = DateTime.Now;
        public string LoadingLocation { get; set; } = string.Empty;
        public string UnloadingLocation { get; set; } = string.Empty;
        public string PetroleumType { get; set; } = string.Empty;
        public decimal LoadingQuantity { get; set; }
        public decimal UnloadingQuantity { get; set; }
        public decimal Shortage { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string VehiclePlateNumber { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";

        // 🔥 كود الجهة string (LOC001, LOC002, LOC003...)
        public string LocationCode { get; set; } = string.Empty;

        // Financial Properties
        public decimal CompanyPrice { get; set; }
        public decimal ProviderPrice { get; set; }
        public decimal CompanyTotal { get; set; }
        public decimal ProviderTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal StampFee { get; set; } = 50;
        public decimal GPSFee { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal AdvancePayment { get; set; }
        public decimal CustodyAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal Balance { get; set; }
    }
}