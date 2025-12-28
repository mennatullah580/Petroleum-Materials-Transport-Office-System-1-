using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Petroleum_Materials_Transport_Office_System.Services;

namespace Petroleum_Materials_Transport_Office_System.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ActionLogger _actionLogger;

        public LoginModel(ActionLogger actionLogger)
        {
            _actionLogger = actionLogger;
        }

        [BindProperty]
        public LoginInput Input { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // التحقق من إدخال البيانات
            if (Input?.EmployeeID == null || string.IsNullOrEmpty(Input.Password))
            {
                ErrorMessage = "من فضلك أدخل الرقم الوظيفي وكلمة المرور";
                return Page();
            }

            string connectionString = @"Server=.;Database=PetroleumTransportDB;Trusted_Connection=True;TrustServerCertificate=True;;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // الاستعلام: نستخدم فقط User_ID والباسورد للتحقق
                    string query = @"
                        SELECT User_ID, Username, Name, Role, Department, Email
                        FROM Users 
                        WHERE User_ID = @EmployeeID";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.Add("@EmployeeID", System.Data.SqlDbType.Int).Value = Input.EmployeeID;

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    // Unified Password Check
                                    if (Input.Password != "123")
                                    {
                                         ErrorMessage = "كلمة المرور غير صحيحة";
                                         return Page();
                                    }

                                    string userName = reader["Name"].ToString();
                                    string username = reader["Username"].ToString();
                                    string userRole = reader["Role"].ToString();
                                    string department = reader["Department"].ToString();
                                    string email = reader["Email"].ToString();

                                    // تسجيل عملية تسجيل الدخول
                                    _actionLogger.Log(
                                        user: userName,
                                        action: "تسجيل الدخول",
                                        details: $"تم تسجيل الدخول بنجاح - الرقم الوظيفي: {Input.EmployeeID} - Username: {username}"
                                    );

                                    // حفظ بيانات الجلسة
                                    HttpContext.Session.SetInt32("UserID", Input.EmployeeID.Value);
                                    HttpContext.Session.SetString("Username", username); // نحفظه للاستخدام الداخلي
                                    HttpContext.Session.SetString("Name", userName);
                                    HttpContext.Session.SetString("Role", userRole);
                                    HttpContext.Session.SetString("Department", department);
                                    HttpContext.Session.SetString("Email", email);

                                    return RedirectToPage("/Dashboard");
                                }
                                else
                                {
                                    ErrorMessage = "الرقم الوظيفي غير صحيح";
                                    return Page();
                                }
                            }
                        }
                }
                catch (Exception ex)
                {
                    ErrorMessage = "حدث خطأ في الاتصال بقاعدة البيانات";
                    // يمكنك تسجيل الخطأ هنا
                    return Page();
                }
            }
        }

        public class LoginInput
        {
            public int? EmployeeID { get; set; }
            public string Password { get; set; } = string.Empty;
        }
    }
}