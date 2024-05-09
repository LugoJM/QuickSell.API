using Microsoft.EntityFrameworkCore;
using QuickSell.Persistence;

namespace QuickSell.Extensions;

public static class MigrationsExtension
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<QuickSellDbContext>();

        dbContext.Database.Migrate();
    }
}

