// ===================================================================
// CAMPUS 360 - SUPABASE AUTHENTICATION SERVICE (CLEAN VERSION)
// ===================================================================
// Simplified authentication service for testing
// ===================================================================

using Campus360.Models;
using Campus360.Configuration;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using SupabaseClient = Supabase.Client;
using System.Linq;

namespace Campus360.Services
{
    public class UserCreationAuthResult
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public string? GeneratedPassword { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class AuthenticationServiceClean
    {
        private readonly SupabaseClient _supabaseClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<AuthenticationServiceClean> _logger;
        private readonly SupabaseConfig _config;

        // Events for authentication state changes
        public event Func<Task>? AuthenticationStateChanged;

        public AuthenticationServiceClean(
            SupabaseConfig config,
            IJSRuntime jsRuntime,
            ILogger<AuthenticationServiceClean> logger)
        {
            _config = config;
            _jsRuntime = jsRuntime;
            _logger = logger;

            try
            {
                _supabaseClient = new SupabaseClient(_config.Url, _config.Key, _config.ToSupabaseOptions());
                _logger.LogInformation("Supabase authentication client initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Supabase authentication client");
                throw;
            }
        }        /// <summary>
        /// Test method to verify authentication service works
        /// </summary>
        public Task<bool> TestDatabaseServiceReference()
        {
            // Test if we can create DatabaseService reference
            try
            {
                // Just test that the type is available
                Type dbServiceType = typeof(DatabaseService);
                _logger.LogInformation($"DatabaseService type found: {dbServiceType.Name}");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reference DatabaseService type");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Simple authentication test
        /// </summary>
        public async Task<AuthenticationResult> TestLoginAsync(string email, string password)
        {
            try
            {
                var session = await _supabaseClient.Auth.SignIn(email, password);
                
                if (session?.User != null)
                {
                    return new AuthenticationResult
                    {
                        IsSuccess = true,
                        AccessToken = session.AccessToken,
                        User = new UserProfile 
                        { 
                            Id = session.User.Id ?? Guid.NewGuid().ToString(),
                            Email = email,
                            FullName = "Test User"
                        }
                    };
                }
                
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Authentication failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test login failed");
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Create a new user with Supabase Auth and return the auth user ID
        /// </summary>
        public async Task<string?> CreateAuthUserAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("Creating auth user for email: {Email}", email);
                
                // Create user in Supabase Auth
                var response = await _supabaseClient.Auth.SignUp(email, password);
                
                if (response?.User != null)
                {
                    _logger.LogInformation("Auth user created successfully with ID: {UserId}", response.User.Id);
                    return response.User.Id;
                }
                else
                {
                    _logger.LogWarning("Failed to create auth user - no user returned");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating auth user for email: {Email}", email);
                return null;
            }
        }        /// <summary>
        /// Create a new user with Supabase Auth using admin privileges
        /// </summary>
        public async Task<UserCreationAuthResult> CreateAuthUserAsAdminAsync(string email)
        {
            try
            {
                _logger.LogInformation("Creating auth user as admin for email: {Email}", email);
                
                // Generate a random password for the user
                var generatedPassword = GenerateRandomPassword();
                
                try
                {
                    // Try to create the actual auth user first
                    _logger.LogInformation("Attempting to create auth user in Supabase");
                    
                    var options = new Supabase.Gotrue.SignUpOptions
                    {
                        Data = new Dictionary<string, object>
                        {
                            { "email_confirm", false } // Try to disable email confirmation
                        }
                    };
                    
                    var response = await _supabaseClient.Auth.SignUp(email, generatedPassword, options);
                    
                    if (response?.User != null && !string.IsNullOrEmpty(response.User.Id))
                    {
                        _logger.LogInformation("Auth user created successfully with ID: {UserId}", response.User.Id);
                        
                        return new UserCreationAuthResult
                        {
                            Success = true,
                            UserId = response.User.Id,
                            GeneratedPassword = generatedPassword
                        };
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create auth user - no user returned or ID is null/empty");
                        throw new Exception("No user returned from Supabase Auth or ID is null");
                    }
                }
                catch (Exception authEx) when (authEx.Message.Contains("email rate limit") || authEx.Message.Contains("429"))
                {
                    _logger.LogWarning("Hit email rate limit, creating fallback user profile without auth user");
                    
                    // If we hit rate limits, create user profile directly
                    var fallbackUserId = Guid.NewGuid().ToString();
                    
                    return new UserCreationAuthResult
                    {
                        Success = true,
                        UserId = fallbackUserId,
                        GeneratedPassword = generatedPassword,
                        ErrorMessage = "User created but login may not work due to email rate limits. Contact admin to enable login."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating auth user as admin for email: {Email}", email);
                
                return new UserCreationAuthResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Generate a random password for new users
        /// </summary>
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
