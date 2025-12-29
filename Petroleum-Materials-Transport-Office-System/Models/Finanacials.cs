using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    [Table("Financials")]
    public class Financials
    {
        [Key]
        [Column("Financial_ID")]
        public int FinancialId { get; set; }

        [Column("Order_ID")]
        [ForeignKey("Order")]
        public int Order_ID { get; set; }

        [Column("Company_Price")]
        public decimal Company_Price { get; set; }

        [Column("Provider_Price")]
        public decimal Provider_Price { get; set; }

        [Column("Company_Total")]
        public decimal Company_Total { get; set; }

        [Column("Provider_Total")]
        public decimal Provider_Total { get; set; }

        [Column("Tax_Amount")]
        public decimal Tax_Amount { get; set; }

        [Column("GPS_Fee")]
        public decimal GPS_Fee { get; set; }

        [Column("Total_Deduction")]
        public decimal Total_Deduction { get; set; }

        [Column("Net_Amount")]
        public decimal Net_Amount { get; set; }

        [Column("Balance")]
        public decimal Balance { get; set; }

        // Navigation Property
        public virtual Order Order { get; set; }
    }
}