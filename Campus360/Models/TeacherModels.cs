using System.ComponentModel.DataAnnotations;

namespace Campus360.Models
{    public class TeacherDashboardStats
    {
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public int PendingAssignments { get; set; }
        public int UpcomingClasses { get; set; }
        public int ClassesToday { get; set; }
        public int TotalNotifications { get; set; }
        public double AverageAttendance { get; set; }
        public List<TeacherActivityModel> RecentActivities { get; set; } = new();
    }

    public class TeacherActivityModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "attendance", "assignment", "result", "notice"
        public DateTime CreatedAt { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
    }    public class TeacherCourse
    {
        public string Id { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty; // Alias for Id for backward compatibility
        public string Name { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty; // Alias for Name for backward compatibility
        public string Code { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty; // Alias for Code for backward compatibility
        public string Description { get; set; } = string.Empty;
        public string DepartmentId { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty; // Alias for DepartmentName
        public int Semester { get; set; }
        public int CreditHours { get; set; }
        public int Credits { get; set; } // Alias for CreditHours
        public int EnrolledStudents { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string AcademicYear { get; set; } = string.Empty; // Academic year (e.g., 2024-2025)
        
        // Teacher-specific properties
        public int TotalClasses { get; set; }
        public int CompletedClasses { get; set; }
        public double AttendanceRate { get; set; }
        public int PendingAssignments { get; set; }
        public DateTime? LastClassDate { get; set; }
        public DateTime? NextClassDate { get; set; }
    }public class CourseStudent
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty; // Student roll number
        public string DepartmentName { get; set; } = string.Empty;
        public int Semester { get; set; }
        public DateTime EnrolledDate { get; set; }
        
        // Academic performance
        public double AttendancePercentage { get; set; }
        public int TotalClasses { get; set; }
        public int AttendedClasses { get; set; }
        public double? OverallGrade { get; set; }
        public string GradeStatus { get; set; } = "In Progress"; // "In Progress", "Passed", "Failed"
    }    public class AttendanceSession
    {
        public string Id { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty; // Alias for Id
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public DateTime ClassDate { get; set; }
        public DateTime SessionDate { get; set; } // Alias for ClassDate
        public string ClassType { get; set; } = "Lecture"; // "Lecture", "Lab", "Tutorial"
        public string SessionType { get; set; } = "Lecture"; // Alias for ClassType
        public string Topic { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int TotalStudents { get; set; }
        public int PresentStudents { get; set; }
        public int AbsentStudents { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<StudentAttendance> StudentAttendances { get; set; } = new();
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new();
    }
    
    public class AttendanceRecord
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
        public string? Remarks { get; set; }
        public DateTime MarkedAt { get; set; }
    }

    public class StudentAttendance
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string AttendanceSessionId { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
        public string? Remarks { get; set; }
        public DateTime MarkedAt { get; set; }
        public string MarkedBy { get; set; } = string.Empty; // Teacher ID
    }    public class AssignmentModel
    {
        public string Id { get; set; } = string.Empty;
        public string AssignmentId { get; set; } = string.Empty; // Alias for Id
        
        [Required(ErrorMessage = "Assignment title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Course is required")]
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Due date is required")]
        public DateTime DueDate { get; set; }
        
        [Range(1, 100, ErrorMessage = "Max score must be between 1 and 100")]
        public int MaxScore { get; set; } = 100;
        public int MaxMarks { get; set; } = 100; // Alias for MaxScore
        
        public string AssignmentType { get; set; } = "Assignment"; // "Assignment", "Quiz", "Project", "Midterm", "Final"
        public bool AllowLateSubmission { get; set; }
        public int? LatePenaltyPercentage { get; set; }
        public string? AttachmentUrl { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Submission statistics
        public int TotalStudents { get; set; }
        public int SubmittedCount { get; set; }
        public int PendingCount { get; set; }
        public int GradedCount { get; set; }
        public double? AverageScore { get; set; }
    }

    public class StudentSubmission
    {
        public string Id { get; set; } = string.Empty;
        public string AssignmentId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string? SubmissionText { get; set; }
        public string? AttachmentUrl { get; set; }
        public double? Score { get; set; }
        public string? Feedback { get; set; }
        public string Status { get; set; } = "Submitted"; // "Submitted", "Graded", "Late", "Missing"
        public bool IsLateSubmission { get; set; }
        public DateTime? GradedAt { get; set; }
        public string? GradedBy { get; set; }
    }    public class ResultModel
    {
        public string Id { get; set; } = string.Empty;
        public string ResultId { get; set; } = string.Empty; // Alias for Id
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty; // Student roll number
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        
        // Exam results
        public double? MidtermScore { get; set; }
        public double? FinalScore { get; set; }
        public double? AssignmentAverage { get; set; }
        public double? QuizAverage { get; set; }
        public double? ProjectScore { get; set; }
        public double? MarksObtained { get; set; } // Marks obtained in the assignment
        
        // Calculated grades
        public double? TotalScore { get; set; }
        public string? LetterGrade { get; set; }
        public string? Grade { get; set; } // Alias for LetterGrade
        public double? GPA { get; set; }
        public string Status { get; set; } = "In Progress"; // "In Progress", "Completed", "Failed"
        
        public bool IsGraded { get; set; } = false; // Whether this result has been graded
        public bool HasChanges { get; set; } = false; // For UI tracking of unsaved changes
        public DateTime? LastUpdated { get; set; }
        public string? Comments { get; set; }
    }    public class NoticeModel
    {
        public string Id { get; set; } = string.Empty;
        public string NoticeId { get; set; } = string.Empty; // Alias for Id
        
        [Required(ErrorMessage = "Notice title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Notice content is required")]
        [StringLength(2000, ErrorMessage = "Content cannot exceed 2000 characters")]
        public string Content { get; set; } = string.Empty;
        
        public string? CourseId { get; set; } // If null, notice is for all courses
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        
        public string Priority { get; set; } = "Normal"; // "Low", "Normal", "High", "Urgent"
        public string Type { get; set; } = "General"; // "General", "Assignment", "Exam", "Holiday", "Emergency"
        
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? PublishedDate { get; set; } // Alias for PublishedAt
        public DateTime? ScheduledDate { get; set; } // For scheduled notices
        public DateTime? CreatedDate { get; set; } // Alias for CreatedAt
        public DateTime? ExpiryDate { get; set; }
        
        public string CreatedBy { get; set; } = string.Empty; // Teacher ID
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Delivery statistics
        public int TotalRecipients { get; set; }
        public int RecipientCount { get; set; } // Alias for TotalRecipients
        public int DeliveredCount { get; set; }
        public int ReadCount { get; set; }
          // Status tracking
        public string Status { get; set; } = "Draft"; // "Draft", "Published", "Scheduled", "Expired"
    }

    public class ClassSchedule
    {
        public string Id { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty; // "Monday", "Tuesday", etc.
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Venue { get; set; } = string.Empty;
        public string ClassType { get; set; } = "Lecture"; // "Lecture", "Lab", "Tutorial"
        public bool IsActive { get; set; } = true;
    }

    public class TeacherCreateAssignmentModel
    {
        [Required(ErrorMessage = "Assignment title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Course is required")]
        public string CourseId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Due date is required")]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);
        
        [Range(1, 100, ErrorMessage = "Max score must be between 1 and 100")]
        public int MaxScore { get; set; } = 100;
        
        public string AssignmentType { get; set; } = "Assignment";
        public bool AllowLateSubmission { get; set; }
        
        [Range(0, 50, ErrorMessage = "Late penalty must be between 0 and 50 percent")]
        public int? LatePenaltyPercentage { get; set; }
        
        public bool IsPublished { get; set; } = true;
    }

    public class TeacherMarkAttendanceModel
    {
        [Required(ErrorMessage = "Course is required")]
        public string CourseId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Class date is required")]
        public DateTime ClassDate { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "Class type is required")]
        public string ClassType { get; set; } = "Lecture";
        
        [StringLength(200, ErrorMessage = "Topic cannot exceed 200 characters")]
        public string Topic { get; set; } = string.Empty;
        
        public List<StudentAttendanceMarkModel> Students { get; set; } = new();
    }

    public class StudentAttendanceMarkModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
        public string? Remarks { get; set; }
    }
}
