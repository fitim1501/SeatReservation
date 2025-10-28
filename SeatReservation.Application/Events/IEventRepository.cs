using CSharpFunctionalExtensions;
using SeatReservation.Domain.Events;
using Shared;

namespace SeatReservation.Application.Events;

public interface IEventRepository
{
    Task<Result<Event, Error>> GetById(EventId eventId, CancellationToken cancellationToken);

    Task<Result<Event, Error>> GetAvailableForReservationById(EventId eventId, CancellationToken cancellationToken);
}