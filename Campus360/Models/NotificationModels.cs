namespace Campus360.Models
{
    public class NotificationModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public NotificationCategory Category { get; set; } = NotificationCategory.General;
        public string? RelatedId { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public DateTime? ExpiryDate { get; set; }
        public string? ActionUrl { get; set; }
        public string? IconClass { get; set; }
        public string? BadgeClass { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsSystemGenerated { get; set; } = true;
        public string? GeneratedBy { get; set; }
        public string? CourseId { get; set; }
        public string? CourseName { get; set; }        public string? CourseCode { get; set; }
        
        // Scheduling and delivery properties
        public DateTime? ScheduledFor { get; set; } // When to send the notification
        public bool IsSent { get; set; } = false; // Whether notification has been sent
        public DateTime? SentAt { get; set; } // When notification was sent
        public DateTime? ExpiresAt { get; set; } // When notification expires
        
        // Computed properties
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;
                if (timeSpan.TotalMinutes < 1) return "Just now";
                if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
                if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
                if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
                return CreatedAt.ToString("MMM dd, yyyy");
            }
        }
        
        public string DisplayIcon => IconClass ?? GetDefaultIcon();
        public string DisplayBadge => BadgeClass ?? GetDefaultBadge();
        
        private string GetDefaultIcon()
        {
            return Type switch
            {
                NotificationType.Result => "fas fa-chart-line",
                NotificationType.Assignment => "fas fa-tasks",
                NotificationType.Attendance => "fas fa-user-check",
                NotificationType.Deadline => "fas fa-clock",
                NotificationType.Warning => "fas fa-exclamation-triangle",
                NotificationType.Alert => "fas fa-bell",
                NotificationType.Success => "fas fa-check-circle",
                NotificationType.Notice => "fas fa-bullhorn",
                _ => "fas fa-info-circle"
            };
        }
        
        private string GetDefaultBadge()
        {
            return Priority switch
            {
                NotificationPriority.Critical => "badge-danger",
                NotificationPriority.High => "badge-warning",
                NotificationPriority.Normal => "badge-primary",
                NotificationPriority.Low => "badge-secondary",
                _ => "badge-info"
            };
        }
    }

    public enum NotificationType
    {
        Info,
        Alert,
        Deadline,
        Warning,
        Result,
        Assignment,
        Attendance,
        Notice,
        Success,
        Error
    }

    public enum NotificationCategory
    {
        General,
        Attendance,
        Result,
        Assignment,
        Notice,
        Academic,
        Administrative,
        System,
        Event,
        Examination,
        Holiday
    }

    public enum NotificationPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    public class AutomationRule
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AutomationTrigger Trigger { get; set; }
        public string TriggerCondition { get; set; } = string.Empty;
        public string MessageTemplate { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public NotificationPriority Priority { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public int TriggerCount { get; set; }
        public DateTime? LastTriggered { get; set; }
        public TimeSpan? DelayBeforeTrigger { get; set; }
        public string? TargetRole { get; set; }
        public bool SendToParents { get; set; } = false;
        public bool SendToTeachers { get; set; } = false;
        public bool SendToAdmin { get; set; } = false;
    }

    public enum AutomationTrigger
    {
        ResultUploaded,
        AttendanceBelowThreshold,
        AssignmentUploaded,
        AssignmentDeadlineApproaching,
        ClassCancelled,
        NoticePublished,
        StudentEnrolled,
        CourseCompleted,
        GradeDropped,
        AttendanceImproved,
        AssignmentSubmitted,
        ExamScheduled,
        ResultPublished,
        SystemMaintenance
    }

    public class AutomationLog
    {
        public string Id { get; set; } = string.Empty;
        public string LogId { get; set; } = string.Empty;
        public AutomationTrigger TriggerType { get; set; }
        public string RuleId { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseCode { get; set; }
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? TeacherId { get; set; }
        public string? TeacherName { get; set; }        public string Status { get; set; } = "pending"; // pending, sent, failed, delivered
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime TriggeredAt { get; set; } = DateTime.Now;
        public string TriggeredBy { get; set; } = "system";
        public string TargetUserId { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public string ActionTaken { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; } = true;
        public string? ResultData { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? TriggerReason { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public string? RelatedEntityId { get; set; }        public string? RelatedEntityType { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
          // Computed and alias properties
        public string? Type { get; set; } // Make it settable instead of computed
        public DateTime Timestamp => TriggeredAt; // Alias for TriggeredAt
        public string ActivityType => TriggerType.ToString(); // Alias for TriggerType
        public string Description => Message; // Alias for Message
        public string Details => ActionTaken; // Additional details
        public int RecipientCount { get; set; } = 1; // Number of recipients
        
        public string StatusBadgeClass => Status switch
        {
            "sent" => "badge-success",
            "delivered" => "badge-primary", 
            "failed" => "badge-danger",
            "pending" => "badge-warning",
            _ => "badge-secondary"
        };
        
        public string StatusIcon => Status switch
        {
            "sent" => "fas fa-check",
            "delivered" => "fas fa-check-double",
            "failed" => "fas fa-times",
            "pending" => "fas fa-clock",
            _ => "fas fa-question"
        };
    }

    public class AutomationMetrics
    {
        public int TotalNotificationsSent { get; set; }
        public int TotalActiveRules { get; set; }
        public int NotificationsSentToday { get; set; }
        public int NotificationsSentThisWeek { get; set; }
        public int NotificationsSentThisMonth { get; set; }
        public int FailedNotifications { get; set; }
        public int PendingNotifications { get; set; }
        public Dictionary<string, int> NotificationsByType { get; set; } = new();
        public Dictionary<string, int> NotificationsByPriority { get; set; } = new();
        public List<AutomationLog> RecentLogs { get; set; } = new();
        public List<TopTriggeredRule> TopRules { get; set; } = new();
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public DateTime LastCalculated { get; set; } = DateTime.Now;
    }    public class TopTriggeredRule
    {
        public string RuleId { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public int TriggerCount { get; set; }
        public DateTime LastTriggered { get; set; }
        public string TriggerType { get; set; } = string.Empty;
        public double SuccessRate { get; set; } = 100.0;
    }

    public class NotificationBatch
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<string> UserIds { get; set; } = new();
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public int TotalRecipients { get; set; }
        public int SentCount { get; set; }
        public int FailedCount { get; set; }        public string Status { get; set; } = "pending"; // pending, processing, completed, failed
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Represents an active automation alert for students
    /// </summary>
    public class AutomationAlert
    {
        public string Id { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty; // "low_attendance", "assignment_due", "result_available"
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseCode { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }
        public string? ActionUrl { get; set; }
        public string IconClass { get; set; } = "fas fa-bell";
        public string BadgeClass { get; set; } = "badge-info";
        public bool IsActive { get; set; } = true;
        public string? RelatedEntityId { get; set; } // Assignment ID, Result ID, etc.
        public string RelatedEntityType { get; set; } = string.Empty; // "assignment", "result", "attendance"
        
        // Computed properties
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;
                if (timeSpan.TotalMinutes < 1) return "Just now";
                if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
                if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
                if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
                return CreatedAt.ToString("MMM dd, yyyy");
            }
        }
        
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate < DateTime.Now;
        
        public string DisplayIcon => IconClass;
        public string DisplayBadge => BadgeClass;
    }
}
