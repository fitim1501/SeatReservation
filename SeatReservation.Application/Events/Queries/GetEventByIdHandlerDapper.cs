using Dapper;
using SeatReservation.Application.DataBase;
using SeatReservation.Contracts.Events;
using SeatReservation.Contracts.Seats;

namespace SeatReservation.Application.Events.Queries;

public class GetEventByIdHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetEventByIdHandlerDapper(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<GetEventDto?> Handle(GeEventByIdRequest query, CancellationToken cancellationToken)
    {
        var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        GetEventDto? eventDto = null;

        await connection.QueryAsync<GetEventDto, AvailableSeatDto, GetEventDto>(
            """
            SELECT  e.id,
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
                    e.id,
                    e.venue_id,
                    s.row_number,
                    s.seat_number,
                    rs.id is null as is_available
            FROM events e
            JOIN event_details ed ON ed.event_id = e.id
            JOIN seats s ON e.venue_id = s.venue_id
            LEFT JOIN reservation_seats rs ON s.id = seat_id AND e.id = e.id
            WHERE e.id = @eventId
            ORDER BY s.row_number, s.seat_number;
            """,
            param: new
            {
                eventId = query.EventId 
            },
            splitOn: "id",
            map: (e, s) =>
            {
                if (eventDto is null)
                {
                    eventDto = e;
                }
                
                eventDto.Seats.Add(s);

                return eventDto;
            });

        return eventDto;
    }
}