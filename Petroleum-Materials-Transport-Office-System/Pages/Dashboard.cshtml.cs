using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Petroleum_Materials_Transport_Office_System.Pages
{
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
            // الصفحة هتفتح عادي
        }

        public IActionResult OnPost()
        {
            // لما المستخدم يدوس "تسجيل الخروج"
            return RedirectToPage("/Login");
        }
    }
}