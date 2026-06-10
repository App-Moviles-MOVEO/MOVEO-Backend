using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

public static class DatabaseConfigurationExtensions
{
    public static IServiceCollection AddAppDbContext(this IServiceCollection services, IConfiguration configuration, bool isProduction = false)
    {
        var connectionString = ResolveConnectionString(configuration);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Database connection string is not set. Configure ConnectionStrings:DefaultConnection or Railway MYSQL_URL/MYSQLHOST/MYSQLDATABASE.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySql(connectionString,
                             new MySqlServerVersion(new Version(8, 0, 21)),
                             mySqlOptions =>
                             {
                                 mySqlOptions.EnableRetryOnFailure(
                                     maxRetryCount: 5,
                                     maxRetryDelay: TimeSpan.FromSeconds(10),
                                     errorNumbersToAdd: null);
                             });

            if (!isProduction)
            {
                options.LogTo(Console.WriteLine, LogLevel.Information)
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors();
            }
        });

        return services;
    }

    private static string? ResolveConnectionString(IConfiguration configuration)
    {
        // Las variables de entorno (Railway) tienen prioridad sobre appsettings
        // para que las credenciales rotadas por la plataforma siempre ganen.
        var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL")
            ?? Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(mysqlUrl) && TryBuildMySqlConnectionString(mysqlUrl, out var urlConnectionString))
            return urlConnectionString;

        var host = Environment.GetEnvironmentVariable("MYSQLHOST");
        var port = Environment.GetEnvironmentVariable("MYSQLPORT");
        var user = Environment.GetEnvironmentVariable("MYSQLUSER");
        var password = Environment.GetEnvironmentVariable("MYSQLPASSWORD");
        var database = Environment.GetEnvironmentVariable("MYSQLDATABASE");

        if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(database))
        {
            var portValue = string.IsNullOrWhiteSpace(port) ? "3306" : port;
            return $"Server={host};Port={portValue};Database={database};User={user};Password={password};";
        }

        return configuration.GetConnectionString("DefaultConnection");
    }

    private static bool TryBuildMySqlConnectionString(string mysqlUrl, out string connectionString)
    {
        connectionString = string.Empty;
        if (!Uri.TryCreate(mysqlUrl, UriKind.Absolute, out var uri))
            return false;
        if (!string.Equals(uri.Scheme, "mysql", StringComparison.OrdinalIgnoreCase))
            return false;

        var userInfo = uri.UserInfo.Split(':', 2);
        var user = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');

        if (string.IsNullOrWhiteSpace(uri.Host) || string.IsNullOrWhiteSpace(database) || string.IsNullOrWhiteSpace(user))
            return false;

        var port = uri.IsDefaultPort ? 3306 : uri.Port;
        connectionString = $"Server={uri.Host};Port={port};Database={database};User={user};Password={password};";
        return true;
    }
}
