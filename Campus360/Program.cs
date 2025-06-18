using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Campus360;
using Campus360.Services;
using Campus360.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Campus360.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register Supabase configuration with fallback
builder.Services.AddSingleton<SupabaseConfig>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    try 
    {
        return SupabaseConfig.FromConfiguration(configuration);
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("Supabase URL is not configured"))
    {
        // Fallback configuration for development - replace with your actual values
        return new SupabaseConfig
        {
            Url = "https://rxvqgsqbicnzeomwjaol.supabase.co",
            Key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InJ4dnFnc3FiaWNuemVvbXdqYW9sIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDk2NjA5ODEsImV4cCI6MjA2NTIzNjk4MX0.lVSg34V3TE73K_N0eOcoI0PW3lIzzCm8n04hrb7iouc",
            Schema = "public"
        };
    }
});

// Register Supabase client for DI
builder.Services.AddSingleton<Supabase.Client>(provider =>
{
    var config = provider.GetRequiredService<SupabaseConfig>();
    var options = config.ToSupabaseOptions();
    return new Supabase.Client(config.Url, config.Key, options);
});

// Register logging
builder.Services.AddLogging();

// Register core database service
builder.Services.AddScoped<DatabaseService>();

// Register activity logging service
builder.Services.AddScoped<ActivityLogService>();

// Register our custom services with database integration
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<AuthenticationServiceClean>();
builder.Services.AddScoped<UserContextService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<AutomationEngineService>();

// Register enhanced services with automation integration
builder.Services.AddScoped<TeacherServiceEnhanced>();
builder.Services.AddScoped<StudentServiceEnhanced>();

// Register calendar service
builder.Services.AddScoped<CalendarService>();

// Register timer service for automation processing (Blazor WebAssembly compatible)
builder.Services.AddScoped<AutomationTimerService>();

// Register automation test service for validation
builder.Services.AddScoped<AutomationTestService>();

// Register file management service
builder.Services.AddScoped<FileManagementService>();

// Register system settings and reporting services
builder.Services.AddScoped<SystemSettingsService>();
builder.Services.AddScoped<ReportingService>();

await builder.Build().RunAsync();
