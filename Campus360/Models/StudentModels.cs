using System.ComponentModel.DataAnnotations;

namespace Campus360.Models
{    // Student Dashboard Statistics
    public class StudentDashboardStats
    {
        public int TotalCourses { get; set; }
        public int EnrolledCourses { get; set; }
        public int ActiveAssignments { get; set; }
        public int PendingAssignments { get; set; }
        public int SubmittedAssignments { get; set; }
        public int OverdueAssignments { get; set; }        public int UnreadNotices { get; set; }
        public int UnreadNotifications { get; set; }
        public int UrgentNotifications { get; set; } // High priority notifications
        public double OverallAttendance { get; set; }
        public double AttendancePercentage { get; set; }
        public double CurrentGPA { get; set; }
        public int RecentResults { get; set; }
        public int UpcomingEvents { get; set; }
        public int UpcomingExams { get; set; }
        public int ActiveAlerts { get; set; }
        public string AverageGrade { get; set; } = "N/A";
        public int TotalClasses { get; set; }
        public int AttendedClasses { get; set; }
        public int TotalCreditHours { get; set; }
        public int CompletedCredits { get; set; }
        public int TotalCredits { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? NextClass { get; set; }
        public int LowAttendanceCourses { get; set; }
        public int PendingSubmissions { get; set; }
        public int CompletedAssignments { get; set; }
        public List<StudentActivityModel> RecentActivities { get; set; } = new();
    }

