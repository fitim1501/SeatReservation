using CSharpFunctionalExtensions;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Application.Reservations;

public interface IReservationRepository
{
    Task<Result<Guid, Error>> Add(Reservation reservation, CancellationToken cancellationToken);
    Task<bool> AnySeatsAlreadyReserved(Guid eventId, IEnumerable<SeatId> seatIds, CancellationToken cancellationToken);
}