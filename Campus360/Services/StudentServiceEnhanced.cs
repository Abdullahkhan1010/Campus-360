using Campus360.Models;

namespace Campus360.Services
{
    public class StudentServiceEnhanced
    {
        private readonly HttpClient _httpClient;
        private readonly AutomationEngineService _automationEngine;

        // Constructor with AutomationEngine dependency
        public StudentServiceEnhanced(HttpClient httpClient, AutomationEngineService automationEngine)
        {
            _httpClient = httpClient;
            _automationEngine = automationEngine;
        }

        // Mock data - inherit from StudentService for compatibility
        private static List<StudentCourse> _studentCourses = new()
        {
            new StudentCourse 
            { 
                Id = "sc1", 
                StudentId = "s1",
                CourseId = "1",
                CourseName = "Data Structures", 
                CourseCode = "CS101",
                TeacherId = "t1",
                TeacherName = "Dr. John Smith",
                DepartmentName = "Computer Science",
                Semester = 3, 
                CreditHours = 4, 
                EnrollmentDate = DateTime.Now.AddDays(-60),
                TotalClasses = 30,
                AttendedClasses = 26,
                AttendancePercentage = 86.7,
                CurrentGrade = 3.3,
                LetterGrade = "B+",
                GPA = 3.3,
                IsActive = true,
                LastAttendanceDate = DateTime.Now.AddDays(-1),
                NextClassDate = DateTime.Now.AddDays(1),
                ClassSchedule = "Mon, Wed, Fri - 10:00 AM",
                ClassRoom = "Room 201"
            },
            new StudentCourse 
            { 
                Id = "sc2", 
                StudentId = "s1",
                CourseId = "2",
                CourseName = "Web Development", 
                CourseCode = "IT201",
                TeacherId = "t2",
                TeacherName = "Prof. Sarah Johnson",
                DepartmentName = "Information Technology",
                Semester = 3, 
                CreditHours = 3, 
                EnrollmentDate = DateTime.Now.AddDays(-55),
                TotalClasses = 25,
                AttendedClasses = 23,
                AttendancePercentage = 92.0,
                CurrentGrade = 3.7,
                LetterGrade = "A-",
                GPA = 3.7,
                IsActive = true,
                LastAttendanceDate = DateTime.Now.AddDays(-2),
                NextClassDate = DateTime.Now.AddDays(2),
                ClassSchedule = "Tue, Thu - 2:00 PM",
                ClassRoom = "Lab 105"
            },
            new StudentCourse 
            { 
                Id = "sc3", 
                StudentId = "s1",
                CourseId = "3",
                CourseName = "Database Systems", 
                CourseCode = "CS301",
                TeacherId = "t1",
                TeacherName = "Dr. John Smith",
                DepartmentName = "Computer Science",
                Semester = 3, 
                CreditHours = 4, 
                EnrollmentDate = DateTime.Now.AddDays(-50),
                TotalClasses = 28,
                AttendedClasses = 20,
                AttendancePercentage = 71.4,
                CurrentGrade = 2.3,
                LetterGrade = "C+",
                GPA = 2.3,
                IsActive = true,
                LastAttendanceDate = DateTime.Now.AddDays(-3),
                NextClassDate = DateTime.Now.AddDays(1),
                ClassSchedule = "Mon, Wed - 1:00 PM",
                ClassRoom = "Room 303"
            }
        };

