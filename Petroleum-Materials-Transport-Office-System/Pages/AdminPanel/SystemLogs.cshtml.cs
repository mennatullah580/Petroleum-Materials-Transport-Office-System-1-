using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Petroleum_Materials_Transport_Office_System.Pages.Admin_Panel
{
    public class SystemLogsModel : PageModel
    {

        public List<LogEntry> Logs { get; set; } = new();
        public ApiSettingsModel ApiSettings { get; set; } = new();

        public void OnGet()
        {
            // Sample logs
            Logs = new List<LogEntry>
            {
                new LogEntry { Time = "2025-12-06 10:30", User = "ahmed_m", Action = "تسجيل الدخول", Details = "تم تسجيل الدخول بنجاح" },
                new LogEntry { Time = "2025-12-06 10:35", User = "sara_a", Action = "تعديل مستخدم", Details = "تم تعديل بيانات المستخدم خالد فهد" },
                new LogEntry { Time = "2025-12-06 10:40", User = "khalid_f", Action = "حذف سجل", Details = "تم حذف سجل من قسم المبيعات" }
            };

            // Sample API settings
            ApiSettings = new ApiSettingsModel
            {
                ApiKey = "abc123xyz456",
                ServerUrl = "https://api.yourcompany.com/logs ",
                Interval = "5"
            };
        }

        public class LogEntry
        {
            public string Time { get; set; } = string.Empty;
            public string User { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public string Details { get; set; } = string.Empty;
        }

        public class ApiSettingsModel
        {
            public string ApiKey { get; set; } = string.Empty;
            public string ServerUrl { get; set; } = string.Empty;
            public string Interval { get; set; } = string.Empty;
        }


    }
}