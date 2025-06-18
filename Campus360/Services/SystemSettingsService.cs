using Campus360.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Supabase.Postgrest;
using Postgrest = Supabase.Postgrest;

namespace Campus360.Services
{
    /// <summary>
    /// System Settings Service for managing global application settings and user preferences
    /// </summary>
    public class SystemSettingsService
    {
        private readonly DatabaseService _databaseService;
        private readonly UserContextService _userContextService;
        private readonly ILogger<SystemSettingsService> _logger;
        private static readonly Dictionary<string, object> _cache = new();
        private static DateTime _lastCacheUpdate = DateTime.MinValue;
        private static readonly TimeSpan CacheTimeout = TimeSpan.FromMinutes(30);

        public SystemSettingsService(
            DatabaseService databaseService,
            UserContextService userContextService,
            ILogger<SystemSettingsService> logger)
        {
            _databaseService = databaseService;
            _userContextService = userContextService;
            _logger = logger;
        }

        // ============= SYSTEM SETTINGS MANAGEMENT =============

        /// <summary>
        /// Get all system settings (admin only)
        /// </summary>
        public async Task<List<SystemSetting>> GetSystemSettingsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all system settings");
                  var response = await _databaseService.Client
                    .From<SystemSettingDb>()
                    .Select("*")
                    .Order("category", Postgrest.Constants.Ordering.Ascending)
                    .Get();

