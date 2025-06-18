using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Campus360.Models;
using Microsoft.Extensions.Logging;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using Supabase.Postgrest;
using Postgrest = Supabase.Postgrest;

namespace Campus360.Services
{    public class DatabaseService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(Supabase.Client supabaseClient, ILogger<DatabaseService> logger)
        {
            _supabaseClient = supabaseClient;
            _logger = logger;
        }

        // Expose the Supabase client for advanced operations
        public Supabase.Client Client => _supabaseClient;// ========== GENERIC CRUD ==========
        public async Task<List<TDb>> GetAllAsync<TDb>() where TDb : Supabase.Postgrest.Models.BaseModel, new()
        {
            try
            {
                var tableName = typeof(TDb).Name;
                _logger.LogInformation("Fetching all records from table type: {TableType}", tableName);
                
                var result = await _supabaseClient.From<TDb>().Get();
                
                _logger.LogInformation("Successfully fetched {Count} records from {TableType}", 
                    result?.Models?.Count ?? 0, tableName);
                    
                return result?.Models ?? new List<TDb>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all records from table type: {TableType}. Error: {ErrorMessage}", 
                    typeof(TDb).Name, ex.Message);
                throw;
            }
        }

        public async Task<TDb?> GetByIdAsync<TDb>(string id) where TDb : Supabase.Postgrest.Models.BaseModel, new()
        {
            var result = await _supabaseClient.From<TDb>().Filter("id", Supabase.Postgrest.Constants.Operator.Equals, id).Get();
            return result.Models.FirstOrDefault();
        }        public async Task<TDb?> InsertAsync<TDb>(TDb entity) where TDb : Supabase.Postgrest.Models.BaseModel, new()
        {
            try
            {
                _logger.LogInformation("Inserting entity of type {Type}", typeof(TDb).Name);
                
                // Special handling for UserProfileDb to ensure ID is properly included
                if (entity is UserProfileDb user)
                {
                    _logger.LogInformation("UserProfileDb - ID: {Id}, Email: {Email}, FullName: {FullName}", 
                        user.Id, user.Email, user.FullName);
                    
                    // Additional validation for UserProfileDb
                    if (string.IsNullOrEmpty(user.Id))
                    {
                        _logger.LogError("Critical error: UserProfileDb.Id is null or empty before insert");
                        throw new InvalidOperationException("User ID cannot be null or empty");
                    }
                    
                    if (string.IsNullOrEmpty(user.Email))
                    {
                        _logger.LogError("Critical error: UserProfileDb.Email is null or empty before insert");
                        throw new InvalidOperationException("User email cannot be null or empty");
                    }
                }
                
                var response = await _supabaseClient.From<TDb>().Insert(entity);
                
                var result = response.Models.FirstOrDefault();
                if (result != null)
                {
                    _logger.LogInformation("Successfully inserted entity of type {Type}", typeof(TDb).Name);
                    
                    // Log the result ID if it's a UserProfileDb
                    if (result is UserProfileDb insertedUser)
                    {
                        _logger.LogInformation("UserProfileDb inserted with ID: {Id}", insertedUser.Id);
                    }
                }
                else
                {
                    _logger.LogWarning("Insert operation returned no models for type {Type}", typeof(TDb).Name);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting entity of type {Type}. Details: {Message}", typeof(TDb).Name, ex.Message);
                
                // Log additional details for UserProfileDb
                if (entity is UserProfileDb user)
                {
                    _logger.LogError("Failed UserProfileDb - ID: {Id}, Email: {Email}, FullName: {FullName}", 
                        user.Id, user.Email, user.FullName);
                }
                
                throw;
            }
        }

        public async Task<TDb?> UpdateAsync<TDb>(TDb entity) where TDb : Supabase.Postgrest.Models.BaseModel, new()
        {
            var response = await _supabaseClient.From<TDb>().Update(entity);
            return response.Models.FirstOrDefault();
        }

        public async Task<bool> DeleteAsync<TDb>(string id) where TDb : Supabase.Postgrest.Models.BaseModel, new()
        {
            await _supabaseClient.From<TDb>().Filter("id", Supabase.Postgrest.Constants.Operator.Equals, id).Delete();
            return true;
        }        // ========== USER MANAGEMENT ==========
        public async Task<UserProfile?> GetUserByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("Getting user by email: {Email}", email);
                
                // Use explicit table name to avoid mapping issues
                var result = await _supabaseClient
                    .From<UserProfileDb>()
                    .Select("*")
                    .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, email)
                    .Get();
                
                var user = result.Models.FirstOrDefault();
                if (user != null)
                {
                    _logger.LogInformation("Found user: {UserId}", user.Id);
                    return user.ToUserProfile();
                }
                else
                {
                    _logger.LogInformation("No user found with email: {Email}", email);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                throw;
            }
        }

        public async Task<UserProfile?> GetUserByIdAsync(string id)
        {
            var result = await _supabaseClient.From<UserProfileDb>().Select("*").Filter("id", Supabase.Postgrest.Constants.Operator.Equals, id).Get();
            return result.Models.FirstOrDefault()?.ToUserProfile();
        }        public async Task<List<UserProfile>> GetUsersByRoleAsync(string role)
        {
            var result = await _supabaseClient.From<UserProfileDb>().Filter("role", Supabase.Postgrest.Constants.Operator.Equals, role).Get();
            return result.Models.Select(x => x.ToUserProfile()).ToList();
        }        // ========== DIRECT SQL METHODS (to bypass ORM mapping issues) ==========
        public async Task<UserProfile?> GetUserByEmailDirectAsync(string email)
        {
            try
            {
                _logger.LogInformation("Getting user by email using direct query: {Email}", email);
                
                // Use the From<T> method instead of Table<T>
                var response = await _supabaseClient
                    .From<UserProfileDb>()
                    .Select("*")
                    .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, email)
                    .Get();
                
                var user = response.Models.FirstOrDefault();
                if (user != null)
                {
                    _logger.LogInformation("Found user with direct query: {UserId}", user.Id);
                    return user.ToUserProfile();
                }
                else
                {
                    _logger.LogInformation("No user found with direct query for email: {Email}", email);
                    return null;
                }            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in direct user query by email: {Email}", email);
                throw;
            }
        }        public async Task<UserProfile?> CreateUserAsync(UserProfile user)
        {
            // Validate input
            if (string.IsNullOrEmpty(user.Id))
            {
                _logger.LogError("Cannot create user: ID is null or empty. Email: {Email}", user.Email);
                throw new ArgumentException("User ID cannot be null or empty", nameof(user));
            }
            
            if (string.IsNullOrEmpty(user.Email))
            {
                _logger.LogError("Cannot create user: Email is null or empty. ID: {Id}", user.Id);
                throw new ArgumentException("User email cannot be null or empty", nameof(user));
            }
            
            _logger.LogInformation("Creating user profile in database - ID: {Id}, Email: {Email}", user.Id, user.Email);
            
            var db = user.ToDb();
            
            // Double-check the converted model
            if (string.IsNullOrEmpty(db.Id))
            {
                _logger.LogError("ToDb conversion resulted in null/empty ID. Original ID: {OriginalId}", user.Id);
                throw new InvalidOperationException("Database model ID is null after conversion");
            }
            
            _logger.LogInformation("About to insert UserProfileDb - ID: {Id}, Email: {Email}, FullName: {FullName}", 
                db.Id, db.Email, db.FullName);
            
            // Try explicit ID assignment as a workaround
            var userProfileDb = new UserProfileDb();
            userProfileDb.Id = user.Id;
            userProfileDb.Email = user.Email;
            userProfileDb.FullName = user.FullName;
            userProfileDb.Role = user.Role;
            userProfileDb.DepartmentId = user.DepartmentId;
            userProfileDb.IsVerified = user.IsVerified;
            userProfileDb.IsActive = user.IsActive;
            userProfileDb.CreatedAt = user.CreatedAt;
            userProfileDb.UpdatedAt = user.UpdatedAt;
            
            _logger.LogInformation("Using explicit assignment - ID: {Id}, Email: {Email}", 
                userProfileDb.Id, userProfileDb.Email);
            
            var response = await _supabaseClient.From<UserProfileDb>().Insert(userProfileDb);
            return response.Models.FirstOrDefault()?.ToUserProfile();
        }

        public async Task<UserProfile?> UpdateUserAsync(UserProfile user)
        {
            var db = user.ToDb();
            var response = await _supabaseClient.From<UserProfileDb>().Update(db);
            return response.Models.FirstOrDefault()?.ToUserProfile();
        }

        // ========== ACTIVITY LOGGING ==========
        public async Task LogActivityAsync(string activityType, string description, string? userId = null)
        {
            var log = new ActivityLogDb
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = activityType,
                Description = description,
                ActivityType = activityType,
                CreatedAt = DateTime.UtcNow
            };
            await _supabaseClient.From<ActivityLogDb>().Insert(log);
        }
        
        // Enhanced activity logging with more details
        public async Task<ActivityLogDb> CreateActivityLogAsync(ActivityLogDb log)
        {
            try
            {
                log.Id = Guid.NewGuid().ToString();
                log.CreatedAt = DateTime.UtcNow;
                
                var response = await _supabaseClient.From<ActivityLogDb>().Insert(log);
                return response.Models.FirstOrDefault() ?? log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity log: {Message}", ex.Message);
                throw;
            }
        }

        // Get recent activity logs with filtering options
        public async Task<List<ActivityModel>> GetRecentActivityLogsAsync(int count = 10, string? userId = null)
        {
            try
            {                var query = _supabaseClient.From<ActivityLogDb>()
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(count);
                    
                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId);
                }
                
                var logsDb = await query.Get();
                
                // Convert to ActivityModel
                return logsDb.Models.Select(log => new ActivityModel
                {
                    Id = log.Id,
                    Title = log.Title,
                    Description = log.Description,
                    Type = log.ActivityType,
                    CreatedAt = log.CreatedAt,
                    UserId = log.UserId ?? string.Empty,
                    UserName = string.Empty, // Will be populated by the service
                    ActivityType = log.ActivityType,
                    Timestamp = log.CreatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activity logs: {Message}", ex.Message);
                return new List<ActivityModel>();
            }
        }

        // Get activity logs with detailed filtering
        public async Task<List<ActivityLogDb>> GetActivityLogsAsync(string? userId = null, string? activityType = null, 
            DateTime? startDate = null, DateTime? endDate = null, string? tableName = null, string? recordId = null, int limit = 100)
        {
            try
            {                var query = _supabaseClient.From<ActivityLogDb>()
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(limit);
                
                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId);
                }
                
                if (!string.IsNullOrEmpty(activityType))
                {
                    query = query.Filter("activity_type", Supabase.Postgrest.Constants.Operator.Equals, activityType);
                }
                
                if (!string.IsNullOrEmpty(tableName))
                {
                    query = query.Filter("table_name", Supabase.Postgrest.Constants.Operator.Equals, tableName);
                }
                
                if (!string.IsNullOrEmpty(recordId))
                {
                    query = query.Filter("record_id", Supabase.Postgrest.Constants.Operator.Equals, recordId);
                }
                
                if (startDate.HasValue)
                {
                    query = query.Filter("created_at", Supabase.Postgrest.Constants.Operator.GreaterThanOrEqual, startDate.Value);
                }
                
                if (endDate.HasValue)
                {
                    query = query.Filter("created_at", Supabase.Postgrest.Constants.Operator.LessThanOrEqual, endDate.Value);
                }
                
                var result = await query.Get();
                return result.Models;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity logs: {Message}", ex.Message);
                return new List<ActivityLogDb>();
            }
        }

        // Track changes to records with old and new values
        public async Task LogRecordChangeAsync(string userId, string tableName, string recordId, 
            Dictionary<string, object>? oldValues, Dictionary<string, object>? newValues, string description)
        {
            var log = new ActivityLogDb
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = $"Updated {tableName}",
                Description = description,
                ActivityType = "record_updated",
                TableName = tableName,
                RecordId = recordId,
                OldValues = oldValues,
                NewValues = newValues,
                CreatedAt = DateTime.UtcNow
            };
            
            await _supabaseClient.From<ActivityLogDb>().Insert(log);
        }
        
        // Get activity logs for a specific user with username enrichment
        public async Task<List<RecentActivity>> GetUserActivitiesAsync(string userId, int limit = 10)
        {
            try
            {
                var logs = await GetActivityLogsAsync(userId, null, null, null, null, null, limit);
                var user = await GetUserByIdAsync(userId);
                string userName = user?.FullName ?? "Unknown User";
                
                return logs.Select(log => new RecentActivity
                {
                    Id = log.Id,
                    Action = log.Title,
                    Description = log.Description,
                    UserName = userName,
                    Timestamp = log.CreatedAt,
                    ActivityType = log.ActivityType
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user activities: {Message}", ex.Message);
                return new List<RecentActivity>();
            }
        }
        // ========== ATTENDANCE ==========
        public async Task<List<StudentAttendanceRecord>> GetAttendanceRecordsAsync(string courseId)
        {
            var result = await _supabaseClient.From<AttendanceRecordDb>().Filter("course_id", Supabase.Postgrest.Constants.Operator.Equals, courseId).Get();
            return result.Models.Select(x => new StudentAttendanceRecord
            {
                Id = x.Id,
                SessionId = x.SessionId,
                StudentId = x.StudentId,
                IsPresent = x.IsPresent,
                Remarks = x.Remarks,
                MarkedAt = x.MarkedAt,
                MarkedBy = x.MarkedBy,
                // The following are not in AttendanceRecordDb, so set to default/empty
                CourseId = courseId,
                CourseName = string.Empty,
                CourseCode = string.Empty,
                ClassDate = DateTime.MinValue,
                SessionDate = DateTime.MinValue,
                ClassType = string.Empty,
                SessionType = string.Empty,
                Topic = string.Empty,
                Status = string.Empty,
                IsLate = false,
                LateByMinutes = 0,
                CheckInTime = null,
                CheckOutTime = null,
                Duration = null,
                CreatedAt = x.MarkedAt,
                DurationTimeSpan = null,
                StatusMarked = "Marked"
            }).ToList();
        }

        // ========== NOTIFICATIONS ==========
        public async Task<List<NotificationModel>> GetUserNotificationsAsync(string userId)
        {
            var result = await _supabaseClient.From<NotificationDb>().Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId).Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending).Get();
            return result.Models.Select(x => x.ToNotification()).ToList();
        }        public async Task MarkNotificationAsReadAsync(string notificationId)
        {
            var notification = await GetByIdAsync<NotificationDb>(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await UpdateAsync(notification);
            }
        }

        public async Task MarkAllNotificationsAsReadAsync(string userId)
        {
            try
            {
                var notifications = await _supabaseClient.From<NotificationDb>()
                    .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                    .Filter("is_read", Supabase.Postgrest.Constants.Operator.Equals, false)
                    .Get();

                foreach (var notification in notifications.Models)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    await UpdateAsync(notification);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}: {Message}", userId, ex.Message);
            }
        }

        // ========== FILE/DOCUMENT MANAGEMENT ==========
        public async Task<List<FileDocument>> GetFilesForUserAsync(string userId)
        {
            var result = await _supabaseClient.From<FileDocumentDb>().Filter("uploaded_by", Supabase.Postgrest.Constants.Operator.Equals, userId).Get();
            return result.Models.Select(x => new FileDocument
            {
                Id = x.Id,
                FileName = x.FileName,
                FileUrl = x.FileUrl,
                FileType = ParseFileType(x.FileType),
                UploadedBy = x.UploadedBy,
                UploaderName = x.UploaderName ?? string.Empty,
                CourseId = x.CourseId,
                DepartmentId = x.DepartmentId,
                Visibility = ParseFileVisibility(x.Visibility),
                Description = x.Description ?? string.Empty,
                FileSize = x.FileSize,
                ContentType = x.ContentType ?? string.Empty,
                DownloadCount = x.DownloadCount,
                LastDownloaded = x.LastDownloaded,
                IsActive = x.IsActive,
                IsFeatured = x.IsFeatured,
                DueDate = x.DueDate,
                MaxScore = x.MaxScore,
                Tags = x.Tags,
                UploadDate = x.UploadDate,
                LastModified = x.LastModified
            }).ToList();
        }

        // ========== CALENDAR/EVENTS ==========
        public async Task<List<AcademicEvent>> GetEventsForUserAsync(string userId, string role)
        {
            var result = await _supabaseClient.From<AcademicEventDb>().Get();
            return result.Models
                .Where(e => e.TargetRole == role || e.TargetRole == "all")
                .Select(e => new AcademicEvent
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description ?? string.Empty, // Null-coalescing as per changes
                    EventType = ParseAcademicEventType(e.EventType),
                    Category = e.Category,
                    Priority = ParseEventPriority(e.Priority),
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    IsAllDay = e.IsAllDay,
                    Venue = e.Venue,
                    Room = e.Room,
                    Building = e.Building,
                    OnlineLink = e.OnlineLink,
                    CourseId = e.CourseId,
                    DepartmentId = e.DepartmentId,
                    TargetRole = ParseEventTargetRole(e.TargetRole),
                    TargetUserIds = e.TargetUserIds,
                    TargetCourseIds = e.TargetCourseIds,
                    TargetDepartmentIds = e.TargetDepartmentIds,
                    CreatedBy = e.CreatedBy,
                    CreatedByRole = e.CreatedByRole ?? string.Empty, // Null-coalescing as per changes
                    Status = ParseEventStatus(e.Status),
                    IsPublished = e.IsPublished,
                    IsRecurring = e.IsRecurring,
                    RecurringPattern = ParseRecurringPattern(e.RecurringPattern),
                    IsSystemGenerated = e.IsSystemGenerated,
                    SourceType = e.SourceType,
                    SourceId = e.SourceId,
                    AutomationRuleId = e.AutomationRuleId,
                    HasReminder = e.HasReminder,
                    Color = e.Color,
                    IconClass = e.IconClass,
                    BadgeClass = e.BadgeClass,
                    Tags = e.Tags,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                }).ToList();
        }

        public async Task<AcademicEvent?> GetEventByIdAsync(string eventId)
        {
            var result = await _supabaseClient.From<AcademicEventDb>()
                .Where(e => e.Id == eventId)
                .Single();
            
            if (result == null) return null;
            
            return new AcademicEvent
            {
                Id = result.Id,
                Title = result.Title,
                Description = result.Description ?? string.Empty,
                EventType = ParseAcademicEventType(result.EventType),
                Category = result.Category,
                Priority = ParseEventPriority(result.Priority),
                StartDate = result.StartDate,
                EndDate = result.EndDate,
                StartTime = result.StartTime,
                EndTime = result.EndTime,
                IsAllDay = result.IsAllDay,
                Venue = result.Venue,
                Room = result.Room,
                Building = result.Building,
                OnlineLink = result.OnlineLink,
                CourseId = result.CourseId,
                DepartmentId = result.DepartmentId,
                TargetRole = ParseEventTargetRole(result.TargetRole),
                TargetUserIds = result.TargetUserIds,
                TargetCourseIds = result.TargetCourseIds,
                TargetDepartmentIds = result.TargetDepartmentIds,
                CreatedBy = result.CreatedBy,
                CreatedByRole = result.CreatedByRole ?? string.Empty,
                Status = ParseEventStatus(result.Status),
                IsPublished = result.IsPublished,
                IsRecurring = result.IsRecurring,
                RecurringPattern = ParseRecurringPattern(result.RecurringPattern),
                IsSystemGenerated = result.IsSystemGenerated,
                SourceType = result.SourceType,
                SourceId = result.SourceId,
                AutomationRuleId = result.AutomationRuleId,
                HasReminder = result.HasReminder,
                Color = result.Color,
                IconClass = result.IconClass,
                BadgeClass = result.BadgeClass,
                Tags = result.Tags,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt
            };
        }

        public async Task<AcademicEventDb?> UpdateEventAsync(AcademicEventDb eventDb)
        {
            eventDb.UpdatedAt = DateTime.UtcNow;
            var result = await _supabaseClient.From<AcademicEventDb>().Update(eventDb);
            return result.Model;
        }

        public async Task<bool> DeleteEventAsync(string eventId)
        {
            try
            {
                await _supabaseClient.From<AcademicEventDb>()
                    .Where(e => e.Id == eventId)
                    .Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }
        // ========== SETTINGS ==========
        public async Task<AppSetting?> GetAppSettingAsync(string key)
        {
            var result = await _supabaseClient.From<AppSettingDb>().Filter("key", Supabase.Postgrest.Constants.Operator.Equals, key).Get();
            if (result.Models.Count == 0) return null;
            var db = result.Models.First();
            return new AppSetting
            {
                Id = db.Id,
                Key = db.Key,
                Value = db.Value,
                Description = db.Description,
                DataType = db.DataType,
                IsPublic = db.IsPublic,
                CreatedAt = db.CreatedAt,
                UpdatedAt = db.UpdatedAt
            };
        }

        public async Task<UserSetting?> GetUserSettingAsync(string userId, string key)
        {
            var result = await _supabaseClient.From<UserSettingDb>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                .Filter("key", Supabase.Postgrest.Constants.Operator.Equals, key)
                .Get();
            if (result.Models.Count == 0) return null;
            var db = result.Models.First();
            return new UserSetting
            {
                Id = db.Id,
                UserId = db.UserId,
                Key = db.Key,
                Value = db.Value,
                CreatedAt = db.CreatedAt,
                UpdatedAt = db.UpdatedAt
            };
        }

        // ========== HEALTH CHECK ==========
        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var result = await _supabaseClient.From<UserProfileDb>().Limit(1).Get();
                return result.Models.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return false;
            }
        }

        // ========== TEMPORARY DEBUG METHOD FOR DEPARTMENTS ==========
        public async Task<List<DepartmentDb>> DebugGetDepartmentsAsync()
        {
            try
            {
                _logger.LogInformation("=== DEBUGGING DEPARTMENTS ACCESS ===");
                _logger.LogInformation("Attempting to fetch departments using direct From<DepartmentDb>()");
                
                // Try basic query
                var result = await _supabaseClient.From<DepartmentDb>().Get();
                
                _logger.LogInformation("Raw result from Supabase: {ResultNotNull}", result != null);
                _logger.LogInformation("Models count: {ModelsCount}", result?.Models?.Count ?? -1);
                
                if (result?.Models != null)
                {
                    _logger.LogInformation("Models is not null, count: {Count}", result.Models.Count);
                    foreach (var dept in result.Models.Take(3))
                    {
                        _logger.LogInformation("Department found: ID={Id}, Name={Name}, IsActive={IsActive}", 
                            dept?.Id ?? "NULL", dept?.Name ?? "NULL", dept?.IsActive ?? false);
                    }
                }
                else
                {
                    _logger.LogWarning("Models collection is null!");
                }
                
                return result?.Models ?? new List<DepartmentDb>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRITICAL ERROR in DebugGetDepartmentsAsync: {Message}", ex.Message);
                _logger.LogError("Exception type: {ExceptionType}", ex.GetType().Name);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

        // ========== DEBUG METHODS ==========
        public async Task<string> TestDatabaseConnectionAsync()
        {
            try
            {
                _logger.LogInformation("Testing database connection...");
                
                // Test 1: Basic connection
                var allUsers = await _supabaseClient.From<UserProfileDb>().Get();
                var userCount = allUsers.Models?.Count ?? 0;
                
                // Test 2: Specific email search
                var adminResult = await _supabaseClient
                    .From<UserProfileDb>()
                    .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, "admin@campus360.com")
                    .Get();
                var adminFound = adminResult.Models?.Count ?? 0;
                
                // Test 3: Case insensitive search
                var adminResultInsensitive = await _supabaseClient
                    .From<UserProfileDb>()
                    .Filter("email", Supabase.Postgrest.Constants.Operator.ILike, "admin@campus360.com")
                    .Get();
                var adminFoundInsensitive = adminResultInsensitive.Models?.Count ?? 0;
                
                var result = $"DB Test Results - Total Users: {userCount}, Admin Found (exact): {adminFound}, Admin Found (case-insensitive): {adminFoundInsensitive}";
                _logger.LogInformation(result);
                return result;
            }
            catch (Exception ex)
            {
                var error = $"Database test failed: {ex.Message}";
                _logger.LogError(ex, error);
                return error;
            }        }

        // ========== AUTOMATION LOGS ==========
        
        /// <summary>
        /// Create a new automation log entry
        /// </summary>
        public async Task<AutomationLogDb> CreateAutomationLogAsync(AutomationLog log)
        {
            try
            {
                _logger.LogInformation("Creating automation log: {Title}", log.Title);
                
                var logDb = log.ToDb();
                logDb.Id = Guid.NewGuid().ToString();
                logDb.CreatedAt = DateTime.UtcNow;
                logDb.UpdatedAt = DateTime.UtcNow;
                
                var response = await _supabaseClient.From<AutomationLogDb>().Insert(logDb);
                return response.Models.FirstOrDefault() ?? logDb;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating automation log: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get automation logs with filtering options
        /// </summary>
        public async Task<List<AutomationLog>> GetAutomationLogsAsync(
            string? userId = null, 
            string? triggerType = null, 
            string? courseId = null,
            string? status = null,
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            int limit = 100)
        {
            try
            {
                _logger.LogInformation("Getting automation logs with filters");
                
                var query = _supabaseClient.From<AutomationLogDb>()
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(limit);

                if (!string.IsNullOrEmpty(userId))
                {
                    query = query.Filter("target_user_id", Supabase.Postgrest.Constants.Operator.Equals, userId);
                }

                if (!string.IsNullOrEmpty(triggerType))
                {
                    query = query.Filter("trigger_type", Supabase.Postgrest.Constants.Operator.Equals, triggerType);
                }

                if (!string.IsNullOrEmpty(courseId))
                {
                    query = query.Filter("course_id", Supabase.Postgrest.Constants.Operator.Equals, courseId);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Filter("status", Supabase.Postgrest.Constants.Operator.Equals, status);
                }

                if (startDate.HasValue)
                {
                    query = query.Filter("created_at", Supabase.Postgrest.Constants.Operator.GreaterThanOrEqual, startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Filter("created_at", Supabase.Postgrest.Constants.Operator.LessThanOrEqual, endDate.Value);
                }

                var result = await query.Get();
                return result.Models.Select(db => db.ToAutomationLog()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automation logs: {Message}", ex.Message);
                return new List<AutomationLog>();
            }
        }

        /// <summary>
        /// Update automation log status and related fields
        /// </summary>
        public async Task<AutomationLogDb?> UpdateAutomationLogAsync(string logId, string status, string? errorMessage = null)
        {
            try
            {
                _logger.LogInformation("Updating automation log {LogId} to status {Status}", logId, status);
                
                var log = await _supabaseClient.From<AutomationLogDb>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, logId)
                    .Single();

                if (log != null)
                {
                    log.Status = status;
                    log.UpdatedAt = DateTime.UtcNow;
                    
                    if (status == "sent")
                    {
                        log.SentAt = DateTime.UtcNow;
                    }
                    else if (status == "delivered")
                    {
                        log.DeliveredAt = DateTime.UtcNow;
                    }
                    else if (status == "failed")
                    {
                        log.ErrorMessage = errorMessage;
                        log.IsSuccessful = false;
                    }

                    var response = await _supabaseClient.From<AutomationLogDb>().Update(log);
                    return response.Models.FirstOrDefault();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating automation log: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get automation metrics for dashboard
        /// </summary>
        public async Task<AutomationMetrics> GetAutomationMetricsAsync()
        {
            try
            {
                _logger.LogInformation("Getting automation metrics");
                
                var logsDb = await _supabaseClient.From<AutomationLogDb>()
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(1000)
                    .Get();

                var logs = logsDb.Models;
                var today = DateTime.Today;
                var thisWeek = today.AddDays(-(int)today.DayOfWeek);
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                var totalLogs = logs.Count;
                var todayLogs = logs.Count(l => l.CreatedAt.Date == today);
                var weekLogs = logs.Count(l => l.CreatedAt >= thisWeek);
                var monthLogs = logs.Count(l => l.CreatedAt >= thisMonth);
                var failedLogs = logs.Count(l => l.Status == "failed");
                var pendingLogs = logs.Count(l => l.Status == "pending");
                var successfulLogs = logs.Count(l => l.Status == "sent" || l.Status == "delivered");

                var notificationsByType = logs
                    .GroupBy(l => l.TriggerType)
                    .ToDictionary(g => g.Key, g => g.Count());

                var notificationsByPriority = logs
                    .GroupBy(l => "normal") // We'd need to add priority to AutomationLogDb
                    .ToDictionary(g => g.Key, g => g.Count());

                var recentLogs = logs.Take(10).Select(db => db.ToAutomationLog()).ToList();

                var topRules = logs
                    .GroupBy(l => new { l.RuleId, l.RuleName })
                    .Select(g => new TopTriggeredRule
                    {
                        RuleId = g.Key.RuleId,
                        RuleName = g.Key.RuleName,
                        TriggerCount = g.Count(),
                        LastTriggered = g.Max(l => l.CreatedAt),
                        SuccessRate = g.Count(l => l.IsSuccessful) * 100.0 / g.Count()
                    })
                    .OrderByDescending(r => r.TriggerCount)
                    .Take(5)
                    .ToList();

                var successRate = totalLogs > 0 ? (double)successfulLogs / totalLogs * 100 : 100;

                return new AutomationMetrics
                {
                    TotalNotificationsSent = totalLogs,
                    TotalActiveRules = 0, // Would need to query automation rules
                    NotificationsSentToday = todayLogs,
                    NotificationsSentThisWeek = weekLogs,
                    NotificationsSentThisMonth = monthLogs,
                    FailedNotifications = failedLogs,
                    PendingNotifications = pendingLogs,
                    NotificationsByType = notificationsByType,
                    NotificationsByPriority = notificationsByPriority,
                    RecentLogs = recentLogs,
                    TopRules = topRules,
                    SuccessRate = successRate,
                    AverageResponseTime = 0.5, // Mock value for now
                    LastCalculated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automation metrics: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retry a failed automation log
        /// </summary>
        public async Task<bool> RetryAutomationLogAsync(string logId)
        {
            try
            {
                _logger.LogInformation("Retrying automation log {LogId}", logId);
                
                var log = await _supabaseClient.From<AutomationLogDb>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, logId)
                    .Single();

                if (log != null)
                {
                    log.Status = "pending";
                    log.RetryCount++;
                    log.UpdatedAt = DateTime.UtcNow;
                    log.ErrorMessage = null;

                    await _supabaseClient.From<AutomationLogDb>().Update(log);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying automation log: {Message}", ex.Message);
                return false;
            }
        }        /// <summary>
        /// Get automation logs for a specific teacher
        /// </summary>
        public async Task<List<AutomationLog>> GetTeacherAutomationLogsAsync(string teacherId, int limit = 50)
        {
            try
            {
                _logger.LogInformation("Getting automation logs for teacher {TeacherId}", teacherId);
                
                var query = _supabaseClient.From<AutomationLogDb>()
                    .Filter("teacher_id", Supabase.Postgrest.Constants.Operator.Equals, teacherId)
                    .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                    .Limit(limit);

                var result = await query.Get();
                return result.Models.Select(db => db.ToAutomationLog()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teacher automation logs: {Message}", ex.Message);
                return new List<AutomationLog>();
            }        }

        // ========== AUTOMATION RULES MANAGEMENT ==========
        
        public async Task<List<AutomationRule>> GetAutomationRulesAsync()
        {
            try
            {
                var result = await _supabaseClient.From<AutomationRuleDb>().Get();
                return result.Models.Select(db => db.ToAutomationRule()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automation rules: {Message}", ex.Message);
                return new List<AutomationRule>();
            }
        }

        public async Task<AutomationRule?> GetAutomationRuleByIdAsync(string ruleId)
        {
            try
            {
                var result = await _supabaseClient.From<AutomationRuleDb>()
                    .Filter("rule_id", Supabase.Postgrest.Constants.Operator.Equals, ruleId)
                    .Single();
                return result?.ToAutomationRule();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting automation rule: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<AutomationRule?> CreateAutomationRuleAsync(AutomationRule rule)
        {
            try
            {
                var ruleDb = rule.ToDb();
                var response = await _supabaseClient.From<AutomationRuleDb>().Insert(ruleDb);
                return response.Models.FirstOrDefault()?.ToAutomationRule();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating automation rule: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<bool> ToggleAutomationRuleAsync(string ruleId)
        {
            try
            {
                var existing = await _supabaseClient.From<AutomationRuleDb>()
                    .Filter("rule_id", Supabase.Postgrest.Constants.Operator.Equals, ruleId)
                    .Single();
                
                if (existing != null)
                {
                    existing.IsActive = !existing.IsActive;
                    await _supabaseClient.From<AutomationRuleDb>().Update(existing);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling automation rule: {Message}", ex.Message);
                return false;
            }
        }        public async Task<bool> IncrementRuleTriggerCountAsync(string ruleId)
        {
            try
            {
                var existing = await _supabaseClient.From<AutomationRuleDb>()
                    .Filter("rule_id", Supabase.Postgrest.Constants.Operator.Equals, ruleId)
                    .Single();
                
                if (existing != null)
                {
                    existing.TriggerCount++;
                    existing.LastTriggered = DateTime.UtcNow;
                    await _supabaseClient.From<AutomationRuleDb>().Update(existing);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing rule trigger count: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<AutomationRule?> UpdateAutomationRuleAsync(AutomationRule rule)
        {
            try
            {
                _logger.LogInformation("Updating automation rule: {RuleId}", rule.Id);
                
                var ruleDb = rule.ToDb();
                ruleDb.UpdatedAt = DateTime.UtcNow;
                
                var response = await _supabaseClient.From<AutomationRuleDb>()
                    .Filter("rule_id", Supabase.Postgrest.Constants.Operator.Equals, rule.Id)
                    .Update(ruleDb);
                
                return response.Models.FirstOrDefault()?.ToAutomationRule();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating automation rule: {Message}", ex.Message);
                return null;
            }
        }

        public async Task<bool> DeleteAutomationRuleAsync(string ruleId)
        {
            try
            {
                _logger.LogInformation("Deleting automation rule: {RuleId}", ruleId);
                
                await _supabaseClient.From<AutomationRuleDb>()
                    .Filter("rule_id", Supabase.Postgrest.Constants.Operator.Equals, ruleId)
                    .Delete();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting automation rule: {Message}", ex.Message);
                return false;
            }
        }

        private AcademicEventType ParseAcademicEventType(string value)
        {
            return Enum.TryParse<AcademicEventType>(value, true, out var result) ? result : AcademicEventType.Other;
        }
        private EventPriority ParseEventPriority(string value)
        {
            return Enum.TryParse<EventPriority>(value, true, out var result) ? result : EventPriority.Normal;
        }
        private EventTargetRole ParseEventTargetRole(string value)
        {
            return Enum.TryParse<EventTargetRole>(value, true, out var result) ? result : EventTargetRole.All;
        }
        private EventStatus ParseEventStatus(string value)
        {
            return Enum.TryParse<EventStatus>(value, true, out var result) ? result : EventStatus.Active;
        }
        private RecurringPattern? ParseRecurringPattern(Dictionary<string, object>? dict)
        {
            if (dict == null) return null;
            var pattern = new RecurringPattern();
            if (dict.TryGetValue("Type", out var typeObj) && Enum.TryParse<RecurringType>(typeObj?.ToString(), true, out var type))
                pattern.Type = type;
            if (dict.TryGetValue("Interval", out var intervalObj) && int.TryParse(intervalObj?.ToString(), out var interval))
                pattern.Interval = interval;
            if (dict.TryGetValue("DaysOfWeek", out var daysObj) && daysObj is IEnumerable<object> days)
                pattern.DaysOfWeek = days.Select(d => Enum.TryParse<DayOfWeek>(d.ToString(), true, out var day) ? day : DayOfWeek.Monday).ToList();
            if (dict.TryGetValue("EndDate", out var endDateObj) && DateTime.TryParse(endDateObj?.ToString(), out var endDate))
                pattern.EndDate = endDate;
            if (dict.TryGetValue("MaxOccurrences", out var maxObj) && int.TryParse(maxObj?.ToString(), out var max))
                pattern.MaxOccurrences = max;
            return pattern;
        }

        private FileType ParseFileType(string value)
        {
            return Enum.TryParse<FileType>(value, true, out var result) ? result : FileType.Other;
        }
        private FileVisibility ParseFileVisibility(string value)
        {
            return Enum.TryParse<FileVisibility>(value, true, out var result) ? result : FileVisibility.Public;
        }
    }

    // Add AppSetting and UserSetting models if missing
    public class AppSetting
    {
        public string Id { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Description { get; set; }
        public string DataType { get; set; } = "string";
        public bool IsPublic { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public class UserSetting
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
