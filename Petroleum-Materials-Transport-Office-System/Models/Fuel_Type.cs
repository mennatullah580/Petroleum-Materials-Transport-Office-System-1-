using System.ComponentModel.DataAnnotations;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Fuel_Type
    {
        [Key]
        public int Fuel_ID { get; set; }
        public string Type_Name { get; set; }
        public string Status { get; set; }
    }
}