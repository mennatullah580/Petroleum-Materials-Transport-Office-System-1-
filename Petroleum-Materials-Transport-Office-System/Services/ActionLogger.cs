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
            var logs = new List<LogEntry>();

            if (!File.Exists(_logFilePath))
                return logs;

            try
            {
                var lines = File.ReadAllLines(_logFilePath, Encoding.UTF8);

                // Process logs from newest to oldest (reverse order)
                for (int i = lines.Length - 1; i >= 0; i--)
                {
                    var line = lines[i];
                    var parts = line.Split(new string[] { " | " }, StringSplitOptions.None);

                    if (parts.Length == 4)
                    {
                        var logEntry = new LogEntry
                        {
                            Time = parts[0],
                            User = parts[1],
                            Action = parts[2],
                            Details = parts[3]
                        };

                        // If search term exists, filter immediately
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            var term = searchTerm.Trim();
                            if (logEntry.User.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                logEntry.Action.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                logEntry.Details.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                logs.Add(logEntry);
                            }
                        }
                        else
                        {
                            logs.Add(logEntry);
                        }

                        // Stop when we have enough logs
                        if (logs.Count >= maxLines)
                            break;
                    }
                }

                // Reverse to show newest first
                logs.Reverse();
            }
            catch (Exception ex)
            {
                // Log error if needed (optional)
                System.Diagnostics.Debug.WriteLine($"Log read error: {ex.Message}");
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