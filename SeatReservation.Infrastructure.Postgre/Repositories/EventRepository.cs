using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeatReservation.Application.Events;
using SeatReservation.Domain.Events;
using Shared;
using EventId = SeatReservation.Domain.Events.EventId;


namespace SeatReservation.Infrastructure.Postgre.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ReservationServiceDbContext _dbContext;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(ReservationServiceDbContext dbContext, ILogger<EventRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Result<Event, Error>> GetById(EventId eventId, CancellationToken cancellationToken)
    {
        var @event = await _dbContext.Event.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
        
        if (@event is null)
        {
            return Error.NotFound("event.not.found", "Event not found");
        }

        return @event;
    }   
    public async Task<Result<Event, Error>> GetAvailableForReservationById(EventId eventId, CancellationToken cancellationToken)
    {
        var @event = await _dbContext.Event.FirstOrDefaultAsync(e => 
            e.Id == eventId && e.StartDate > DateTime.UtcNow && e.Status == EventStatus.Planned, cancellationToken);
        
        if (@event is null)
        {
            return Error.NotFound("event.not.found", "Event not found");
        }

        return @event;
    }
}