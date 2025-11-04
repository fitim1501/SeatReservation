namespace SeatReservation.Infrastructure.Postgre.Seeding;

public interface ISeeder
{
    Task SeedAsync();
}