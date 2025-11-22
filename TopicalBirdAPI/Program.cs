using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Reflection;
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

            // -------------------------------------------------------------
            // Database
            // -------------------------------------------------------------
            var connString =
                Environment.GetEnvironmentVariable("PSQL") ??
                builder.Configuration.GetConnectionString("PSQL");

            Console.WriteLine($"Using database: {connString}");

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connString));

            builder.Services.AddHealthChecks()
                .AddCheck("api", () => HealthCheckResult.Healthy("API is healthy."))
                .AddDbContextCheck<AppDbContext>(
                    name: "database",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "database" }
                );

            // -------------------------------------------------------------
            // CORS
            // -------------------------------------------------------------
            builder.Services.AddCors(o =>
                o.AddPolicy("MyCorsPolicy", corsBuilder =>
                {
                    corsBuilder
                        .WithOrigins(
                            "http://localhost:3000",
                            "http://localhost:8888",
                            "http://localhost:5173",
                            "http://0.0.0.0:3000",
                            "http://0.0.0.0:8888",
                            "http://0.0.0.0:5173",
                            "https://topicalbirdapi.xanthis.xyz",
                            "https://topicalbird.xanthis.xyz",
                            "http://topicalbirdapi.xanthis.xyz",
                            "http://topicalbird.xanthis.xyz"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }));

            // -------------------------------------------------------------
            // Controllers & Swagger
            // -------------------------------------------------------------
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Topicalbird API",
                    Description = "Backend API for Topicalbird forums."
                });

                // Include XML documentation (if available)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            // -------------------------------------------------------------
            // Authentication / Identity
            // -------------------------------------------------------------
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);

            builder.Services.AddIdentityCore<Users>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddApiEndpoints();

            // -------------------------------------------------------------
            // Logging
            // -------------------------------------------------------------
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Services.AddSingleton<LoggingHelper>();

            // -------------------------------------------------------------
            // Host settings (port binding for Docker/VPS)
            // -------------------------------------------------------------
            var port = Environment.GetEnvironmentVariable("PORT") ?? "9999";
            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

            var app = builder.Build();

            // -------------------------------------------------------------
            // Swagger / Scalar UI
            // -------------------------------------------------------------
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
                o.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(
                    ScalarTarget.JavaScript, ScalarClient.Axios);
                o.Favicon = "/content/assets/defaults/api_logo.svg";
            });

            app.ApplyMigrations();

            // -------------------------------------------------------------
            // Middleware
            // -------------------------------------------------------------
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.Write
            });

            app.UseCors("MyCorsPolicy");

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            // API controllers
            app.MapControllers();

            // Static file hosting
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.Run();
        }
    }
}
