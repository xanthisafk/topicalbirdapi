using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace TopicalBirdAPI.Helpers
{
    public static class HealthCheckResponseWriter
    {
        public static Task Write(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                api = result.Entries["api"].Status.ToString(),
                database = result.Entries["database"].Status.ToString()
            };

            return JsonSerializer.SerializeAsync(context.Response.Body, response);
        }
    }
}
