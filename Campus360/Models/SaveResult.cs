namespace Campus360.Models
{
    public class SaveResult
    {
        public bool Success { get; set; }
        public bool AutomationTriggered { get; set; }
        public string Message { get; set; } = string.Empty;

        public static SaveResult Successful(bool automationTriggered = false, string message = "Operation completed successfully")
        {
            return new SaveResult
            {
                Success = true,
                AutomationTriggered = automationTriggered,
                Message = message
            };
        }

        public static SaveResult Failed(string message = "Operation failed")
        {
            return new SaveResult
            {
                Success = false,
                AutomationTriggered = false,
                Message = message
            };
        }
    }
}
