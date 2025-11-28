using Dapper;
using SeatReservation.Application.DataBase;
using SeatReservation.Contracts.Events;

namespace SeatReservation.Application.Events.Queries;

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

        var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;

        var direction = query.SortDirection?.ToLower() == "asc" ? "asc" : "desc";

        var orderByField = query.SortBy?.ToLower() switch
        {
            "date" => "event_date",
            "name" => "name",
            "status" => "status",
            "type" => "type",
            "popularity" => "popularity_percentage",
            
            _=> "event_date"
        };

        var orderByClause = $"ORDER BY {orderByField} {direction}";

        long? totalCount = null;

        var events = await connection.QueryAsync<EventDto, long, EventDto>(
            $"""
             with event_stats as (SELECT e.id,
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
                                         count(*) over() as total_count
             
                                  FROM events e
                                           JOIN event_details ed ON ed.event_id = e.id
                                           {whereClause})
             select id,
                    venue_id,
                    name,
                    type,
                    event_date,
                    start_date,
                    end_date,
                    status,
                    "Info",
                    capacity,
                    description, 
                    total_seats,
                    reserved_count,
                    total_seats - reserved_count                         as available_seats, 
                    round(reserved_count::decimal / total_count * 100,2) as popularity_percentage,
                    total_count
             from event_stats
             {orderByClause}
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

        return new GetEventsDto(events.ToList(), totalCount ?? 0);
    }
}