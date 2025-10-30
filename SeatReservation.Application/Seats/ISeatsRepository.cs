using CSharpFunctionalExtensions;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Application.Seats;

public interface ISeatsRepository
{
    Task<IReadOnlyList<Seat>> GetByIds(IEnumerable<SeatId> seatIds, CancellationToken cancellationToken);

    Task<IReadOnlyList<Seat>> GetAvailableSeats(
        VenueId venueId,
        Domain.Events.EventId eventId,
        int? rowNumber,
        CancellationToken cancellationToken);
}