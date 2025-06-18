using Campus360.Models;

namespace Campus360.Services
{
    public class UserContextService
    {
        private UserProfile? _currentUser;
        private readonly AuthenticationService _authService;
        
        public event Action? OnUserChanged;

        public UserProfile? CurrentUser 
        { 
            get => _currentUser;
            private set
            {
                _currentUser = value;
                OnUserChanged?.Invoke();
            }
        }

        public bool IsLoggedIn => CurrentUser != null;

        public bool IsAdmin => CurrentUser?.Role == "admin";
        public bool IsTeacher => CurrentUser?.Role == "teacher";
        public bool IsStudent => CurrentUser?.Role == "student";

        public UserContextService(AuthenticationService authService)
        {
            _authService = authService;
        }

        public void SetUser(UserProfile? user)
        {
            CurrentUser = user;
        }        public void ClearUser()
        {
            CurrentUser = null;
        }        public async Task LogoutAsync()
        {
            try
            {
                await _authService.LogoutAsync();
                ClearUser();
            }
            catch
            {
                // Ensure user is cleared even if auth service fails
                ClearUser();
            }
        }

        public bool HasRole(string role)
        {
            return CurrentUser?.Role?.Equals(role, StringComparison.OrdinalIgnoreCase) == true;
        }

        public bool CanAccessAdminRoutes()
        {
            return IsAdmin;
        }

        public bool CanAccessTeacherRoutes()
        {
            return IsAdmin || IsTeacher;
        }        public bool CanAccessStudentRoutes()
        {
            return IsLoggedIn; // All authenticated users can access basic student views
        }

        public async Task<UserProfile?> GetCurrentUserAsync()
        {
            if (CurrentUser != null)
            {
                return CurrentUser;
            }
            
            // Try to restore from session
            var restored = await RestoreUserSessionAsync();
            return restored ? CurrentUser : null;
        }        public async Task<bool> RestoreUserSessionAsync()
        {
            try
            {
                // Add a small delay for Blazor WebAssembly to initialize
                await Task.Delay(50);
                
                var isAuthenticated = await _authService.IsAuthenticatedAsync();
                if (isAuthenticated)
                {
                    var user = await _authService.GetCurrentUserAsync();
                    if (user != null)
                    {
                        SetUser(user);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error restoring user session: {ex.Message}");
                return false;
            }
        }
    }
}
