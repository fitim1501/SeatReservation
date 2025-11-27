using System.Data;
using Dapper;
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
            .OrderByDescending(e => e.EventDate);
        
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

public class GetEventsHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetEventsHandlerDapper(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GetEventsDto> Handle(GetEventsRequest query, CancellationToken cancellationToken)
    {
        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        
        // parameters.Add("search", query.Search, DbType.String);
        // parameters.Add("type", query.EventType, DbType.String);
        // parameters.Add("date_from", query.DateFrom?.ToUniversalTime(), DbType.DateTime);
        // parameters.Add("date_to", query.DateTo?.ToUniversalTime(), DbType.DateTime);
        // parameters.Add("status", query.Status, DbType.String);
        // parameters.Add("venue_id", query.VenueId, DbType.Guid);
        // parameters.Add("min_available_seats", query.MinAvailableSeats, DbType.Int32);
        //
        // parameters.Add("offset", (query.Pagination.Page - 1) * query.Pagination.PageSize, DbType.Int32);
        // parameters.Add("page_size", query.Pagination.PageSize, DbType.Int32);

        var conditions = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            conditions.Add("e.name ILIKE @search");
            parameters.Add("search", $"%{query.Search}%");
        }
        
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            conditions.Add("e.status = @status");
            parameters.Add("status", query.Status);
        }
        
        if (!string.IsNullOrWhiteSpace(query.EventType))
        {
            conditions.Add("e.type = @event_type");
            parameters.Add("event_type", query.EventType);
        }
        
        if (query.DateFrom.HasValue)
        {
            conditions.Add("e.event_date >= @date_from");
            parameters.Add("date_from", query.DateFrom?.ToUniversalTime());
        }
        
        if (query.DateTo.HasValue)
        {
            conditions.Add("e.event_date <= @date_to");
            parameters.Add("date_to", query.DateTo?.ToUniversalTime());
        }
        
        if (query.VenueId.HasValue)
        {
            conditions.Add("e.venue_id = @venue_id");
            parameters.Add("venue_id", query.VenueId);
        }
        
        if (query.MinAvailableSeats.HasValue)
        {
            conditions.Add(
                """
                    ((select count(*) from seats s where s.venue_id = e.venue_id) -
                        coalesce((select count(*)
                                  from reservation_seats rs
                                           join reservations r on rs.reservation_id = r.id
                                  where r.event_id = e.id
                                    and r.status in ('Confirmed', 'Pending')), 0)) >= @min_available_seats
                """);
            parameters.Add("min_available_seats", query.MinAvailableSeats);
        }
        
        parameters.Add("offset", (query.Pagination.Page - 1) * query.Pagination.PageSize);
        parameters.Add("page_size", query.Pagination.PageSize);
        
        var whereClause = conditions.Count > 0
            ? "WHERE " + string.Join(" AND ", conditions)
            : string.Empty;
        
        long? totalCount = null;

        var events = await connection.QueryAsync<EventDto, long, EventDto>(
            $"""
             SELECT e.id,
                    e.venue_id,
                    e.name,
                    e.type,
                    e.event_date,
                    e.start_date,
                    e.end_date,
                    e.status,
                    e."Info",
                    ed.capacity,
                    ed.description,
                    (select count(*)
                     from seats s
                     where s.venue_id = e.venue_id)                                                as total_seats,
                    (select count(*)
                     from reservation_seats rs
                              join reservations r on rs.reservation_id = r.id
                     where r."event_id" = e.id
                       and r.status in ('Confirmed', 'Pending'))                                   as reserved_count,

                    (select count(*)
                     from seats s
                     where s.venue_id = e.venue_id) - (select count(*)
                                                       from reservation_seats rs
                                                                join reservations r on rs.reservation_id = r.id
                                                       where r.event_id = e.id
                                                         and r.status in ('Confirmed', 'Pending')) as available_seats,
                     count(*) over() as total_count

             FROM events e
                      JOIN event_details ed ON ed.event_id = e.id
             {whereClause}
             ORDER BY e.event_date DESC
             LIMIT @page_size OFFSET @offset;
             """,
            splitOn: "total_count",
                map: (e, total) =>
                {
                    if (totalCount == null)
                    {
                        totalCount = total;
                    }
    
                    return e;
                },
            param: parameters);

        return new  GetEventsDto(events.ToList(), totalCount ?? 0);
    }
}