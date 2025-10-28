namespace SeatReservation.Domain.Events;

public record ConferenceInfo(string Speaker, string Topic) : IEventInfo;