        private static List<StudentAssignment> _studentAssignments = new()
        {
            new StudentAssignment
            {
                Id = "sa1",
                AssignmentId = "a1",
                StudentId = "s1",
                Title = "Linked List Implementation",
                Description = "Implement a doubly linked list with basic operations",
                CourseId = "1",
                CourseName = "Data Structures",
                CourseCode = "CS101",
                TeacherName = "Dr. John Smith",
                DueDate = DateTime.Now.AddDays(5),
                MaxMarks = 100,
                ObtainedMarks = null,
                SubmissionStatus = "Pending",
                SubmissionDate = null,
                IsLateSubmission = false,
                GradedDate = null,
                Feedback = null,
                AssignmentType = "Programming",
                Priority = "High",
                IsOverdue = false,
                RemainingDays = 5,
                AttachmentUrl = null,
                CreatedAt = DateTime.Now.AddDays(-7)
            },
            new StudentAssignment
            {
                Id = "sa2",
                AssignmentId = "a2",
                StudentId = "s1",
                Title = "HTML/CSS Portfolio",
                Description = "Create a personal portfolio website using HTML and CSS",
                CourseId = "2",
                CourseName = "Web Development",
                CourseCode = "IT201",
                TeacherName = "Prof. Sarah Johnson",
                DueDate = DateTime.Now.AddHours(23), // Due in 23 hours for testing deadline reminders
                MaxMarks = 100,
                ObtainedMarks = null,
                SubmissionStatus = "Pending",
                SubmissionDate = null,
                IsLateSubmission = false,
                GradedDate = null,
                Feedback = null,
                AssignmentType = "Project",
                Priority = "High",
                IsOverdue = false,
                RemainingDays = 1,
                AttachmentUrl = null,
                CreatedAt = DateTime.Now.AddDays(-14)
            }
        };

        private static List<StudentNotice> _studentNotices = new();
        private static List<StudentAlert> _studentAlerts = new();

        // ============= ENHANCED METHODS WITH AUTOMATION INTEGRATION =============

        /// <summary>
        /// Enhanced dashboard stats that includes real-time automation notifications
        /// </summary>
        public async Task<StudentDashboardStats> GetDashboardStatsWithAutomationAsync(string studentId)
        {
            await Task.Delay(300); // Simulate API call
            
            // Get base stats
            var enrolledCourses = _studentCourses.Where(c => c.StudentId == studentId && c.IsActive).ToList();
            var activeAssignments = _studentAssignments.Where(a => a.StudentId == studentId && a.SubmissionStatus == "Pending").ToList();
            
            // Get real-time notifications from automation engine
            var notifications = await _automationEngine.GetStudentNotificationsAsync(studentId);            var unreadNotifications = notifications.Count(n => !n.IsRead);
            var urgentNotifications = notifications.Count(n => n.Priority == NotificationPriority.High && !n.IsRead);
            
            // Get automation-generated alerts
            var automationAlerts = await _automationEngine.GetActiveAlertsAsync(studentId);
            
            var stats = new StudentDashboardStats
            {
                TotalCourses = enrolledCourses.Count,
                ActiveAssignments = activeAssignments.Count,
                OverdueAssignments = activeAssignments.Count(a => a.IsOverdue),
                UnreadNotices = unreadNotifications, // From automation engine
                OverallAttendance = enrolledCourses.Any() ? enrolledCourses.Average(c => c.AttendancePercentage) : 0,
                CurrentGPA = enrolledCourses.Any() ? enrolledCourses.Average(c => c.GPA) : 0,
                RecentResults = 0, // Will be enhanced with automation
                UpcomingEvents = 0, // Will be enhanced with automation
                ActiveAlerts = automationAlerts.Count(),
                AverageGrade = enrolledCourses.Any() ? CalculateAverageGrade(enrolledCourses) : "N/A",
                TotalClasses = enrolledCourses.Sum(c => c.TotalClasses),
                AttendedClasses = enrolledCourses.Sum(c => c.AttendedClasses),
                TotalCreditHours = enrolledCourses.Sum(c => c.CreditHours),
                LastLoginDate = DateTime.Now.AddHours(-2),
                NextClass = GetNextClass(studentId),
                LowAttendanceCourses = enrolledCourses.Count(c => c.AttendancePercentage < 75),
                PendingSubmissions = activeAssignments.Count(a => !a.IsOverdue),
                CompletedAssignments = _studentAssignments.Count(a => a.StudentId == studentId && (a.SubmissionStatus == "Submitted" || a.SubmissionStatus == "Graded")),
                UrgentNotifications = urgentNotifications
            };

            return stats;
        }

