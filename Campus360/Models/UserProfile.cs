namespace Campus360.Models
{    
    public class UserProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? DepartmentId { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Authentication and status properties
        public bool IsVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }

        // Extended student profile properties
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? StudentId { get; set; }
        public string? Program { get; set; }
        public string? Department { get; set; }
        public string? AcademicYear { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelationship { get; set; }
    }

    public class Assignment
    {
        public string Id { get; set; } = string.Empty;
        public string CourseId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public int MaxScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AuthenticationResult
    {
        public bool IsSuccess { get; set; }
        public string? AccessToken { get; set; }
        public UserProfile? User { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
