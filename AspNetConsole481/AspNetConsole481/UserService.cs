using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Serilog;

namespace AspNetConsole481
{
    public class UserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly Serilog.ILogger _seriLogger;

        public UserService(ILogger<UserService> logger, Serilog.ILogger seriLogger)
        {
            _logger = logger;
            _seriLogger = seriLogger;
        }

        public bool Authenticate(string username, string password)
        {
            using (var activity = Activity.Current)
            {
                if (activity != null)
                {
                    activity.TraceStateString = $"userid={username}";
                    // Simulate authentication logic
                    if (username == "foo" && password == "bar")
                    {
                        _logger.LogInformation($"User '{username}' logged in successfully.");
                        _seriLogger.Error($"SERILOG - ERROR - User '{username}' logged in successfully.");
                        _seriLogger.Fatal($"SERILOG - FATAL - User '{username}' logged in successfully.");
                        return true;
                    }
                    else
                    {
                        _seriLogger.Error($"SERILOG - ERROR - User '{username}' login failed.");
                        _seriLogger.Fatal($"SERILOG - FATAL - User '{username}' login failed.");
                        return false;
                    }
                }
                else
                {
                    // Activity is null, handle if needed
                    _logger.LogWarning("Failed to start UserAuthentication activity.");
                    return false;
                }
            }
        }

    }
}
