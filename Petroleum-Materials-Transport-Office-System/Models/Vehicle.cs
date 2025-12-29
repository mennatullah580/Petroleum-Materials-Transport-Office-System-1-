using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Vehicle
    {
        [Key]
        public int Vehicle_ID { get; set; }

        public string Plate_Number { get; set; }
        public string? Model { get; set; }
        public decimal Capacity { get; set; }

        [ForeignKey("Provider")]
        public int? Assigned_Provider { get; set; }
        public virtual Provider Provider { get; set; }

        public string Status { get; set; } // 'Active', 'Under Maintenance'
        public bool GPS_Installed { get; set; }
        public DateTime Created_At { get; set; } = DateTime.Now;
    }
}