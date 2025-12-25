using System.Text;

namespace Petroleum_Materials_Transport_Office_System.Services
{
    public class ActionLogger
    {
        private readonly string _logFilePath;

        public ActionLogger(IConfiguration config)
        {
            var logDir = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "Logs");
            Directory.CreateDirectory(logDir);
            _logFilePath = Path.Combine(logDir, "system_actions.log");
        }

        public void Log(string user, string action, string details)
        {
            try
            {
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm} | {user} | {action} | {details}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, line, Encoding.UTF8);
            }
            catch
            {
                // Fail silently in production
            }
        }

        public List<LogEntry> GetRecentLogs(int maxLines = 200, string? searchTerm = null)
        {
            if (!File.Exists(_logFilePath))
                return new List<LogEntry>();

            var allLines = File.ReadAllLines(_logFilePath, Encoding.UTF8);
            var logs = new List<LogEntry>();

            // Filter lines if search term exists
            var filteredLines = searchTerm == null
                ? allLines
                : allLines.Where(line =>
                {
                    var parts = line.Split(" | ", 4);
                    return parts.Length == 4 &&
                   (parts[1].Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || // User
                    parts[2].Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || // Action
                    parts[3].Contains(searchTerm, StringComparison.OrdinalIgnoreCase));  // Details
                });

            // Take last N lines
            var linesToDisplay = filteredLines
                .Skip(Math.Max(0, filteredLines.Count() - maxLines))
                .ToArray();

            foreach (var line in linesToDisplay)
            {
                var parts = line.Split(" | ", 4);
                if (parts.Length == 4)
                {
                    logs.Add(new LogEntry
                    {
                        Time = parts[0],
                        User = parts[1],
                        Action = parts[2],
                        Details = parts[3]
                    });
                }
            }

            return logs;
        }
    }

    public class LogEntry
    {
        public string Time { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}