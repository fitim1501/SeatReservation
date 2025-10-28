namespace SeatReservation.Domain.Events;

public record ConcertInfo(string Performer) : IEventInfo;