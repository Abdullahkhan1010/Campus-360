using Campus360.Models;

namespace Campus360.Services
{
    public class TeacherServiceEnhanced
    {
        private readonly HttpClient _httpClient;
        private readonly AutomationEngineService _automationEngine;

        // Constructor with AutomationEngine dependency
        public TeacherServiceEnhanced(HttpClient httpClient, AutomationEngineService automationEngine)
        {
            _httpClient = httpClient;
            _automationEngine = automationEngine;
        }

        // Mock data for demonstration - replace with Supabase calls later
        private static List<TeacherCourse> _teacherCourses = new()
        {
            new TeacherCourse 
            { 
                Id = "1", 
                CourseId = "1",
                Name = "Data Structures", 
                CourseName = "Data Structures",
                Code = "CS101", 
                CourseCode = "CS101",
                DepartmentId = "1", 
                DepartmentName = "Computer Science",
                Semester = 3, 
                CreditHours = 4, 
                EnrolledStudents = 45,
                TotalClasses = 30,
                CompletedClasses = 18,
                AttendanceRate = 87.5,
                PendingAssignments = 2,
                LastClassDate = DateTime.Now.AddDays(-2),
                NextClassDate = DateTime.Now.AddDays(1),
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-60)
            },
            new TeacherCourse 
            { 
                Id = "2", 
                CourseId = "2",
                Name = "Web Development", 
                CourseName = "Web Development",
                Code = "IT201", 
                CourseCode = "IT201",
                DepartmentId = "2", 
                DepartmentName = "Information Technology",
                Semester = 4, 
                CreditHours = 3, 
                EnrolledStudents = 38,
                TotalClasses = 25,
                CompletedClasses = 15,
                AttendanceRate = 92.3,
                PendingAssignments = 1,
                LastClassDate = DateTime.Now.AddDays(-1),
                NextClassDate = DateTime.Now.AddDays(2),
                IsActive = true,
                CreatedAt = DateTime.Now.AddDays(-45)
            }
        };

        private static List<CourseStudent> _courseStudents = new()
        {
            // Students for CS101
            new CourseStudent { Id = "student1", FullName = "Alice Johnson", Email = "alice@example.com", StudentId = "CS2021001", DepartmentName = "Computer Science", Semester = 3, AttendancePercentage = 95.5, TotalClasses = 18, AttendedClasses = 17, OverallGrade = 88.5, GradeStatus = "In Progress", EnrolledDate = DateTime.Now.AddDays(-60) },
            new CourseStudent { Id = "student2", FullName = "Bob Smith", Email = "bob@example.com", StudentId = "CS2021002", DepartmentName = "Computer Science", Semester = 3, AttendancePercentage = 72.0, TotalClasses = 18, AttendedClasses = 13, OverallGrade = 75.2, GradeStatus = "In Progress", EnrolledDate = DateTime.Now.AddDays(-60) },
            new CourseStudent { Id = "student3", FullName = "Carol Wilson", Email = "carol@example.com", StudentId = "CS2021003", DepartmentName = "Computer Science", Semester = 3, AttendancePercentage = 88.9, TotalClasses = 18, AttendedClasses = 16, OverallGrade = 92.1, GradeStatus = "In Progress", EnrolledDate = DateTime.Now.AddDays(-58) },
            
            // Students for IT201
            new CourseStudent { Id = "student4", FullName = "David Brown", Email = "david@example.com", StudentId = "IT2021001", DepartmentName = "Information Technology", Semester = 4, AttendancePercentage = 100, TotalClasses = 15, AttendedClasses = 15, OverallGrade = 95.7, GradeStatus = "In Progress", EnrolledDate = DateTime.Now.AddDays(-45) },
            new CourseStudent { Id = "student5", FullName = "Eva Davis", Email = "eva@example.com", StudentId = "IT2021002", DepartmentName = "Information Technology", Semester = 4, AttendancePercentage = 86.7, TotalClasses = 15, AttendedClasses = 13, OverallGrade = 82.3, GradeStatus = "In Progress", EnrolledDate = DateTime.Now.AddDays(-45) }
        };

        // ============= ENHANCED METHODS WITH AUTOMATION TRIGGERS =============

