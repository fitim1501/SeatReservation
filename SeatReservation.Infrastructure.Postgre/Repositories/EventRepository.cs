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
    
    public async Task<Result<Event, Error>> GetByIdWithLock(EventId eventId, CancellationToken cancellationToken)
    {
        var @event = await _dbContext.Event
            .FromSql($"SELECT * FROM events WHERE id = {eventId.Value} FOR UPDATE")
            .Include(e => e.Details)
            .FirstOrDefaultAsync(cancellationToken);

        if (@event is null)
        {
            return Error.Failure("event.notfound", $"Event with id {eventId.Value} not found");
        }
        
        return @event;
    }
    
    public async Task<Event?> GetById(EventId eventId, CancellationToken cancellationToken)
    {
        return await _dbContext.Event
            .Include(e => e.Details)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
    }
}