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
            string username = "Unknown";
            try
            {
                var user = _repo.GetUserById(id);
                username = user?.Username ?? "Unknown";
            }
            catch { }

            _repo.DeleteUser(id);

            var currentUser = HttpContext.Session.GetString("Username") ?? "system";
            _actionLogger.Log(
                currentUser,
                "حذف مستخدم",
                $"تم حذف المستخدم {username} (ID: {id})"
            );

            // Preserve search term after delete
            return RedirectToPage(new { search = SearchTerm });
        }
    }
}