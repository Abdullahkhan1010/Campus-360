using System.ComponentModel.DataAnnotations;

namespace Campus360.Models
{
    public class Department
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Department code is required")]
        [StringLength(10, ErrorMessage = "Department code cannot exceed 10 characters")]
        public string Code { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class Course
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Course name is required")]
        [StringLength(100, ErrorMessage = "Course name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Course code is required")]
        [StringLength(20, ErrorMessage = "Course code cannot exceed 20 characters")]
        public string Code { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Department is required")]
        public string DepartmentId { get; set; } = string.Empty;
        
        public string? TeacherId { get; set; }
        
        [Range(1, 8, ErrorMessage = "Semester must be between 1 and 8")]
        public int Semester { get; set; } = 1;
        
        [Range(1, 10, ErrorMessage = "Credit hours must be between 1 and 10")]
        public int CreditHours { get; set; } = 3;
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public Department? Department { get; set; }
        public UserProfile? Teacher { get; set; }
    }

    public class UserManagementModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public List<string> AssignedCourses { get; set; } = new();
    }    public class AdminDashboardStats
    {
        public int TotalUsers { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalDepartments { get; set; }
        public int ActiveCourses { get; set; }
        public int PendingApprovals { get; set; }
        public int UnverifiedUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int TotalDepartmentCount { get; set; }
        public int TotalCourseCount { get; set; }
        public List<RecentActivity> RecentActivities { get; set; } = new();
    }

    public class RecentActivity
    {
        public string Id { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ActivityType { get; set; } = string.Empty; // "user_created", "course_assigned", etc.
    }    public class ActivityModel
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class CourseAssignmentModel
    {
        public string CourseId { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }    public class UserCreationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public bool RequiresEmailVerification { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public string? GeneratedPassword { get; set; }
    }
}
