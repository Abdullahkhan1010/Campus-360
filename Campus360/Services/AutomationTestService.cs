using Campus360.Models;
using Campus360.Services;
using Microsoft.Extensions.Logging;

namespace Campus360.Services
{
    /// <summary>
    /// Service for testing and demonstrating automation functionality
    /// Use this to trigger sample automation events for testing
    /// </summary>
    public class AutomationTestService
    {
        private readonly AutomationEngineService _automationEngine;
        private readonly ILogger<AutomationTestService> _logger;

        public AutomationTestService(
            AutomationEngineService automationEngine,
            ILogger<AutomationTestService> logger)
        {
            _automationEngine = automationEngine;
            _logger = logger;
        }

        /// <summary>
        /// Test low attendance automation trigger
        /// </summary>
        public async Task<bool> TestLowAttendanceAlert()
        {
            try
            {
                _logger.LogInformation("Testing low attendance automation trigger");
                
                return await _automationEngine.TriggerLowAttendanceAlertAsync(
                    studentId: "test-student-1",
                    courseId: "CS101",
                    courseName: "Data Structures",
                    attendancePercentage: 65.5
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing low attendance alert");
                return false;
            }
        }

        /// <summary>
        /// Test assignment upload automation trigger
        /// </summary>
        public async Task<bool> TestAssignmentUploadNotification()
        {
            try
            {
                _logger.LogInformation("Testing assignment upload automation trigger");
                
                var studentIds = new List<string> { "test-student-1", "test-student-2", "test-student-3" };
                
                return await _automationEngine.TriggerAssignmentUploadNotificationAsync(
                    courseId: "CS101",
                    courseName: "Data Structures",
                    assignmentTitle: "Binary Tree Implementation",
                    dueDate: DateTime.UtcNow.AddDays(7),
                    studentIds: studentIds
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing assignment upload notification");
                return false;
            }
        }

        /// <summary>
        /// Test assignment deadline reminder automation trigger
        /// </summary>
        public async Task<bool> TestAssignmentDeadlineReminder()
        {
            try
            {
                _logger.LogInformation("Testing assignment deadline reminder automation trigger");
                
                return await _automationEngine.TriggerAssignmentDeadlineReminderAsync(
                    studentId: "test-student-1",
                    courseId: "CS101",
                    courseName: "Data Structures",
                    assignmentTitle: "Binary Tree Implementation",
                    hoursRemaining: 24
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing assignment deadline reminder");
                return false;
            }
        }

        /// <summary>
        /// Test result upload automation trigger
        /// </summary>
        public async Task<bool> TestResultNotification()
        {
            try
            {
                _logger.LogInformation("Testing result upload automation trigger");
                
                return await _automationEngine.TriggerResultNotificationAsync(
                    studentId: "test-student-1",
                    courseId: "CS101",
                    examType: "Midterm Exam",
                    score: 85.5
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing result notification");
                return false;
            }
        }

        /// <summary>
        /// Test class cancellation automation trigger
        /// </summary>
        public async Task<bool> TestClassCancellationNotice()
        {
            try
            {
                _logger.LogInformation("Testing class cancellation automation trigger");
                
                var studentIds = new List<string> { "test-student-1", "test-student-2", "test-student-3" };
                
                return await _automationEngine.TriggerClassCancellationNoticeAsync(
                    courseId: "CS101",
                    courseName: "Data Structures",
                    classDate: DateTime.UtcNow.AddDays(1),
                    reason: "Faculty meeting scheduled",
                    studentIds: studentIds
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing class cancellation notice");
                return false;
            }
        }

        /// <summary>
        /// Test notice publication automation trigger
        /// </summary>
        public async Task<bool> TestNoticePublicationAlert()
        {
            try
            {
                _logger.LogInformation("Testing notice publication automation trigger");
                
                var userIds = new List<string> { "test-student-1", "test-student-2", "test-teacher-1" };
                
                return await _automationEngine.TriggerNoticePublicationAlertAsync(
                    noticeTitle: "Important: Campus Closure Notice",
                    noticeContent: "Campus will be closed on December 25th for Christmas holiday. Regular classes will resume on December 26th.",
                    userIds: userIds,
                    priority: NotificationPriority.High
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing notice publication alert");
                return false;
            }
        }

        /// <summary>
        /// Run all automation tests to generate sample data
        /// </summary>
        public async Task<AutomationTestResults> RunAllTests()
        {
            var results = new AutomationTestResults();
            
            _logger.LogInformation("Running comprehensive automation test suite");
            
            try
            {
                // Test all automation triggers
                results.LowAttendanceTest = await TestLowAttendanceAlert();
                await Task.Delay(100); // Small delay between tests
                
                results.AssignmentUploadTest = await TestAssignmentUploadNotification();
                await Task.Delay(100);
                
                results.DeadlineReminderTest = await TestAssignmentDeadlineReminder();
                await Task.Delay(100);
                
                results.ResultNotificationTest = await TestResultNotification();
                await Task.Delay(100);
                
                results.ClassCancellationTest = await TestClassCancellationNotice();
                await Task.Delay(100);
                
                results.NoticePublicationTest = await TestNoticePublicationAlert();
                
                results.OverallSuccess = results.LowAttendanceTest && 
                                       results.AssignmentUploadTest && 
                                       results.DeadlineReminderTest && 
                                       results.ResultNotificationTest && 
                                       results.ClassCancellationTest && 
                                       results.NoticePublicationTest;
                
                results.TestCompletedAt = DateTime.UtcNow;
                
                _logger.LogInformation("Automation test suite completed. Success rate: {SuccessCount}/6", 
                    new[] { results.LowAttendanceTest, results.AssignmentUploadTest, results.DeadlineReminderTest, 
                           results.ResultNotificationTest, results.ClassCancellationTest, results.NoticePublicationTest }
                    .Count(r => r));
                
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running automation test suite");
                results.OverallSuccess = false;
                results.ErrorMessage = ex.Message;
                return results;
            }
        }

        /// <summary>
        /// Get automation system health status
        /// </summary>
        public async Task<AutomationSystemHealth> GetSystemHealth()
        {
            try
            {
                var health = new AutomationSystemHealth();
                
                // Check if automation rules exist
                var rules = await _automationEngine.GetAutomationRulesAsync();
                health.RulesAvailable = rules.Count;
                health.ActiveRules = rules.Count(r => r.IsActive);
                
                // Check recent automation logs
                var recentLogs = await _automationEngine.GetAutomationLogsAsync(limit: 10);
                health.RecentLogsCount = recentLogs.Count;
                health.RecentSuccessRate = recentLogs.Any() ? 
                    (double)recentLogs.Count(l => l.IsSuccessful) / recentLogs.Count * 100 : 100;
                
                // Get automation metrics
                var metrics = await _automationEngine.GetAutomationMetricsAsync();
                health.TotalNotificationsSent = metrics.TotalNotificationsSent;
                health.OverallSuccessRate = metrics.SuccessRate;
                
                health.SystemHealthy = health.RulesAvailable > 0 && health.ActiveRules > 0;
                health.CheckedAt = DateTime.UtcNow;
                
                return health;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking automation system health");
                return new AutomationSystemHealth 
                { 
                    SystemHealthy = false, 
                    ErrorMessage = ex.Message,
                    CheckedAt = DateTime.UtcNow
                };
            }
        }
    }

    /// <summary>
    /// Results from automation test suite
    /// </summary>
    public class AutomationTestResults
    {
        public bool LowAttendanceTest { get; set; }
        public bool AssignmentUploadTest { get; set; }
        public bool DeadlineReminderTest { get; set; }
        public bool ResultNotificationTest { get; set; }
        public bool ClassCancellationTest { get; set; }
        public bool NoticePublicationTest { get; set; }
        public bool OverallSuccess { get; set; }
        public DateTime TestCompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Automation system health information
    /// </summary>
    public class AutomationSystemHealth
    {
        public bool SystemHealthy { get; set; }
        public int RulesAvailable { get; set; }
        public int ActiveRules { get; set; }
        public int RecentLogsCount { get; set; }
        public double RecentSuccessRate { get; set; }
        public int TotalNotificationsSent { get; set; }
        public double OverallSuccessRate { get; set; }
        public DateTime CheckedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
