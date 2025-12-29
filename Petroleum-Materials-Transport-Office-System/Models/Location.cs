using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Location
    {
        [Key]
        public string Location_Code { get; set; } // This is the Primary Key (Text)

        public string Location_Name { get; set; }

        public string Region { get; set; }
    }
}