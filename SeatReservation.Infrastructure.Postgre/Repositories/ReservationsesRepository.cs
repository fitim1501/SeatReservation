using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeatReservation.Application.Reservations;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;
using Shared;
using EventId = SeatReservation.Domain.Events.EventId;

namespace SeatReservation.Infrastructure.Postgre.Repositories;

public class ReservationsesRepository : IReservationsRepository
{
    private readonly ReservationServiceDbContext _dbContext;
    private readonly ILogger<ReservationsesRepository> _logger;

    public ReservationsesRepository(ReservationServiceDbContext dbContext, ILogger<ReservationsesRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Error>> Add(Reservation reservation, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Reservation.AddAsync(reservation, cancellationToken);

            await _dbContext.SaveChangesAsync();
            
            return reservation.Id.Value;
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail to insert reservation");
            
            return Error.Failure("reservation.insert", "Fail to insert reservation");
        }
    }
    
    public async Task<bool> AnySeatsAlreadyReserved(EventId eventId, IEnumerable<SeatId> seatIds, CancellationToken cancellationToken)
    {
        var hasReservedSeats = await _dbContext.Reservation
            .Where(r => r.EventId == eventId)
            .Where(r => r.ReservedSeats.Any(rs => seatIds.Contains(rs.SeatId)))
            .AnyAsync(cancellationToken);

        return hasReservedSeats;
    }

    public async Task<int> GetReservedSeatsCount(EventId eventId, CancellationToken cancellationToken)
    {
        // await _dbContext.Database.ExecuteSqlAsync(
        //     $"SELECT \"Capacity\" FROM event_details WHERE event_id = {eventId} FOR UPDATE", 
        //     cancellationToken);
        
        return await _dbContext.Reservation
            .Where(r => r.EventId == eventId)
            .Where(r => r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.Pending)
            .SelectMany(r => r.ReservedSeats)
            .CountAsync(cancellationToken);
    }
}