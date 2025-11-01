using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Reflection;
using System.Threading.RateLimiting;
using TopicalBirdAPI.Data;
using TopicalBirdAPI.Helpers;
using TopicalBirdAPI.Models;

namespace TopicalBirdAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Rate limit the API to 100 requests per minute
            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey:
                            httpContext.User.Identity?.Name
                            ?? httpContext.Connection.RemoteIpAddress?.ToString()
                            ?? httpContext.Request.Headers.Host.ToString()
                            ?? "unknown",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });


            // Database Context
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("PSQL")));

            builder.Services.AddHealthChecks()
                .AddCheck("api", () => HealthCheckResult.Healthy("API is healthy."))
                .AddDbContextCheck<AppDbContext>(
                    name: "database",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "database" }
                );

            // Allow CORS
            builder.Services.AddCors(o => o.AddPolicy("MyCorsPolicy", corsBuilder =>
            {
                corsBuilder.WithOrigins(
                    "http://localhost:3000",
                    "http://localhost:8888",
                    "http://localhost:5173",
                    "http://0.0.0.0:3000",
                    "http://0.0.0.0:8888",
                    "http://0.0.0.0:5173"
                    )
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials();
            }));

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Topicalbird API",
                    Description = "Backend API for Topicalbird forums.",
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            // Authentication
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);
            builder.Services.AddIdentityCore<Users>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddApiEndpoints();

            // Logging
            builder.Logging.ClearProviders().AddConsole().AddDebug();
            builder.Services.AddSingleton<LoggingHelper>(); // CustomLogger

            builder.Host.UseWindowsService();
            builder.WebHost.UseUrls("http://0.0.0.0:9999");

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = "swagger";
                options.InjectStylesheet("/css/swagger-custom.css");
            });
            app.MapSwagger("/openapi/{documentName}.json");
            app.MapScalarApiReference("/", o =>
            {
                o.WithTitle("Topicalbird API Documentation");
                o.WithTheme(ScalarTheme.BluePlanet);
                o.HideModels = true;
                o.SortOperationsByMethod();
                o.SortTagsAlphabetically();
                o.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.JavaScript, ScalarClient.Axios);
                o.Favicon = "/content/assets/defaults/api_logo.svg";

            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.ApplyMigrations();
            }

            app.MapHealthChecks("/health", new HealthCheckOptions()
            {
                ResponseWriter = HealthCheckResponseWriter.Write
            });

            app.UseHttpsRedirection();

            app.UseCors("MyCorsPolicy"); // apply the CORS policy
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Allow file hosting
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Use Identity for User management
            // app.MapIdentityApi<Users>(); // Now using custom controller


            app.Run();
        }
    }
}