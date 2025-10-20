using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SeatReservation.Application;
using SeatReservation.Application.DataBase;
using SeatReservation.Domain;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Infrastructure.Postgre;

public class ReservertionServiceDbContext : DbContext, IReservationServiceDbContext
{
    private readonly string _connectionString;

    public ReservertionServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservertionServiceDbContext).Assembly);
    }

    public DbSet<Venue> Venues => Set<Venue>();
    
    public DbSet<User> Users => Set<User>();

    // private ILoggerFactory CreateLoggerFactory() =>
    //     LoggerFactory.Create(builder => { builder.AddConsole();});

}