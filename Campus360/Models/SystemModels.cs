using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Campus360.Models
{
    /// <summary>
    /// System setting model for global application configuration
    /// </summary>
    public class SystemSetting
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Category is required")]
        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string Category { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Key is required")]
        [StringLength(100, ErrorMessage = "Key cannot exceed 100 characters")]
        public string Key { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Value is required")]
        public string Value { get; set; } = string.Empty;
        
        [Required]
        public SettingValueType ValueType { get; set; } = SettingValueType.String;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
        
        public bool IsEncrypted { get; set; } = false;
        public bool IsPublic { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Helper methods for type conversion
        public T GetValue<T>()
        {
            return ValueType switch
            {
                SettingValueType.Boolean => (T)(object)bool.Parse(Value),
                SettingValueType.Number => (T)(object)double.Parse(Value),
                SettingValueType.Json => JsonSerializer.Deserialize<T>(Value) ?? default(T)!,
                _ => (T)(object)Value
            };
        }

        public void SetValue<T>(T value)
        {
            Value = ValueType switch
            {
                SettingValueType.Json => JsonSerializer.Serialize(value),
                _ => value?.ToString() ?? string.Empty
            };
        }
    }

    /// <summary>
    /// User preference model for individual user settings
    /// </summary>
    public class UserPreference
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Key is required")]
        public string Key { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Value is required")]
        public string Value { get; set; } = string.Empty;
        
        public SettingValueType ValueType { get; set; } = SettingValueType.String;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Helper methods for type conversion
        public T GetValue<T>()
        {
            return ValueType switch
            {
                SettingValueType.Boolean => (T)(object)bool.Parse(Value),
                SettingValueType.Number => (T)(object)double.Parse(Value),
                SettingValueType.Json => JsonSerializer.Deserialize<T>(Value) ?? default(T)!,
                _ => (T)(object)Value
            };
        }

        public void SetValue<T>(T value)
        {
            Value = ValueType switch
            {
                SettingValueType.Json => JsonSerializer.Serialize(value),
                _ => value?.ToString() ?? string.Empty
            };
        }
    }

    /// <summary>
    /// Audit log model for tracking system changes
    /// </summary>
    public class AuditLog
    {
        public string Id { get; set; } = string.Empty;
        public string? UserId { get; set; }
        
        [Required]
        public string Action { get; set; } = string.Empty;
        
        [Required]
        public string EntityType { get; set; } = string.Empty;
        
        public string? EntityId { get; set; }
        public Dictionary<string, object>? OldValues { get; set; }
        public Dictionary<string, object>? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// System report configuration model
    /// </summary>
    public class SystemReport
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
        
        [Required]
        public ReportType ReportType { get; set; }
        
        [Required]
        public ReportCategory Category { get; set; }
        
        [Required(ErrorMessage = "Query template is required")]
        public string QueryTemplate { get; set; } = string.Empty;
        
        public Dictionary<string, object>? Parameters { get; set; }
        public List<string> AccessRoles { get; set; } = new() { "admin" };
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Report schedule model for automated reports
    /// </summary>
    public class ReportSchedule
    {
        public string Id { get; set; } = string.Empty;
        public string ReportId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public ScheduleType ScheduleType { get; set; }
        
        public string? CronExpression { get; set; }
        public List<string> Recipients { get; set; } = new();
        public ReportFormat Format { get; set; } = ReportFormat.Pdf;
        public Dictionary<string, object>? Parameters { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastRunAt { get; set; }
        public DateTime? NextRunAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        
        // Navigation property
        public SystemReport? Report { get; set; }
    }

    /// <summary>
    /// Report execution result model
    /// </summary>
    public class ReportResult
    {
        public string Id { get; set; } = string.Empty;
        public string ReportId { get; set; } = string.Empty;
        public string? ScheduleId { get; set; }
        public ReportStatus Status { get; set; }
        public object? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public int RowCount { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string? GeneratedBy { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }

    /// <summary>
    /// Dashboard widget configuration model
    /// </summary>
    public class DashboardWidget
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public WidgetType Type { get; set; }
        public string? ReportId { get; set; }
        public Dictionary<string, object>? Configuration { get; set; }
        public int Position { get; set; }
        public int Width { get; set; } = 12; // Grid columns (1-12)
        public int Height { get; set; } = 300; // Height in pixels
        public bool IsVisible { get; set; } = true;
        public List<string> AccessRoles { get; set; } = new() { "admin" };
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // ================= ENUMS =================

    public enum SettingValueType
    {
        String,
        Number,
        Boolean,
        Json
    }

    public enum ReportType
    {
        Dashboard,
        Export,
        Scheduled
    }

    public enum ReportCategory
    {
        Academic,
        Attendance,
        Performance,
        System,
        Financial,
        User
    }

    public enum ScheduleType
    {
        Daily,
        Weekly,
        Monthly,
        Custom
    }

    public enum ReportFormat
    {
        Pdf,
        Excel,
        Csv,
        Json
    }

    public enum ReportStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Cancelled
    }

    public enum WidgetType
    {
        Chart,
        Table,
        Card,
        Metric,
        List,
        Calendar
    }

    public enum AuditAction
    {
        Create,
        Update,
        Delete,
        Login,
        Logout,
        View,
        Export,
        Import
    }

    // ================= DTOs =================

    /// <summary>
    /// DTO for system settings operations
    /// </summary>
    public class SystemSettingDto
    {
        public string? Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public SettingValueType ValueType { get; set; }
        public string? Description { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsPublic { get; set; }
    }

    /// <summary>
    /// DTO for user preferences operations
    /// </summary>
    public class UserPreferenceDto
    {
        public string? Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public SettingValueType ValueType { get; set; }
    }

    /// <summary>
    /// DTO for report generation requests
    /// </summary>
    public class ReportGenerationRequest
    {
        public string ReportId { get; set; } = string.Empty;
        public Dictionary<string, object>? Parameters { get; set; }
        public ReportFormat Format { get; set; } = ReportFormat.Pdf;
        public bool IsAsync { get; set; } = false;
    }

    /// <summary>
    /// DTO for dashboard data
    /// </summary>
    public class DashboardData
    {
        public List<DashboardWidget> Widgets { get; set; } = new();
        public Dictionary<string, object> GlobalMetrics { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO for system health status
    /// </summary>
    public class SystemHealthStatus
    {
        public bool IsHealthy { get; set; }
        public Dictionary<string, object> Metrics { get; set; } = new();
        public List<string> Issues { get; set; } = new();
        public DateTime CheckedAt { get; set; }
    }
}
