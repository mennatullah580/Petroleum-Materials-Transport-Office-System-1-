// Models/LogModels.cs
namespace Petroleum_Materials_Transport_Office_System.Models
{
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