    // Student Activities
    public class StudentActivityModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "assignment", "result", "notice", "attendance"
        public DateTime CreatedAt { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
    }    // Student Course Enrollment
    public class StudentCourse
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string TeacherEmail { get; set; } = string.Empty;
        public string DepartmentId { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int Semester { get; set; }
        public int CreditHours { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime EnrolledDate { get; set; }
        
        // Academic Progress
        public double AttendancePercentage { get; set; }
        public int TotalClasses { get; set; }
        public int AttendedClasses { get; set; }
        public int AbsentClasses { get; set; }
        public double? CurrentGrade { get; set; }
        public string? LetterGrade { get; set; }
        public string GradeStatus { get; set; } = "In Progress";
        public double GPA { get; set; } // Add missing GPA property
        
        // Course Statistics
        public int TotalAssignments { get; set; }
        public int SubmittedAssignments { get; set; }
        public int PendingAssignments { get; set; }
        public int UpcomingExams { get; set; }
        public DateTime? LastClassDate { get; set; }
        public DateTime? NextClassDate { get; set; }
        public DateTime? LastAttendanceDate { get; set; }
        public string? ClassSchedule { get; set; }        public string? ClassRoom { get; set; }
        
        // Additional properties needed for UI
        public string Status { get; set; } = "Active"; // "Active", "Completed", "Dropped"
        public int AssignmentCount { get; set; }
        public string InstructorName { get; set; } = string.Empty; // Alias for TeacherName
        
        public bool IsActive { get; set; } = true;
    }

    // Student Assignment View
    public class StudentAssignment
    {
        public string Id { get; set; } = string.Empty;
        public string AssignmentId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public DateTime? GradedDate { get; set; }
        public int MaxMarks { get; set; } = 100;
        public int MaxScore { get; set; } = 100;
        public double? ObtainedMarks { get; set; }
        public string AssignmentType { get; set; } = "Assignment";
        public bool AllowLateSubmission { get; set; }
        public bool IsLateSubmission { get; set; }
        public int? LatePenaltyPercentage { get; set; }
        
        // Student-specific properties
        public string SubmissionStatus { get; set; } = "Not Submitted"; // "Not Submitted", "Submitted", "Late", "Graded", "Pending", "Overdue"
        public string? SubmissionText { get; set; }
        public string? AttachmentUrl { get; set; }
        public double? Score { get; set; }
        public string? Grade { get; set; }
        public string? Feedback { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsGraded { get; set; }
        public int DaysRemaining { get; set; }
        public int RemainingDays { get; set; }
        public string Priority { get; set; } = "Normal"; // "Low", "Normal", "High", "Critical", "Medium", "Urgent"
    }    // Student Attendance Record
    public class StudentAttendanceRecord
    {
        public string Id { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public DateTime ClassDate { get; set; }
        public DateTime SessionDate { get; set; }
        public string ClassType { get; set; } = "Lecture"; // "Lecture", "Lab", "Tutorial"
        public string SessionType { get; set; } = "Lecture"; // "Lecture", "Lab", "Tutorial"
        public string Topic { get; set; } = string.Empty;
        public string Status { get; set; } = "Present"; // "Present", "Absent", "Late"
        public bool IsPresent { get; set; }
        public bool IsLate { get; set; }
        public int LateByMinutes { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string? Duration { get; set; }
        public string? Remarks { get; set; }
        public DateTime MarkedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string MarkedBy { get; set; } = string.Empty; // Teacher name
        public TimeSpan? DurationTimeSpan { get; set; }
        public string StatusMarked { get; set; } = "Marked"; // "Marked", "Pending", "Disputed"
    }    // Student Results/Grades
    public class StudentResult
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string ExamType { get; set; } = string.Empty;
        public DateTime? ExamDate { get; set; }
        
        // Assessment scores
        public double? MidtermScore { get; set; }
        public double? FinalScore { get; set; }
        public double? AssignmentAverage { get; set; }
        public double? QuizAverage { get; set; }
        public double? ProjectScore { get; set; }
        public double? LabScore { get; set; }
        public double MaxMarks { get; set; }
        public double ObtainedMarks { get; set; }
        public double Percentage { get; set; }
          // Overall grades
        public double? TotalScore { get; set; }
        public string? LetterGrade { get; set; }
        public string? Grade { get; set; } // Alias for LetterGrade
        public double? GPA { get; set; }
        public double? GradePoint { get; set; }
        public double? MarksObtained { get; set; } // Alias for ObtainedMarks
        public double? TotalMarks { get; set; } // Alias for MaxMarks
        public string? Semester { get; set; } // Add semester property
        public int Rank { get; set; }
        public int TotalStudents { get; set; }
        public string Status { get; set; } = "In Progress";
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? PublishedDate { get; set; }        public DateTime? LastUpdated { get; set; }
        public string? Comments { get; set; }
        
        // Additional properties for comprehensive tracking
        public string? TeacherComments { get; set; }
        public double? SubjectAverage { get; set; }
        public bool IsImprovement { get; set; }
        public double? PreviousMarks { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Progress tracking
        public double ProgressPercentage { get; set; }
        public string Trend { get; set; } = "Stable"; // "Improving", "Declining", "Stable"
        public List<AssessmentBreakdown> Assessments { get; set; } = new();
    }

    public class AssessmentBreakdown
    {
        public string Type { get; set; } = string.Empty;
        public double Score { get; set; }
        public double MaxScore { get; set; }
        public double Percentage => MaxScore > 0 ? (Score / MaxScore) * 100 : 0;
        public string Grade { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
    }    // Student Notices/Notifications
    public class StudentNotice
    {
        public string Id { get; set; } = string.Empty;
        public string NoticeId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = "General"; // "General", "Assignment", "Exam", "Holiday", "Emergency", "Alert"
        public string Priority { get; set; } = "Normal"; // "Low", "Normal", "High", "Urgent"
        
        public string? CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string SenderType { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsImportant { get; set; }
        public bool IsUrgent { get; set; }
        public bool RequiresAction { get; set; }
        public string? ActionUrl { get; set; }
        public string Status { get; set; } = "Unread"; // "Unread", "Read", "Archived"
        public bool HasAttachment { get; set; }
        public string? AttachmentUrl { get; set; }
        public string DeliveryStatus { get; set; } = "Delivered";
    }    // Student Schedule/Timetable
    public class StudentSchedule
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ClassType { get; set; } = "Lecture";
        public string Venue { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? LastModified { get; set; }
    }    // Student Event/Deadline
    public class StudentEvent
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // "Assignment", "Exam", "Quiz", "Project", "Holiday"
        public string Type { get; set; } = string.Empty; // "Assignment", "Exam", "Quiz", "Project", "Holiday"
        public string? CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;        public string CourseCode { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Venue { get; set; } = string.Empty;
        public bool IsAllDay { get; set; }
        public string Priority { get; set; } = "Normal";
        public bool ReminderSet { get; set; }
        public DateTime? ReminderTime { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = "Upcoming"; // "Upcoming", "Today", "Overdue", "Completed"
        public string? Location { get; set; }
        public bool HasReminder { get; set; }
        public string Color { get; set; } = "#007bff";
    }

    // Student Performance Analytics
    public class StudentPerformance
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public double OverallGPA { get; set; }
        public double CurrentSemesterGPA { get; set; }
        public double AttendanceRate { get; set; }
        public int CompletedCourses { get; set; }
        public int CurrentCourses { get; set; }
        public int TotalCreditsEarned { get; set; }        public double AverageAssignmentScore { get; set; }
        public string AcademicStanding { get; set; } = "Good Standing";
        public List<SemesterPerformance> SemesterHistory { get; set; } = new();
        public List<CoursePerformance> CoursePerformances { get; set; } = new();
        
        // Additional properties for comprehensive tracking
        public double OverallAttendance { get; set; }
        public int TotalCourses { get; set; }
        public double AverageScore { get; set; }
        public double HighestScore { get; set; }
        public double LowestScore { get; set; }
        public int TotalExams { get; set; }
        public int PassedExams { get; set; }
        public int FailedExams { get; set; }
        public string CurrentSemester { get; set; } = string.Empty;
        public double TotalCreditHours { get; set; }
        public double CompletedCreditHours { get; set; }
        public string AttendanceStatus { get; set; } = string.Empty;
        public string GradeStatus { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class SemesterPerformance
    {
        public string Semester { get; set; } = string.Empty;
        public int Year { get; set; }
        public double GPA { get; set; }
        public int CreditsEarned { get; set; }
        public double AttendanceRate { get; set; }
        public string Standing { get; set; } = string.Empty;
    }

    public class CoursePerformance
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public double FinalGrade { get; set; }
        public string LetterGrade { get; set; } = string.Empty;
        public double AttendanceRate { get; set; }
        public string Semester { get; set; } = string.Empty;
        public int Year { get; set; }
    }

    // Student Profile
    public class StudentProfile
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }
        
        // Academic Information
        public string DepartmentId { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int CurrentSemester { get; set; }
        public string AcademicYear { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public string? Batch { get; set; }
        public string Status { get; set; } = "Active"; // "Active", "Inactive", "Graduated", "Dropped"
        
        // Emergency Contact
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelation { get; set; }
        
        // Profile Settings
        public bool EmailNotifications { get; set; } = true;
        public bool SMSNotifications { get; set; } = false;
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    // Student Request/Communication
    public class StudentRequest
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Leave", "Certificate", "Transcript", "General"
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
        public string Status { get; set; } = "Pending"; // "Pending", "In Progress", "Approved", "Rejected"
        public DateTime RequestDate { get; set; }
        public DateTime? ResponseDate { get; set; }
        public string? Response { get; set; }
        public string? ApprovedBy { get; set; }        public List<string> Attachments { get; set; } = new();
    }

    // Automation Alert for Students
    public class StudentAlert
    {
        public string Id { get; set; } = string.Empty;
        public string AlertId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty; // "Low Attendance", "Assignment Due", "poor_performance", "schedule_change"
        public string Type { get; set; } = string.Empty; // "low_attendance", "assignment_due", "poor_performance", "schedule_change"
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Normal"; // "Low", "Normal", "High", "Critical", "Medium"
        public string Priority { get; set; } = "Normal"; // "Low", "Normal", "High", "Critical"
        public string? CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? ReadDate { get; set; }
        public bool IsRead { get; set; }
        public bool ActionRequired { get; set; }
        public bool RequiresAction { get; set; }
        public string? ActionMessage { get; set; }
        public string? ActionUrl { get; set; }
        public string? ActionText { get; set; }        public string? TriggerValue { get; set; }
        public string? ThresholdValue { get; set; }
        public bool AutoGenerated { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}
