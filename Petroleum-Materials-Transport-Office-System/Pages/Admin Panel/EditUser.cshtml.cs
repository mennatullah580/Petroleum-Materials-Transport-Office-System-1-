using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Petroleum_Materials_Transport_Office_System.Pages.AdminPanel
{
    public class EditUserModel : PageModel
    {
        [BindProperty]
        public UserModel User { get; set; } = new();

        [BindProperty]
        public PermissionsModel Permissions { get; set; } = new();

        public List<SelectListItem> Roles { get; set; } = new();

        public void OnGet(int? id)
        {
            // Define roles (must match VALUE, not Text)
            Roles = new List<SelectListItem>
    {
        new SelectListItem { Value = "مدير", Text = "مدير النظام" },      // ← VALUE = "مدير"
        new SelectListItem { Value = "محاسب", Text = "محاسب" },         // ← VALUE = "محاسب"
        new SelectListItem { Value = "موظف", Text = "موظف" }            // ← VALUE = "موظف"
    };

            if (id.HasValue)
            {
                // Simulate loading from "database" using the same mock data as UserManagement
                var mockUsers = new List<UserModel>
        {
            new UserModel {
                Id = 1,
                FullName = "أحمد محمد",
                Username = "ahmed_m",
                Email = "ahmed@example.com",
                Phone = "0501234567",
                Role = "مدير", // ← Must match SelectListItem.Value!
                Department = "المبيعات",
                AssignedBranch = "فرع الرياض",
                IsActive = true
            },
            new UserModel {
                Id = 2,
                FullName = "سارة علي",
                Username = "sara_a",
                Email = "sara@example.com",
                Phone = "0509876543",
                Role = "محاسب",
                Department = "المالية",
                AssignedBranch = "فرع جدة",
                IsActive = true
            },
            new UserModel {
                Id = 3,
                FullName = "خالد فهد",
                Username = "khalid_f",
                Email = "khalid@example.com",
                Phone = "0501122334",
                Role = "موظف",
                Department = "الدعم الفني",
                AssignedBranch = "فرع الدمام",
                IsActive = false
            }
        };

                User = mockUsers.FirstOrDefault(u => u.Id == id.Value) ?? new UserModel();

                // Load permissions (you can customize per user if needed)
                Permissions = new PermissionsModel
                {
                    CanViewOrders = true,
                    CanCreateOrders = User.Role == "مدير",
                    CanEditOrders = true,
                    CanDeleteOrders = User.Role == "مدير",
                    CanViewFinancialReports = User.Role != "موظف",
                    CanViewProfitReports = User.Role != "موظف",
                    CanExportReports = User.Role == "مدير" || User.Role == "محاسب",
                    CanAccessContractors = true,
                    CanEditContractors = User.Role == "مدير",
                    CanManagePayments = User.Role == "محاسب" || User.Role == "مدير",
                    CanManageFuelPrices = User.Role == "مدير",
                    CanManageUsers = User.Role == "مدير",
                    CanManageDeductions = User.Role == "مدير",
                    CanViewSystemLogs = User.Role == "مدير"
                };
            }
            else
            {
                User = new UserModel();
                Permissions = new PermissionsModel();
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Save to database (User + Permissions)
            // For now, redirect
            return RedirectToPage("/AdminPanel/UserManagement");
        }

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

            [Required]
            public string Role { get; set; } = string.Empty;

            public string Department { get; set; } = string.Empty;
            public string AssignedBranch { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string ConfirmPassword { get; set; } = string.Empty;

            public bool IsActive { get; set; } = true;
        }

        public class PermissionsModel
        {
            // Orders
            public bool CanViewOrders { get; set; }
            public bool CanCreateOrders { get; set; }
            public bool CanEditOrders { get; set; }
            public bool CanDeleteOrders { get; set; }

            // Reports
            public bool CanViewFinancialReports { get; set; }
            public bool CanViewProfitReports { get; set; }
            public bool CanExportReports { get; set; }

            // Contractors
            public bool CanAccessContractors { get; set; }
            public bool CanEditContractors { get; set; }
            public bool CanManagePayments { get; set; }

            // System
            public bool CanManageFuelPrices { get; set; }
            public bool CanManageUsers { get; set; }
            public bool CanManageDeductions { get; set; }
            public bool CanViewSystemLogs { get; set; }
        }
    }
}