        // Enhanced Result Upload with Automation
        public async Task<SaveResult> UploadStudentResultAsync(string studentId, string courseId, string examType, double marksObtained, double totalMarks, string grade)
        {
            await Task.Delay(500);

            try
            {
                // Get course details
                var course = _teacherCourses.FirstOrDefault(c => c.Id == courseId);
                if (course == null) return SaveResult.Failed("Course not found");

                // Trigger automation for result upload
                await _automationEngine.TriggerResultUploadedAsync(
                    studentId, 
                    courseId, 
                    course.Name, 
                    examType, 
                    marksObtained, 
                    totalMarks, 
                    grade
                );

                return SaveResult.Successful(true, "Result uploaded and notification sent to student");
            }
            catch (Exception ex)
            {
                return SaveResult.Failed($"Failed to upload result: {ex.Message}");
            }
        }

        // Enhanced Attendance Marking with Low Attendance Automation
        public async Task<SaveResult> MarkAttendanceWithAutomationAsync(TeacherMarkAttendanceModel model, string teacherId)
        {
            await Task.Delay(600);

            try
            {
                var course = _teacherCourses.FirstOrDefault(c => c.Id == model.CourseId);
                if (course == null) return SaveResult.Failed("Course not found");

                bool automationTriggered = false;

                // Process each student's attendance
                foreach (var student in model.Students)
                {
                    // Calculate updated attendance percentage
                    var courseStudent = _courseStudents.FirstOrDefault(s => s.Id == student.StudentId);
                    if (courseStudent != null)
                    {
                        // Update attendance
                        if (student.IsPresent)
                        {
                            courseStudent.AttendedClasses++;
                        }
                        courseStudent.TotalClasses++;
                        
                        // Recalculate attendance percentage
                        courseStudent.AttendancePercentage = (courseStudent.AttendedClasses * 100.0) / courseStudent.TotalClasses;

                        // Trigger low attendance automation if below threshold
                        if (courseStudent.AttendancePercentage < 75)
                        {
                            await _automationEngine.TriggerAttendanceBelowThresholdAsync(
                                student.StudentId,
                                model.CourseId,
                                course.Name,
                                courseStudent.AttendancePercentage
                            );
                            automationTriggered = true;
                        }
                    }
                }

                return SaveResult.Successful(automationTriggered, "Attendance marked successfully" + 
                    (automationTriggered ? " and low attendance alerts sent" : ""));
            }
            catch (Exception ex)
            {
                return SaveResult.Failed($"Failed to mark attendance: {ex.Message}");
            }
        }

        // Enhanced Assignment Creation with Automation
        public async Task<SaveResult> CreateAssignmentWithAutomationAsync(TeacherCreateAssignmentModel model, string teacherId)
        {
            await Task.Delay(500);

            try
            {
                var course = _teacherCourses.FirstOrDefault(c => c.Id == model.CourseId);
                if (course == null) return SaveResult.Failed("Course not found");

                // Get all students in the course
                var courseStudents = await GetCourseStudentsAsync(model.CourseId);
                var studentIds = courseStudents.Select(s => s.Id).ToList();

                bool automationTriggered = false;

                // If assignment is published, trigger automation
                if (model.IsPublished)
                {
                    await _automationEngine.TriggerAssignmentUploadedAsync(
                        model.CourseId,
                        course.Name,
                        model.Title,
                        model.DueDate,
                        studentIds
                    );
                    automationTriggered = true;
                }

                return SaveResult.Successful(automationTriggered, "Assignment created successfully" + 
                    (automationTriggered ? " and notifications sent to students" : ""));
            }
            catch (Exception ex)
            {
                return SaveResult.Failed($"Failed to create assignment: {ex.Message}");
            }
        }

        // Enhanced Notice Publishing with Automation
        public async Task<SaveResult> PublishNoticeWithAutomationAsync(NoticeModel notice, string teacherId)
        {
            await Task.Delay(400);

            try
            {
                notice.Id = Guid.NewGuid().ToString();
                notice.CreatedBy = teacherId;
                notice.CreatedAt = DateTime.Now;
                notice.IsPublished = true;
                notice.PublishedAt = DateTime.Now;

                // Get recipient user IDs
                List<string> recipientIds = new();
                
                if (!string.IsNullOrEmpty(notice.CourseId))
                {
                    // Course-specific notice
                    var courseStudents = await GetCourseStudentsAsync(notice.CourseId);
                    recipientIds = courseStudents.Select(s => s.Id).ToList();
                }
                else
                {
                    // General notice to all students
                    recipientIds = _courseStudents.Select(s => s.Id).ToList();
                }

                // Determine priority based on notice type
                var priority = notice.Priority?.ToLower() switch
                {
                    "urgent" => NotificationPriority.Critical,
                    "high" => NotificationPriority.High,
                    "low" => NotificationPriority.Low,
                    _ => NotificationPriority.Normal
                };

                // Trigger automation for notice publication
                await _automationEngine.TriggerNoticePublishedAsync(
                    notice.Title,
                    notice.Content,
                    recipientIds,
                    priority
                );

                return SaveResult.Successful(true, $"Notice published and sent to {recipientIds.Count} recipients");
            }
            catch (Exception ex)
            {
                return SaveResult.Failed($"Failed to publish notice: {ex.Message}");
            }
        }

