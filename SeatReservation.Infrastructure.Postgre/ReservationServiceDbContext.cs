using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SeatReservation.Application;
using SeatReservation.Application.DataBase;
using SeatReservation.Domain;
using SeatReservation.Domain.Departments;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Infrastructure.Postgre;

public class ReservationServiceDbContext : DbContext, IReadDbContext
{
    private readonly string _connectionString;

    public ReservationServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);

        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.HasPostgresExtension("ltree");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationServiceDbContext).Assembly);
    }

    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Reservation> Reservation => Set<Reservation>();
    public DbSet<ReservationSeat> ReservationSeats => Set<ReservationSeat>();
    public DbSet<Event> Events => Set<Event>();
    
    public DbSet<Department> Departments => Set<Department>();
    
    public DbSet<User> Users => Set<User>();
    
    public IQueryable<Event> EventsRead => Set<Event>().AsQueryable().AsNoTracking();
    public IQueryable<Venue> VenuesRead => Set<Venue>().AsQueryable().AsNoTracking();
    public IQueryable<Seat> SeatsRead => Set<Seat>().AsQueryable().AsNoTracking();
    public IQueryable<Reservation> ReservationsRead => Set<Reservation>().AsQueryable().AsNoTracking();
    public IQueryable<ReservationSeat> ReservationSeatsRead => Set<ReservationSeat>().AsQueryable().AsNoTracking();
    public IQueryable<Department> DepartmentsRead => Set<Department>().AsQueryable().AsNoTracking();

    private ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(builder => { builder.AddConsole();});

}