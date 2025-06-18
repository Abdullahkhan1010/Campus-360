// Temporary file to test DatabaseService compilation
using Campus360.Configuration;
using Campus360.Models;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using SupabaseClient = Supabase.Client;

namespace Campus360.Services
{
    public class AuthenticationServiceTemp
    {
        private readonly SupabaseClient _supabaseClient;
        private readonly DatabaseService _databaseService;
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<AuthenticationServiceTemp> _logger;
        private readonly SupabaseConfig _config;

        public AuthenticationServiceTemp(
            SupabaseConfig config,
            DatabaseService databaseService,
            IJSRuntime jsRuntime,
            ILogger<AuthenticationServiceTemp> logger)
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
        }

        public async Task<bool> TestDatabaseConnection()
        {
            // Simple test method
            return await Task.FromResult(true);
        }
    }
}
