using System.ComponentModel.DataAnnotations;

namespace Campus360.Models
{
    // =============== FILE MANAGEMENT SYSTEM MODELS ===============

    /// <summary>
    /// File metadata model for document management
    /// </summary>
    public class FileDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        public string OriginalFileName { get; set; } = string.Empty;
        
        [Required]
        public string FileUrl { get; set; } = string.Empty;
        
        [Required]
        public FileType FileType { get; set; }
        
        [Required]
        public string UploadedBy { get; set; } = string.Empty;
        
        public string UploaderName { get; set; } = string.Empty;
        
        public string? SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        
        [Required]
        public FileVisibility Visibility { get; set; }
        
        public string Description { get; set; } = string.Empty;
        
        public long FileSize { get; set; }
        public string FileSizeFormatted => FormatFileSize(FileSize);
        
        public string ContentType { get; set; } = string.Empty;
        
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
        
        public int DownloadCount { get; set; }
        public DateTime? LastDownloaded { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }
        
        // Assignment specific fields
        public DateTime? DueDate { get; set; }
        public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.Now;
        public int? MaxScore { get; set; }
        
        // Notice specific fields
        public DateTime? ExpiryDate { get; set; }
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate < DateTime.Now;
        public NoticePriority Priority { get; set; } = NoticePriority.Normal;
        
        // Result specific fields
        public string? ExamType { get; set; }
        public string? Semester { get; set; }
        
        // Tags for categorization
        public List<string> Tags { get; set; } = new();
        public string TagsString => string.Join(", ", Tags);
        
        // Access control
        public List<string> AllowedRoles { get; set; } = new();
        public List<string> AllowedUserIds { get; set; } = new();
        
        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }

    /// <summary>
    /// File type enumeration
    /// </summary>
    public enum FileType
    {
        Assignment = 1,
        Notes = 2,
        Result = 3,
        Notice = 4,
        StudyMaterial = 5,
        Syllabus = 6,
        Timetable = 7,
        Circular = 8,
        Other = 99
    }

    /// <summary>
    /// File visibility levels
    /// </summary>
    public enum FileVisibility
    {
        Public = 1,        // Visible to all
        Student = 2,       // Visible to students only
        Teacher = 3,       // Visible to teachers only
        Admin = 4,         // Visible to admins only
        Department = 5,    // Visible to department members
        Course = 6,        // Visible to course enrollees
        Custom = 7         // Custom visibility rules
    }

    /// <summary>
    /// Notice priority levels
    /// </summary>
    public enum NoticePriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Urgent = 4
    }

    /// <summary>
    /// File upload model
    /// </summary>
    public class FileUploadModel
    {
        [Required]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        public FileType FileType { get; set; }
        
        public string? SubjectId { get; set; }
        public string? CourseId { get; set; }
        public string? DepartmentId { get; set; }
        
        [Required]
        public FileVisibility Visibility { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime? DueDate { get; set; }
        public int? MaxScore { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public NoticePriority Priority { get; set; } = NoticePriority.Normal;
        public string? ExamType { get; set; }
        public string? Semester { get; set; }
        
        public List<string> Tags { get; set; } = new();
        public List<string> AllowedRoles { get; set; } = new();
        public List<string> AllowedUserIds { get; set; } = new();
        
        // File data (in real implementation, this would be handled differently)
        public string FileContent { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }

    /// <summary>
    /// File download tracking model
    /// </summary>
    public class FileDownload
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FileId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public DateTime DownloadDate { get; set; } = DateTime.Now;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }

    /// <summary>
    /// File statistics model
    /// </summary>
    public class FileStatistics
    {
        public int TotalFiles { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalNotes { get; set; }
        public int TotalResults { get; set; }
        public int TotalNotices { get; set; }
        public int TotalDownloads { get; set; }
        public long TotalStorageUsed { get; set; }
        public string TotalStorageFormatted => FormatFileSize(TotalStorageUsed);
        
        public int FilesToday { get; set; }
        public int DownloadsToday { get; set; }
        
        public List<FileDocument> RecentFiles { get; set; } = new();
        public List<FileDocument> PopularFiles { get; set; } = new();
        public List<FileDocument> OverdueAssignments { get; set; } = new();
        public List<FileDocument> ExpiringNotices { get; set; } = new();
        
        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }

    /// <summary>
    /// File filter model for search and filtering
    /// </summary>
    public class FileFilterModel
    {
        public FileType? FileType { get; set; }
        public string? SubjectId { get; set; }
        public string? CourseId { get; set; }
        public string? DepartmentId { get; set; }
        public string? UploaderRole { get; set; }
        public DateTime? UploadedAfter { get; set; }
        public DateTime? UploadedBefore { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public string? Tag { get; set; }
        public bool? IsOverdue { get; set; }
        public bool? IsExpired { get; set; }
        public bool OnlyFeatured { get; set; }
        public string SortBy { get; set; } = "UploadDate";
        public bool SortDescending { get; set; } = true;
        public int PageSize { get; set; } = 20;
        public int Page { get; set; } = 1;
    }

    /// <summary>
    /// Bulk file operation model
    /// </summary>
    public class BulkFileOperation
    {
        public List<string> FileIds { get; set; } = new();
        public BulkOperationType Operation { get; set; }
        public string? NewVisibility { get; set; }
        public string? NewDepartment { get; set; }
        public string? NewCourse { get; set; }
        public List<string> NewTags { get; set; } = new();
    }

    /// <summary>
    /// Bulk operation types
    /// </summary>
    public enum BulkOperationType
    {
        Delete = 1,
        ChangeVisibility = 2,
        MoveToDepartment = 3,
        MoveToCourse = 4,
        AddTags = 5,
        RemoveTags = 6,
        Archive = 7,
        Restore = 8
    }

    /// <summary>
    /// File operation result
    /// </summary>
    public class FileOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FileId { get; set; }
        public string? FileUrl { get; set; }
        public bool AutomationTriggered { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        
        public static FileOperationResult Successful(string message = "Operation completed successfully", string? fileId = null, string? fileUrl = null, bool automationTriggered = false)
        {
            return new FileOperationResult
            {
                Success = true,
                Message = message,
                FileId = fileId,
                FileUrl = fileUrl,
                AutomationTriggered = automationTriggered
            };
        }
        
        public static FileOperationResult Failed(string message = "Operation failed", List<string>? errors = null)
        {
            return new FileOperationResult
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string> { message }
            };
        }
    }

    /// <summary>
    /// File access permission model
    /// </summary>
    public class FileAccessPermission
    {
        public string FileId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanDownload { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public DateTime? AccessExpiry { get; set; }
        public string GrantedBy { get; set; } = string.Empty;
        public DateTime GrantedDate { get; set; } = DateTime.Now;
    }
}
