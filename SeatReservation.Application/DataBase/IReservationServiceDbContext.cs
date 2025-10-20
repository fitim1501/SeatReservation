using Microsoft.EntityFrameworkCore;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Application.DataBase;

public interface IReservationServiceDbContext
{
    public DbSet<Venue> Venues { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}