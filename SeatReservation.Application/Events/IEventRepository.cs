using CSharpFunctionalExtensions;
using SeatReservation.Domain.Events;
using Shared;

namespace SeatReservation.Application.Events;

public interface IEventRepository
{
    Task<Result<Event, Error>> GetByIdWithLock(EventId eventId, CancellationToken cancellationToken);

    Task<Event?> GetById(EventId eventId, CancellationToken cancellationToken);

}