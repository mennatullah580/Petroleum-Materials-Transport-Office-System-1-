using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Petroleum_Materials_Transport_Office_System.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginInput Input { get; set; }

        public class LoginInput
        {
            public string ID { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public void OnGet()
        {
            // الصفحة هتفتح عادي
        }

        public IActionResult OnPost()
        {
            // تحقق من البيانات
            if (Input != null && Input.Username == "admin" && Input.Password == "123")
            {
                return RedirectToPage("/Dashboard");
            }

            // لو البيانات غلط، ارجع لنفس الصفحة
            ModelState.AddModelError(string.Empty, "اسم المستخدم أو كلمة المرور غير صحيحة");
            return Page();
        }
    }
}