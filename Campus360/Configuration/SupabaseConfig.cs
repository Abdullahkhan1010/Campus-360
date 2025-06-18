using Microsoft.Extensions.Configuration;

namespace Campus360.Configuration
{
    public class SupabaseConfig
    {
        public string Url { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Schema { get; set; } = "public";

        public static SupabaseConfig FromConfiguration(IConfiguration configuration)
        {
            var config = new SupabaseConfig();
            configuration.GetSection("Supabase").Bind(config);

            // Validate required settings
            if (string.IsNullOrEmpty(config.Url))
                throw new InvalidOperationException("Supabase URL is not configured");
            
            if (string.IsNullOrEmpty(config.Key))
                throw new InvalidOperationException("Supabase Key is not configured");

            return config;
        }        public Supabase.SupabaseOptions ToSupabaseOptions()
        {
            return new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = true,
                AutoRefreshToken = true,
                // Ensure proper table name resolution
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Prefer", "return=representation" }
                },
                // Explicitly set schema
                Schema = this.Schema
            };
        }
    }
}