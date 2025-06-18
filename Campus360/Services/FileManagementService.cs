using Campus360.Models;

namespace Campus360.Services
{
    /// <summary>
    /// Comprehensive File Management Service for Campus360
    /// Handles file uploads, downloads, storage, and access control
    /// </summary>    
    public class FileManagementService
    {
        private readonly HttpClient _httpClient;
        private readonly AutomationEngineService _automationEngine;
        private readonly UserContextService _userContext;
        private readonly DatabaseService _databaseService;

        public FileManagementService(
            HttpClient httpClient, 
            AutomationEngineService automationEngine,
            UserContextService userContext,
            DatabaseService databaseService)
        {
            _httpClient = httpClient;
            _automationEngine = automationEngine;
            _userContext = userContext;
            _databaseService = databaseService;        }

        // File downloads tracking (will be moved to database in future)
        private static List<FileDownload> _fileDownloads = new();

        // =============== FILE UPLOAD METHODS ===============

        /// <summary>
        /// Upload a new file to the system
        /// </summary>
        public async Task<FileOperationResult> UploadFileAsync(FileUploadModel uploadModel, string uploadedBy)
        {
            try
            {
                await Task.Delay(500); // Simulate file upload processing

                // Validate file
                var validationResult = ValidateFile(uploadModel);
                if (!validationResult.Success)
                {
                    return validationResult;
                }

                // Generate unique file name
                var fileExtension = Path.GetExtension(uploadModel.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = GetStoragePath(uploadModel.FileType, uniqueFileName);

                // Create file document
                var fileDocument = new FileDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    FileName = uniqueFileName,
                    OriginalFileName = uploadModel.FileName,
                    FileUrl = filePath,
                    FileType = uploadModel.FileType,
                    UploadedBy = uploadedBy,
                    UploaderName = await GetUploaderNameAsync(uploadedBy),
                    SubjectId = uploadModel.SubjectId,
                    SubjectName = await GetSubjectNameAsync(uploadModel.SubjectId),
                    CourseId = uploadModel.CourseId,
                    CourseName = await GetCourseNameAsync(uploadModel.CourseId),
                    DepartmentId = uploadModel.DepartmentId,
                    DepartmentName = await GetDepartmentNameAsync(uploadModel.DepartmentId),
                    Visibility = uploadModel.Visibility,
                    Description = uploadModel.Description,
                    FileSize = uploadModel.FileSize,
                    ContentType = uploadModel.ContentType,
                    UploadDate = DateTime.Now,
                    DueDate = uploadModel.DueDate,
                    MaxScore = uploadModel.MaxScore,
                    ExpiryDate = uploadModel.ExpiryDate,
                    Priority = uploadModel.Priority,
                    ExamType = uploadModel.ExamType,
                    Semester = uploadModel.Semester,
                    Tags = uploadModel.Tags,
                    AllowedRoles = uploadModel.AllowedRoles,
                    AllowedUserIds = uploadModel.AllowedUserIds,
                    IsActive = true
                };                // Convert to database model and save
                var fileDocumentDb = new FileDocumentDb
                {
                    Id = fileDocument.Id,
                    FileName = fileDocument.FileName,
                    OriginalFileName = fileDocument.OriginalFileName,
                    FileUrl = fileDocument.FileUrl,
                    FileType = fileDocument.FileType.ToString(),
                    UploadedBy = fileDocument.UploadedBy,
                    UploaderName = fileDocument.UploaderName,
                    SubjectId = fileDocument.SubjectId,
                    SubjectName = fileDocument.SubjectName,
                    CourseId = fileDocument.CourseId,
                    CourseName = fileDocument.CourseName,
                    DepartmentId = fileDocument.DepartmentId,
                    DepartmentName = fileDocument.DepartmentName,
                    Visibility = fileDocument.Visibility.ToString(),
                    Description = fileDocument.Description,
                    FileSize = fileDocument.FileSize,
                    ContentType = fileDocument.ContentType,
                    DownloadCount = fileDocument.DownloadCount,
                    LastDownloaded = fileDocument.LastDownloaded,
                    IsActive = fileDocument.IsActive,
                    IsFeatured = fileDocument.IsFeatured,
                    DueDate = fileDocument.DueDate,
                    MaxScore = fileDocument.MaxScore,
                    Tags = fileDocument.Tags,
                    Metadata = new Dictionary<string, object>(), // Initialize empty metadata
                    UploadDate = fileDocument.UploadDate,
                    LastModified = fileDocument.LastModified ?? DateTime.Now
                };
                
                await _databaseService.InsertAsync(fileDocumentDb);

                // Trigger automation based on file type
                bool automationTriggered = await TriggerFileUploadAutomationAsync(fileDocument);

                return FileOperationResult.Successful(
                    "File uploaded successfully",
                    fileDocument.Id,
                    fileDocument.FileUrl,
                    automationTriggered);
            }
            catch (Exception ex)
            {
                return FileOperationResult.Failed($"Upload failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Upload multiple files in bulk
        /// </summary>
        public async Task<List<FileOperationResult>> BulkUploadFilesAsync(List<FileUploadModel> uploadModels, string uploadedBy)
        {
            var results = new List<FileOperationResult>();

            foreach (var uploadModel in uploadModels)
            {
                var result = await UploadFileAsync(uploadModel, uploadedBy);
                results.Add(result);
                
                // Small delay between uploads
                await Task.Delay(100);
            }

            return results;
        }

        // =============== FILE RETRIEVAL METHODS ===============

        /// <summary>
        /// Get files accessible to a user with filtering
        /// </summary>        
         public async Task<List<FileDocument>> GetUserAccessibleFilesAsync(string userId, string userRole, FileFilterModel? filter = null)
        {
            // Get files from database
            var allFiles = await _databaseService.GetFilesForUserAsync(userId);
            
            var query = allFiles
                .Where(f => f.IsActive && CanUserAccessFile(f, userId, userRole))
                .AsQueryable();

            if (filter != null)
            {
                query = ApplyFilters(query, filter);
            }

            // Apply sorting
            var sortBy = filter?.SortBy ?? "UploadDate";
            var sortDesc = filter?.SortDescending ?? true;

            query = sortBy.ToLower() switch
            {
                "filename" => sortDesc ? query.OrderByDescending(f => f.OriginalFileName) : query.OrderBy(f => f.OriginalFileName),
                "filesize" => sortDesc ? query.OrderByDescending(f => f.FileSize) : query.OrderBy(f => f.FileSize),
                "downloads" => sortDesc ? query.OrderByDescending(f => f.DownloadCount) : query.OrderBy(f => f.DownloadCount),
                "duedate" => sortDesc ? query.OrderByDescending(f => f.DueDate) : query.OrderBy(f => f.DueDate),
                _ => sortDesc ? query.OrderByDescending(f => f.UploadDate) : query.OrderBy(f => f.UploadDate)
            };

            // Apply pagination
            var pageSize = filter?.PageSize ?? 20;
            var page = filter?.Page ?? 1;
            var skip = (page - 1) * pageSize;

            return query.Skip(skip).Take(pageSize).ToList();
        }        /// <summary>
        /// Get files uploaded by a specific user
        /// </summary>
        public async Task<List<FileDocument>> GetUserUploadedFilesAsync(string userId, FileType? fileType = null)
        {
            // Get files from database and filter by uploader
            var allFiles = await _databaseService.GetFilesForUserAsync(userId);
            
            var query = allFiles
                .Where(f => f.UploadedBy == userId && f.IsActive)
                .AsQueryable();

            if (fileType.HasValue)
            {
                query = query.Where(f => f.FileType == fileType.Value);
            }

            return query.OrderByDescending(f => f.UploadDate).ToList();
        }/// <summary>
        /// Get a specific file by ID
        /// </summary>
        public async Task<FileDocument?> GetFileByIdAsync(string fileId)
        {
            var allFiles = await _databaseService.GetFilesForUserAsync("system");
            return allFiles.FirstOrDefault(f => f.Id == fileId && f.IsActive);
        }        /// <summary>
        /// Get files by course
        /// </summary>
        public async Task<List<FileDocument>> GetCourseFilesAsync(string courseId, string userId, string userRole)
        {
            var allFiles = await _databaseService.GetFilesForUserAsync(userId);
            
            return allFiles
                .Where(f => f.CourseId == courseId && f.IsActive && CanUserAccessFile(f, userId, userRole))
                .OrderByDescending(f => f.UploadDate)
                .ToList();
        }        /// <summary>
        /// Get files by department
        /// </summary>
        public async Task<List<FileDocument>> GetDepartmentFilesAsync(string departmentId, string userId, string userRole)
        {
            var allFiles = await _databaseService.GetFilesForUserAsync(userId);
            
            return allFiles
                .Where(f => f.DepartmentId == departmentId && f.IsActive && CanUserAccessFile(f, userId, userRole))
                .OrderByDescending(f => f.UploadDate)
                .ToList();
        }

        // =============== FILE DOWNLOAD METHODS ===============

        /// <summary>
        /// Download a file (track download and return file URL)
        /// </summary>
        public async Task<FileOperationResult> DownloadFileAsync(string fileId, string userId, string userRole)
        {
            try
            {
                await Task.Delay(100);

                var file = await GetFileByIdAsync(fileId);
                if (file == null)
                {
                    return FileOperationResult.Failed("File not found");
                }

                if (!CanUserAccessFile(file, userId, userRole))
                {
                    return FileOperationResult.Failed("Access denied");
                }

                // Track download
                await TrackFileDownloadAsync(fileId, userId, userRole);

                // Update file download count
                file.DownloadCount++;
                file.LastDownloaded = DateTime.Now;

                return FileOperationResult.Successful(
                    "File download initiated",
                    fileId,
                    file.FileUrl);
            }
            catch (Exception ex)
            {
                return FileOperationResult.Failed($"Download failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get download URL for a file (for direct access)
        /// </summary>
        public async Task<string?> GetFileDownloadUrlAsync(string fileId, string userId, string userRole)
        {
            var result = await DownloadFileAsync(fileId, userId, userRole);
            return result.Success ? result.FileUrl : null;
        }

        // =============== FILE MANAGEMENT METHODS ===============

        /// <summary>
        /// Update file metadata
        /// </summary>
        public async Task<FileOperationResult> UpdateFileAsync(string fileId, FileUploadModel updateModel, string updatedBy)
        {
            try
            {
                await Task.Delay(200);

                var file = await GetFileByIdAsync(fileId);
                if (file == null)
                {
                    return FileOperationResult.Failed("File not found");
                }

                // Check permissions
                if (!CanUserEditFile(file, updatedBy))
                {
                    return FileOperationResult.Failed("Permission denied");
                }

                // Update file metadata
                file.Description = updateModel.Description;
                file.Visibility = updateModel.Visibility;
                file.DueDate = updateModel.DueDate;
                file.MaxScore = updateModel.MaxScore;
                file.ExpiryDate = updateModel.ExpiryDate;
                file.Priority = updateModel.Priority;
                file.ExamType = updateModel.ExamType;
                file.Semester = updateModel.Semester;
                file.Tags = updateModel.Tags;
                file.AllowedRoles = updateModel.AllowedRoles;
                file.AllowedUserIds = updateModel.AllowedUserIds;
                file.LastModified = DateTime.Now;

                return FileOperationResult.Successful("File updated successfully");
            }
            catch (Exception ex)
            {
                return FileOperationResult.Failed($"Update failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        public async Task<FileOperationResult> DeleteFileAsync(string fileId, string deletedBy)
        {
            try
            {
                await Task.Delay(200);

                var file = await GetFileByIdAsync(fileId);
                if (file == null)
                {
                    return FileOperationResult.Failed("File not found");
                }

                // Check permissions
                if (!CanUserDeleteFile(file, deletedBy))
                {
                    return FileOperationResult.Failed("Permission denied");
                }

                // Soft delete
                file.IsActive = false;

                return FileOperationResult.Successful("File deleted successfully");
            }
            catch (Exception ex)
            {
                return FileOperationResult.Failed($"Delete failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Perform bulk operations on files
        /// </summary>
        public async Task<List<FileOperationResult>> BulkOperationAsync(BulkFileOperation operation, string performedBy)
        {
            var results = new List<FileOperationResult>();

            foreach (var fileId in operation.FileIds)
            {
                var result = operation.Operation switch
                {
                    BulkOperationType.Delete => await DeleteFileAsync(fileId, performedBy),
                    BulkOperationType.Archive => await ArchiveFileAsync(fileId, performedBy),
                    BulkOperationType.Restore => await RestoreFileAsync(fileId, performedBy),
                    _ => FileOperationResult.Failed("Unsupported operation")
                };

                results.Add(result);
            }

            return results;
        }

        // =============== STATISTICS AND REPORTING ===============        /// <summary>
        /// Get file statistics for dashboard
        /// </summary>
        public async Task<FileStatistics> GetFileStatisticsAsync(string userId, string userRole)
        {
            var allFiles = await _databaseService.GetFilesForUserAsync(userId);
            var accessibleFiles = allFiles.Where(f => f.IsActive && CanUserAccessFile(f, userId, userRole)).ToList();
            var today = DateTime.Today;

            return new FileStatistics
            {
                TotalFiles = accessibleFiles.Count,
                TotalAssignments = accessibleFiles.Count(f => f.FileType == FileType.Assignment),
                TotalNotes = accessibleFiles.Count(f => f.FileType == FileType.Notes),
                TotalResults = accessibleFiles.Count(f => f.FileType == FileType.Result),
                TotalNotices = accessibleFiles.Count(f => f.FileType == FileType.Notice),
                TotalDownloads = accessibleFiles.Sum(f => f.DownloadCount),
                TotalStorageUsed = accessibleFiles.Sum(f => f.FileSize),
                FilesToday = accessibleFiles.Count(f => f.UploadDate.Date == today),
                DownloadsToday = _fileDownloads.Count(d => d.DownloadDate.Date == today),
                RecentFiles = accessibleFiles.OrderByDescending(f => f.UploadDate).Take(5).ToList(),
                PopularFiles = accessibleFiles.OrderByDescending(f => f.DownloadCount).Take(5).ToList(),
                OverdueAssignments = accessibleFiles.Where(f => f.FileType == FileType.Assignment && f.IsOverdue).ToList(),
                ExpiringNotices = accessibleFiles.Where(f => f.FileType == FileType.Notice && f.ExpiryDate.HasValue && f.ExpiryDate.Value.AddDays(-3) <= DateTime.Now).ToList()
            };
        }

        // =============== PRIVATE HELPER METHODS ===============

        private FileOperationResult ValidateFile(FileUploadModel uploadModel)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(uploadModel.FileName))
                errors.Add("File name is required");

            if (uploadModel.FileSize <= 0)
                errors.Add("File size must be greater than 0");

            if (uploadModel.FileSize > 50 * 1024 * 1024) // 50MB limit
                errors.Add("File size cannot exceed 50MB");

            // Validate file type based on content type
            if (!IsValidFileType(uploadModel.ContentType, uploadModel.FileType))
                errors.Add("Invalid file type for the selected category");

            if (errors.Any())
                return FileOperationResult.Failed("Validation failed", errors);

            return FileOperationResult.Successful();
        }

        private bool IsValidFileType(string contentType, FileType fileType)
        {
            var allowedTypes = new Dictionary<FileType, List<string>>
            {
                [FileType.Assignment] = new() { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                [FileType.Notes] = new() { "application/pdf", "application/vnd.ms-powerpoint", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                [FileType.Result] = new() { "application/pdf", "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                [FileType.Notice] = new() { "application/pdf", "image/jpeg", "image/png" },
                [FileType.StudyMaterial] = new() { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                [FileType.Syllabus] = new() { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                [FileType.Timetable] = new() { "application/pdf", "image/jpeg", "image/png", "application/vnd.ms-excel" },
                [FileType.Circular] = new() { "application/pdf", "image/jpeg", "image/png" }
            };

            return allowedTypes.GetValueOrDefault(fileType, new())?.Contains(contentType) ?? true;
        }

        private string GetStoragePath(FileType fileType, string fileName)
        {
            var folder = fileType.ToString().ToLower();
            return $"/storage/{folder}/{fileName}";
        }

        private async Task<string> GetUploaderNameAsync(string uploaderId)
        {
            // In real implementation, fetch from user service
            await Task.Delay(50);
            return uploaderId switch
            {
                "teacher1" => "Dr. John Smith",
                "teacher2" => "Prof. Sarah Johnson",
                "00000000-0000-0000-0000-000000000001" => "Admin User",
                _ => "Unknown User"
            };
        }

        private async Task<string> GetSubjectNameAsync(string? subjectId)
        {
            if (string.IsNullOrEmpty(subjectId)) return string.Empty;
            await Task.Delay(50);
            return subjectId switch
            {
                "sub1" => "Data Structures",
                "sub2" => "Web Development",
                _ => "Unknown Subject"
            };
        }

        private async Task<string> GetCourseNameAsync(string? courseId)
        {
            if (string.IsNullOrEmpty(courseId)) return string.Empty;
            await Task.Delay(50);
            return courseId switch
            {
                "1" => "Data Structures",
                "2" => "Web Development",
                _ => "Unknown Course"
            };
        }

        private async Task<string> GetDepartmentNameAsync(string? departmentId)
        {
            if (string.IsNullOrEmpty(departmentId)) return string.Empty;
            await Task.Delay(50);
            return departmentId switch
            {
                "1" => "Computer Science",
                "2" => "Information Technology",
                _ => "Unknown Department"
            };
        }

        private bool CanUserAccessFile(FileDocument file, string userId, string userRole)
        {
            // Check if user is the uploader
            if (file.UploadedBy == userId) return true;

            // Check role-based access
            if (file.AllowedRoles.Contains(userRole)) return true;

            // Check specific user access
            if (file.AllowedUserIds.Contains(userId)) return true;

            // Check visibility rules
            return file.Visibility switch
            {
                FileVisibility.Public => true,
                FileVisibility.Student => userRole == "student" || userRole == "teacher" || userRole == "admin",
                FileVisibility.Teacher => userRole == "teacher" || userRole == "admin",
                FileVisibility.Admin => userRole == "admin",
                FileVisibility.Department => true, // In real implementation, check department membership
                FileVisibility.Course => true, // In real implementation, check course enrollment
                _ => false
            };
        }

        private bool CanUserEditFile(FileDocument file, string userId)
        {
            // Only uploader or admin can edit
            return file.UploadedBy == userId || IsAdmin(userId);
        }

        private bool CanUserDeleteFile(FileDocument file, string userId)
        {
            // Only uploader or admin can delete
            return file.UploadedBy == userId || IsAdmin(userId);
        }

        private bool IsAdmin(string userId)
        {
            // In real implementation, check user role from context
            return userId == "00000000-0000-0000-0000-000000000001";
        }

        private IQueryable<FileDocument> ApplyFilters(IQueryable<FileDocument> query, FileFilterModel filter)
        {
            if (filter.FileType.HasValue)
                query = query.Where(f => f.FileType == filter.FileType.Value);

            if (!string.IsNullOrEmpty(filter.SubjectId))
                query = query.Where(f => f.SubjectId == filter.SubjectId);

            if (!string.IsNullOrEmpty(filter.CourseId))
                query = query.Where(f => f.CourseId == filter.CourseId);

            if (!string.IsNullOrEmpty(filter.DepartmentId))
                query = query.Where(f => f.DepartmentId == filter.DepartmentId);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchLower = filter.SearchTerm.ToLower();
                query = query.Where(f => 
                    f.OriginalFileName.ToLower().Contains(searchLower) ||
                    f.Description.ToLower().Contains(searchLower) ||
                    f.Tags.Any(t => t.ToLower().Contains(searchLower)));
            }

            if (!string.IsNullOrEmpty(filter.Tag))
                query = query.Where(f => f.Tags.Contains(filter.Tag));

            if (filter.IsOverdue.HasValue)
                query = query.Where(f => f.IsOverdue == filter.IsOverdue.Value);

            if (filter.IsExpired.HasValue)
                query = query.Where(f => f.IsExpired == filter.IsExpired.Value);

            if (filter.OnlyFeatured)
                query = query.Where(f => f.IsFeatured);

            if (filter.UploadedAfter.HasValue)
                query = query.Where(f => f.UploadDate >= filter.UploadedAfter.Value);

            if (filter.UploadedBefore.HasValue)
                query = query.Where(f => f.UploadDate <= filter.UploadedBefore.Value);

            return query;
        }

        private async Task<bool> TriggerFileUploadAutomationAsync(FileDocument file)
        {
            try
            {
                switch (file.FileType)
                {
                    case FileType.Assignment:
                        await _automationEngine.TriggerCustomEventAsync(
                            file.Id,
                            $"New Assignment: {file.OriginalFileName}",
                            $"A new assignment '{file.OriginalFileName}' has been uploaded for {file.CourseName}. Due: {file.DueDate:yyyy-MM-dd}",
                            "student",
                            file.UploadedBy);
                        return true;

                    case FileType.Result:
                        await _automationEngine.TriggerCustomEventAsync(
                            file.Id,
                            $"Results Published: {file.OriginalFileName}",
                            $"Results for {file.ExamType} in {file.CourseName} have been published.",
                            "student",
                            file.UploadedBy);
                        return true;

                    case FileType.Notice:
                        await _automationEngine.TriggerCustomEventAsync(
                            file.Id,
                            $"Important Notice: {file.OriginalFileName}",
                            file.Description,
                            "all",
                            file.UploadedBy);
                        return true;

                    case FileType.Notes:
                        await _automationEngine.TriggerCustomEventAsync(
                            file.Id,
                            $"Study Material: {file.OriginalFileName}",
                            $"New study material has been uploaded for {file.CourseName}.",
                            "student",
                            file.UploadedBy);
                        return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Automation trigger failed: {ex.Message}");
            }

            return false;
        }

        private async Task TrackFileDownloadAsync(string fileId, string userId, string userRole)
        {
            var download = new FileDownload
            {
                FileId = fileId,
                UserId = userId,
                UserRole = userRole,
                DownloadDate = DateTime.Now,
                IpAddress = "127.0.0.1", // In real implementation, get actual IP
                UserAgent = "Campus360" // In real implementation, get actual user agent
            };

            _fileDownloads.Add(download);
            await Task.CompletedTask;
        }

        private async Task<FileOperationResult> ArchiveFileAsync(string fileId, string archivedBy)
        {
            // Implementation for archiving files
            await Task.Delay(100);
            return FileOperationResult.Successful("File archived successfully");
        }

        private async Task<FileOperationResult> RestoreFileAsync(string fileId, string restoredBy)
        {
            // Implementation for restoring files
            await Task.Delay(100);
            return FileOperationResult.Successful("File restored successfully");
        }
    }
}
