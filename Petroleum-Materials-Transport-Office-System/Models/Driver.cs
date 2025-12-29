using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Driver
    {
        [Key]
        public int Driver_ID { get; set; }

        public int? User_ID { get; set; }
        public string Name { get; set; }
        public string License_Number { get; set; }
        public string? Contact_Info { get; set; }
        public string? Phone_Number { get; set; }
        public string Availability_Status { get; set; } // 'Available', 'On Trip', etc.

        [ForeignKey("Provider")]
        public int? Provider_ID { get; set; }
        public virtual Provider Provider { get; set; }

        public DateTime Created_At { get; set; } = DateTime.Now;
    }
}