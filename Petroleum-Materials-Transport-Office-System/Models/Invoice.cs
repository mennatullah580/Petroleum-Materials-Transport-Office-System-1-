using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    // explicitly maps this class to the "Invoice" table in SQL
    [Table("Invoice")]
    public class Invoice
    {
        [Key] // Tells EF this is the Primary Key
        [Column("Invoice_Number")] // Maps "InvoiceNumber" to SQL column "Invoice_Number"
        [Display(Name = "رقم الفاتورة")]
        public string InvoiceNumber { get; set; }

        [Column("Order_ID")]
        public int OrderId { get; set; }

        [Column("Company_ID")]
        public int CompanyId { get; set; }

        [Column("Provider_ID")]
        public int? ProviderId { get; set; }

        [Column("Issue_Date")] // Maps "Date" to "Issue_Date"
        [Display(Name = "تاريخ الإصدار")]
        public DateTime Date { get; set; }

        [Column("Company_Amount")] // Maps "CompanyAmount" to "Company_Amount"
        [Display(Name = "مبلغ الشركة")]
        public decimal CompanyAmount { get; set; }

        [Column("Provider_Amount")]
        [Display(Name = "مبلغ المقاول")]
        public decimal ProviderAmount { get; set; }

        [Column("Net_Amount")]
        [Display(Name = "الصافي")]
        public decimal Net { get; set; }

        [Column("Payment_Status")]
        [Display(Name = "الحالة")]
        public string Status { get; set; }

        // --- Navigation Properties (Links to other tables) ---
        // These allow us to grab the Names instead of just IDs

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        [ForeignKey("ProviderId")]
        public virtual Provider Provider { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}