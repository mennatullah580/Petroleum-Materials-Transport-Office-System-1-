using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Petroleum_Materials_Transport_Office_System.Pages.Admin_Panel
{
    public class User_ManagementModel : PageModel
    {

        public List<User> Users { get; set; } = new();

        public void OnGet()
        {
            // Replace this with real database call later
            Users = new List<User>
            {
                new User { Id = 1, Name = "أحمد محمد", Username = "ahmed_m", Role = "مدير", Department = "المبيعات", Branch = "فرع الرياض" },
                new User { Id = 2, Name = "سارة علي", Username = "sara_a", Role = "محاسب", Department = "المالية", Branch = "فرع جدة" },
                new User { Id = 3, Name = "خالد فهد", Username = "khalid_f", Role = "موظف", Department = "الدعم الفني", Branch = "فرع الدمام" }
            };
        }

        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string Department { get; set; } = string.Empty;
            public string Branch { get; set; } = string.Empty;
        }
    }
}