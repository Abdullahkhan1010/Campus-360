using Campus360.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Campus360.Services
{
    /// <summary>
    /// Service for managing activity logging in Campus360
    /// </summary>
    public class ActivityLogService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<ActivityLogService> _logger;

        public ActivityLogService(DatabaseService databaseService, ILogger<ActivityLogService> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new activity log entry
        /// </summary>
        public async Task<ActivityLogDb> LogActivityAsync(string userId, string title, string description, string activityType)
        {
            try
            {
                _logger.LogInformation("Creating activity log: {Title} for user {UserId}", title, userId);
                
                var log = new ActivityLogDb
                {
                    UserId = userId,
                    Title = title,
                    Description = description,
                    ActivityType = activityType,
                    CreatedAt = DateTime.UtcNow
                };
                
                return await _databaseService.CreateActivityLogAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity log: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Logs a record change with old and new values for auditing
        /// </summary>
        public async Task LogRecordChangeAsync(string userId, string tableName, string recordId, 
            Dictionary<string, object>? oldValues, Dictionary<string, object>? newValues, string description)
        {
            try
            {
                _logger.LogInformation("Logging record change in {TableName} for record {RecordId}", tableName, recordId);
                await _databaseService.LogRecordChangeAsync(userId, tableName, recordId, oldValues, newValues, description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging record change: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets recent activity logs with optional filtering
        /// </summary>
        public async Task<List<ActivityModel>> GetRecentActivityLogsAsync(int count = 10, string? userId = null)
        {
            try
            {
                _logger.LogInformation("Getting recent activity logs, count: {Count}, userId: {UserId}", count, userId ?? "all");
                var logs = await _databaseService.GetRecentActivityLogsAsync(count, userId);
                
                // Enrich with user names if needed
                foreach (var log in logs)
                {
                    if (!string.IsNullOrEmpty(log.UserId) && string.IsNullOrEmpty(log.UserName))
                    {
                        var user = await _databaseService.GetUserByIdAsync(log.UserId);
                        if (user != null)
                        {
                            log.UserName = user.FullName;
                        }
                    }
                }
                
                return logs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activity logs: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets detailed activity logs with comprehensive filtering options
        /// </summary>
        public async Task<List<ActivityLogDb>> GetActivityLogsAsync(string? userId = null, string? activityType = null,
            DateTime? startDate = null, DateTime? endDate = null, string? tableName = null, string? recordId = null, int limit = 100)
        {
            try
            {
                _logger.LogInformation("Getting activity logs with filters: userId={UserId}, type={Type}, table={Table}", 
                    userId ?? "all", activityType ?? "all", tableName ?? "all");
                    
                return await _databaseService.GetActivityLogsAsync(userId, activityType, startDate, endDate, tableName, recordId, limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed activity logs: {Message}", ex.Message);
                throw;
            }
        }
    }
}
