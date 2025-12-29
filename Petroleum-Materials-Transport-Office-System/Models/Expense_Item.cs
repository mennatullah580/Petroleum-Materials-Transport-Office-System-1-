using System.ComponentModel.DataAnnotations;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class Expense_Item
    {
        [Key]
        public int Expense_ID { get; set; }

        public string Name { get; set; }
        public string Category { get; set; }       // Operating, Admin, Maintenance
        public string Value_Type { get; set; }     // Fixed, Variable
        public bool Is_Reportable { get; set; }
    }
}