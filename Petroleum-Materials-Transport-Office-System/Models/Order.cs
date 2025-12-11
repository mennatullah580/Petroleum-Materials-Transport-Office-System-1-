namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Order
    {
        // معلومات أساسية
        public int OrderId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string LocationCode { get; set; }

        // المواقع
        public string LoadingLocation { get; set; }
        public string UnloadingLocation { get; set; }

        // نوع الوقود
        public string PetroleumType { get; set; }

        // الكميات
        public decimal LoadingQuantity { get; set; }
        public decimal UnloadingQuantity { get; set; }
        public decimal Shortage { get; set; } // العجز (محسوب تلقائياً)

        // المقاول والسيارة
        public string ProviderName { get; set; }
        public string VehiclePlateNumber { get; set; }
        public string DriverName { get; set; }

        // الأسعار والمبالغ
        public decimal CompanyPrice { get; set; }
        public decimal ProviderPrice { get; set; }
        public decimal CompanyTotal { get; set; }
        public decimal ProviderTotal { get; set; }

        // الخصومات
        public decimal TaxAmount { get; set; }
        public decimal StampFee { get; set; }
        public decimal GPSFee { get; set; }
        public decimal TotalDeductions { get; set; }

        // الدفعات
        public decimal AdvancePayment { get; set; } // سلفة
        public decimal CustodyAmount { get; set; } // عهدة
        public decimal NetAmount { get; set; }
        public decimal Balance { get; set; }

        // التاريخ والحالة
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; }
    }
}
