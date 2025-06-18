// filepath: d:\projects\Campus 360\Campus360\Models\CalendarModels.cs
using System.ComponentModel.DataAnnotations;

namespace Campus360.Models
{
    // =============== ACADEMIC CALENDAR SYSTEM MODELS ===============

    /// <summary>
    /// Unified Academic Event Model for Campus360 Calendar System
    /// Combines assignments, exams, notices, results, and custom events
    /// </summary>
    public class AcademicEvent
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Event Classification
        public AcademicEventType EventType { get; set; }
        public string Category { get; set; } = string.Empty; // "Academic", "Administrative", "Holiday", "Exam"
        public EventPriority Priority { get; set; } = EventPriority.Normal;
        
        // Date & Time
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool IsAllDay { get; set; } = false;
        
        // Location & Context
        public string? Venue { get; set; }
        public string? Room { get; set; }
        public string? Building { get; set; }
        public string? OnlineLink { get; set; }
        
        // Course Association
        public string? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseCode { get; set; }
        public string? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        
        // Target Audience
        public EventTargetRole TargetRole { get; set; } = EventTargetRole.All;
        public List<string> TargetUserIds { get; set; } = new();
        public List<string> TargetCourseIds { get; set; } = new();
        public List<string> TargetDepartmentIds { get; set; } = new();
        
        // Event Management
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public string CreatedByRole { get; set; } = string.Empty; // "Admin", "Teacher", "System"
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        
        // Event Status
        public EventStatus Status { get; set; } = EventStatus.Active;
        public bool IsPublished { get; set; } = true;
        public bool IsRecurring { get; set; } = false;
        public RecurringPattern? RecurringPattern { get; set; }
        
        // Automation Integration
        public bool IsSystemGenerated { get; set; } = false;
        public string? SourceType { get; set; } // "Assignment", "Result", "Notice", "Manual"
        public string? SourceId { get; set; } // Original assignment/notice/result ID
        public string? AutomationRuleId { get; set; }
        
        // Notification & Reminder
        public bool HasReminder { get; set; } = false;
        public List<EventReminder> Reminders { get; set; } = new();
        public int NotificationsSent { get; set; } = 0;
        
        // Display Properties
        public string Color { get; set; } = "#007bff";
        public string? IconClass { get; set; }
        public string? BadgeClass { get; set; }
        
        // Additional Data
        public Dictionary<string, object> Metadata { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }

    /// <summary>
    /// Academic Event Types for Calendar System
    /// </summary>
    public enum AcademicEventType
    {
        Assignment,
        Exam,
        Quiz,
        Project,
        Notice,
        Result,
        ClassScheduled,
        ClassCancelled,
        Holiday,
        Semester,
        Registration,
        AdmissionDeadline,
        FeePayment,
        Workshop,
        Seminar,
        Meeting,
        Conference,
        Other
    }

    /// <summary>
    /// Event Priority Levels
    /// </summary>
    public enum EventPriority
    {
        Low,
        Normal,
        High,
        Critical,
        Urgent
    }

    /// <summary>
    /// Event Target Roles
    /// </summary>
    public enum EventTargetRole
    {
        Student,
        Teacher,
        Admin,
        All,
        Department,
        Course,
        Custom
    }

    /// <summary>
    /// Event Status Types
    /// </summary>
    public enum EventStatus
    {
        Draft,
        Active,
        Completed,
        Cancelled,
        Postponed,
        InProgress,
        Expired
    }

    /// <summary>
    /// Recurring Pattern for Events
    /// </summary>
    public class RecurringPattern
    {
        public RecurringType Type { get; set; }
        public int Interval { get; set; } = 1; // Every X days/weeks/months
        public List<DayOfWeek> DaysOfWeek { get; set; } = new();
        public DateTime? EndDate { get; set; }
        public int? MaxOccurrences { get; set; }
    }

    public enum RecurringType
    {
        None,
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    /// <summary>
    /// Event Reminder Configuration
    /// </summary>
    public class EventReminder
    {
        public string Id { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public int MinutesBefore { get; set; } // Minutes before event
        public ReminderType Type { get; set; }
        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }
        public string? CustomMessage { get; set; }
    }

    public enum ReminderType
    {
        Notification,
        Email,
        SMS,
        Push
    }

    // =============== CALENDAR VIEW MODELS ===============    /// <summary>
    /// Calendar View Configuration
    /// </summary>
    public class CalendarViewModel
    {
        public DateTime CurrentDate { get; set; } = DateTime.Now;
        public CalendarViewType ViewType { get; set; } = CalendarViewType.Month;
        public List<AcademicEvent> Events { get; set; } = new();
        public CalendarFilter Filter { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        
        // Navigation
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Statistics
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int TodayEvents { get; set; }
        public int OverdueEvents { get; set; }
          // Available data for filters
        public List<string> AvailableCourses { get; set; } = new();
        public List<AcademicEventType> EventTypes { get; set; } = new();
        public List<EventPriority> Priorities { get; set; } = new();
    }

