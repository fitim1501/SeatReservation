using Microsoft.Extensions.DependencyInjection;

namespace SeatReservation.Infrastructure.Postgre.Seeding;

public static class SeederExtension
{
    public static async Task<IServiceProvider> RunSeeding(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var seeders = scope.ServiceProvider.GetServices<ISeeder>();

        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync();
        }

        return serviceProvider;
    }
}