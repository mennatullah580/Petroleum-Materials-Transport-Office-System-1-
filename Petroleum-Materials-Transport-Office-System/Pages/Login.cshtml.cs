using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Petroleum_Materials_Transport_Office_System.Services; // 👈 Add this

namespace Petroleum_Materials_Transport_Office_System.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ActionLogger _actionLogger; // 👈 Add this

        public LoginModel(ActionLogger actionLogger) // 👈 Inject logger
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
            if (string.IsNullOrEmpty(Input?.ID) ||
                string.IsNullOrEmpty(Input.Username) ||
                string.IsNullOrEmpty(Input.Password))
            {
                ErrorMessage = "من فضلك أدخل جميع البيانات";
                return Page();
            }

            string connectionString = @"Server=DESKTOP-1QHK872;Database=PetroleumTransportDB;Trusted_Connection=True;TrustServerCertificate=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT Role, Name, Department 
                    FROM Users 
                    WHERE User_ID = @ID 
                      AND Username = @Username 
                      AND Password = @Password";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Use proper SQL parameter type for ID (int)
                    if (!int.TryParse(Input.ID, out int userId))
                    {
                        ErrorMessage = "الرقم الوظيفي غير صالح";
                        return Page();
                    }

                    cmd.Parameters.Add("@ID", System.Data.SqlDbType.Int).Value = userId;
                    cmd.Parameters.AddWithValue("@Username", Input.Username);
                    cmd.Parameters.AddWithValue("@Password", Input.Password);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // ✅ LOG THE LOGIN ACTION
                            _actionLogger.Log(
                                user: Input.Username,
                                action: "تسجيل الدخول",
                                details: "تم تسجيل الدخول بنجاح"
                            );

                            // Store session data
                            HttpContext.Session.SetString("UserID", Input.ID);
                            HttpContext.Session.SetString("Username", Input.Username);
                            HttpContext.Session.SetString("Role", reader["Role"].ToString());
                            HttpContext.Session.SetString("Name", reader["Name"].ToString());
                            HttpContext.Session.SetString("Department", reader["Department"].ToString());

                            return RedirectToPage("/Dashboard");
                        }
                        else
                        {
                            ErrorMessage = "الرقم الوظيفي أو اسم المستخدم أو كلمة المرور غير صحيحة";
                            return Page();
                        }
                    }
                }
            }
        }

        public class LoginInput
        {
            public string ID { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}