using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Petroleum_Materials_Transport_Office_System.Models;
using Petroleum_Materials_Transport_Office_System.Services;
using Petroleum_Materials_Transport_Office_System.Utils;

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

            if (string.IsNullOrWhiteSpace(User.FullName))
            {
                ModelState.AddModelError("User.FullName", "الاسم الكامل مطلوب");
                return Page();
            }

            // Handle new user logic
            if (User.Id == 0)
            {
                // Auto-generate username if empty
                if (string.IsNullOrWhiteSpace(User.Username))
                {
                    User.Username = UsernameGenerator.GenerateUniqueUsername(
                        User.FullName,
                        username => _repo.IsUsernameTaken(username)
                    );
                }

                // Password is REQUIRED for new users
                if (string.IsNullOrWhiteSpace(User.Password))
                {
                    ModelState.AddModelError("User.Password", "كلمة المرور مطلوبة للمستخدم الجديد");
                    return Page();
                }
            }

            // Convert ID=0 to null for exclusion
            int? excludeId = User.Id == 0 ? null : (int?)User.Id;

            // Validate username
            if (_repo.IsUsernameTaken(User.Username, excludeId))
            {
                ModelState.AddModelError("User.Username", "اسم المستخدم مستخدم بالفعل");
                return Page();
            }

            // Validate email
            if (!string.IsNullOrEmpty(User.Email) && _repo.IsEmailTaken(User.Email, excludeId))
            {
                ModelState.AddModelError("User.Email", "البريد الإلكتروني مستخدم بالفعل");
                return Page();
            }

            // Save the user
            _repo.SaveUser(User);

            // Log the action
            var currentUser = HttpContext.Session.GetString("Username") ?? "system";
            string action = User.Id == 0 ? "إضافة مستخدم" : "تعديل مستخدم";
            _actionLogger.Log(currentUser, action, $"تم {action} '{User.Username}'");

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