using System.ComponentModel.DataAnnotations;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Location
    {
        [Key]
        public int Location_ID { get; set; }
        public string Location_Code { get; set; }
        public string Location_Name { get; set; }
        public string Status { get; set; }
    }
}