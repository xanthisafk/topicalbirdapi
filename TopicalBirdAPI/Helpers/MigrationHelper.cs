using Microsoft.EntityFrameworkCore;
using TopicalBirdAPI.Data;

namespace TopicalBirdAPI.Helpers
{
    public static class MigrationHelper
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            using AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.Database.Migrate();
        }
    }
}
