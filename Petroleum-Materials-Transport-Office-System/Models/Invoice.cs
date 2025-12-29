using Petroleum_Materials_Transport_Office_System.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Invoice")]
public class Invoice
{
    [Key]
    [Column("Invoice_Number")]
    public string InvoiceNumber { get; set; }

    [Column("Order_ID")]
    public int OrderId { get; set; }

    [Column("Company_ID")]
    public int CompanyId { get; set; }

    [Column("Provider_ID")]
    public int? ProviderId { get; set; }

    [Column("Issue_Date")]
    public DateTime Date { get; set; }

    [Column("Company_Amount")]
    public decimal CompanyAmount { get; set; }

    [Column("Provider_Amount")]
    public decimal ProviderAmount { get; set; }

    [Column("Deductions")]
    public decimal Deductions { get; set; }

    [Column("Advance_Payment")]
    public decimal AdvancePayment { get; set; }

    [Column("Custody_Amount")]
    public decimal CustodyAmount { get; set; }

    [Column("Net_Amount")]
    public decimal Net { get; set; }

    [Column("Payment_Status")]
    public string Status { get; set; }

    [ForeignKey("CompanyId")]
    public virtual Company Company { get; set; }

    [ForeignKey("ProviderId")]
    public virtual Provider Provider { get; set; }

    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; }
}