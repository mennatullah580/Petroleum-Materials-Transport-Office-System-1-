using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Petroleum_Materials_Transport_Office_System.Models;

namespace Petroleum_Materials_Transport_Office_System.Pages.AdminPanel
{
    public class UserManagementModel : PageModel
    {
        private readonly UserRepository _repo;

        public UserManagementModel(UserRepository repo)
        {
            _repo = repo;
        }

        public List<UserModel> Users { get; set; } = new();

        public void OnGet()
        {
            Users = _repo.GetAllUsers();
        }

        // 🔥 Handle DELETE request
        public IActionResult OnPostDelete(int id)
        {
            if (id > 0)
            {
                _repo.DeleteUser(id);
            }
            return RedirectToPage(); // Refresh the page
        }
    }
}