        // Enhanced Class Cancellation with Automation
        public async Task<SaveResult> CancelClassWithAutomationAsync(string courseId, DateTime classDate, string reason, string teacherId)
        {
            await Task.Delay(400);

            try
            {
                var course = _teacherCourses.FirstOrDefault(c => c.Id == courseId);
                if (course == null) return SaveResult.Failed("Course not found");

                // Get all students in the course
                var courseStudents = await GetCourseStudentsAsync(courseId);
                var studentIds = courseStudents.Select(s => s.Id).ToList();

                // Trigger automation for class cancellation
                await _automationEngine.TriggerClassCancelledAsync(
                    courseId,
                    course.Name,
                    classDate,
                    reason,
                    studentIds
                );

                return SaveResult.Successful(true, $"Class cancelled and notifications sent to {studentIds.Count} students");
            }
            catch (Exception ex)
            {
                return SaveResult.Failed($"Failed to cancel class: {ex.Message}");
            }
        }

        // Method to trigger assignment deadline reminders (called by background service)
        public async Task<SaveResult> TriggerAssignmentDeadlineRemindersAsync()
        {
            await Task.Delay(300);

            try
            {
                // Check for assignments due in 24 hours
                var upcomingAssignments = _assignments.Where(a => 
                    a.IsPublished && 
                    a.DueDate > DateTime.Now && 
                    a.DueDate <= DateTime.Now.AddHours(24)
                ).ToList();

                int remindersSent = 0;

                foreach (var assignment in upcomingAssignments)
                {
                    var courseStudents = await GetCourseStudentsAsync(assignment.CourseId);
                    var hoursRemaining = (int)(assignment.DueDate - DateTime.Now).TotalHours;

                    foreach (var student in courseStudents)
                    {
                        await _automationEngine.TriggerAssignmentDeadlineApproachingAsync(
                            student.Id,
                            assignment.CourseId,
                            assignment.CourseName,
                            assignment.Title,
                            hoursRemaining
                        );
                        remindersSent++;
                    }
                }

                return SaveResult.Successful(remindersSent > 0, $"Sent {remindersSent} deadline reminders");
            }
            catch (Exception ex)
            {
                return SaveResult.Failed($"Failed to send deadline reminders: {ex.Message}");
            }
        }

        // ============= EXISTING METHODS =============

        private static List<AssignmentModel> _assignments = new()
        {
            new AssignmentModel 
            { 
                Id = "a1", 
                AssignmentId = "a1",
                Title = "Linked List Implementation", 
                Description = "Implement a doubly linked list with basic operations",
                CourseId = "1", 
                CourseName = "Data Structures", 
                CourseCode = "CS101",
                DueDate = DateTime.Now.AddDays(5),
                MaxScore = 100,
                MaxMarks = 100,
                AssignmentType = "Assignment",
                AllowLateSubmission = true,
                LatePenaltyPercentage = 10,
                IsPublished = true,
                TotalStudents = 45,
                SubmittedCount = 23,
                PendingCount = 22,
                GradedCount = 15,
                AverageScore = 78.5,
                CreatedAt = DateTime.Now.AddDays(-7)
            },
            new AssignmentModel 
            { 
                Id = "a2", 
                AssignmentId = "a2",
                Title = "HTML/CSS Portfolio", 
                Description = "Create a personal portfolio website using HTML and CSS",
                CourseId = "2", 
                CourseName = "Web Development", 
                CourseCode = "IT201",
                DueDate = DateTime.Now.AddHours(23), // Due in 23 hours for testing deadline reminders
                MaxScore = 100,
                MaxMarks = 100,
                AssignmentType = "Project",
                AllowLateSubmission = false,
                IsPublished = true,
                TotalStudents = 38,
                SubmittedCount = 35,
                PendingCount = 3,
                GradedCount = 30,
                AverageScore = 85.2,
                CreatedAt = DateTime.Now.AddDays(-14)
            }
        };

