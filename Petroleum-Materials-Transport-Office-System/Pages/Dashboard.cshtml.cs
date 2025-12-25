using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Petroleum_Materials_Transport_Office_System.Pages
{
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
            // التحقق من وجود Session
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Role")))
            {
                Response.Redirect("/Login");
            }
        }

        public IActionResult OnPost()
        {
            // مسح الـ Session وتسجيل الخروج
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }
    }
}