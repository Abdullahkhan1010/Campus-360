using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Campus360.Models;

namespace Campus360.Services
{
    /// <summary>
    /// Timer-based service for Blazor WebAssembly that runs periodic automation checks
    /// </summary>
    public class AutomationTimerService : IDisposable
    {
        private readonly ILogger<AutomationTimerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes
        private bool _isRunning = false;

        public AutomationTimerService(ILogger<AutomationTimerService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Start the automation timer
        /// </summary>
        public void StartAutomationChecks()
        {
            if (_isRunning) return;

            _logger.LogInformation("Automation Timer Service started at: {time}", DateTimeOffset.Now);
            _isRunning = true;

            _timer = new Timer(async _ => await ProcessAutomationChecksAsync(), null, TimeSpan.Zero, _checkInterval);
        }

        /// <summary>
        /// Stop the automation timer
        /// </summary>
        public void StopAutomationChecks()
        {
            if (!_isRunning) return;

            _timer?.Dispose();
            _timer = null;
            _isRunning = false;
            _logger.LogInformation("Automation Timer Service stopped at: {time}", DateTimeOffset.Now);
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            StopAutomationChecks();
            _timer?.Dispose();
        }

        private async Task ProcessAutomationChecksAsync()
        {
            if (!_isRunning) return;

            using var scope = _serviceProvider.CreateScope();
            var automationEngine = scope.ServiceProvider.GetRequiredService<AutomationEngineService>();
            var teacherService = scope.ServiceProvider.GetService<TeacherServiceEnhanced>();
            
            _logger.LogDebug("Running automation checks at: {time}", DateTimeOffset.Now);

            try
            {
                // 1. Check for assignment deadline reminders
                await CheckAssignmentDeadlinesAsync(automationEngine, teacherService);

                // 2. Check for low attendance alerts
                await CheckAttendanceThresholdsAsync(automationEngine);

                // 3. Check for result publication notifications
                await CheckResultPublicationsAsync(automationEngine);

                // 4. Process scheduled notifications
                await ProcessScheduledNotificationsAsync(automationEngine);

                // 5. Clean up expired notifications
                await CleanupExpiredNotificationsAsync(automationEngine);

                _logger.LogDebug("Automation checks completed successfully at: {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during automation checks");
            }
        }

        /// <summary>
        /// Check for assignments approaching deadlines and send reminders
        /// </summary>
        private async Task CheckAssignmentDeadlinesAsync(AutomationEngineService automationEngine, TeacherServiceEnhanced? teacherService)
        {
            try
            {
                _logger.LogDebug("Checking assignment deadlines...");

                // Use TeacherServiceEnhanced if available, otherwise use AutomationEngine directly
                if (teacherService != null)
                {
                    await teacherService.TriggerAssignmentDeadlineRemindersAsync();
                }
                else
                {
                    // Fallback to direct automation engine call
                    await automationEngine.ProcessDeadlineRemindersAsync();
                }

                _logger.LogDebug("Assignment deadline checks completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking assignment deadlines");
            }
        }

        /// <summary>
        /// Check attendance percentages and trigger low attendance alerts
        /// </summary>
        private async Task CheckAttendanceThresholdsAsync(AutomationEngineService automationEngine)
        {
            try
            {
                _logger.LogDebug("Checking attendance thresholds...");

                // Mock student data for checking attendance (in real implementation, this would come from database)
                var studentsToCheck = new List<AttendanceCheckModel>
                {
                    new AttendanceCheckModel { StudentId = "s1", CourseId = "3", AttendancePercentage = 71.4, CourseName = "Database Systems", CourseCode = "CS301" },
                    new AttendanceCheckModel { StudentId = "s2", CourseId = "1", AttendancePercentage = 72.5, CourseName = "Data Structures", CourseCode = "CS101" },
                    new AttendanceCheckModel { StudentId = "s3", CourseId = "2", AttendancePercentage = 68.9, CourseName = "Web Development", CourseCode = "IT201" }
                };

                foreach (var student in studentsToCheck)
                {
                    if (student.AttendancePercentage < 75) // Threshold for low attendance
                    {
                        await automationEngine.TriggerLowAttendanceAlertAsync(
                            student.StudentId,
                            student.CourseId,
                            student.CourseName,
                            student.AttendancePercentage
                        );

                        _logger.LogInformation("Triggered low attendance alert for student {StudentId} in course {CourseId}", 
                            student.StudentId, student.CourseId);
                    }
                }

                _logger.LogDebug("Attendance threshold checks completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking attendance thresholds");
            }
        }

        /// <summary>
        /// Check for published results and send notifications
        /// </summary>
        private async Task CheckResultPublicationsAsync(AutomationEngineService automationEngine)
        {
            try
            {
                _logger.LogDebug("Checking for result publications...");

                // Mock recently published results (in real implementation, this would query for recently published results)
                var recentResults = new List<ResultPublicationModel>
                {
                    new ResultPublicationModel 
                    { 
                        StudentId = "s1", 
                        CourseId = "1", 
                        CourseName = "Data Structures",
                        ExamType = "Midterm",
                        Score = 85.0,
                        PublishedAt = DateTime.Now.AddMinutes(-2)
                    },
                    new ResultPublicationModel 
                    { 
                        StudentId = "s2", 
                        CourseId = "2", 
                        CourseName = "Web Development",
                        ExamType = "Assignment",
                        Score = 92.5,
                        PublishedAt = DateTime.Now.AddMinutes(-5)
                    }
                };

                foreach (var result in recentResults.Where(r => r.PublishedAt > DateTime.Now.AddMinutes(-10)))
                {
                    await automationEngine.TriggerResultNotificationAsync(
                        result.StudentId,
                        result.CourseId,
                        result.ExamType,
                        result.Score
                    );

                    _logger.LogInformation("Triggered result notification for student {StudentId} - {ExamType}: {Score}", 
                        result.StudentId, result.ExamType, result.Score);
                }

                _logger.LogDebug("Result publication checks completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking result publications");
            }
        }

        /// <summary>
        /// Process notifications that are scheduled to be sent at specific times
        /// </summary>
        private async Task ProcessScheduledNotificationsAsync(AutomationEngineService automationEngine)
        {
            try
            {
                _logger.LogDebug("Processing scheduled notifications...");

                // Get pending scheduled notifications
                await automationEngine.ProcessScheduledNotificationsAsync();

                _logger.LogDebug("Scheduled notifications processing completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled notifications");
            }
        }

        /// <summary>
        /// Clean up expired notifications and automation logs
        /// </summary>
        private async Task CleanupExpiredNotificationsAsync(AutomationEngineService automationEngine)
        {
            try
            {
                _logger.LogDebug("Cleaning up expired notifications...");

                // Clean up notifications older than 30 days
                var cutoffDate = DateTime.Now.AddDays(-30);
                await automationEngine.CleanupExpiredNotificationsAsync(cutoffDate);

                _logger.LogDebug("Expired notifications cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired notifications");
            }
        }
    }
}
