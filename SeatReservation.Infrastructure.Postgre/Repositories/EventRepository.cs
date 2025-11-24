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
        var @event = await _dbContext.Events
            .FromSql($"SELECT * FROM events WHERE id = {eventId.Value} FOR UPDATE")
            .Include(e => e.Details)
            .FirstOrDefaultAsync(cancellationToken);

        if (@event is null)
        {
            return Error.Failure("event.notfound", $"Events with id {eventId.Value} not found");
        }
        
        return @event;
    }
}