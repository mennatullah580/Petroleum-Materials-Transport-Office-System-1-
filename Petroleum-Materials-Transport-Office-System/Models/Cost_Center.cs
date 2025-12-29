using System;
using System.ComponentModel.DataAnnotations;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Cost_Center
    {
        [Key]
        public int CostCenter_ID { get; set; }

        public string Name { get; set; }
        public string Supervisor { get; set; }
        public string Status { get; set; }
        public DateTime Created_At { get; set; } = DateTime.Now;
    }
}