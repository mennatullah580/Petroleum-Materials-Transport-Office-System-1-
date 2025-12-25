using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Petroleum_Materials_Transport_Office_System.Models;
using Petroleum_Materials_Transport_Office_System.Services;

namespace Petroleum_Materials_Transport_Office_System.Pages.AdminPanel
{
    public class EditUserModel : PageModel
    {
        private readonly UserRepository _repo;
        private readonly ActionLogger _actionLogger;

        public EditUserModel(UserRepository repo, ActionLogger actionLogger)
        {
            _repo = repo;
            _actionLogger = actionLogger;
        }

        [BindProperty]
        public UserModel User { get; set; } = new();

        public List<SelectListItem> Departments { get; set; } = new();
        public List<SelectListItem> Roles { get; set; } = new();

        // ✅ Accept 'id' parameter
        public void OnGet(int? id)
        {
            if (id.HasValue)
            {
                var existingUser = _repo.GetUserById(id.Value);
                if (existingUser != null)
                {
                    User = existingUser;
                }
            }

            LoadDepartments();
            LoadRoles();
        }

        public IActionResult OnPost()
        {
            LoadDepartments();
            LoadRoles();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (_repo.UserExists(User.Username, User.Email, User.Id))
            {
                ModelState.AddModelError("", "اسم المستخدم أو البريد الإلكتروني مستخدم بالفعل");
                return Page();
            }

            _repo.SaveUser(User);

            var currentUser = HttpContext.Session.GetString("Username") ?? "system";
            _actionLogger.Log(currentUser, "تعديل مستخدم", $"تم تعديل المستخدم {User.Username}");

            return RedirectToPage("/AdminPanel/UserManagement");
        }

        private void LoadDepartments() => Departments = new()
        {
            new("الإدارة", "Administration"),
            new("العمليات", "Operations"),
            new("اللوجستيات", "Logistics"),
            new("المالية", "Finance")
        };

        private void LoadRoles() => Roles = new()
        {
            new("مدير النظام", "Admin"),
            new("محاسب", "Accountant"),
            new("موظف", "Operator")
        };
    }
}