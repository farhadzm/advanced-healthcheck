using AdvancedHealthCheck.Api.HealthChecks;
using HealthChecks.SqlServer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using System;
using System.Net.Mime;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace AdvancedHealthCheck.Api.Extensions
{
    public static class HealthCheckExtension
    {
        public static void AddAdvancedHealthCheck(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var sqlConnection = configuration["ConnectionStrings:SqlServer"];
            var redisConnection = configuration["ConnectionStrings:Redis"];
            var externalServiceUri = configuration["someService:uri"];

            services
                .AddHealthChecks()
                .AddRedis(redisConnection, "redis")
                .AddSqlServer(new SqlServerHealthCheckOptions
                {
                    ConnectionString = sqlConnection,
                })
                .AddTcpHealthCheck(option =>
                {
                    var uri = new Uri(externalServiceUri);
                    option.AddHost(uri.Host, uri.Port);
                })
                .AddCheck<PingApiHealthCheck>("PingApi");
        }

        public static Task WriteResponse(
            HttpContext context,
            HealthReport report)
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            string json = JsonSerializer.Serialize(
                new
                {
                    Status = report.Status.ToString(),
                    Duration = report.TotalDuration,
                    Info = report.Entries
                        .Select(e =>
                            new
                            {
                                Key = e.Key,
                                Description = e.Value.Description,
                                Duration = e.Value.Duration,
                                Status = Enum.GetName(
                                    typeof(HealthStatus),
                                    e.Value.Status),
                                Error = e.Value.Exception?.Message,
                                Data = e.Value.Data
                            })
                        .ToList()
                },
                jsonSerializerOptions);

            context.Response.ContentType = MediaTypeNames.Application.Json;
            return context.Response.WriteAsync(json);
        }
    }
}
