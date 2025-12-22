using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Petroleum_Materials_Transport_Office_System.Models;

namespace Petroleum_Materials_Transport_Office_System.Pages.AdminPanel
{
    public class EditUserModel : PageModel
    {
        private readonly UserRepository _repo;

        public EditUserModel(UserRepository repo) => _repo = repo;

        [BindProperty]
        public UserModel User { get; set; } = new UserModel();

        public List<SelectListItem> Departments { get; set; }
        public List<SelectListItem> Roles { get; set; }

        public void OnGet()
        {
            LoadDepartments();
            LoadRoles();
        }

        public IActionResult OnPost()
        {
            LoadDepartments();
            LoadRoles();

            if (_repo.UserExists(User.Username, User.Email, User.Id))
            {
                ModelState.AddModelError("", "اسم المستخدم أو البريد الإلكتروني مستخدم بالفعل");
                return Page();
            }

            _repo.SaveUser(User);
            return RedirectToPage("/AdminPanel/UserManagement");
        }

        private void LoadDepartments() => Departments = new List<SelectListItem>
    {
        new("الإدارة", "Administration"),
        new("العمليات", "Operations"),
        new("اللوجستيات", "Logistics"),
        new("المالية", "Finance")
    };

        private void LoadRoles() => Roles = new List<SelectListItem>
    {
        new("مدير النظام", "Admin"),
        new("محاسب", "Accountant"),
        new("موظف", "Operator")
    };
    }
}
