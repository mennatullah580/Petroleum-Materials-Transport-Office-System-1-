using System.ComponentModel.DataAnnotations;

namespace Petroleum_Materials_Transport_Office_System.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "يرجى اختيار الدور")]
        public string Role { get; set; } = "Operator";

        public string Department { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
