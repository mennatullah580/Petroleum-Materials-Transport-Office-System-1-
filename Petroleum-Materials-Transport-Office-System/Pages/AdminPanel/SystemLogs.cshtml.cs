using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Petroleum_Materials_Transport_Office_System.Services;

namespace Petroleum_Materials_Transport_Office_System.Pages.AdminPanel
{
    public class SystemLogsModel : PageModel
    {
        private readonly ActionLogger _logger;

        public SystemLogsModel(ActionLogger logger)
        {
            _logger = logger;
        }

        public List<LogEntry> Logs { get; set; } = new();
        public string? SearchTerm { get; set; }

        public void OnGet(string? search)
        {
            SearchTerm = search;
            Logs = _logger.GetRecentLogs(200, search); // Pass search term
        }
    }
}