// ===================================================================
// CAMPUS 360 - DATABASE MODELS FOR SUPABASE INTEGRATION
// ===================================================================
// These models map directly to the Supabase database tables
// Uses Supabase attributes for proper ORM mapping
// ===================================================================

using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.ComponentModel.DataAnnotations;

namespace Campus360.Models
{    // ===================================================================
    // 1. CORE USER MANAGEMENT MODELS
    // ===================================================================
    [Table("user_profiles")]
    public class UserProfileDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("role")]
        public string Role { get; set; } = string.Empty;

        [Column("department_id")]
        public string? DepartmentId { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("gender")]
        public string? Gender { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("student_id")]
        public string? StudentId { get; set; }

        [Column("program")]
        public string? Program { get; set; }

        [Column("academic_year")]
        public string? AcademicYear { get; set; }

        [Column("admission_date")]
        public DateTime? AdmissionDate { get; set; }

        [Column("emergency_contact_name")]
        public string? EmergencyContactName { get; set; }

        [Column("emergency_contact_phone")]
        public string? EmergencyContactPhone { get; set; }

        [Column("emergency_contact_relationship")]
        public string? EmergencyContactRelationship { get; set; }

        [Column("is_verified")]
        public bool IsVerified { get; set; } = false;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    [Table("departments")]
    public class DepartmentDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    // ===================================================================
    // 2. ACADEMIC MANAGEMENT MODELS
    // ===================================================================

    [Table("courses")]
    public class CourseDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("department_id")]
        public string DepartmentId { get; set; } = string.Empty;

        [Column("teacher_id")]
        public string? TeacherId { get; set; }

        [Column("semester")]
        public int Semester { get; set; }

        [Column("credit_hours")]
        public int CreditHours { get; set; }

        [Column("academic_year")]
        public string? AcademicYear { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    [Table("course_enrollments")]
    public class CourseEnrollmentDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("student_id")]
        public string StudentId { get; set; } = string.Empty;

        [Column("course_id")]
        public string CourseId { get; set; } = string.Empty;

        [Column("enrolled_date")]
        public DateTime EnrolledDate { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }

    [Table("assignments")]
    public class AssignmentDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("course_id")]
        public string CourseId { get; set; } = string.Empty;

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("due_date")]
        public DateTime DueDate { get; set; }

        [Column("max_score")]
        public int MaxScore { get; set; } = 100;

        [Column("assignment_type")]
        public string AssignmentType { get; set; } = "homework";

        [Column("is_published")]
        public bool IsPublished { get; set; } = false;

        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    [Table("assignment_submissions")]
    public class AssignmentSubmissionDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("assignment_id")]
        public string AssignmentId { get; set; } = string.Empty;

        [Column("student_id")]
        public string StudentId { get; set; } = string.Empty;

        [Column("submission_text")]
        public string? SubmissionText { get; set; }

        [Column("file_attachments")]
        public List<string> FileAttachments { get; set; } = new();

        [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; }

        [Column("score")]
        public int? Score { get; set; }

        [Column("feedback")]
        public string? Feedback { get; set; }

        [Column("graded_by")]
        public string? GradedBy { get; set; }

        [Column("graded_at")]
        public DateTime? GradedAt { get; set; }

        [Column("is_late")]
        public bool IsLate { get; set; } = false;
    }

    // ===================================================================
    // 3. ATTENDANCE MANAGEMENT MODELS
    // ===================================================================

    [Table("attendance_sessions")]
    public class AttendanceSessionDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("course_id")]
        public string CourseId { get; set; } = string.Empty;

        [Column("class_date")]
        public DateTime ClassDate { get; set; }

        [Column("class_type")]
        public string ClassType { get; set; } = "lecture";

        [Column("topic")]
        public string? Topic { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;

        [Column("total_students")]
        public int TotalStudents { get; set; } = 0;

        [Column("present_students")]
        public int PresentStudents { get; set; } = 0;

        [Column("absent_students")]
        public int AbsentStudents { get; set; } = 0;

        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("attendance_records")]
    public class AttendanceRecordDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("session_id")]
        public string SessionId { get; set; } = string.Empty;

        [Column("student_id")]
        public string StudentId { get; set; } = string.Empty;

        [Column("is_present")]
        public bool IsPresent { get; set; } = false;

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("marked_at")]
        public DateTime MarkedAt { get; set; }

        [Column("marked_by")]
        public string MarkedBy { get; set; } = string.Empty;
    }

    // ===================================================================
    // 4. CALENDAR & EVENTS MODELS
    // ===================================================================

    [Table("academic_events")]
    public class AcademicEventDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("event_type")]
        public string EventType { get; set; } = string.Empty;

        [Column("category")]
        public string Category { get; set; } = "academic";

        [Column("priority")]
        public string Priority { get; set; } = "normal";

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("start_time")]
        public TimeSpan? StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan? EndTime { get; set; }

        [Column("is_all_day")]
        public bool IsAllDay { get; set; } = false;

        [Column("venue")]
        public string? Venue { get; set; }

        [Column("room")]
        public string? Room { get; set; }

        [Column("building")]
        public string? Building { get; set; }

        [Column("online_link")]
        public string? OnlineLink { get; set; }

        [Column("course_id")]
        public string? CourseId { get; set; }

        [Column("department_id")]
        public string? DepartmentId { get; set; }

        [Column("target_role")]
        public string TargetRole { get; set; } = "all";

        [Column("target_user_ids")]
        public List<string> TargetUserIds { get; set; } = new();

        [Column("target_course_ids")]
        public List<string> TargetCourseIds { get; set; } = new();

        [Column("target_department_ids")]
        public List<string> TargetDepartmentIds { get; set; } = new();

        [Column("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [Column("created_by_role")]
        public string? CreatedByRole { get; set; }

        [Column("status")]
        public string Status { get; set; } = "active";

        [Column("is_published")]
        public bool IsPublished { get; set; } = true;

        [Column("is_recurring")]
        public bool IsRecurring { get; set; } = false;

        [Column("recurring_pattern")]
        public Dictionary<string, object>? RecurringPattern { get; set; }

        [Column("is_system_generated")]
        public bool IsSystemGenerated { get; set; } = false;

        [Column("source_type")]
        public string? SourceType { get; set; }

        [Column("source_id")]
        public string? SourceId { get; set; }

        [Column("automation_rule_id")]
        public string? AutomationRuleId { get; set; }

        [Column("has_reminder")]
        public bool HasReminder { get; set; } = false;

        [Column("color")]
        public string Color { get; set; } = "#007bff";

        [Column("icon_class")]
        public string? IconClass { get; set; }

        [Column("badge_class")]
        public string? BadgeClass { get; set; }

        [Column("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new();

        [Column("tags")]
        public List<string> Tags { get; set; } = new();

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    [Table("event_reminders")]
    public class EventReminderDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("event_id")]
        public string EventId { get; set; } = string.Empty;

        [Column("reminder_type")]
        public string ReminderType { get; set; } = string.Empty;

        [Column("reminder_time")]
        public int ReminderTime { get; set; }

        [Column("is_sent")]
        public bool IsSent { get; set; } = false;

        [Column("sent_at")]
        public DateTime? SentAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    // ===================================================================
    // 5. NOTIFICATION MODELS
    // ===================================================================

    [Table("notifications")]
    public class NotificationDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("type")]
        public string Type { get; set; } = "info";

        [Column("category")]
        public string Category { get; set; } = "general";

        [Column("priority")]
        public string Priority { get; set; } = "normal";

        [Column("related_id")]
        public string? RelatedId { get; set; }

        [Column("action_url")]
        public string? ActionUrl { get; set; }

        [Column("icon_class")]
        public string? IconClass { get; set; }

        [Column("badge_class")]
        public string? BadgeClass { get; set; }

        [Column("course_id")]
        public string? CourseId { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }

        [Column("is_system_generated")]
        public bool IsSystemGenerated { get; set; } = true;

        [Column("generated_by")]
        public string? GeneratedBy { get; set; }

        [Column("scheduled_for")]
        public DateTime? ScheduledFor { get; set; }

        [Column("is_sent")]
        public bool IsSent { get; set; } = false;

        [Column("sent_at")]
        public DateTime? SentAt { get; set; }

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    // ===================================================================
    // 6. FILE MANAGEMENT MODELS
    // ===================================================================

    [Table("file_documents")]
    public class FileDocumentDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        [Column("original_file_name")]
        public string OriginalFileName { get; set; } = string.Empty;

        [Column("file_url")]
        public string FileUrl { get; set; } = string.Empty;

        [Column("file_type")]
        public string FileType { get; set; } = string.Empty;

        [Column("uploaded_by")]
        public string UploadedBy { get; set; } = string.Empty;

        [Column("uploader_name")]
        public string? UploaderName { get; set; }

        [Column("subject_id")]
        public string? SubjectId { get; set; }

        [Column("subject_name")]
        public string? SubjectName { get; set; }

        [Column("course_id")]
        public string? CourseId { get; set; }

        [Column("course_name")]
        public string? CourseName { get; set; }

        [Column("department_id")]
        public string? DepartmentId { get; set; }

        [Column("department_name")]
        public string? DepartmentName { get; set; }

        [Column("visibility")]
        public string Visibility { get; set; } = "course";

        [Column("description")]
        public string? Description { get; set; }

        [Column("file_size")]
        public long FileSize { get; set; } = 0;

        [Column("content_type")]
        public string? ContentType { get; set; }

        [Column("download_count")]
        public int DownloadCount { get; set; } = 0;

        [Column("last_downloaded")]
        public DateTime? LastDownloaded { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_featured")]
        public bool IsFeatured { get; set; } = false;

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("max_score")]
        public int? MaxScore { get; set; }

        [Column("tags")]
        public List<string> Tags { get; set; } = new();

        [Column("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new();

        [Column("upload_date")]
        public DateTime UploadDate { get; set; }

        [Column("last_modified")]
        public DateTime LastModified { get; set; }
    }

    [Table("file_access_logs")]
    public class FileAccessLogDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("file_id")]
        public string FileId { get; set; } = string.Empty;

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("access_type")]
        public string AccessType { get; set; } = string.Empty;

        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("accessed_at")]
        public DateTime AccessedAt { get; set; }
    }

    // ===================================================================
    // 7. AUDIT & ACTIVITY MODELS
    // ===================================================================

    [Table("activity_logs")]
    public class ActivityLogDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;        [Column("activity_type")]
        public string ActivityType { get; set; } = string.Empty;

        [Column("table_name")]
        public new string? TableName { get; set; }

        [Column("record_id")]
        public string? RecordId { get; set; }

        [Column("old_values")]
        public Dictionary<string, object>? OldValues { get; set; }

        [Column("new_values")]
        public Dictionary<string, object>? NewValues { get; set; }

        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }    [Table("system_logs")]
    public class SystemLogDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("level")]
        public string Level { get; set; } = string.Empty;

        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("component")]
        public string? Component { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("context")]
        public Dictionary<string, object> Context { get; set; } = new();

        [Column("stack_trace")]
        public string? StackTrace { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    // ===================================================================
    // AUTOMATION LOG MODELS
    // ===================================================================

    [Table("automation_logs")]
    public class AutomationLogDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("log_id")]
        public string LogId { get; set; } = string.Empty;

        [Column("trigger_type")]
        public string TriggerType { get; set; } = string.Empty;

        [Column("rule_id")]
        public string RuleId { get; set; } = string.Empty;

        [Column("rule_name")]
        public string RuleName { get; set; } = string.Empty;

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("course_id")]
        public string? CourseId { get; set; }

        [Column("course_name")]
        public string? CourseName { get; set; }

        [Column("course_code")]
        public string? CourseCode { get; set; }

        [Column("student_id")]
        public string? StudentId { get; set; }

        [Column("student_name")]
        public string? StudentName { get; set; }

        [Column("teacher_id")]
        public string? TeacherId { get; set; }

        [Column("teacher_name")]
        public string? TeacherName { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("triggered_at")]
        public DateTime TriggeredAt { get; set; }

        [Column("triggered_by")]
        public string TriggeredBy { get; set; } = "system";

        [Column("target_user_id")]
        public string TargetUserId { get; set; } = string.Empty;

        [Column("target_type")]
        public string TargetType { get; set; } = string.Empty;

        [Column("action_taken")]
        public string ActionTaken { get; set; } = string.Empty;

        [Column("is_successful")]
        public bool IsSuccessful { get; set; } = true;

        [Column("result_data")]
        public string? ResultData { get; set; }

        [Column("sent_at")]
        public DateTime? SentAt { get; set; }

        [Column("delivered_at")]
        public DateTime? DeliveredAt { get; set; }

        [Column("trigger_reason")]
        public string? TriggerReason { get; set; }

        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        [Column("retry_count")]
        public int RetryCount { get; set; } = 0;

        [Column("related_entity_id")]
        public string? RelatedEntityId { get; set; }

        [Column("related_entity_type")]
        public string? RelatedEntityType { get; set; }

        [Column("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new();

        [Column("recipient_count")]
        public int RecipientCount { get; set; } = 1;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]        
        public DateTime UpdatedAt { get; set; }
    }

    [Table("automation_rules")]
    public class AutomationRuleDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("rule_id")]
        public string RuleId { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("trigger_type")]
        public string TriggerType { get; set; } = string.Empty;

        [Column("trigger_condition")]
        public string? TriggerCondition { get; set; }

        [Column("message_template")]
        public string MessageTemplate { get; set; } = string.Empty;

        [Column("notification_type")]
        public string NotificationType { get; set; } = string.Empty;

        [Column("priority")]
        public string Priority { get; set; } = "normal";

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("target_role")]
        public string? TargetRole { get; set; }

        [Column("delay_before_trigger")]
        public TimeSpan? DelayBeforeTrigger { get; set; }

        [Column("trigger_count")]
        public int TriggerCount { get; set; } = 0;

        [Column("last_triggered")]
        public DateTime? LastTriggered { get; set; }

        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    // ===================================================================
    // 8. CONFIGURATION MODELS
    // ===================================================================

    [Table("app_settings")]
    public class AppSettingDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("key")]
        public string Key { get; set; } = string.Empty;

        [Column("value")]
        public string? Value { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("data_type")]
        public string DataType { get; set; } = "string";

        [Column("is_public")]
        public bool IsPublic { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    [Table("user_settings")]
    public class UserSettingDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("key")]
        public string Key { get; set; } = string.Empty;

        [Column("value")]
        public string? Value { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    // ===================================================================
    // 8. SYSTEM SETTINGS AND REPORTING MODELS
    // ===================================================================

    [Table("system_settings")]
    public class SystemSettingDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("category")]
        public string Category { get; set; } = string.Empty;

        [Column("key")]
        public string Key { get; set; } = string.Empty;

        [Column("value")]
        public string Value { get; set; } = string.Empty;

        [Column("value_type")]
        public string ValueType { get; set; } = "string";

        [Column("description")]
        public string? Description { get; set; }

        [Column("is_encrypted")]
        public bool IsEncrypted { get; set; } = false;

        [Column("is_public")]
        public bool IsPublic { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("updated_by")]
        public string? UpdatedBy { get; set; }
    }

    [Table("user_preferences")]
    public class UserPreferenceDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("category")]
        public string Category { get; set; } = string.Empty;

        [Column("key")]
        public string Key { get; set; } = string.Empty;

        [Column("value")]
        public string Value { get; set; } = string.Empty;

        [Column("value_type")]
        public string ValueType { get; set; } = "string";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    [Table("audit_logs")]
    public class AuditLogDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("user_id")]
        public string? UserId { get; set; }        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("entity_type")]
        public string? EntityType { get; set; }

        [Column("entity_id")]
        public string? EntityId { get; set; }

        [Column("table_name")]
        public new string? TableName { get; set; }

        [Column("record_id")]
        public string? RecordId { get; set; }

        [Column("old_values")]
        public Dictionary<string, object>? OldValues { get; set; }        [Column("new_values")]
        public Dictionary<string, object>? NewValues { get; set; }

        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("session_id")]
        public string? SessionId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    [Table("system_reports")]
    public class SystemReportDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("category")]
        public string Category { get; set; } = string.Empty;

        [Column("sql_query")]
        public string? SqlQuery { get; set; }

        [Column("parameters")]
        public Dictionary<string, object>? Parameters { get; set; }

        [Column("chart_config")]
        public Dictionary<string, object>? ChartConfig { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("updated_by")]
        public string? UpdatedBy { get; set; }

        [Column("report_type")]
        public string ReportType { get; set; } = "dashboard";

        [Column("query_template")]
        public string? QueryTemplate { get; set; }

        [Column("access_roles")]
        public string[] AccessRoles { get; set; } = Array.Empty<string>();
    }

    [Table("report_schedules")]
    public class ReportScheduleDb : Supabase.Postgrest.Models.BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("report_id")]
        public string ReportId { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("frequency")]
        public string Frequency { get; set; } = string.Empty;

        [Column("parameters")]
        public Dictionary<string, object>? Parameters { get; set; }

        [Column("recipients")]
        public string[] Recipients { get; set; } = Array.Empty<string>();

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("last_run")]
        public DateTime? LastRun { get; set; }

        [Column("next_run")]
        public DateTime? NextRun { get; set; }        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("schedule_type")]
        public string ScheduleType { get; set; } = "daily";

        [Column("cron_expression")]
        public string? CronExpression { get; set; }

        [Column("format")]
        public string Format { get; set; } = "pdf";

        [Column("last_run_at")]
        public DateTime? LastRunAt { get; set; }

        [Column("next_run_at")]
        public DateTime? NextRunAt { get; set; }
    }

    // ===================================================================
    // 9. MODEL CONVERTERS (Database to Application Models)
    // ===================================================================

    public static class ModelConverters
    {
        // UserProfile converters
        public static UserProfile ToUserProfile(this UserProfileDb db)
        {
            return new UserProfile
            {
                Id = db.Id,
                FullName = db.FullName,
                Email = db.Email,
                Role = db.Role,
                DepartmentId = db.DepartmentId,
                StudentId = db.StudentId,
                Phone = db.Phone,
                Address = db.Address,
                IsVerified = db.IsVerified,
                IsActive = db.IsActive,
                LastLoginAt = db.LastLoginAt,
                CreatedAt = db.CreatedAt,
                UpdatedAt = db.UpdatedAt
            };
        }

        public static UserProfileDb ToDb(this UserProfile model)
        {
            return new UserProfileDb
            {
                Id = model.Id,
                FullName = model.FullName,
                Email = model.Email,
                Role = model.Role,
                DepartmentId = model.DepartmentId,
                StudentId = model.StudentId,
                Phone = model.Phone,
                Address = model.Address,
                IsVerified = model.IsVerified,
                IsActive = model.IsActive,
                LastLoginAt = model.LastLoginAt,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt
            };
        }

        // AutomationLog converters
        public static AutomationLog ToAutomationLog(this AutomationLogDb db)
        {
            return new AutomationLog
            {
                Id = db.Id,
                LogId = db.LogId,
                TriggerType = Enum.TryParse<AutomationTrigger>(db.TriggerType, true, out var trigger) ? trigger : AutomationTrigger.AttendanceBelowThreshold,
                RuleId = db.RuleId,
                RuleName = db.RuleName,
                Title = db.Title,
                Message = db.Message,
                CourseId = db.CourseId,
                CourseName = db.CourseName,
                CourseCode = db.CourseCode,
                StudentId = db.StudentId,
                StudentName = db.StudentName,
                TeacherId = db.TeacherId,
                TeacherName = db.TeacherName,
                Status = db.Status,
                TriggeredAt = db.TriggeredAt,
                TriggeredBy = db.TriggeredBy,
                TargetUserId = db.TargetUserId,
                TargetType = db.TargetType,
                ActionTaken = db.ActionTaken,
                IsSuccessful = db.IsSuccessful,
                ResultData = db.ResultData,
                SentAt = db.SentAt,
                DeliveredAt = db.DeliveredAt,
                TriggerReason = db.TriggerReason,
                ErrorMessage = db.ErrorMessage,
                RetryCount = db.RetryCount,
                RelatedEntityId = db.RelatedEntityId,
                RelatedEntityType = db.RelatedEntityType,
                Metadata = db.Metadata,
                RecipientCount = db.RecipientCount,
                CreatedAt = db.CreatedAt
            };
        }

        public static AutomationLogDb ToDb(this AutomationLog model)
        {
            return new AutomationLogDb
            {
                Id = model.Id,
                LogId = model.LogId,
                TriggerType = model.TriggerType.ToString(),
                RuleId = model.RuleId,
                RuleName = model.RuleName,
                Title = model.Title,
                Message = model.Message,
                CourseId = model.CourseId,
                CourseName = model.CourseName,
                CourseCode = model.CourseCode,
                StudentId = model.StudentId,
                StudentName = model.StudentName,
                TeacherId = model.TeacherId,
                TeacherName = model.TeacherName,
                Status = model.Status,
                TriggeredAt = model.TriggeredAt,
                TriggeredBy = model.TriggeredBy,
                TargetUserId = model.TargetUserId,
                TargetType = model.TargetType,
                ActionTaken = model.ActionTaken,
                IsSuccessful = model.IsSuccessful,
                ResultData = model.ResultData,
                SentAt = model.SentAt,
                DeliveredAt = model.DeliveredAt,
                TriggerReason = model.TriggerReason,
                ErrorMessage = model.ErrorMessage,
                RetryCount = model.RetryCount,
                RelatedEntityId = model.RelatedEntityId,
                RelatedEntityType = model.RelatedEntityType,
                Metadata = model.Metadata ?? new Dictionary<string, object>(),
                RecipientCount = model.RecipientCount,
                CreatedAt = model.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };        }

        // AutomationRule converters
        public static AutomationRule ToAutomationRule(this AutomationRuleDb db)
        {
            return new AutomationRule
            {
                Id = db.RuleId,
                Name = db.Name,
                Description = db.Description ?? string.Empty,
                Trigger = Enum.TryParse<AutomationTrigger>(db.TriggerType, true, out var trigger) ? trigger : AutomationTrigger.AttendanceBelowThreshold,
                TriggerCondition = db.TriggerCondition ?? string.Empty,
                MessageTemplate = db.MessageTemplate,
                NotificationType = Enum.TryParse<NotificationType>(db.NotificationType, true, out var notifType) ? notifType : NotificationType.Info,
                Priority = Enum.TryParse<NotificationPriority>(db.Priority, true, out var priority) ? priority : NotificationPriority.Normal,
                IsActive = db.IsActive,
                TargetRole = db.TargetRole,
                DelayBeforeTrigger = db.DelayBeforeTrigger,
                TriggerCount = db.TriggerCount,
                LastTriggered = db.LastTriggered,
                CreatedBy = db.CreatedBy ?? "system",
                CreatedAt = db.CreatedAt
            };
        }

        public static AutomationRuleDb ToDb(this AutomationRule model)
        {
            return new AutomationRuleDb
            {
                Id = Guid.NewGuid().ToString(),
                RuleId = model.Id,
                Name = model.Name,
                Description = model.Description,
                TriggerType = model.Trigger.ToString(),
                TriggerCondition = model.TriggerCondition,
                MessageTemplate = model.MessageTemplate,
                NotificationType = model.NotificationType.ToString(),
                Priority = model.Priority.ToString().ToLower(),
                IsActive = model.IsActive,
                TargetRole = model.TargetRole,
                DelayBeforeTrigger = model.DelayBeforeTrigger,
                TriggerCount = model.TriggerCount,
                LastTriggered = model.LastTriggered,
                CreatedBy = model.CreatedBy,
                CreatedAt = model.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // Assignment converters
        public static Assignment ToAssignment(this AssignmentDb db)
        {
            return new Assignment
            {
                Id = db.Id,
                CourseId = db.CourseId,
                Title = db.Title,
                Description = db.Description,
                DueDate = db.DueDate,
                MaxScore = db.MaxScore,
                CreatedAt = db.CreatedAt
            };
        }

        public static AssignmentDb ToDb(this Assignment model)
        {
            return new AssignmentDb
            {
                Id = model.Id,
                CourseId = model.CourseId,
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate,
                MaxScore = model.MaxScore,
                CreatedAt = model.CreatedAt
            };
        }

        // Notification converters
        public static NotificationModel ToNotification(this NotificationDb db)
        {
            return new NotificationModel
            {
                Id = db.Id,
                UserId = db.UserId,
                Title = db.Title,
                Message = db.Message,
                CreatedAt = db.CreatedAt,
                IsRead = db.IsRead,
                RelatedId = db.RelatedId,
                ActionUrl = db.ActionUrl,
                IconClass = db.IconClass,
                BadgeClass = db.BadgeClass,
                CourseId = db.CourseId,
                ReadAt = db.ReadAt,
                IsSystemGenerated = db.IsSystemGenerated,
                GeneratedBy = db.GeneratedBy,
                ScheduledFor = db.ScheduledFor,
                IsSent = db.IsSent,
                SentAt = db.SentAt,
                ExpiresAt = db.ExpiresAt
            };
        }

        public static NotificationDb ToDb(this NotificationModel model)
        {
            return new NotificationDb
            {
                Id = model.Id,
                UserId = model.UserId,
                Title = model.Title,
                Message = model.Message,
                Type = model.Type.ToString().ToLower(),
                Category = model.Category.ToString().ToLower(),
                Priority = model.Priority.ToString().ToLower(),
                RelatedId = model.RelatedId,
                ActionUrl = model.ActionUrl,
                IconClass = model.IconClass,
                BadgeClass = model.BadgeClass,
                CourseId = model.CourseId,
                IsRead = model.IsRead,
                ReadAt = model.ReadAt,
                IsSystemGenerated = model.IsSystemGenerated,
                GeneratedBy = model.GeneratedBy,
                ScheduledFor = model.ScheduledFor,
                IsSent = model.IsSent,
                SentAt = model.SentAt,
                ExpiresAt = model.ExpiresAt,                CreatedAt = model.CreatedAt
            };
        }        // Department converters
        public static Department ToDepartment(this DepartmentDb db)
        {
            return new Department
            {
                Id = db.Id,
                Name = db.Name,
                Code = db.Code,
                Description = db.Description,
                CreatedAt = db.CreatedAt,
                IsActive = db.IsActive
            };
        }

        public static DepartmentDb ToDb(this Department model)
        {
            return new DepartmentDb
            {
                Id = model.Id,
                Name = model.Name,
                Code = model.Code,
                Description = model.Description,
                CreatedAt = model.CreatedAt,
                IsActive = model.IsActive
            };
        }

        // Course converters
        public static Course ToCourse(this CourseDb db)
        {
            return new Course
            {
                Id = db.Id,
                Name = db.Name,
                Code = db.Code,
                DepartmentId = db.DepartmentId,
                TeacherId = db.TeacherId,
                Description = db.Description,
                Semester = db.Semester,
                CreditHours = db.CreditHours,
                CreatedAt = db.CreatedAt,
                IsActive = db.IsActive
            };
        }

        public static CourseDb ToDb(this Course model)
        {
            return new CourseDb
            {
                Id = model.Id,
                Name = model.Name,
                Code = model.Code,
                DepartmentId = model.DepartmentId,
                TeacherId = model.TeacherId,
                Description = model.Description,
                Semester = model.Semester,
                CreditHours = model.CreditHours,
                CreatedAt = model.CreatedAt,
                IsActive = model.IsActive
            };        }

        // SystemSetting converters
        public static SystemSetting ToSystemSetting(this SystemSettingDb db)
        {
            return new SystemSetting
            {
                Id = db.Id,
                Category = db.Category,
                Key = db.Key,
                Value = db.Value,
                ValueType = Enum.TryParse<SettingValueType>(db.ValueType, true, out var valueType) ? valueType : SettingValueType.String,
                Description = db.Description,
                IsEncrypted = db.IsEncrypted,
                IsPublic = db.IsPublic,
                CreatedAt = db.CreatedAt,
                UpdatedAt = db.UpdatedAt,
                CreatedBy = db.CreatedBy,
                UpdatedBy = db.UpdatedBy
            };
        }

        public static SystemSettingDb ToDb(this SystemSetting model)
        {
            return new SystemSettingDb
            {
                Id = model.Id,
                Category = model.Category,
                Key = model.Key,
                Value = model.Value,
                ValueType = model.ValueType.ToString().ToLower(),
                Description = model.Description,
                IsEncrypted = model.IsEncrypted,
                IsPublic = model.IsPublic,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
                CreatedBy = model.CreatedBy,
                UpdatedBy = model.UpdatedBy
            };
        }

        // UserPreference converters
        public static UserPreference ToUserPreference(this UserPreferenceDb db)
        {
            return new UserPreference
            {
                Id = db.Id,
                UserId = db.UserId,
                Category = db.Category,
                Key = db.Key,
                Value = db.Value,
                ValueType = Enum.TryParse<SettingValueType>(db.ValueType, true, out var valueType) ? valueType : SettingValueType.String,
                CreatedAt = db.CreatedAt,
                UpdatedAt = db.UpdatedAt
            };
        }

        public static UserPreferenceDb ToDb(this UserPreference model)
        {
            return new UserPreferenceDb
            {
                Id = model.Id,
                UserId = model.UserId,
                Category = model.Category,
                Key = model.Key,
                Value = model.Value,
                ValueType = model.ValueType.ToString().ToLower(),
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt
            };
        }

        // AuditLog converters
        public static AuditLog ToAuditLog(this AuditLogDb db)
        {
            return new AuditLog
            {
                Id = db.Id,
                UserId = db.UserId,
                Action = db.Action,
                EntityType = db.EntityType ?? string.Empty,
                EntityId = db.EntityId,
                OldValues = db.OldValues,
                NewValues = db.NewValues,
                IpAddress = db.IpAddress,
                UserAgent = db.UserAgent,
                SessionId = db.SessionId,
                CreatedAt = db.CreatedAt
            };
        }

        public static AuditLogDb ToDb(this AuditLog model)
        {
            return new AuditLogDb
            {
                Id = model.Id,
                UserId = model.UserId,
                Action = model.Action,
                EntityType = model.EntityType,
                EntityId = model.EntityId,
                OldValues = model.OldValues,
                NewValues = model.NewValues,
                IpAddress = model.IpAddress,
                UserAgent = model.UserAgent,
                SessionId = model.SessionId,
                CreatedAt = model.CreatedAt
            };
        }

        // SystemReport converters
        public static SystemReport ToSystemReport(this SystemReportDb db)
        {
            return new SystemReport
            {
                Id = db.Id,
                Name = db.Name,
                Description = db.Description,
                ReportType = Enum.TryParse<ReportType>(db.ReportType, true, out var reportType) ? reportType : ReportType.Dashboard,
                Category = Enum.TryParse<ReportCategory>(db.Category, true, out var category) ? category : ReportCategory.Academic,
                QueryTemplate = db.QueryTemplate ?? string.Empty,
                Parameters = db.Parameters,
                AccessRoles = db.AccessRoles?.ToList() ?? new List<string>(),
                IsActive = db.IsActive,
                CreatedAt = db.CreatedAt,
                UpdatedAt = db.UpdatedAt,
                CreatedBy = db.CreatedBy,
                UpdatedBy = db.UpdatedBy
            };
        }

        public static SystemReportDb ToDb(this SystemReport model)
        {
            return new SystemReportDb
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                ReportType = model.ReportType.ToString().ToLower(),
                Category = model.Category.ToString().ToLower(),
                QueryTemplate = model.QueryTemplate,
                Parameters = model.Parameters,
                AccessRoles = model.AccessRoles?.ToArray() ?? Array.Empty<string>(),
                IsActive = model.IsActive,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
                CreatedBy = model.CreatedBy,
                UpdatedBy = model.UpdatedBy
            };
        }

        // ReportSchedule converters
        public static ReportSchedule ToReportSchedule(this ReportScheduleDb db)
        {
            return new ReportSchedule
            {
                Id = db.Id,
                ReportId = db.ReportId,
                Name = db.Name,
                ScheduleType = Enum.TryParse<ScheduleType>(db.ScheduleType, true, out var scheduleType) ? scheduleType : ScheduleType.Daily,
                CronExpression = db.CronExpression,
                Recipients = db.Recipients?.ToList() ?? new List<string>(),
                Format = Enum.TryParse<ReportFormat>(db.Format, true, out var format) ? format : ReportFormat.Pdf,
                Parameters = db.Parameters,
                IsActive = db.IsActive,
                LastRunAt = db.LastRunAt,
                NextRunAt = db.NextRunAt,
                CreatedAt = db.CreatedAt,
                UpdatedAt = db.UpdatedAt,
                CreatedBy = db.CreatedBy
            };
        }

        public static ReportScheduleDb ToDb(this ReportSchedule model)
        {
            return new ReportScheduleDb
            {
                Id = model.Id,
                ReportId = model.ReportId,
                Name = model.Name,
                ScheduleType = model.ScheduleType.ToString().ToLower(),
                CronExpression = model.CronExpression,
                Recipients = model.Recipients?.ToArray() ?? Array.Empty<string>(),
                Format = model.Format.ToString().ToLower(),
                Parameters = model.Parameters,
                IsActive = model.IsActive,
                LastRunAt = model.LastRunAt,
                NextRunAt = model.NextRunAt,
                CreatedAt = model.CreatedAt,                UpdatedAt = model.UpdatedAt,
                CreatedBy = model.CreatedBy
            };
        }
    }
}