                return response.Models?.Select(db => db.ToSystemSetting()).ToList() ?? new List<SystemSetting>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system settings: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get public system settings (accessible to all users)
        /// </summary>
        public async Task<List<SystemSetting>> GetPublicSystemSettingsAsync()
        {
            try
            {
                _logger.LogInformation("Getting public system settings");
                  var response = await _databaseService.Client
                    .From<SystemSettingDb>()
                    .Select("*")
                    .Where(s => s.IsPublic == true)
                    .Order("category", Postgrest.Constants.Ordering.Ascending)
                    .Get();

                return response.Models?.Select(db => db.ToSystemSetting()).ToList() ?? new List<SystemSetting>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public system settings: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get a specific system setting by category and key
        /// </summary>
        public async Task<SystemSetting?> GetSystemSettingAsync(string category, string key)
        {
            try
            {
                _logger.LogInformation("Getting system setting: {Category}.{Key}", category, key);
                
                var response = await _databaseService.Client
                    .From<SystemSettingDb>()
                    .Select("*")
                    .Where(s => s.Category == category && s.Key == key)
                    .Limit(1)
                    .Get();

                return response.Models?.FirstOrDefault()?.ToSystemSetting();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system setting {Category}.{Key}: {Message}", category, key, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get system setting value with type conversion
        /// </summary>
        public async Task<T?> GetSystemSettingValueAsync<T>(string category, string key, T? defaultValue = default)
        {
            try
            {
                var setting = await GetSystemSettingAsync(category, key);
                if (setting == null) return defaultValue;

                return setting.GetValue<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system setting value {Category}.{Key}: {Message}", category, key, ex.Message);
                return defaultValue;
            }
        }

        /// <summary>
        /// Create or update a system setting
        /// </summary>
        public async Task<SystemSetting> SaveSystemSettingAsync(SystemSettingDto dto)
        {
            try
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                if (currentUser?.Role != "admin")
                {
                    throw new UnauthorizedAccessException("Only administrators can modify system settings");
                }

                _logger.LogInformation("Saving system setting: {Category}.{Key}", dto.Category, dto.Key);

                var existing = await GetSystemSettingAsync(dto.Category, dto.Key);
                
                if (existing != null)
                {
                    // Update existing setting
                    var updateModel = new SystemSettingDb
                    {
                        Id = existing.Id,
                        Category = dto.Category,
                        Key = dto.Key,
                        Value = dto.Value,
                        ValueType = dto.ValueType.ToString().ToLower(),
                        Description = dto.Description,
                        IsEncrypted = dto.IsEncrypted,
                        IsPublic = dto.IsPublic,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = currentUser.Id
                    };

                    var response = await _databaseService.Client
                        .From<SystemSettingDb>()
                        .Where(s => s.Id == existing.Id)
                        .Update(updateModel);

                    if (response.Models?.Any() == true)
                    {
                        InvalidateCache();
                        return response.Models.First().ToSystemSetting();
                    }
                }
                else
                {
                    // Create new setting
                    var newModel = new SystemSettingDb
                    {
                        Id = Guid.NewGuid().ToString(),
                        Category = dto.Category,
                        Key = dto.Key,
                        Value = dto.Value,
                        ValueType = dto.ValueType.ToString().ToLower(),
                        Description = dto.Description,
                        IsEncrypted = dto.IsEncrypted,
                        IsPublic = dto.IsPublic,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = currentUser.Id,
                        UpdatedBy = currentUser.Id
                    };

                    var response = await _databaseService.Client
                        .From<SystemSettingDb>()
                        .Insert(newModel);

                    if (response.Models?.Any() == true)
                    {
                        InvalidateCache();
                        return response.Models.First().ToSystemSetting();
                    }
                }

                throw new InvalidOperationException("Failed to save system setting");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving system setting {Category}.{Key}: {Message}", dto.Category, dto.Key, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Delete a system setting
        /// </summary>
        public async Task<bool> DeleteSystemSettingAsync(string id)
        {
            try
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                if (currentUser?.Role != "admin")
                {
                    throw new UnauthorizedAccessException("Only administrators can delete system settings");
                }

                _logger.LogInformation("Deleting system setting: {Id}", id);

                await _databaseService.Client
                    .From<SystemSettingDb>()
                    .Where(s => s.Id == id)
                    .Delete();

                InvalidateCache();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting system setting {Id}: {Message}", id, ex.Message);
                return false;
            }
        }

        // ============= USER PREFERENCES MANAGEMENT =============

        /// <summary>
        /// Get user preferences for current user
        /// </summary>
        public async Task<List<UserPreference>> GetUserPreferencesAsync()
        {
            try
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                if (currentUser == null) return new List<UserPreference>();

                _logger.LogInformation("Getting user preferences for user: {UserId}", currentUser.Id);
                  var response = await _databaseService.Client
                    .From<UserPreferenceDb>()
                    .Select("*")
                    .Where(p => p.UserId == currentUser.Id)
                    .Order("category", Postgrest.Constants.Ordering.Ascending)
                    .Get();

                return response.Models?.Select(db => db.ToUserPreference()).ToList() ?? new List<UserPreference>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preferences: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get specific user preference
        /// </summary>
        public async Task<UserPreference?> GetUserPreferenceAsync(string category, string key)
        {
            try
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                if (currentUser == null) return null;

                _logger.LogInformation("Getting user preference: {Category}.{Key} for user: {UserId}", category, key, currentUser.Id);
                
                var response = await _databaseService.Client
                    .From<UserPreferenceDb>()
                    .Select("*")
                    .Where(p => p.UserId == currentUser.Id && p.Category == category && p.Key == key)
                    .Limit(1)
                    .Get();

                return response.Models?.FirstOrDefault()?.ToUserPreference();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preference {Category}.{Key}: {Message}", category, key, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get user preference value with type conversion
        /// </summary>
        public async Task<T?> GetUserPreferenceValueAsync<T>(string category, string key, T? defaultValue = default)
        {
            try
            {
                var preference = await GetUserPreferenceAsync(category, key);
                if (preference == null) return defaultValue;

                return preference.GetValue<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preference value {Category}.{Key}: {Message}", category, key, ex.Message);
                return defaultValue;
            }
        }

        /// <summary>
        /// Save user preference
        /// </summary>
        public async Task<UserPreference> SaveUserPreferenceAsync(UserPreferenceDto dto)
        {
            try
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    throw new UnauthorizedAccessException("User must be logged in to save preferences");
                }

                _logger.LogInformation("Saving user preference: {Category}.{Key} for user: {UserId}", dto.Category, dto.Key, currentUser.Id);

                var existing = await GetUserPreferenceAsync(dto.Category, dto.Key);
                
                if (existing != null)
                {
                    // Update existing preference
                    var updateModel = new UserPreferenceDb
                    {
                        Id = existing.Id,
                        UserId = currentUser.Id,
                        Category = dto.Category,
                        Key = dto.Key,
                        Value = dto.Value,
                        ValueType = dto.ValueType.ToString().ToLower(),
                        UpdatedAt = DateTime.UtcNow
                    };

                    var response = await _databaseService.Client
                        .From<UserPreferenceDb>()
                        .Where(p => p.Id == existing.Id)
                        .Update(updateModel);

                    if (response.Models?.Any() == true)
                    {
                        return response.Models.First().ToUserPreference();
                    }
                }
                else
                {
                    // Create new preference
                    var newModel = new UserPreferenceDb
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = currentUser.Id,
                        Category = dto.Category,
                        Key = dto.Key,
                        Value = dto.Value,
                        ValueType = dto.ValueType.ToString().ToLower(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var response = await _databaseService.Client
                        .From<UserPreferenceDb>()
                        .Insert(newModel);

                    if (response.Models?.Any() == true)
                    {
                        return response.Models.First().ToUserPreference();
                    }
                }

                throw new InvalidOperationException("Failed to save user preference");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user preference {Category}.{Key}: {Message}", dto.Category, dto.Key, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Delete user preference
        /// </summary>
        public async Task<bool> DeleteUserPreferenceAsync(string id)
        {
            try
            {
                var currentUser = await _userContextService.GetCurrentUserAsync();
                if (currentUser == null) return false;

                _logger.LogInformation("Deleting user preference: {Id} for user: {UserId}", id, currentUser.Id);

                await _databaseService.Client
                    .From<UserPreferenceDb>()
                    .Where(p => p.Id == id && p.UserId == currentUser.Id)
                    .Delete();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user preference {Id}: {Message}", id, ex.Message);
                return false;
            }
        }

        // ============= CACHE MANAGEMENT =============

        /// <summary>
        /// Get cached system setting value
        /// </summary>
        public async Task<T?> GetCachedSystemSettingAsync<T>(string category, string key, T? defaultValue = default)
        {
            var cacheKey = $"{category}.{key}";
            
            if (_cache.ContainsKey(cacheKey) && DateTime.UtcNow - _lastCacheUpdate < CacheTimeout)
            {
                try
                {
                    return (T?)_cache[cacheKey];
                }
                catch
                {
                    // If conversion fails, remove from cache and fetch fresh
                    _cache.Remove(cacheKey);
                }
            }

            var value = await GetSystemSettingValueAsync(category, key, defaultValue);
            _cache[cacheKey] = value!;
            _lastCacheUpdate = DateTime.UtcNow;
            
            return value;
        }

        /// <summary>
        /// Invalidate cache
        /// </summary>
        public void InvalidateCache()
        {
            _cache.Clear();
            _lastCacheUpdate = DateTime.MinValue;
        }

        // ============= CONFIGURATION HELPERS =============

        /// <summary>
        /// Get application configuration
        /// </summary>
        public async Task<Dictionary<string, object>> GetAppConfigurationAsync()
        {
            try
            {
                var settings = await GetPublicSystemSettingsAsync();
                var config = new Dictionary<string, object>();

                foreach (var setting in settings)
                {
                    var key = $"{setting.Category}.{setting.Key}";
                    config[key] = setting.ValueType switch
                    {
                        SettingValueType.Boolean => bool.Parse(setting.Value),
                        SettingValueType.Number => double.Parse(setting.Value),
                        SettingValueType.Json => JsonSerializer.Deserialize<object>(setting.Value),
                        _ => setting.Value
                    };
                }

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting app configuration: {Message}", ex.Message);
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Initialize default settings if they don't exist
        /// </summary>
        public async Task InitializeDefaultSettingsAsync()
        {
            try
            {
                _logger.LogInformation("Initializing default system settings");

                var existingSettings = await GetSystemSettingsAsync();
                var existingKeys = existingSettings.Select(s => $"{s.Category}.{s.Key}").ToHashSet();

                var defaultSettings = GetDefaultSettings();
                var currentUser = await _userContextService.GetCurrentUserAsync();

                foreach (var setting in defaultSettings)
                {
                    var key = $"{setting.Category}.{setting.Key}";
                    if (!existingKeys.Contains(key))
                    {
                        await SaveSystemSettingAsync(new SystemSettingDto
                        {
                            Category = setting.Category,
                            Key = setting.Key,
                            Value = setting.Value,
                            ValueType = setting.ValueType,
                            Description = setting.Description,
                            IsPublic = setting.IsPublic
                        });
                    }
                }

                _logger.LogInformation("Default system settings initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default settings: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Get default system settings
        /// </summary>
        private List<SystemSetting> GetDefaultSettings()
        {
            return new List<SystemSetting>
            {
                // Application Settings
                new SystemSetting { Category = "app", Key = "name", Value = "Campus360", ValueType = SettingValueType.String, Description = "Application name", IsPublic = true },
                new SystemSetting { Category = "app", Key = "version", Value = "1.0.0", ValueType = SettingValueType.String, Description = "Application version", IsPublic = true },
                new SystemSetting { Category = "app", Key = "description", Value = "Complete Campus Management System", ValueType = SettingValueType.String, Description = "Application description", IsPublic = true },
                new SystemSetting { Category = "app", Key = "maintenance_mode", Value = "false", ValueType = SettingValueType.Boolean, Description = "Enable maintenance mode", IsPublic = false },
                
                // Academic Settings
                new SystemSetting { Category = "academic", Key = "current_semester", Value = "1", ValueType = SettingValueType.Number, Description = "Current active semester", IsPublic = true },
                new SystemSetting { Category = "academic", Key = "academic_year", Value = "2024-2025", ValueType = SettingValueType.String, Description = "Current academic year", IsPublic = true },
                new SystemSetting { Category = "academic", Key = "max_absence_percentage", Value = "25", ValueType = SettingValueType.Number, Description = "Maximum allowed absence percentage", IsPublic = true },
                new SystemSetting { Category = "academic", Key = "passing_grade", Value = "40", ValueType = SettingValueType.Number, Description = "Minimum passing grade percentage", IsPublic = true },
                
                // Notification Settings
                new SystemSetting { Category = "notifications", Key = "assignment_reminder_days", Value = "3", ValueType = SettingValueType.Number, Description = "Days before assignment due date to send reminder", IsPublic = true },
                new SystemSetting { Category = "notifications", Key = "low_attendance_threshold", Value = "75", ValueType = SettingValueType.Number, Description = "Attendance percentage threshold for alerts", IsPublic = true },
                
                // Security Settings
                new SystemSetting { Category = "security", Key = "session_timeout_minutes", Value = "60", ValueType = SettingValueType.Number, Description = "Session timeout in minutes", IsPublic = false },
                new SystemSetting { Category = "security", Key = "password_min_length", Value = "8", ValueType = SettingValueType.Number, Description = "Minimum password length", IsPublic = true },
                
                // Performance Settings
                new SystemSetting { Category = "performance", Key = "max_page_size", Value = "100", ValueType = SettingValueType.Number, Description = "Maximum items per page in lists", IsPublic = true }
            };
        }
    }
}