    public enum CalendarViewType
    {
        Day,
        Week,
        Month,
        Timeline,
        Agenda
    }

    /// <summary>
    /// Calendar Filter Options
    /// </summary>
    public class CalendarFilter
    {
        public List<AcademicEventType> EventTypes { get; set; } = new();
        public List<EventPriority> Priorities { get; set; } = new();
        public List<string> CourseIds { get; set; } = new();
        public List<string> DepartmentIds { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public DateRange? DateRange { get; set; }
        public bool ShowCompleted { get; set; } = false;
        public bool ShowCancelled { get; set; } = false;
        public string? SearchQuery { get; set; }
    }

    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // =============== TIMELINE MODELS ===============

    /// <summary>
    /// Timeline Event for Academic History View
    /// </summary>
    public class TimelineEvent
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public AcademicEventType EventType { get; set; }
        public EventPriority Priority { get; set; }
        public string Color { get; set; } = "#007bff";
        public string IconClass { get; set; } = "fas fa-calendar";
        public string? CourseInfo { get; set; }
        public string? ActionUrl { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPast { get; set; }
        public bool IsToday { get; set; }
        public bool IsUpcoming { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public string? Venue { get; set; }
    }

    /// <summary>
    /// Timeline Group for organizing events by date
    /// </summary>
    public class TimelineGroup
    {
        public string GroupTitle { get; set; } = string.Empty; // "Today", "Yesterday", "This Week", etc.
        public DateTime GroupDate { get; set; }
        public List<TimelineEvent> Events { get; set; } = new();
        public int EventCount { get; set; }
        public bool IsToday { get; set; }
        public bool IsPast { get; set; }
        public bool IsUpcoming { get; set; }
    }

    // =============== EVENT CREATION MODELS ===============

    /// <summary>
    /// Model for creating new academic events
    /// </summary>
    public class CreateEventModel
    {
        [Required(ErrorMessage = "Event title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Event description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Event type is required")]
        public AcademicEventType EventType { get; set; }
        
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(1);
        
        public DateTime? EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool IsAllDay { get; set; } = false;
        
        public string? Venue { get; set; }
        public string? CourseId { get; set; }
        public EventTargetRole TargetRole { get; set; } = EventTargetRole.All;
        public List<string> TargetUserIds { get; set; } = new();
        public EventPriority Priority { get; set; } = EventPriority.Normal;
        
        public bool HasReminder { get; set; } = false;
        public List<int> ReminderMinutes { get; set; } = new(); // [60, 1440] = 1 hour, 1 day before
        
        public string Color { get; set; } = "#007bff";
        public List<string> Tags { get; set; } = new();
        
        public bool IsRecurring { get; set; } = false;
        public RecurringPattern? RecurringPattern { get; set; }
    }

    // =============== CALENDAR STATISTICS ===============

    /// <summary>
    /// Calendar statistics for dashboard
    /// </summary>
    public class CalendarStats
    {
        public int TotalEvents { get; set; }
        public int TodayEvents { get; set; }
        public int ThisWeekEvents { get; set; }
        public int ThisMonthEvents { get; set; }
        public int UpcomingAssignments { get; set; }
        public int UpcomingExams { get; set; }
        public int OverdueItems { get; set; }
        public int UnreadNotices { get; set; }
        
        // Breakdown by type
        public Dictionary<AcademicEventType, int> EventsByType { get; set; } = new();
        public Dictionary<EventPriority, int> EventsByPriority { get; set; } = new();
        public Dictionary<string, int> EventsByCourse { get; set; } = new();
        
        // Recent activity
        public List<TimelineEvent> RecentEvents { get; set; } = new();
        public List<TimelineEvent> UpcomingEvents { get; set; } = new();
    }

    // =============== CALENDAR INTEGRATION MODELS ===============

    /// <summary>
    /// Model for integrating automation events into calendar
    /// </summary>
    public class AutomationEventMapping
    {
        public string SourceType { get; set; } = string.Empty; // "Assignment", "Result", "Notice"
        public string SourceId { get; set; } = string.Empty;
        public string CalendarEventId { get; set; } = string.Empty;        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Model for calendar event sync with external sources
    /// </summary>
    public class EventSyncResult
    {
        public int EventsCreated { get; set; }
        public int EventsUpdated { get; set; }
        public int EventsDeleted { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime LastSyncTime { get; set; } = DateTime.Now;
        public bool Success { get; set; } = true;
    }

    // =============== CALENDAR EVENT MODEL ===============
    
    /// <summary>
    /// Simplified Calendar Event Model for widget components
    /// </summary>
    public class CalendarEvent
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool HasTime { get; set; } = true;
        public string Location { get; set; } = string.Empty;
        public EventType Type { get; set; }
        public EventPriority Priority { get; set; } = EventPriority.Normal;
        public string Color { get; set; } = "#007bff";
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Event types for calendar widget
    /// </summary>
    public enum EventType
    {
        Assignment,
        Exam,
        Class,
        Event,
        Holiday,
        Notice,
        Meeting,
        Deadline
    }
}
