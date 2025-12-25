using System.ComponentModel.DataAnnotations;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Company
    {
        [Key]
        public int Company_ID { get; set; }
        public string Company_Name { get; set; }
        public string Status { get; set; }
    }
}