        public async Task<TeacherDashboardStats> GetTeacherDashboardStatsAsync(string teacherId)
        {
            await Task.Delay(500);

            var teacherCourses = _teacherCourses.Where(c => c.IsActive).ToList();
            var automationMetrics = await _automationEngine.GetAutomationMetricsAsync();

            var stats = new TeacherDashboardStats
            {
                TotalCourses = teacherCourses.Count,
                TotalStudents = teacherCourses.Sum(c => c.EnrolledStudents),
                PendingAssignments = teacherCourses.Sum(c => c.PendingAssignments),
                UpcomingClasses = GetUpcomingClassesCount(teacherId),
                TotalNotifications = automationMetrics.TotalNotificationsSent,
                AverageAttendance = teacherCourses.Any() ? teacherCourses.Average(c => c.AttendanceRate) : 0,
                RecentActivities = GetRecentTeacherActivities()
            };

            return stats;
        }

        public async Task<List<TeacherCourse>> GetTeacherCoursesAsync(string teacherId)
        {
            await Task.Delay(400);
            return _teacherCourses.Where(c => c.IsActive).OrderBy(c => c.Code).ToList();
        }

        public async Task<List<CourseStudent>> GetCourseStudentsAsync(string courseId)
        {
            await Task.Delay(300);
            if (courseId == "1") // CS101
                return _courseStudents.Where(s => s.StudentId.StartsWith("CS")).ToList();
            else if (courseId == "2") // IT201
                return _courseStudents.Where(s => s.StudentId.StartsWith("IT")).ToList();
            else
                return _courseStudents.Take(10).ToList();
        }

        public async Task<List<AssignmentModel>> GetAssignmentsAsync(string? courseId = null)
        {
            await Task.Delay(400);
            var query = _assignments.AsQueryable();

            if (!string.IsNullOrEmpty(courseId))
                query = query.Where(a => a.CourseId == courseId);

            return query.OrderByDescending(a => a.CreatedAt).ToList();
        }

        private int GetUpcomingClassesCount(string teacherId)
        {
            return Random.Shared.Next(5, 11);
        }

        private List<TeacherActivityModel> GetRecentTeacherActivities()
        {
            return new List<TeacherActivityModel>
            {
                new TeacherActivityModel
                {
                    Id = "1",
                    Title = "Automation triggered",
                    Description = "Low attendance alert sent to Bob Smith",
                    Type = "automation",
                    CourseCode = "CS101",
                    StudentName = "Bob Smith",
                    CreatedAt = DateTime.Now.AddHours(-1)
                },
                new TeacherActivityModel
                {
                    Id = "2",
                    Title = "Assignment notification sent",
                    Description = "New assignment notifications sent to 38 students",
                    Type = "automation",
                    CourseCode = "IT201",
                    CreatedAt = DateTime.Now.AddHours(-3)
                },
                new TeacherActivityModel
                {
                    Id = "3",
                    Title = "Result notification sent",
                    Description = "Midterm result notifications sent to 45 students",
                    Type = "automation",
                    CourseCode = "CS101",
                    CreatedAt = DateTime.Now.AddHours(-5)
                }
            };
        }

        // Method aliases for compatibility with existing code
        public async Task<TeacherDashboardStats> GetDashboardStatsAsync(string teacherId)
        {
            return await GetTeacherDashboardStatsAsync(teacherId);
        }

        public async Task<SaveResult> CreateAssignmentAsync(TeacherCreateAssignmentModel model, string teacherId)
        {
            return await CreateAssignmentWithAutomationAsync(model, teacherId);
        }

        public async Task<SaveResult> MarkAttendanceAsync(TeacherMarkAttendanceModel model, string teacherId)
        {
            return await MarkAttendanceWithAutomationAsync(model, teacherId);
        }        public async Task<SaveResult> CreateNoticeAsync(NoticeModel notice, string teacherId)
        {
            return await PublishNoticeWithAutomationAsync(notice, teacherId);
        }

        /// <summary>
        /// Get teacher assignments for calendar integration
        /// </summary>
        public async Task<List<AssignmentModel>> GetTeacherAssignmentsAsync(string teacherId)
        {
            await Task.Delay(300);
            return _assignments.OrderByDescending(a => a.CreatedAt).ToList();
        }
    }
}
