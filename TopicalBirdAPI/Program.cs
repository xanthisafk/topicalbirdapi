using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            // Database Context
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("PSQL")));

            // Allow CORS
            builder.Services.AddCors(o => o.AddPolicy("MyCorsPolicy", corsBuilder =>
            {
                corsBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            }));

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Authentication
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication().AddCookie(IdentityConstants.ApplicationScheme);
            builder.Services.AddIdentityCore<Users>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddApiEndpoints();

            // Logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.ApplyMigrations();
            }

            app.UseHttpsRedirection();
            app.UseCors("MyCorsPolicy"); // apply the CORS policy
            app.UseAuthorization();

            app.MapControllers();

            // Allow file hosting
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Use Identity for User management
            // app.MapIdentityApi<Users>();


            app.Run();
        }
    }
}