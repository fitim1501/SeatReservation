namespace SeatReservation.Domain.Events;

public record OnlineInfo(string Url) : IEventInfo;