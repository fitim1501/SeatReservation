namespace SeatReservation.Domain.Events;

public record ConcertInfo(string Performer) : IEventInfo
{
    public override string ToString() => $"Concert by {Performer}";
    
}