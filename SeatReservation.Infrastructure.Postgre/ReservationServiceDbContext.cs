using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SeatReservation.Application;
using SeatReservation.Application.DataBase;
using SeatReservation.Domain;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Infrastructure.Postgre;

public class ReservationServiceDbContext : DbContext, IReservationServiceDbContext
{
    private readonly string _connectionString;

    public ReservationServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
        
        optionsBuilder.
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationServiceDbContext).Assembly);
    }

    public DbSet<Venue> Venues => Set<Venue>();
    
    public DbSet<User> Users => Set<User>();

    // private ILoggerFactory CreateLoggerFactory() =>
    //     LoggerFactory.Create(builder => { builder.AddConsole();});

}