        /// <summary>
        /// Get real-time notifications from automation engine
        /// </summary>
        public async Task<List<NotificationModel>> GetRealTimeNotificationsAsync(string studentId)
        {
            try
            {
                var notifications = await _automationEngine.GetStudentNotificationsAsync(studentId);
                return notifications.OrderByDescending(n => n.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                // Log error and return empty list
                Console.WriteLine($"Error fetching notifications: {ex.Message}");
                return new List<NotificationModel>();
            }
        }

        /// <summary>
        /// Get unread notifications count for badge display
        /// </summary>
        public async Task<int> GetUnreadNotificationCountAsync(string studentId)
        {
            try
            {
                var notifications = await _automationEngine.GetStudentNotificationsAsync(studentId);
                return notifications.Count(n => !n.IsRead);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching unread count: {ex.Message}");
                return 0;
            }
        }        /// <summary>
        /// Mark notification as read
        /// </summary>
        public async Task<SaveResult> MarkNotificationAsReadAsync(string studentId, string notificationId)
        {
            try
            {
                var result = await _automationEngine.MarkNotificationAsReadAsync(notificationId);
                return new SaveResult { Success = result, Message = result ? "Notification marked as read" : "Failed to mark notification as read" };
            }
            catch (Exception ex)
            {
                return new SaveResult { Success = false, Message = $"Error marking notification as read: {ex.Message}" };
            }
        }        /// <summary>
        /// Get active automation alerts for student
        /// </summary>
        public async Task<List<AutomationAlert>> GetActiveAutomationAlertsAsync(string studentId)
        {
            try
            {
                var notifications = await _automationEngine.GetActiveAlertsAsync(studentId);
                
                // Convert NotificationModel to AutomationAlert
                return notifications.Select(n => new AutomationAlert
                {
                    Id = n.Id,
                    AlertType = GetAlertTypeFromNotification(n),
                    Title = n.Title,
                    Message = n.Message,
                    CourseId = n.CourseId,
                    CourseName = n.CourseName,
                    CourseCode = n.CourseCode,
                    Priority = n.Priority,
                    CreatedAt = n.CreatedAt,
                    ExpiryDate = n.ExpiryDate,
                    ActionUrl = n.ActionUrl,
                    IconClass = n.IconClass ?? "fas fa-bell",
                    BadgeClass = n.BadgeClass ?? "badge-info",
                    IsActive = !n.IsRead,
                    RelatedEntityId = n.RelatedId,
                    RelatedEntityType = GetEntityTypeFromNotification(n)
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching automation alerts: {ex.Message}");
                return new List<AutomationAlert>();
            }
        }

        private string GetAlertTypeFromNotification(NotificationModel notification)
        {
            return notification.Type switch
            {
                NotificationType.Assignment => "assignment_due",
                NotificationType.Result => "result_available",
                NotificationType.Attendance => "low_attendance",
                NotificationType.Notice => "notice_posted",
                _ => "general"
            };
        }

        private string GetEntityTypeFromNotification(NotificationModel notification)
        {
            return notification.Type switch
            {
                NotificationType.Assignment => "assignment",
                NotificationType.Result => "result",
                NotificationType.Attendance => "attendance",
                NotificationType.Notice => "notice",
                _ => "general"
            };
        }

        /// <summary>
        /// Submit assignment with automation trigger
        /// </summary>
        public async Task<SaveResult> SubmitAssignmentWithAutomationAsync(string studentId, string assignmentId, string submissionData)
        {
            try
            {
                // Simulate assignment submission
                await Task.Delay(500);
                
                var assignment = _studentAssignments.FirstOrDefault(a => a.Id == assignmentId && a.StudentId == studentId);
                if (assignment == null)
                {
                    return new SaveResult { Success = false, Message = "Assignment not found" };
                }                // Update assignment status
                assignment.SubmissionStatus = "Submitted";
                assignment.SubmissionDate = DateTime.Now;
                assignment.IsOverdue = assignment.DueDate < DateTime.Now;

                // Trigger automation notification for assignment submission
                await _automationEngine.TriggerAssignmentSubmissionAsync(
                    studentId,
                    assignment.CourseId,
                    assignment.CourseName ?? "Unknown Course",
                    assignment.Title,
                    DateTime.Now
                );

                return new SaveResult { Success = true, Message = "Assignment submitted successfully!" };
            }
            catch (Exception ex)
            {
                return new SaveResult { Success = false, Message = $"Error submitting assignment: {ex.Message}" };
            }
        }

        /// <summary>
        /// Enhanced attendance method that consumes automation alerts
        /// </summary>
        public async Task<List<StudentCourse>> GetCoursesWithAutomationAlertsAsync(string studentId)
        {
            try
            {
                var courses = await GetStudentCoursesAsync(studentId);
                var alerts = await _automationEngine.GetActiveAlertsAsync(studentId);                // Enhance courses with automation alert information
                foreach (var course in courses)
                {
                    var courseAlerts = alerts.Where(a => a.CourseId == course.CourseId).ToList();
                    // You can add a property to StudentCourse to hold alert information
                    // For now, we'll modify attendance status based on alerts
                    if (courseAlerts.Any(a => a.Type == NotificationType.Attendance))
                    {
                        // Mark course as having attendance issues
                        course.AttendancePercentage = Math.Min(course.AttendancePercentage, 74.9); // Ensure it's below threshold
                    }
                }

                return courses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting courses with automation alerts: {ex.Message}");
                return await GetStudentCoursesAsync(studentId);
            }
        }        /// <summary>
        /// Get assignments with deadline reminders from automation
        /// </summary>
        public async Task<List<StudentAssignment>> GetAssignmentsWithRemindersAsync(string studentId)
        {
            try
            {
                var assignments = await GetStudentAssignmentsAsync(studentId);
                var alerts = await _automationEngine.GetActiveAlertsAsync(studentId);                // Filter assignment deadline alerts
                var deadlineAlerts = alerts.Where(a => a.Type == NotificationType.Assignment).ToList();

                // Mark assignments that have active deadline reminders
                foreach (var assignment in assignments)
                {                    var hasReminder = deadlineAlerts.Any(a => 
                        a.Title.Contains(assignment.Title) || 
                        a.Message.Contains(assignment.Title) ||
                        a.RelatedId == assignment.Id);
                    
                    if (hasReminder)
                    {
                        assignment.Priority = "High"; // Increase priority for assignments with active reminders
                    }
                }

                return assignments.OrderBy(a => a.DueDate).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting assignments with reminders: {ex.Message}");
                return await GetStudentAssignmentsAsync(studentId);
            }
        }

        // ============= AUTOMATION NOTIFICATION METHODS =============

        /// <summary>
        /// Process result notification from automation engine
        /// </summary>
        public async Task<SaveResult> ProcessResultNotificationAsync(string studentId, string courseId, string resultType, double score)
        {
            try
            {
                // This would be called when a teacher uploads results
                await _automationEngine.TriggerResultNotificationAsync(
                    studentId,
                    courseId,
                    resultType,
                    score
                );

                return new SaveResult { Success = true, Message = "Result notification processed" };
            }
            catch (Exception ex)
            {
                return new SaveResult { Success = false, Message = $"Error processing result notification: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get personalized automation recommendations
        /// </summary>
        public async Task<List<AutomationRecommendation>> GetPersonalizedRecommendationsAsync(string studentId)
        {
            try
            {
                var courses = await GetStudentCoursesAsync(studentId);
                var assignments = await GetStudentAssignmentsAsync(studentId);
                var recommendations = new List<AutomationRecommendation>();

                // Low attendance recommendations
                var lowAttendanceCourses = courses.Where(c => c.AttendancePercentage < 75).ToList();
                foreach (var course in lowAttendanceCourses)
                {
                    recommendations.Add(new AutomationRecommendation
                    {
                        Id = Guid.NewGuid().ToString(),
                        StudentId = studentId,
                        Type = "attendance_improvement",
                        Title = $"Improve Attendance in {course.CourseName}",
                        Message = $"Your attendance in {course.CourseName} is {course.AttendancePercentage:F1}%. Attend the next {Math.Ceiling((75 - course.AttendancePercentage) / 100 * course.TotalClasses)} classes to reach 75%.",
                        Priority = "High",
                        CourseId = course.CourseId,
                        CourseName = course.CourseName,
                        ActionItems = new List<string>
                        {
                            "Check your class schedule",
                            "Set calendar reminders",
                            "Contact teacher if needed"
                        },
                        CreatedAt = DateTime.Now
                    });
                }

                // Assignment deadline recommendations
                var upcomingAssignments = assignments.Where(a => 
                    a.SubmissionStatus == "Pending" && 
                    a.DueDate >= DateTime.Now && 
                    a.DueDate <= DateTime.Now.AddDays(7)).ToList();

                foreach (var assignment in upcomingAssignments)
                {
                    var daysRemaining = (assignment.DueDate - DateTime.Now).Days;
                    recommendations.Add(new AutomationRecommendation
                    {
                        Id = Guid.NewGuid().ToString(),
                        StudentId = studentId,
                        Type = "assignment_reminder",
                        Title = $"Complete {assignment.Title}",
                        Message = $"Assignment '{assignment.Title}' is due in {daysRemaining} day(s). Start working on it now to avoid late submission.",
                        Priority = daysRemaining <= 2 ? "High" : "Medium",
                        CourseId = assignment.CourseId,
                        CourseName = assignment.CourseName,
                        ActionItems = new List<string>
                        {
                            "Review assignment requirements",
                            "Plan your work schedule",
                            "Ask questions if unclear"
                        },
                        CreatedAt = DateTime.Now
                    });
                }

                return recommendations.OrderByDescending(r => r.Priority == "High").ThenBy(r => r.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting personalized recommendations: {ex.Message}");
                return new List<AutomationRecommendation>();
            }
        }

        // ============= EXISTING METHODS (DELEGATED OR ENHANCED) =============

        public async Task<List<StudentCourse>> GetStudentCoursesAsync(string studentId)
        {
            await Task.Delay(300); // Simulate API call
            return _studentCourses.Where(c => c.StudentId == studentId && c.IsActive).OrderBy(c => c.CourseName).ToList();
        }

        public async Task<List<StudentAssignment>> GetStudentAssignmentsAsync(string studentId, string? courseId = null)
        {
            await Task.Delay(300); // Simulate API call
            var assignments = _studentAssignments.Where(a => a.StudentId == studentId).AsQueryable();
            
            if (!string.IsNullOrEmpty(courseId))
                assignments = assignments.Where(a => a.CourseId == courseId);
                
            return assignments.OrderBy(a => a.DueDate).ToList();
        }

        public async Task<StudentDashboardStats> GetDashboardStatsAsync(string studentId)
        {
            // Use the enhanced version by default
            return await GetDashboardStatsWithAutomationAsync(studentId);
        }

        // Helper Methods
        private string CalculateAverageGrade(List<StudentCourse> courses)
        {
            if (!courses.Any()) return "N/A";
            
            var gpa = courses.Average(c => c.GPA);
            return gpa switch
            {
                >= 3.7 => "A",
                >= 3.3 => "B+",
                >= 3.0 => "B",
                >= 2.7 => "C+",
                >= 2.0 => "C",
                >= 1.0 => "D",
                _ => "F"
            };
        }

        private string? GetNextClass(string studentId)
        {
            // This would check the schedule for next class
            return "Data Structures at 10:00 AM";
        }

        // Method aliases for compatibility with existing code
        public async Task<StudentCourse?> GetCourseDetailsAsync(string studentId, string courseId)
        {
            await Task.Delay(200);
            return _studentCourses.FirstOrDefault(c => c.StudentId == studentId && c.CourseId == courseId && c.IsActive);
        }

        public async Task<List<StudentAssignment>> GetPendingAssignmentsAsync(string studentId)
        {
            var assignments = await GetStudentAssignmentsAsync(studentId);
            return assignments.Where(a => a.SubmissionStatus == "Pending" && !a.IsOverdue).ToList();
        }

        public async Task<List<StudentAssignment>> GetOverdueAssignmentsAsync(string studentId)
        {
            var assignments = await GetStudentAssignmentsAsync(studentId);
            return assignments.Where(a => a.IsOverdue).ToList();
        }
    }

    // Supporting models for automation integration
    public class AutomationRecommendation
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public List<string> ActionItems { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
