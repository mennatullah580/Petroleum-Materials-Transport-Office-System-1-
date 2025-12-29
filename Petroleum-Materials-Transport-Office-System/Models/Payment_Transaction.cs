using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Payment_Transaction
    {
        [Key]
        public int Transaction_ID { get; set; }

        public string? Invoice_Number { get; set; }

        [ForeignKey("Provider")]
        public int? Provider_ID { get; set; }
        public virtual Provider Provider { get; set; }

        [ForeignKey("Company")]
        public int? Company_ID { get; set; }
        public virtual Company Company { get; set; }

        public decimal Amount { get; set; }
        public string Method { get; set; } // Cash, Bank Transfer
        public string Type { get; set; }   // Payment, Advance, Custody, Receipt
        public DateTime Date { get; set; }
        public string? Remarks { get; set; }
        public string Status { get; set; } // Completed, Pending
    }
}