using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeatReservation.Application.Reservations;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Infrastructure.Postgre.Repositories;

public class ReservationsRepository : IReservationRepository
{
    private readonly ReservationServiceDbContext _dbContext;
    private readonly ILogger<ReservationsRepository> _logger;

    public ReservationsRepository(ReservationServiceDbContext dbContext, ILogger<ReservationsRepository> logger)
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
    
    public async Task<bool> AnySeatsAlreadyReserved(Guid eventId, IEnumerable<SeatId> seatIds, CancellationToken cancellationToken)
    {
        var hasReservedSeats = await _dbContext.Reservation
            .Where(r => r.EventId == eventId)
            .Where(r => r.ReservedSeats.Any(rs => seatIds.Contains(rs.SeatId)))
            .AnyAsync(cancellationToken);

        return hasReservedSeats;
    }
}