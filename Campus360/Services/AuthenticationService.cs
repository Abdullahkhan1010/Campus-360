// ===================================================================
// CAMPUS 360 - SUPABASE AUTHENTICATION SERVICE
// ===================================================================
// Complete authentication service with Supabase Auth integration
// Maintains compatibility with existing authentication flows
// ===================================================================

using Campus360.Models;
using Campus360.Configuration;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using SupabaseClient = Supabase.Client;

namespace Campus360.Services
{
    public class AuthenticationService
    {
        private readonly SupabaseClient _supabaseClient;
        private readonly DatabaseService _databaseService;
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly SupabaseConfig _config;

        // Events for authentication state changes
        public event Func<Task>? AuthenticationStateChanged;

        public AuthenticationService(
            SupabaseConfig config,
            DatabaseService databaseService,
            IJSRuntime jsRuntime,
            ILogger<AuthenticationService> logger)
        {
            _config = config;
            _databaseService = databaseService;
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
        /// Login with email and password
        /// </summary>
        public async Task<AuthenticationResult> LoginAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("Attempting login for email: {Email}", email);

                var session = await _supabaseClient.Auth.SignIn(email, password);
                  if (session?.User != null)
                {
                    UserProfile? userProfile = null;                    try
                    {
                        // Add database connection test for debugging
                        var testResult = await _databaseService.TestDatabaseConnectionAsync();
                        _logger.LogInformation("Database test result: {TestResult}", testResult);
                        
                        // Get user profile from database using standard method
                        userProfile = await _databaseService.GetUserByEmailAsync(email);
                        
                        if (userProfile == null)
                        {
                            _logger.LogWarning("User profile is null after database query for: {Email}", email);
                        }
                    }                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, "Database query failed for email: {Email}. Error: {Error}", email, dbEx.Message);
                        throw; // Don't create fallback, show the real error
                    }
                    
                    if (userProfile != null)
                    {
                        try
                        {
                            // Try to update last login time
                            userProfile.LastLoginAt = DateTime.UtcNow;
                            await _databaseService.UpdateUserAsync(userProfile);
                            
                            // Try to log successful login
                            await _databaseService.LogActivityAsync("user_login", $"User {email} logged in successfully");
                        }
                        catch (Exception updateEx)
                        {
                            _logger.LogWarning(updateEx, "Could not update user data due to database issues, but login succeeded");
                        }
                        
                        _logger.LogInformation("Login successful for user: {Email}", email);
                        
                        // Notify of authentication state change
                        AuthenticationStateChanged?.Invoke();
                        
                        return new AuthenticationResult
                        {
                            IsSuccess = true,
                            AccessToken = session.AccessToken,
                            User = userProfile
                        };
                    }
                    else
                    {
                        _logger.LogWarning("User profile not found for authenticated user: {Email}", email);
                        return new AuthenticationResult
                        {
                            IsSuccess = false,
                            ErrorMessage = "User profile not found. Please contact administrator."
                        };
                    }
                }
                else
                {
                    _logger.LogWarning("Authentication failed for email: {Email}", email);
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid email or password"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", email);
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Login failed. Please try again."
                };
            }
        }

        /// <summary>
        /// Register a new user with email and password, and create user profile
        /// </summary>
        public async Task<AuthenticationResult> RegisterAsync(string email, string password, string fullName, string role, string? departmentId = null)
        {
            try
            {
                var signUpResponse = await _supabaseClient.Auth.SignUp(email, password);
                if (signUpResponse?.User != null)
                {
                    // Create user profile in database
                    var userProfile = new UserProfile
                    {
                        Id = signUpResponse.User.Id ?? Guid.NewGuid().ToString(),
                        Email = email,
                        FullName = fullName,
                        Role = role,
                        DepartmentId = departmentId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsVerified = false,
                        IsActive = true
                    };
                    await _databaseService.CreateUserAsync(userProfile);

                    // Optionally send verification email
                    await SendEmailVerificationAsync(email);

                    AuthenticationStateChanged?.Invoke();

                    return new AuthenticationResult
                    {
                        IsSuccess = true,
                        User = userProfile
                    };
                }
                else
                {
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Registration failed."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", email);
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Send email verification to the user (Supabase will send automatically on sign up; no manual resend API in .NET SDK)
        /// </summary>
        public Task<bool> SendEmailVerificationAsync(string email)
        {
            try
            {
                // Supabase .NET SDK does not support manual resend; this is a no-op for now.
                _logger.LogInformation("Supabase will send verification email automatically to {Email} if configured.", email);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}", email);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                await _supabaseClient.Auth.ResetPasswordForEmail(email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                return false;
            }
        }

        /// <summary>
        /// Check if the current user's email is verified
        /// </summary>
        public async Task<bool> IsEmailVerifiedAsync()
        {
            try
            {
                var session = await GetSessionAsync();
                return session?.User?.ConfirmedAt != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var session = await GetSessionAsync();
                return session != null && !string.IsNullOrEmpty(session.AccessToken);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get current session
        /// </summary>
        public Task<Session?> GetSessionAsync()
        {
            try
            {
                return Task.FromResult(_supabaseClient.Auth.CurrentSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current session");
                return Task.FromResult<Session?>(null);
            }
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        public async Task<bool> LogoutAsync()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                
                await _supabaseClient.Auth.SignOut();
                
                if (currentUser != null)
                {
                    await _databaseService.LogActivityAsync("user_logout", $"User {currentUser.Email} logged out");
                }
                
                // Notify of authentication state change
                AuthenticationStateChanged?.Invoke();
                
                _logger.LogInformation("User logged out successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return false;
            }
        }

        /// <summary>
        /// Get current authenticated user
        /// </summary>
        public async Task<UserProfile?> GetCurrentUserAsync()
        {
            try
            {
                var session = await GetSessionAsync();
                if (session?.User != null && !string.IsNullOrEmpty(session.User.Id))
                {
                    var userProfile = await _databaseService.GetUserByIdAsync(session.User.Id);
                    return userProfile;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }

        /// <summary>
        /// Check authentication health
        /// </summary>
        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                // Simple health check - try to get current session
                var session = await GetSessionAsync();
                return true; // If we get here without exception, it's healthy
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication service health check failed");
                return false;
            }
        }
    }
}
