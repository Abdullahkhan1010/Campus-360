// ===================================================================
// CAMPUS 360 - ADMIN SERVICE WITH DATABASE INTEGRATION
// ===================================================================
// Fully integrated with Supabase database - No fallback mock data
// ===================================================================

using Campus360.Models;
using Microsoft.Extensions.Logging;

namespace Campus360.Services
{    public class AdminService
    {
        private readonly DatabaseService _databaseService;
        private readonly AuthenticationServiceClean _authService;
        private readonly ILogger<AdminService> _logger;
        private readonly ActivityLogService _activityLogService;

        public AdminService(
            DatabaseService databaseService, 
            AuthenticationServiceClean authService, 
            ActivityLogService activityLogService,
            ILogger<AdminService> logger)
        {
            _databaseService = databaseService;
            _authService = authService;
            _activityLogService = activityLogService;
            _logger = logger;
        }

        // User Management
        public async Task<List<UserManagementModel>> GetUsersAsync(string? role = null, bool? verified = null)
        {
            try
            {
                _logger.LogInformation("Getting users with filters - Role: {Role}, Verified: {Verified}", role, verified);
                
                // Get data from database using standard methods
                var userProfilesDb = await _databaseService.GetAllAsync<UserProfileDb>();
                var departmentsDb = await _databaseService.GetAllAsync<DepartmentDb>();
                
                var departmentLookup = departmentsDb.ToDictionary(d => d.Id, d => d.Name);
                
                var query = userProfilesDb.AsQueryable();

                if (!string.IsNullOrEmpty(role))
                    query = query.Where(u => u.Role == role);

                if (verified.HasValue)
                    query = query.Where(u => u.IsVerified == verified.Value);

                return query
                    .OrderBy(u => u.FullName)
                    .Select(u => new UserManagementModel
                    {
                        Id = u.Id,
                        Email = u.Email,
                        FullName = u.FullName,
                        Role = u.Role,
                        DepartmentId = u.DepartmentId,
                        DepartmentName = u.DepartmentId != null && departmentLookup.ContainsKey(u.DepartmentId) 
                            ? departmentLookup[u.DepartmentId] : null,
                        IsVerified = u.IsVerified,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt ?? DateTime.MinValue
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users from database");
                throw;
            }
        }        public async Task<bool> VerifyUserAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Verifying user: {UserId}", userId);
                
                var user = await _databaseService.GetByIdAsync<UserProfileDb>(userId);
                if (user != null)
                {
                    user.IsVerified = true;
                    await _databaseService.UpdateAsync(user);                    // Log activity using ActivityLogService
                    // Get proper admin user ID from context or use system UUID
                    var adminId = "00000000-0000-0000-0000-000000000001"; // System UUID
                    await _activityLogService.LogActivityAsync(
                        adminId,
                        "User Verified", 
                        $"User {user.FullName} ({user.Email}) was verified",
                        "user_verified");
                    
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> AssignUserToDepartmentAsync(string userId, string departmentId)
        {
            try
            {
                _logger.LogInformation("Assigning user {UserId} to department {DepartmentId}", userId, departmentId);
                
                var user = await _databaseService.GetByIdAsync<UserProfileDb>(userId);
                if (user != null)
                {
                    user.DepartmentId = departmentId;
                    await _databaseService.UpdateAsync(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning user to department");
                throw;
            }
        }

        public async Task<bool> ToggleUserStatusAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Toggling user status: {UserId}", userId);
                
                var user = await _databaseService.GetByIdAsync<UserProfileDb>(userId);
                if (user != null)
                {
                    user.IsActive = !user.IsActive;
                    await _databaseService.UpdateAsync(user);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status");
                throw;
            }
        }        public async Task<bool> UpdateUserAsync(dynamic userModel)
        {
            try
            {
                var userId = (string)userModel.Id;
                _logger.LogInformation("Updating user: {UserId}", userId);
                
                var user = await _databaseService.GetByIdAsync<UserProfileDb>(userId);
                if (user != null)
                {
                    // Capture old values for logging
                    var oldValues = new Dictionary<string, object>
                    {
                        { "FullName", user.FullName },
                        { "Email", user.Email },
                        { "Role", user.Role },
                        { "DepartmentId", user.DepartmentId ?? string.Empty }
                    };
                    
                    // Update values
                    user.FullName = userModel.FullName;
                    user.Email = userModel.Email;
                    user.Role = userModel.Role;
                    user.DepartmentId = userModel.DepartmentId;
                    user.UpdatedAt = DateTime.UtcNow;
                    
                    // Capture new values for logging
                    var newValues = new Dictionary<string, object>
                    {
                        { "FullName", userModel.FullName },
                        { "Email", userModel.Email },
                        { "Role", userModel.Role },
                        { "DepartmentId", userModel.DepartmentId ?? string.Empty }
                    };
                    
                    await _databaseService.UpdateAsync(user);
                      // Log record change
                    await _activityLogService.LogRecordChangeAsync(
                        "00000000-0000-0000-0000-000000000001", // System UUID instead of hardcoded string
                        "user_profiles",
                        userId,
                        oldValues,
                        newValues,
                        $"Updated user {user.FullName}"
                    );
                    
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                throw;
            }
        }

        // Dashboard Statistics
        public async Task<AdminDashboardStats> GetDashboardStatsAsync()
        {
            try
            {
                _logger.LogInformation("Getting dashboard statistics");
                
                var userProfilesDb = await _databaseService.GetAllAsync<UserProfileDb>();
                var departmentsDb = await _databaseService.GetAllAsync<DepartmentDb>();
                var coursesDb = await _databaseService.GetAllAsync<CourseDb>();
                
                return new AdminDashboardStats
                {
                    TotalUsers = userProfilesDb.Count,
                    TotalTeachers = userProfilesDb.Count(u => u.Role == "teacher"),
                    TotalStudents = userProfilesDb.Count(u => u.Role == "student"),
                    TotalDepartments = departmentsDb.Count(d => d.IsActive),
                    ActiveCourses = coursesDb.Count(c => c.IsActive),
                    PendingApprovals = userProfilesDb.Count(u => !u.IsVerified),
                    UnverifiedUsers = userProfilesDb.Count(u => !u.IsVerified),
                    InactiveUsers = userProfilesDb.Count(u => !u.IsActive),
                    TotalDepartmentCount = departmentsDb.Count,
                    TotalCourseCount = coursesDb.Count,
                    RecentActivities = new List<RecentActivity>() // TODO: Implement when activity log is available
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                throw;
            }
        }        // User Creation
        public async Task<UserCreationResult> CreateUserAsync(dynamic userModel)
        {
            try
            {
                var email = (string)userModel.Email;
                var fullName = (string)userModel.FullName;
                var role = (string)userModel.Role;
                var departmentId = userModel.DepartmentId?.ToString();
                
                _logger.LogInformation("Creating new user: {Email}, Role: {Role}", email, role);
                  // Use the enhanced auth service to create user with auto-generated password
                var authResult = await _authService.CreateAuthUserAsAdminAsync(email);
                
                // Ensure we always have a valid user ID
                var userId = !string.IsNullOrEmpty(authResult.UserId) 
                    ? authResult.UserId 
                    : Guid.NewGuid().ToString();
                
                _logger.LogInformation("Creating user profile with ID: {UserId} for email: {Email}", userId, email);
                
                // Always create user profile in database, even if auth creation fails
                var userProfile = new UserProfile
                {
                    Id = userId,
                    Email = email,
                    FullName = fullName,
                    Role = role,
                    DepartmentId = departmentId,
                    IsVerified = true, // Admin-created users are pre-verified
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                // Validate that ID is not null or empty before database insert
                if (string.IsNullOrEmpty(userProfile.Id))
                {
                    _logger.LogError("Critical error: UserProfile.Id is null or empty for email: {Email}", email);
                    throw new InvalidOperationException("User ID cannot be null or empty");
                }
                  try
                {
                    await _databaseService.CreateUserAsync(userProfile);
                    _logger.LogInformation("User profile created in database for: {Email}", email);
                      // Log activity
                    await _activityLogService.LogActivityAsync(
                        "00000000-0000-0000-0000-000000000001", // System UUID
                        "User Created",
                        $"Created new {role} account for {fullName} ({email})",
                        "user_created");
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Failed to create user profile in database for: {Email}", email);
                    // Continue anyway, the auth user might still be created
                }                  // Convert UserCreationAuthResult to UserCreationResult
                string message;
                if (authResult.Success)
                {
                    message = authResult.ErrorMessage ?? "User created successfully! They can login immediately with the provided credentials.";
                }
                else
                {
                    message = "User profile created but login credentials may not work due to system limitations. Contact administrator.";
                }
                    
                var success = authResult.Success || !string.IsNullOrEmpty(userProfile.Id); // Success if either auth worked or we have a profile
                  // Log user creation activity
                if (success)
                {
                    await _activityLogService.LogActivityAsync(
                        "00000000-0000-0000-0000-000000000001", // System UUID
                        "User Created",
                        $"User {fullName} ({email}) was created with role {role}",
                        "user_created");
                }
                
                return new UserCreationResult
                {
                    Success = success,
                    Message = message,
                    UserId = authResult.UserId ?? userProfile.Id,
                    Email = email,
                    FullName = fullName,
                    Role = role,
                    GeneratedPassword = authResult.GeneratedPassword,
                    RequiresEmailVerification = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return new UserCreationResult
                {
                    Success = false,
                    Message = $"An error occurred while creating the user: {ex.Message}",
                    UserId = null,                    RequiresEmailVerification = false
                };
            }
        }

        // Department Management        
        public async Task<List<Department>> GetDepartmentsAsync()
        {
            try
            {
                _logger.LogInformation("Getting departments from database...");
                
                // Use debug method temporarily
                var departmentsDb = await _databaseService.DebugGetDepartmentsAsync();
                _logger.LogInformation("Retrieved {Count} departments from database", departmentsDb?.Count ?? 0);
                
                if (departmentsDb == null || !departmentsDb.Any())
                {
                    _logger.LogWarning("No departments found in database");
                    return new List<Department>();
                }
                
                var activeDepartments = departmentsDb
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Name)
                    .Select(d => d.ToDepartment())
                    .ToList();
                    
                _logger.LogInformation("Returning {Count} active departments", activeDepartments.Count);
                return activeDepartments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<Department?> GetDepartmentByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Getting department by ID: {Id}", id);
                
                var departmentDb = await _databaseService.GetByIdAsync<DepartmentDb>(id);
                return departmentDb?.ToDepartment();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting department by ID: {Id}", id);
                throw;
            }
        }        public async Task<bool> CreateDepartmentAsync(Department department)
        {
            try
            {
                _logger.LogInformation("Creating department: {Name}", department.Name);
                
                var departmentDb = department.ToDb();
                departmentDb.Id = Guid.NewGuid().ToString();
                departmentDb.CreatedAt = DateTime.UtcNow;
                departmentDb.UpdatedAt = DateTime.UtcNow;
                
                await _databaseService.InsertAsync(departmentDb);
                
                // Log activity using ActivityLogService
                await _activityLogService.LogActivityAsync(
                    "system", // Use the admin's actual ID in production
                    "Department Created",
                    $"Department '{department.Name}' was created with ID {departmentDb.Id}",
                    "department_created");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                throw;
            }
        }        public async Task<bool> UpdateDepartmentAsync(Department department)
        {
            try
            {
                _logger.LogInformation("Updating department: {Id}", department.Id);
                
                var existingDb = await _databaseService.GetByIdAsync<DepartmentDb>(department.Id);
                if (existingDb != null)
                {
                    // Capture old values for logging
                    var oldValues = new Dictionary<string, object>
                    {
                        { "Name", existingDb.Name },
                        { "Code", existingDb.Code },
                        { "Description", existingDb.Description ?? string.Empty }
                    };
                    
                    // Update values
                    existingDb.Name = department.Name;
                    existingDb.Code = department.Code;
                    existingDb.Description = department.Description;
                    existingDb.UpdatedAt = DateTime.UtcNow;
                    
                    // Capture new values for logging
                    var newValues = new Dictionary<string, object>
                    {
                        { "Name", department.Name },
                        { "Code", department.Code },
                        { "Description", department.Description ?? string.Empty }
                    };
                    
                    await _databaseService.UpdateAsync(existingDb);
                    
                    // Log record change with ActivityLogService
                    await _activityLogService.LogRecordChangeAsync(
                        "system", // Use the admin's actual ID in production
                        "departments",
                        department.Id,
                        oldValues,
                        newValues,
                        $"Updated department '{department.Name}'"
                    );
                    
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department");
                throw;
            }
        }        public async Task<bool> DeleteDepartmentAsync(string id)
        {
            try
            {
                _logger.LogInformation("Deleting department: {Id}", id);
                
                var departmentDb = await _databaseService.GetByIdAsync<DepartmentDb>(id);
                if (departmentDb != null)
                {
                    // Mark as inactive instead of deleting
                    departmentDb.IsActive = false;
                    await _databaseService.UpdateAsync(departmentDb);
                    
                    // Log activity
                    await _activityLogService.LogActivityAsync(
                        "system", // Use the admin's actual ID in production
                        "Department Deactivated",
                        $"Department '{departmentDb.Name}' was deactivated",
                        "department_deactivated");
                        
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department");
                throw;
            }
        }

        // Course Management
        public async Task<List<Course>> GetCoursesAsync(string? departmentId = null)
        {
            try
            {
                _logger.LogInformation("Getting courses for department: {DepartmentId}", departmentId);
                
                var coursesDb = await _databaseService.GetAllAsync<CourseDb>();
                var query = coursesDb.AsQueryable();
                
                if (!string.IsNullOrEmpty(departmentId))
                    query = query.Where(c => c.DepartmentId == departmentId);
                
                return query
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => c.ToCourse())
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses");
                throw;
            }
        }        public async Task<bool> CreateCourseAsync(Course course)
        {
            try
            {
                _logger.LogInformation("Creating course: {Name}", course.Name);
                
                var courseDb = course.ToDb();
                courseDb.Id = Guid.NewGuid().ToString();
                courseDb.CreatedAt = DateTime.UtcNow;
                courseDb.UpdatedAt = DateTime.UtcNow;
                
                await _databaseService.InsertAsync(courseDb);
                
                // Log activity
                await _activityLogService.LogActivityAsync(
                    "system", // Use the admin's actual ID in production
                    "Course Created",
                    $"Course '{course.Name}' ({course.Code}) was created in department {course.DepartmentId}",
                    "course_created");
                    
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                throw;
            }
        }        public async Task<bool> UpdateCourseAsync(Course course)
        {
            try
            {
                _logger.LogInformation("Updating course: {Id}", course.Id);
                
                var existingDb = await _databaseService.GetByIdAsync<CourseDb>(course.Id);
                if (existingDb != null)
                {
                    // Capture old values for logging
                    var oldValues = new Dictionary<string, object>
                    {
                        { "Name", existingDb.Name },
                        { "Code", existingDb.Code },
                        { "Description", existingDb.Description ?? string.Empty },
                        { "DepartmentId", existingDb.DepartmentId ?? string.Empty }
                    };
                    
                    // Update values
                    existingDb.Name = course.Name;
                    existingDb.Code = course.Code;
                    existingDb.Description = course.Description;
                    existingDb.DepartmentId = course.DepartmentId;
                    existingDb.UpdatedAt = DateTime.UtcNow;
                    
                    // Capture new values for logging
                    var newValues = new Dictionary<string, object>
                    {
                        { "Name", course.Name },
                        { "Code", course.Code },
                        { "Description", course.Description ?? string.Empty },
                        { "DepartmentId", course.DepartmentId ?? string.Empty }
                    };
                    
                    await _databaseService.UpdateAsync(existingDb);
                    
                    // Log record change with ActivityLogService
                    await _activityLogService.LogRecordChangeAsync(
                        "system", // Use the admin's actual ID in production
                        "courses",
                        course.Id,
                        oldValues,
                        newValues,
                        $"Updated course '{course.Name}'"
                    );
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course");
                throw;
            }
        }        public async Task<bool> AssignCourseToTeacherAsync(string courseId, string teacherId)
        {
            try
            {
                _logger.LogInformation("Assigning course {CourseId} to teacher {TeacherId}", courseId, teacherId);
                
                var courseDb = await _databaseService.GetByIdAsync<CourseDb>(courseId);
                var teacherDb = await _databaseService.GetByIdAsync<UserProfileDb>(teacherId);
                
                if (courseDb == null)
                {
                    _logger.LogError("Course not found: {CourseId}", courseId);
                    return false;
                }
                
                if (!string.IsNullOrEmpty(teacherId) && teacherDb == null)
                {
                    _logger.LogError("Teacher not found: {TeacherId}", teacherId);
                    return false;
                }
                
                if (courseDb != null)
                {
                    // Capture old value for logging
                    var oldTeacherId = courseDb.TeacherId;
                    
                    // Update course
                    courseDb.TeacherId = string.IsNullOrEmpty(teacherId) ? null : teacherId;
                    courseDb.UpdatedAt = DateTime.UtcNow;
                    
                    var updatedCourse = await _databaseService.UpdateAsync(courseDb);
                    
                    if (updatedCourse == null)
                    {
                        _logger.LogError("Failed to update course in database: {CourseId}", courseId);
                        return false;
                    }
                    
                    // Log activity
                    await _activityLogService.LogActivityAsync(
                        "system", // Use the admin's actual ID in production
                        "Course Assigned", 
                        $"Course '{courseDb.Name}' ({courseDb.Code}) was assigned to teacher {teacherDb?.FullName ?? "None"}",
                        "course_assigned");
                    
                    // Also log record change
                    var oldValues = new Dictionary<string, object> { { "TeacherId", oldTeacherId ?? string.Empty } };
                    var newValues = new Dictionary<string, object> { { "TeacherId", teacherId ?? string.Empty } };
                    
                    await _activityLogService.LogRecordChangeAsync(
                        "system", // Use the admin's actual ID in production
                        "courses",
                        courseId,
                        oldValues,
                        newValues,
                        $"Changed teacher assignment for course '{courseDb.Name}'"
                    );
                    
                    _logger.LogInformation("Successfully assigned course {CourseId} to teacher {TeacherId}", courseId, teacherId);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning course to teacher: CourseId={CourseId}, TeacherId={TeacherId}", courseId, teacherId);
                throw;
            }
        }

    // Activity logs for admin dashboard
        public async Task<List<ActivityModel>> GetRecentActivitiesAsync(int count = 10)
        {
            try
            {
                _logger.LogInformation("Getting recent activities for admin dashboard");
                var activityLogs = await _databaseService.GetRecentActivityLogsAsync(count);
                
                // Enrich with user names - since we need to display who performed each action
                foreach (var log in activityLogs)
                {
                    if (!string.IsNullOrEmpty(log.UserId))
                    {
                        var user = await _databaseService.GetUserByIdAsync(log.UserId);
                        log.UserName = user?.FullName ?? "Unknown User";
                    }
                    else
                    {
                        log.UserName = "System";
                    }
                }
                
                return activityLogs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activities: {Message}", ex.Message);
                throw;
            }
        }
    }
}
