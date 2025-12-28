using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Petroleum_Materials_Transport_Office_System.Models;
using Petroleum_Materials_Transport_Office_System.Services;

namespace Petroleum_Materials_Transport_Office_System.Pages.AdminPanel
{
    public class UserManagementModel : PageModel
    {
        private readonly UserRepository _repo;
        private readonly ActionLogger _actionLogger;

        public UserManagementModel(UserRepository repo, ActionLogger actionLogger)
        {
            _repo = repo;
            _actionLogger = actionLogger;
        }

        public List<UserModel> Users { get; set; } = new();
        public string? SearchTerm { get; set; }

        public void OnGet(string? search)
        {
            SearchTerm = search;
            Users = _repo.SearchUsers(search);
        }

        public IActionResult OnPostDelete(int id)
        {
            var currentUserIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(currentUserIdStr) || !int.TryParse(currentUserIdStr, out int currentUserId))
            {
                return RedirectToPage();
            }

            // 🔒 Cannot delete yourself
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "لا يمكنك حذف حسابك الخاص";
                return RedirectToPage(new { search = SearchTerm });
            }

            string? targetUserRole = _repo.GetUserRoleById(id);
            if (string.IsNullOrEmpty(targetUserRole))
            {
                TempData["ErrorMessage"] = "المستخدم غير موجود";
                return RedirectToPage(new { search = SearchTerm });
            }

            string? currentUserRole = _repo.GetUserRoleById(currentUserId);
            if (string.IsNullOrEmpty(currentUserRole))
            {
                return RedirectToPage();
            }

            // 🔒 Only Super Admin (ID=1) can delete other admins
            if (targetUserRole == "Admin" && currentUserId != 1)
            {
                TempData["ErrorMessage"] = "لا يمكنك حذف حسابات المدراء الآخرين";
                return RedirectToPage(new { search = SearchTerm });
            }

            // ✅ Proceed with delete
            try
            {
                string username = "Unknown";
                var user = _repo.GetUserById(id);
                if (user != null) username = user.Username;

                _repo.DeleteUser(id);

                var currentUsername = HttpContext.Session.GetString("Username") ?? "system";
                _actionLogger.Log(currentUsername, "حذف مستخدم", $"تم حذف المستخدم {username} (ID: {id})");
            }
            catch
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء حذف المستخدم";
            }

            return RedirectToPage(new { search = SearchTerm });
        }
    }
}