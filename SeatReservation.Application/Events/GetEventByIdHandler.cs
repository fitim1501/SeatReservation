using SeatReservation.Contracts;
using SeatReservation.Contracts.Events;
using SeatReservation.Domain.Events;

namespace SeatReservation.Application.Events;

public class GetEventByIdHandler
{
    private readonly IEventRepository _repository;

    public GetEventByIdHandler(IEventRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<GetEventDto?> Handle(GeEventByIdRequest request, CancellationToken cancellationToken)
    {
        var @event = await _repository.GetById(new EventId(request.EventId), cancellationToken);

        if (@event is null)
        {
            return null;
        }

        return new GetEventDto()
        {
            Id = @event.Id.Value,
            Capacity = @event.Details.Capacity,
            Description = @event.Details.Description,
            EndDate = @event.EndDate,
            EventDate = @event.EventDate,
            Name = @event.Name,
            StartDate = @event.StartDate,
            Type = @event.Type.ToString(),
            VenueId = @event.VenueId.Value,
            Status = @event.Status.ToString(),
            Info = @event.Info.ToString()
        };
    }
}