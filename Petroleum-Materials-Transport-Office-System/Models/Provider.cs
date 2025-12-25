using System.ComponentModel.DataAnnotations;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Provider
    {
        [Key]
        public int Provider_ID { get; set; }
        public string Provider_Name { get; set; }
        public string Status { get; set; }
    }
}