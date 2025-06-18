using Campus360.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Data;
using Supabase.Postgrest;
using Postgrest = Supabase.Postgrest;

namespace Campus360.Services
{
    /// <summary>
    /// Reporting Service for generating reports, analytics, and dashboards
    /// </summary>
    public class ReportingService
    {
        private readonly DatabaseService _databaseService;
        private readonly UserContextService _userContextService;
        private readonly ILogger<ReportingService> _logger;

        public ReportingService(
            DatabaseService databaseService,
            UserContextService userContextService,
            ILogger<ReportingService> logger)
        {
            _databaseService = databaseService;
            _userContextService = userContextService;
            _logger = logger;
        }

        // ============= SYSTEM REPORTS MANAGEMENT =============

        /// <summary>
        /// Get all system reports accessible to current user
        /// </summary>
        public async Task<List<SystemReport>> GetSystemReportsAsync()
        {
            try
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                if (currentUser == null) return new List<SystemReport>();

                _logger.LogInformation("Getting system reports for user: {UserId} with role: {Role}", currentUser.Id, currentUser.Role);
                  var response = await _databaseService.Client
                    .From<SystemReportDb>()
                    .Select("*")
                    .Where(r => r.IsActive == true)
                    .Order("category", Postgrest.Constants.Ordering.Ascending)
                    .Get();

                var allReports = response.Models?.Select(db => db.ToSystemReport()).ToList() ?? new List<SystemReport>();
                
                // Filter by user role access
                return allReports.Where(r => r.AccessRoles.Contains(currentUser.Role)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system reports: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get system report by ID
        /// </summary>
        public async Task<SystemReport?> GetSystemReportAsync(string reportId)
        {
            try
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                if (currentUser == null) return null;

                _logger.LogInformation("Getting system report: {ReportId}", reportId);
                
                var response = await _databaseService.Client
                    .From<SystemReportDb>()
                    .Select("*")
                    .Where(r => r.Id == reportId && r.IsActive == true)
                    .Limit(1)
                    .Get();

                var report = response.Models?.FirstOrDefault()?.ToSystemReport();
                
                // Check access permissions
                if (report != null && !report.AccessRoles.Contains(currentUser.Role))
                {
                    throw new UnauthorizedAccessException($"User does not have access to report: {reportId}");
                }

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system report {ReportId}: {Message}", reportId, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Create or update system report
        /// </summary>
        public async Task<SystemReport> SaveSystemReportAsync(SystemReport report)
        {
            try
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                if (currentUser?.Role != "admin")
                {
                    throw new UnauthorizedAccessException("Only administrators can manage system reports");
                }

                _logger.LogInformation("Saving system report: {ReportName}", report.Name);

                if (string.IsNullOrEmpty(report.Id))
                {
                    // Create new report
                    var newModel = report.ToDb();
                    newModel.Id = Guid.NewGuid().ToString();
                    newModel.CreatedAt = DateTime.UtcNow;
                    newModel.UpdatedAt = DateTime.UtcNow;
                    newModel.CreatedBy = currentUser.Id;
                    newModel.UpdatedBy = currentUser.Id;

                    var response = await _databaseService.Client
                        .From<SystemReportDb>()
                        .Insert(newModel);

                    if (response.Models?.Any() == true)
                    {
                        return response.Models.First().ToSystemReport();
                    }
                }
                else
                {
                    // Update existing report
                    var updateModel = report.ToDb();
                    updateModel.UpdatedAt = DateTime.UtcNow;
                    updateModel.UpdatedBy = currentUser.Id;

                    var response = await _databaseService.Client
                        .From<SystemReportDb>()
                        .Where(r => r.Id == report.Id)
                        .Update(updateModel);

                    if (response.Models?.Any() == true)
                    {
                        return response.Models.First().ToSystemReport();
                    }
                }

                throw new InvalidOperationException("Failed to save system report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving system report {ReportName}: {Message}", report.Name, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Delete system report
        /// </summary>
        public async Task<bool> DeleteSystemReportAsync(string reportId)
        {
            try
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                if (currentUser?.Role != "admin")
                {
                    throw new UnauthorizedAccessException("Only administrators can delete system reports");
                }

                _logger.LogInformation("Deleting system report: {ReportId}", reportId);

                await _databaseService.Client
                    .From<SystemReportDb>()
                    .Where(r => r.Id == reportId)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting system report {ReportId}: {Message}", reportId, ex.Message);
                return false;
            }
        }

        // ============= REPORT GENERATION =============

        /// <summary>
        /// Generate report data
        /// </summary>
        public async Task<ReportResult> GenerateReportAsync(ReportGenerationRequest request)
        {
            try
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    throw new UnauthorizedAccessException("User must be logged in to generate reports");
                }

                var report = await GetSystemReportAsync(request.ReportId);
                if (report == null)
                {
                    throw new ArgumentException($"Report not found: {request.ReportId}");
                }

                _logger.LogInformation("Generating report: {ReportName} for user: {UserId}", report.Name, currentUser.Id);

                var startTime = DateTime.UtcNow;
                var result = new ReportResult
                {
                    Id = Guid.NewGuid().ToString(),
                    ReportId = request.ReportId,
                    Status = ReportStatus.Running,
                    GeneratedAt = startTime,
                    GeneratedBy = currentUser.Id,
                    Parameters = request.Parameters
                };

                try
                {
                    // Execute report based on category
                    var data = report.Category switch
                    {
                        ReportCategory.Academic => await GenerateAcademicReportAsync(report, request.Parameters),
                        ReportCategory.Attendance => await GenerateAttendanceReportAsync(report, request.Parameters),
                        ReportCategory.Performance => await GeneratePerformanceReportAsync(report, request.Parameters),
                        ReportCategory.System => await GenerateSystemReportAsync(report, request.Parameters),
                        ReportCategory.User => await GenerateUserReportAsync(report, request.Parameters),
                        _ => throw new NotSupportedException($"Report category not supported: {report.Category}")
                    };

                    result.Data = data;
                    result.Status = ReportStatus.Completed;
                    result.RowCount = GetRowCount(data);
                    result.ExecutionTime = DateTime.UtcNow - startTime;

                    _logger.LogInformation("Report generated successfully: {ReportName}, Rows: {RowCount}, Time: {ExecutionTime}ms", 
                        report.Name, result.RowCount, result.ExecutionTime.TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    result.Status = ReportStatus.Failed;
                    result.ErrorMessage = ex.Message;
                    result.ExecutionTime = DateTime.UtcNow - startTime;
                    
                    _logger.LogError(ex, "Error generating report {ReportName}: {Message}", report.Name, ex.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateReportAsync: {Message}", ex.Message);
                throw;
            }
        }

        // ============= DASHBOARD DATA =============

        /// <summary>
        /// Get dashboard data for current user
        /// </summary>
        public async Task<DashboardData> GetDashboardDataAsync()
        {
            try
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                if (currentUser == null) return new DashboardData();

                _logger.LogInformation("Getting dashboard data for user: {UserId} with role: {Role}", currentUser.Id, currentUser.Role);

                var dashboardData = new DashboardData
                {
                    LastUpdated = DateTime.UtcNow
                };

                // Get role-specific metrics
                dashboardData.GlobalMetrics = currentUser.Role switch
                {
                    "admin" => await GetAdminDashboardMetricsAsync(),
                    "teacher" => await GetTeacherDashboardMetricsAsync(currentUser.Id),
                    "student" => await GetStudentDashboardMetricsAsync(currentUser.Id),
                    _ => new Dictionary<string, object>()
                };

                return dashboardData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data: {Message}", ex.Message);
                return new DashboardData();
            }
        }

        // ============= ANALYTICS =============

        /// <summary>
        /// Get system health metrics
        /// </summary>
        public async Task<SystemHealthStatus> GetSystemHealthAsync()
        {
            try
            {
                _logger.LogInformation("Getting system health status");

                var health = new SystemHealthStatus
                {
                    CheckedAt = DateTime.UtcNow,
                    IsHealthy = true,
                    Issues = new List<string>()
                };

                // Check database connectivity
                try
                {
                    await _databaseService.Client.From<UserProfileDb>().Select("id").Limit(1).Get();
                    health.Metrics["database_status"] = "connected";
                }
                catch (Exception ex)
                {
                    health.IsHealthy = false;
                    health.Issues.Add("Database connectivity issue");
                    health.Metrics["database_status"] = "disconnected";
                    health.Metrics["database_error"] = ex.Message;
                }

                // Get user statistics
                var userStats = await GetUserStatisticsAsync();
                health.Metrics["total_users"] = userStats.GetValueOrDefault("total_users", 0);
                health.Metrics["active_users"] = userStats.GetValueOrDefault("active_users", 0);

                // Get system performance metrics
                health.Metrics["memory_usage"] = GC.GetTotalMemory(false);
                health.Metrics["last_gc_gen0"] = GC.CollectionCount(0);
                health.Metrics["last_gc_gen1"] = GC.CollectionCount(1);
                health.Metrics["last_gc_gen2"] = GC.CollectionCount(2);

                return health;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system health: {Message}", ex.Message);
                return new SystemHealthStatus
                {
                    CheckedAt = DateTime.UtcNow,
                    IsHealthy = false,
                    Issues = new List<string> { $"Health check failed: {ex.Message}" }
                };
            }
        }

        // ============= PRIVATE HELPER METHODS =============

        private async Task<object> GenerateAcademicReportAsync(SystemReport report, Dictionary<string, object>? parameters)
        {
            return report.Name.ToLower() switch
            {
                "student performance summary" => await GetStudentPerformanceSummaryAsync(parameters),
                "course enrollment report" => await GetCourseEnrollmentReportAsync(parameters),
                "assignment submission report" => await GetAssignmentSubmissionReportAsync(parameters),
                _ => throw new NotSupportedException($"Academic report not implemented: {report.Name}")
            };
        }

        private async Task<object> GenerateAttendanceReportAsync(SystemReport report, Dictionary<string, object>? parameters)
        {
            return report.Name.ToLower() switch
            {
                "daily attendance report" => await GetDailyAttendanceReportAsync(parameters),
                "low attendance alert report" => await GetLowAttendanceReportAsync(parameters),
                _ => throw new NotSupportedException($"Attendance report not implemented: {report.Name}")
            };
        }

        private async Task<object> GeneratePerformanceReportAsync(SystemReport report, Dictionary<string, object>? parameters)
        {
            // Implementation for performance reports
            return new { message = "Performance reports coming soon" };
        }

        private async Task<object> GenerateSystemReportAsync(SystemReport report, Dictionary<string, object>? parameters)
        {
            return report.Name.ToLower() switch
            {
                "user activity report" => await GetUserActivityReportAsync(parameters),
                "automation logs report" => await GetAutomationLogsReportAsync(parameters),
                "system health report" => await GetSystemHealthAsync(),
                _ => throw new NotSupportedException($"System report not implemented: {report.Name}")
            };
        }

        private async Task<object> GenerateUserReportAsync(SystemReport report, Dictionary<string, object>? parameters)
        {
            // Implementation for user reports
            return new { message = "User reports coming soon" };
        }

        private async Task<object> GetStudentPerformanceSummaryAsync(Dictionary<string, object>? parameters)
        {
            // Mock data for now - replace with actual database queries
            return new
            {
                students = new[]
                {
                    new { name = "John Doe", totalScore = 85, averageScore = 85.5, coursesCount = 5, status = "Good" },
                    new { name = "Jane Smith", totalScore = 92, averageScore = 92.3, coursesCount = 6, status = "Excellent" },
                    new { name = "Bob Wilson", totalScore = 76, averageScore = 76.8, coursesCount = 4, status = "Satisfactory" }
                },
                summary = new { totalStudents = 3, averagePerformance = 84.9, topPerformer = "Jane Smith" }
            };
        }

        private async Task<object> GetCourseEnrollmentReportAsync(Dictionary<string, object>? parameters)
        {
            // Mock data for now
            return new
            {
                courses = new[]
                {
                    new { name = "Mathematics", enrolledStudents = 45, capacity = 50, utilizationRate = 90 },
                    new { name = "Physics", enrolledStudents = 38, capacity = 40, utilizationRate = 95 },
                    new { name = "Chemistry", enrolledStudents = 42, capacity = 45, utilizationRate = 93 }
                },
                summary = new { totalEnrollments = 125, averageUtilization = 92.7 }
            };
        }

        private async Task<object> GetAssignmentSubmissionReportAsync(Dictionary<string, object>? parameters)
        {
            // Mock data for now
            return new
            {
                assignments = new[]
                {
                    new { title = "Math Assignment 1", submitted = 42, total = 45, submissionRate = 93.3, averageScore = 82.5 },
                    new { title = "Physics Lab Report", submitted = 35, total = 38, submissionRate = 92.1, averageScore = 78.3 },
                    new { title = "Chemistry Quiz", submitted = 40, total = 42, submissionRate = 95.2, averageScore = 85.7 }
                },
                summary = new { totalAssignments = 3, averageSubmissionRate = 93.5, averageScore = 82.2 }
            };
        }

        private async Task<object> GetDailyAttendanceReportAsync(Dictionary<string, object>? parameters)
        {
            // Mock data for now
            return new
            {
                date = DateTime.Today.ToString("yyyy-MM-dd"),
                classes = new[]
                {
                    new { course = "Mathematics", present = 42, absent = 3, total = 45, attendanceRate = 93.3 },
                    new { course = "Physics", present = 36, absent = 2, total = 38, attendanceRate = 94.7 },
                    new { course = "Chemistry", present = 39, absent = 3, total = 42, attendanceRate = 92.9 }
                },
                summary = new { totalPresent = 117, totalAbsent = 8, overallAttendanceRate = 93.6 }
            };
        }

        private async Task<object> GetLowAttendanceReportAsync(Dictionary<string, object>? parameters)
        {
            // Mock data for now
            return new
            {
                threshold = 75,
                students = new[]
                {
                    new { name = "Alice Brown", attendanceRate = 72.5, course = "Mathematics", absences = 8 },
                    new { name = "Charlie Davis", attendanceRate = 68.9, course = "Physics", absences = 12 },
                    new { name = "Diana Wilson", attendanceRate = 74.2, course = "Chemistry", absences = 7 }
                },
                summary = new { studentsAtRisk = 3, averageAttendanceAtRisk = 71.9 }
            };
        }

        private async Task<object> GetUserActivityReportAsync(Dictionary<string, object>? parameters)
        {
            // Mock data for now
            return new
            {
                period = "Last 7 days",
                activities = new[]
                {
                    new { date = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), logins = 125, submissions = 23, notifications = 89 },
                    new { date = DateTime.Today.AddDays(-2).ToString("yyyy-MM-dd"), logins = 118, submissions = 19, notifications = 76 },
                    new { date = DateTime.Today.AddDays(-3).ToString("yyyy-MM-dd"), logins = 132, submissions = 27, notifications = 94 }
                },
                summary = new { totalLogins = 375, totalSubmissions = 69, totalNotifications = 259 }
            };
        }

        private async Task<object> GetAutomationLogsReportAsync(Dictionary<string, object>? parameters)
        {
            try
            {
                var response = await _databaseService.Client
                    .From<AutomationLogDb>()
                    .Select("*")
                    .Order("triggered_at", Postgrest.Constants.Ordering.Descending)
                    .Limit(100)
                    .Get();

                var logs = response.Models?.Select(db => db.ToAutomationLog()).ToList() ?? new List<AutomationLog>();
                
                return new
                {
                    logs = logs.Take(20).Select(log => new 
                    {
                        title = log.Title,
                        triggerType = log.TriggerType.ToString(),
                        status = log.Status,
                        triggeredAt = log.TriggeredAt,
                        isSuccessful = log.IsSuccessful,
                        errorMessage = log.ErrorMessage
                    }),
                    summary = new
                    {
                        totalLogs = logs.Count,
                        successfulLogs = logs.Count(l => l.IsSuccessful),
                        failedLogs = logs.Count(l => !l.IsSuccessful),
                        successRate = logs.Count > 0 ? Math.Round((double)logs.Count(l => l.IsSuccessful) / logs.Count * 100, 1) : 0
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automation logs report: {Message}", ex.Message);
                return new { error = "Failed to load automation logs" };
            }
        }

        private async Task<Dictionary<string, object>> GetAdminDashboardMetricsAsync()
        {
            var metrics = new Dictionary<string, object>();

            try
            {
                // Get user statistics
                var userStats = await GetUserStatisticsAsync();
                metrics.AddRange(userStats);

                // Get system statistics
                metrics["total_courses"] = 45; // Mock data
                metrics["total_assignments"] = 128; // Mock data
                metrics["pending_submissions"] = 23; // Mock data
                metrics["system_health_score"] = 95.8; // Mock data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard metrics: {Message}", ex.Message);
                metrics["error"] = "Failed to load metrics";
            }

            return metrics;
        }

        private async Task<Dictionary<string, object>> GetTeacherDashboardMetricsAsync(string teacherId)
        {
            var metrics = new Dictionary<string, object>();

            try
            {
                metrics["my_courses"] = 3; // Mock data
                metrics["total_students"] = 125; // Mock data
                metrics["pending_submissions"] = 18; // Mock data
                metrics["average_attendance"] = 87.5; // Mock data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teacher dashboard metrics: {Message}", ex.Message);
                metrics["error"] = "Failed to load metrics";
            }

            return metrics;
        }

        private async Task<Dictionary<string, object>> GetStudentDashboardMetricsAsync(string studentId)
        {
            var metrics = new Dictionary<string, object>();

            try
            {
                metrics["enrolled_courses"] = 5; // Mock data
                metrics["completed_assignments"] = 23; // Mock data
                metrics["pending_assignments"] = 4; // Mock data
                metrics["overall_grade"] = 85.7; // Mock data
                metrics["attendance_rate"] = 92.3; // Mock data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student dashboard metrics: {Message}", ex.Message);
                metrics["error"] = "Failed to load metrics";
            }

            return metrics;
        }

        private async Task<Dictionary<string, object>> GetUserStatisticsAsync()
        {
            try
            {
                var response = await _databaseService.Client
                    .From<UserProfileDb>()
                    .Select("role, is_active")
                    .Get();

                var users = response.Models ?? new List<UserProfileDb>();
                
                return new Dictionary<string, object>
                {
                    ["total_users"] = users.Count,
                    ["active_users"] = users.Count(u => u.IsActive),
                    ["admin_users"] = users.Count(u => u.Role == "admin"),
                    ["teacher_users"] = users.Count(u => u.Role == "teacher"),
                    ["student_users"] = users.Count(u => u.Role == "student")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user statistics: {Message}", ex.Message);
                return new Dictionary<string, object>();
            }
        }

        private int GetRowCount(object data)
        {
            try
            {
                if (data == null) return 0;
                
                var json = JsonSerializer.Serialize(data);
                var doc = JsonDocument.Parse(json);
                
                // Try to find array properties and return the largest count
                return FindLargestArraySize(doc.RootElement);
            }
            catch
            {
                return 0;
            }
        }

        private int FindLargestArraySize(JsonElement element)
        {
            var maxSize = 0;
            
            if (element.ValueKind == JsonValueKind.Array)
            {
                return element.GetArrayLength();
            }
            
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    var size = FindLargestArraySize(property.Value);
                    maxSize = Math.Max(maxSize, size);
                }
            }
            
            return maxSize;
        }
    }

    // Extension method for Dictionary
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source)
            where TKey : notnull
        {
            foreach (var kvp in source)
            {
                target[kvp.Key] = kvp.Value;
            }
        }
    }
}
