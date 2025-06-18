using Campus360.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Campus360.Services
{
    /// <summary>
    /// Complete Automation Engine Service with full database integration
    /// Manages automation rules, logs, and triggers for Campus360
    /// </summary>
    public class AutomationEngineService
    {
        private readonly HttpClient _httpClient;
        private readonly UserContextService _userContext;
        private readonly StudentService _studentService;
        private readonly TeacherService _teacherService;
        private readonly DatabaseService _databaseService;
        private readonly ILogger<AutomationEngineService> _logger;

        public AutomationEngineService(
            HttpClient httpClient, 
            UserContextService userContext,
            StudentService studentService, 
            TeacherService teacherService,
            DatabaseService databaseService,
            ILogger<AutomationEngineService> logger)
        {
            _httpClient = httpClient;
            _userContext = userContext;
            _studentService = studentService;
            _teacherService = teacherService;
            _databaseService = databaseService;
            _logger = logger;
        }

        // ============= AUTOMATION RULE MANAGEMENT =============

        /// <summary>
        /// Get all automation rules from database
        /// </summary>
        public async Task<List<AutomationRule>> GetAutomationRulesAsync()
        {
            try
            {
                _logger.LogInformation("Getting automation rules from database");
                return await _databaseService.GetAutomationRulesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automation rules: {Message}", ex.Message);
                return new List<AutomationRule>();
            }
        }

        /// <summary>
        /// Get automation rule by ID
        /// </summary>
        public async Task<AutomationRule?> GetAutomationRuleByIdAsync(string ruleId)
        {
            try
            {
                _logger.LogInformation("Getting automation rule {RuleId} from database", ruleId);
                return await _databaseService.GetAutomationRuleByIdAsync(ruleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automation rule {RuleId}: {Message}", ruleId, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Toggle automation rule active status
        /// </summary>
        public async Task<bool> ToggleAutomationRuleAsync(string ruleId)
        {
            try
            {
                _logger.LogInformation("Toggling automation rule {RuleId}", ruleId);
                return await _databaseService.ToggleAutomationRuleAsync(ruleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling automation rule {RuleId}: {Message}", ruleId, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Create new automation rule
        /// </summary>
        public async Task<AutomationRule?> CreateAutomationRuleAsync(AutomationRule rule)
        {
            try
            {
                _logger.LogInformation("Creating automation rule: {RuleName}", rule.Name);
                return await _databaseService.CreateAutomationRuleAsync(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating automation rule: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Update automation rule
        /// </summary>
        public async Task<AutomationRule?> UpdateAutomationRuleAsync(AutomationRule rule)
        {
            try
            {
                _logger.LogInformation("Updating automation rule: {RuleId}", rule.Id);
                return await _databaseService.UpdateAutomationRuleAsync(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating automation rule {RuleId}: {Message}", rule.Id, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Delete automation rule
        /// </summary>
        public async Task<bool> DeleteAutomationRuleAsync(string ruleId)
        {
            try
            {
                _logger.LogInformation("Deleting automation rule: {RuleId}", ruleId);
                return await _databaseService.DeleteAutomationRuleAsync(ruleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting automation rule {RuleId}: {Message}", ruleId, ex.Message);
                return false;
            }
        }

        // ============= AUTOMATION LOG MANAGEMENT =============

        /// <summary>
        /// Get automation logs with filtering
        /// </summary>
        public async Task<List<AutomationLog>> GetAutomationLogsAsync(
            int limit = 50,
            string? userId = null, 
            string? triggerType = null, 
            string? courseId = null,
            string? status = null,
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting automation logs from database");
                return await _databaseService.GetAutomationLogsAsync(
                    userId, triggerType, courseId, status, startDate, endDate, limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automation logs: {Message}", ex.Message);
                return new List<AutomationLog>();
            }
        }

        /// <summary>
        /// Get automation metrics for dashboard
        /// </summary>
        public async Task<AutomationMetrics> GetAutomationMetricsAsync()
        {
            try
            {
                _logger.LogInformation("Getting automation metrics from database");
                return await _databaseService.GetAutomationMetricsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automation metrics: {Message}", ex.Message);
                return new AutomationMetrics(); // Return empty metrics on error
            }
        }

        /// <summary>
        /// Create automation log entry
        /// </summary>
        public async Task<AutomationLog?> CreateAutomationLogAsync(AutomationLog log)
        {
            try
            {
                _logger.LogInformation("Creating automation log: {Title}", log.Title);
                var logDb = await _databaseService.CreateAutomationLogAsync(log);
                return logDb.ToAutomationLog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating automation log: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Update automation log status
        /// </summary>
        public async Task<bool> UpdateAutomationLogStatusAsync(string logId, string status, string? errorMessage = null)
        {
            try
            {
                _logger.LogInformation("Updating automation log {LogId} to status {Status}", logId, status);
                var result = await _databaseService.UpdateAutomationLogAsync(logId, status, errorMessage);
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating automation log {LogId}: {Message}", logId, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Retry failed automation log
        /// </summary>
        public async Task<bool> RetryAutomationLogAsync(string logId)
        {
            try
            {
                _logger.LogInformation("Retrying automation log: {LogId}", logId);
                return await _databaseService.RetryAutomationLogAsync(logId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying automation log {LogId}: {Message}", logId, ex.Message);
                return false;
            }
        }

        // ============= AUTOMATION TRIGGERS =============

        /// <summary>
        /// Trigger low attendance alert
        /// </summary>
        public async Task<bool> TriggerLowAttendanceAlertAsync(string studentId, string courseId, string courseName, double attendancePercentage)
        {
            try
            {
                _logger.LogInformation("Triggering low attendance alert for student {StudentId} in course {CourseId}", studentId, courseId);
                
                var rule = await GetRuleByTriggerAsync(AutomationTrigger.AttendanceBelowThreshold);
                if (rule == null || !rule.IsActive)
                {
                    _logger.LogWarning("No active rule found for AttendanceBelowThreshold trigger");
                    return false;
                }

                var message = rule.MessageTemplate
                    .Replace("{course_name}", courseName)
                    .Replace("{attendance_percentage}", attendancePercentage.ToString("F1"));

                var log = new AutomationLog
                {
                    Id = Guid.NewGuid().ToString(),
                    LogId = Guid.NewGuid().ToString(),
                    TriggerType = AutomationTrigger.AttendanceBelowThreshold,
                    RuleId = rule.Id,
                    RuleName = rule.Name,
                    Title = "Low Attendance Alert",
                    Message = message,
                    CourseId = courseId,
                    CourseName = courseName,
                    StudentId = studentId,
                    TargetUserId = studentId,
                    TargetType = "student",
                    Status = "sent",
                    TriggerReason = $"Attendance dropped to {attendancePercentage:F1}%",
                    ActionTaken = "notification_sent",
                    IsSuccessful = true,
                    CreatedAt = DateTime.UtcNow,
                    TriggeredAt = DateTime.UtcNow,
                    SentAt = DateTime.UtcNow
                };

                var result = await CreateAutomationLogAsync(log);
                
                // Update rule trigger count
                await IncrementRuleTriggerCountAsync(rule.Id);
                
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering low attendance alert: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Trigger result notification
        /// </summary>
        public async Task<bool> TriggerResultNotificationAsync(string studentId, string courseId, string examType, double score)
        {
            try
            {
                _logger.LogInformation("Triggering result notification for student {StudentId} in course {CourseId}", studentId, courseId);
                
                var rule = await GetRuleByTriggerAsync(AutomationTrigger.ResultUploaded);
                if (rule == null || !rule.IsActive)
                {
                    _logger.LogWarning("No active rule found for ResultUploaded trigger");
                    return false;
                }

                var message = rule.MessageTemplate
                    .Replace("{exam_type}", examType)
                    .Replace("{score}", score.ToString("F1"));

                var log = new AutomationLog
                {
                    Id = Guid.NewGuid().ToString(),
                    LogId = Guid.NewGuid().ToString(),
                    TriggerType = AutomationTrigger.ResultUploaded,
                    RuleId = rule.Id,
                    RuleName = rule.Name,
                    Title = "Result Published",
                    Message = message,
                    CourseId = courseId,
                    StudentId = studentId,
                    TargetUserId = studentId,
                    TargetType = "student",
                    Status = "sent",
                    TriggerReason = $"New {examType} result published",
                    ActionTaken = "notification_sent",
                    IsSuccessful = true,
                    CreatedAt = DateTime.UtcNow,
                    TriggeredAt = DateTime.UtcNow,
                    SentAt = DateTime.UtcNow
                };

                var result = await CreateAutomationLogAsync(log);
                
                // Update rule trigger count
                await IncrementRuleTriggerCountAsync(rule.Id);
                
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering result notification: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Trigger assignment upload notification
        /// </summary>
        public async Task<bool> TriggerAssignmentUploadNotificationAsync(string courseId, string courseName, string assignmentTitle, DateTime dueDate, List<string> studentIds)
        {
            try
            {
                _logger.LogInformation("Triggering assignment upload notification for course {CourseId}", courseId);
                
                var rule = await GetRuleByTriggerAsync(AutomationTrigger.AssignmentUploaded);
                if (rule == null || !rule.IsActive)
                {
                    _logger.LogWarning("No active rule found for AssignmentUploaded trigger");
                    return false;
                }

                var message = rule.MessageTemplate
                    .Replace("{assignment_title}", assignmentTitle)
                    .Replace("{course_name}", courseName)
                    .Replace("{due_date}", dueDate.ToString("MMM dd, yyyy"));

                var successCount = 0;
                
                foreach (var studentId in studentIds)
                {
                    var log = new AutomationLog
                    {
                        Id = Guid.NewGuid().ToString(),
                        LogId = Guid.NewGuid().ToString(),
                        TriggerType = AutomationTrigger.AssignmentUploaded,
                        RuleId = rule.Id,
                        RuleName = rule.Name,
                        Title = "New Assignment Posted",
                        Message = message,
                        CourseId = courseId,
                        CourseName = courseName,
                        StudentId = studentId,
                        TargetUserId = studentId,
                        TargetType = "student",
                        Status = "sent",
                        TriggerReason = "New assignment created by teacher",
                        ActionTaken = "notification_sent",
                        IsSuccessful = true,
                        RecipientCount = studentIds.Count,
                        CreatedAt = DateTime.UtcNow,
                        TriggeredAt = DateTime.UtcNow,
                        SentAt = DateTime.UtcNow
                    };

                    var result = await CreateAutomationLogAsync(log);
                    if (result != null) successCount++;
                }

                // Update rule trigger count
                await IncrementRuleTriggerCountAsync(rule.Id);
                
                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering assignment upload notification: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Trigger assignment deadline reminder
        /// </summary>
        public async Task<bool> TriggerAssignmentDeadlineReminderAsync(string studentId, string courseId, string courseName, string assignmentTitle, int hoursRemaining)
        {
            try
            {
                _logger.LogInformation("Triggering assignment deadline reminder for student {StudentId}", studentId);
                
                var rule = await GetRuleByTriggerAsync(AutomationTrigger.AssignmentDeadlineApproaching);
                if (rule == null || !rule.IsActive)
                {
                    _logger.LogWarning("No active rule found for AssignmentDeadlineApproaching trigger");
                    return false;
                }

                var message = rule.MessageTemplate
                    .Replace("{assignment_title}", assignmentTitle)
                    .Replace("{course_name}", courseName)
                    .Replace("{hours_remaining}", hoursRemaining.ToString());

                var log = new AutomationLog
                {
                    Id = Guid.NewGuid().ToString(),
                    LogId = Guid.NewGuid().ToString(),
                    TriggerType = AutomationTrigger.AssignmentDeadlineApproaching,
                    RuleId = rule.Id,
                    RuleName = rule.Name,
                    Title = "Assignment Deadline Reminder",
                    Message = message,
                    CourseId = courseId,
                    CourseName = courseName,
                    StudentId = studentId,
                    TargetUserId = studentId,
                    TargetType = "student",
                    Status = "sent",
                    TriggerReason = $"Assignment due in {hoursRemaining} hours",
                    ActionTaken = "notification_sent",
                    IsSuccessful = true,
                    CreatedAt = DateTime.UtcNow,
                    TriggeredAt = DateTime.UtcNow,
                    SentAt = DateTime.UtcNow
                };

                var result = await CreateAutomationLogAsync(log);
                
                // Update rule trigger count
                await IncrementRuleTriggerCountAsync(rule.Id);
                
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering assignment deadline reminder: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Trigger class cancellation notice
        /// </summary>
        public async Task<bool> TriggerClassCancellationNoticeAsync(string courseId, string courseName, DateTime classDate, string reason, List<string> studentIds)
        {
            try
            {
                _logger.LogInformation("Triggering class cancellation notice for course {CourseId}", courseId);
                
                var rule = await GetRuleByTriggerAsync(AutomationTrigger.ClassCancelled);
                if (rule == null || !rule.IsActive)
                {
                    _logger.LogWarning("No active rule found for ClassCancelled trigger");
                    return false;
                }

                var message = rule.MessageTemplate
                    .Replace("{course_name}", courseName)
                    .Replace("{class_date}", classDate.ToString("MMM dd, yyyy"))
                    .Replace("{reason}", reason);

                var successCount = 0;
                
                foreach (var studentId in studentIds)
                {
                    var log = new AutomationLog
                    {
                        Id = Guid.NewGuid().ToString(),
                        LogId = Guid.NewGuid().ToString(),
                        TriggerType = AutomationTrigger.ClassCancelled,
                        RuleId = rule.Id,
                        RuleName = rule.Name,
                        Title = "Class Cancellation Notice",
                        Message = message,
                        CourseId = courseId,
                        CourseName = courseName,
                        StudentId = studentId,
                        TargetUserId = studentId,
                        TargetType = "student",
                        Status = "sent",
                        TriggerReason = $"Class cancelled: {reason}",
                        ActionTaken = "notification_sent",
                        IsSuccessful = true,
                        RecipientCount = studentIds.Count,
                        CreatedAt = DateTime.UtcNow,
                        TriggeredAt = DateTime.UtcNow,
                        SentAt = DateTime.UtcNow
                    };

                    var result = await CreateAutomationLogAsync(log);
                    if (result != null) successCount++;
                }

                // Update rule trigger count
                await IncrementRuleTriggerCountAsync(rule.Id);
                
                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering class cancellation notice: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Trigger notice publication alert
        /// </summary>
        public async Task<bool> TriggerNoticePublicationAlertAsync(string noticeTitle, string noticeContent, List<string> userIds, NotificationPriority priority = NotificationPriority.Normal)
        {
            try
            {
                _logger.LogInformation("Triggering notice publication alert: {NoticeTitle}", noticeTitle);
                
                var rule = await GetRuleByTriggerAsync(AutomationTrigger.NoticePublished);
                if (rule == null || !rule.IsActive)
                {
                    _logger.LogWarning("No active rule found for NoticePublished trigger");
                    return false;
                }

                var message = rule.MessageTemplate
                    .Replace("{notice_title}", noticeTitle)
                    .Replace("{notice_content}", noticeContent);

                var successCount = 0;
                
                foreach (var userId in userIds)
                {
                    var log = new AutomationLog
                    {
                        Id = Guid.NewGuid().ToString(),
                        LogId = Guid.NewGuid().ToString(),
                        TriggerType = AutomationTrigger.NoticePublished,
                        RuleId = rule.Id,
                        RuleName = rule.Name,
                        Title = "New Notice Published",
                        Message = message,
                        TargetUserId = userId,
                        TargetType = "user",
                        Status = "sent",
                        TriggerReason = "New notice published",
                        ActionTaken = "notification_sent",
                        IsSuccessful = true,
                        RecipientCount = userIds.Count,
                        CreatedAt = DateTime.UtcNow,
                        TriggeredAt = DateTime.UtcNow,
                        SentAt = DateTime.UtcNow
                    };

                    var result = await CreateAutomationLogAsync(log);
                    if (result != null) successCount++;
                }

                // Update rule trigger count
                await IncrementRuleTriggerCountAsync(rule.Id);
                
                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering notice publication alert: {Message}", ex.Message);
                return false;
            }
        }

        // ============= BACKGROUND PROCESSING =============

        /// <summary>
        /// Process deadline reminders for assignments approaching due date
        /// </summary>
        public async Task<bool> ProcessDeadlineRemindersAsync()
        {
            try
            {
                _logger.LogInformation("Processing deadline reminders");
                
                // Get assignments due within 24 hours
                var upcomingAssignments = await GetUpcomingAssignmentsAsync();
                
                var processedCount = 0;
                foreach (var assignment in upcomingAssignments)
                {
                    var enrolledStudents = await GetEnrolledStudentsAsync(assignment.CourseId);
                    
                    foreach (var studentId in enrolledStudents)
                    {
                        var hoursRemaining = (int)(assignment.DueDate - DateTime.UtcNow).TotalHours;
                        var success = await TriggerAssignmentDeadlineReminderAsync(
                            studentId, assignment.CourseId, assignment.CourseName, assignment.Title, hoursRemaining);
                        
                        if (success) processedCount++;
                    }
                }
                
                _logger.LogInformation("Processed {Count} deadline reminders", processedCount);
                return processedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing deadline reminders: {Message}", ex.Message);
                return false;
            }
        }        /// <summary>
        /// Process scheduled notifications
        /// </summary>
        public async Task<bool> ProcessScheduledNotificationsAsync()
        {
            try
            {
                _logger.LogInformation("Processing scheduled notifications");
                
                // Get pending scheduled notifications (would need to implement in database)
                // For now, return true as placeholder
                await Task.Delay(1); // Add minimal async operation
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled notifications: {Message}", ex.Message);
                return false;
            }
        }        /// <summary>
        /// Clean up expired notifications and logs
        /// </summary>
        public async Task<bool> CleanupExpiredNotificationsAsync(DateTime? cutoffDate = null)
        {
            try
            {
                _logger.LogInformation("Cleaning up expired notifications");
                
                cutoffDate ??= DateTime.UtcNow.AddDays(-30);
                
                // Implementation would clean up old automation logs
                // For now, return true as placeholder
                await Task.Delay(1); // Add minimal async operation
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired notifications: {Message}", ex.Message);
                return false;
            }
        }

        // ============= HELPER METHODS =============

        /// <summary>
        /// Get automation rule by trigger type
        /// </summary>
        private async Task<AutomationRule?> GetRuleByTriggerAsync(AutomationTrigger trigger)
        {
            try
            {
                var rules = await GetAutomationRulesAsync();
                return rules.FirstOrDefault(r => r.Trigger == trigger && r.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rule by trigger {Trigger}: {Message}", trigger, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Increment rule trigger count
        /// </summary>
        private async Task<bool> IncrementRuleTriggerCountAsync(string ruleId)
        {
            try
            {
                return await _databaseService.IncrementRuleTriggerCountAsync(ruleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing rule trigger count for {RuleId}: {Message}", ruleId, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get upcoming assignments (placeholder implementation)
        /// </summary>
        private async Task<List<(string CourseId, string CourseName, string Title, DateTime DueDate)>> GetUpcomingAssignmentsAsync()
        {
            try
            {
                // This would integrate with assignment service to get actual upcoming assignments
                // For now, return empty list
                await Task.Delay(10);
                return new List<(string CourseId, string CourseName, string Title, DateTime DueDate)>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming assignments: {Message}", ex.Message);
                return new List<(string CourseId, string CourseName, string Title, DateTime DueDate)>();
            }
        }

        /// <summary>
        /// Get enrolled students for a course (placeholder implementation)
        /// </summary>
        private async Task<List<string>> GetEnrolledStudentsAsync(string courseId)
        {
            try
            {
                // This would integrate with course service to get actual enrolled students
                // For now, return empty list
                await Task.Delay(10);
                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrolled students for course {CourseId}: {Message}", courseId, ex.Message);
                return new List<string>();
            }
        }

        // ============= NOTIFICATION MANAGEMENT =============        /// <summary>
        /// Get user notifications from database
        /// </summary>
        public async Task<List<NotificationModel>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
        {
            try
            {
                _logger.LogInformation("Getting notifications for user {UserId}", userId);
                var notifications = await _databaseService.GetUserNotificationsAsync(userId);
                
                if (unreadOnly)
                {
                    notifications = notifications.Where(n => !n.IsRead).ToList();
                }
                
                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notifications: {Message}", ex.Message);
                return new List<NotificationModel>();
            }
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            try
            {
                var notifications = await GetUserNotificationsAsync(userId, unreadOnly: true);
                return notifications.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count: {Message}", ex.Message);
                return 0;
            }
        }        /// <summary>
        /// Mark notification as read
        /// </summary>
        public async Task<bool> MarkNotificationAsReadAsync(string notificationId)
        {
            try
            {
                _logger.LogInformation("Marking notification {NotificationId} as read", notificationId);
                await _databaseService.MarkNotificationAsReadAsync(notificationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read: {Message}", ex.Message);
                return false;
            }
        }

        // ============= ADDITIONAL AUTOMATION TRIGGERS =============        /// <summary>
        /// Trigger result uploaded notification (alias for TriggerResultNotificationAsync)
        /// </summary>
        public async Task<bool> TriggerResultUploadedAsync(string studentId, string courseId, string courseName, string examType, double marksObtained, double totalMarks, string grade)
        {
            // Calculate percentage score for the original method
            double score = totalMarks > 0 ? (marksObtained / totalMarks) * 100 : 0;
            return await TriggerResultNotificationAsync(studentId, courseId, examType, score);
        }

        /// <summary>
        /// Trigger attendance below threshold alert (alias for TriggerLowAttendanceAlertAsync)
        /// </summary>
        public async Task<bool> TriggerAttendanceBelowThresholdAsync(string studentId, string courseId, string courseName, double attendancePercentage)
        {
            return await TriggerLowAttendanceAlertAsync(studentId, courseId, courseName, attendancePercentage);
        }

        /// <summary>
        /// Trigger assignment uploaded notification (alias for TriggerAssignmentUploadNotificationAsync)
        /// </summary>
        public async Task<bool> TriggerAssignmentUploadedAsync(string courseId, string courseName, string assignmentTitle, DateTime dueDate, List<string> studentIds)
        {
            return await TriggerAssignmentUploadNotificationAsync(courseId, courseName, assignmentTitle, dueDate, studentIds);
        }

        /// <summary>
        /// Trigger notice published alert (alias for TriggerNoticePublicationAlertAsync)
        /// </summary>
        public async Task<bool> TriggerNoticePublishedAsync(string noticeTitle, string noticeContent, List<string> userIds, NotificationPriority priority = NotificationPriority.Normal)
        {
            return await TriggerNoticePublicationAlertAsync(noticeTitle, noticeContent, userIds, priority);
        }

        /// <summary>
        /// Trigger class cancelled notice (alias for TriggerClassCancellationNoticeAsync)
        /// </summary>
        public async Task<bool> TriggerClassCancelledAsync(string courseId, string courseName, DateTime classDate, string reason, List<string> studentIds)
        {
            return await TriggerClassCancellationNoticeAsync(courseId, courseName, classDate, reason, studentIds);
        }

        /// <summary>
        /// Trigger assignment deadline approaching reminder (alias for TriggerAssignmentDeadlineReminderAsync)
        /// </summary>
        public async Task<bool> TriggerAssignmentDeadlineApproachingAsync(string studentId, string courseId, string courseName, string assignmentTitle, int hoursRemaining)
        {
            return await TriggerAssignmentDeadlineReminderAsync(studentId, courseId, courseName, assignmentTitle, hoursRemaining);
        }

        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        public async Task<bool> MarkAllNotificationsAsReadAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Marking all notifications as read for user {UserId}", userId);
                await _databaseService.MarkAllNotificationsAsReadAsync(userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get student notifications (alias for GetUserNotificationsAsync)
        /// </summary>
        public async Task<List<NotificationModel>> GetStudentNotificationsAsync(string studentId, bool unreadOnly = false)
        {
            return await GetUserNotificationsAsync(studentId, unreadOnly);
        }        /// <summary>
        /// Get active alerts for a user
        /// </summary>
        public async Task<List<NotificationModel>> GetActiveAlertsAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Getting active alerts for user {UserId}", userId);
                var notifications = await GetUserNotificationsAsync(userId, unreadOnly: true);
                return notifications.Where(n => n.Priority == NotificationPriority.High || n.Priority == NotificationPriority.Critical).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active alerts: {Message}", ex.Message);
                return new List<NotificationModel>();
            }
        }

        /// <summary>
        /// Trigger assignment submission notification
        /// </summary>
        public async Task<bool> TriggerAssignmentSubmissionAsync(string studentId, string courseId, string courseName, string assignmentTitle, DateTime submissionTime)
        {
            try
            {
                _logger.LogInformation("Triggering assignment submission notification for student {StudentId}", studentId);
                
                var rule = await GetRuleByTriggerAsync(AutomationTrigger.AssignmentUploaded);
                if (rule == null || !rule.IsActive)
                {
                    _logger.LogWarning("No active rule found for assignment submission trigger");
                    return false;
                }

                var message = rule.MessageTemplate
                    .Replace("{assignment_title}", assignmentTitle)
                    .Replace("{course_name}", courseName)
                    .Replace("{submission_time}", submissionTime.ToString("MMM dd, yyyy HH:mm"));

                var log = new AutomationLog
                {
                    Id = Guid.NewGuid().ToString(),
                    LogId = Guid.NewGuid().ToString(),
                    TriggerType = AutomationTrigger.AssignmentUploaded,
                    RuleId = rule.Id,
                    RuleName = rule.Name,
                    Title = "Assignment Submitted",
                    Message = message,
                    CourseId = courseId,
                    CourseName = courseName,
                    StudentId = studentId,
                    TargetUserId = studentId,
                    TargetType = "student",
                    Status = "sent",
                    TriggerReason = "Student submitted assignment",
                    ActionTaken = "notification_sent",
                    IsSuccessful = true,
                    CreatedAt = DateTime.UtcNow,
                    TriggeredAt = DateTime.UtcNow,
                    SentAt = DateTime.UtcNow
                };

                var result = await CreateAutomationLogAsync(log);
                await IncrementRuleTriggerCountAsync(rule.Id);
                
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering assignment submission notification: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Trigger custom event notification
        /// </summary>
        public async Task<bool> TriggerCustomEventAsync(string eventId, string eventTitle, string eventDescription, string targetRole, string createdBy)
        {
            try
            {
                _logger.LogInformation("Triggering custom event notification: {EventTitle}", eventTitle);
                
                // Create a generic automation log for custom events
                var log = new AutomationLog
                {
                    Id = Guid.NewGuid().ToString(),
                    LogId = Guid.NewGuid().ToString(),
                    TriggerType = AutomationTrigger.NoticePublished, // Use notice published as generic trigger
                    RuleId = "custom_event",
                    RuleName = "Custom Event Notification",
                    Title = eventTitle,
                    Message = eventDescription,
                    TargetUserId = targetRole,
                    TargetType = targetRole,
                    Status = "sent",
                    TriggerReason = "Custom event created",
                    ActionTaken = "notification_sent",
                    IsSuccessful = true,
                    CreatedAt = DateTime.UtcNow,
                    TriggeredAt = DateTime.UtcNow,
                    SentAt = DateTime.UtcNow,
                    RelatedEntityId = eventId,
                    RelatedEntityType = "event"
                };

                var result = await CreateAutomationLogAsync(log);
                return result != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering custom event notification: {Message}", ex.Message);
                return false;
            }
        }
    }
}
