using System;
using System.ComponentModel.DataAnnotations;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Treasury_Bank
    {
        [Key]
        public int Treasury_ID { get; set; }

        public string Name { get; set; }           // Name of Safe or Bank
        public string Type { get; set; }           // 'Safe' or 'Bank'
        public string? Account_Number { get; set; }
        public decimal Initial_Balance { get; set; }
        public decimal Current_Balance { get; set; }
        public string Status { get; set; }         // 'Active', 'Inactive'
        public DateTime Created_At { get; set; } = DateTime.Now;
    }
}