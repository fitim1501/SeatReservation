using Microsoft.EntityFrameworkCore;
using SeatReservation.Application.DataBase;
using SeatReservation.Contracts.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;

namespace SeatReservation.Application.Events.Queries;

public class GetEventsHandler
{
    private readonly IReadDbContext _readDbContext;

    public GetEventsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetEventsDto> Handle(GetEventsRequest query, CancellationToken cancellationToken)
    {
        var eventsQuery = _readDbContext.EventsRead;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            eventsQuery = eventsQuery.Where(e => EF.Functions.Like(e.Name.ToLower(), $"%{query.Search.ToLower()}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.EventType))
            eventsQuery = eventsQuery.Where(e => e.Type.ToString().ToLower() == query.EventType.ToLower());
        
        if (query.DateFrom.HasValue)
            eventsQuery = eventsQuery.Where(e => e.EventDate >= query.DateFrom.Value.ToUniversalTime());
        
        if (query.DateTo.HasValue)
            eventsQuery = eventsQuery.Where(e => e.EventDate <= query.DateTo.Value.ToUniversalTime());
        
        if (!string.IsNullOrWhiteSpace(query.Status))
            eventsQuery = eventsQuery.Where(e => e.Status.ToString().ToLower() == query.Status.ToLower());

        if (query.VenueId.HasValue)
        {
            eventsQuery = eventsQuery.Where(e => e.VenueId == new VenueId(query.VenueId.Value));
        }

        if (query.MinAvailableSeats.HasValue)
        {
            eventsQuery = eventsQuery.Where(e =>
                _readDbContext.SeatsRead.Count(s => s.VenueId == e.VenueId) - 
                _readDbContext.ReservationSeatsRead.Count(rs => 
                    rs.EventId == e.Id &&
                    (rs.Reservation.Status == ReservationStatus.Confirmed ||
                     rs.Reservation.Status == ReservationStatus.Cancelled))
                >= query.MinAvailableSeats.Value);
        }
        

        eventsQuery = eventsQuery
            .OrderBy(e => e.EventDate);
        
        var totalCount = await eventsQuery.LongCountAsync(cancellationToken);
            
        eventsQuery = eventsQuery
            .Skip((query.Pagination.Page - 1) * query.Pagination.PageSize)
            .Take( query.Pagination.PageSize);

        var events = await eventsQuery
            .Select(e => new EventDto
            {
                Id = e.Id.Value,
                Capacity = e.Details.Capacity,
                Description = e.Details.Description,
                EndDate = e.EndDate,
                EventDate = e.EventDate,
                Name = e.Name,
                StartDate = e.StartDate,
                Type = e.Type.ToString(),
                VenueId = e.VenueId.Value,
                Status = e.Status.ToString(),
                Info = e.Info.ToString(),
                TotalSeats = _readDbContext.SeatsRead.Count(s => s.VenueId == e.VenueId),
                ReservedSeats = _readDbContext.ReservationSeatsRead.Count(rs => rs.EventId == e.Id),
                AvailableSeats = _readDbContext.SeatsRead.Count(s => s.VenueId == e.VenueId) - 
                                 _readDbContext.ReservationSeatsRead.Count(rs => rs.EventId == e.Id  &&
                                     (rs.Reservation.Status == ReservationStatus.Confirmed ||
                                      rs.Reservation.Status == ReservationStatus.Cancelled))
            })
            .ToListAsync(cancellationToken);

        return new GetEventsDto(events, totalCount);
    }
}