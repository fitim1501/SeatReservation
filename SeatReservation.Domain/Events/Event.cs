using SeatReservation.Domain.Venues;

namespace SeatReservation.Domain.Events;

public record EventId(Guid Value);

public class Event
{
    private readonly IEventInfo _info;

    public Event()
    {
        
    }
    public Event(EventId id, VenueId venueId, string name, DateTime eventDate, EventDetails details, IEventInfo info)
    {
        Info = info;
        Id = id;
        VenueId = venueId;
        Name = name;
        EventDate = eventDate;
        Details = details;
    }
    public EventId Id { get; } 
    
    public EventDetails Details { get; private set; }
    public VenueId VenueId { get; private set; }
    
    public string Name { get; private set; }
    
    public EventType Type { get; private set; }
    
    public DateTime EventDate { get; private set; }
    
    public IEventInfo Info { get; set; } 
}

public enum EventType
{
    Concert,
    Conference,
    Online
}

public interface IEventInfo {}



public record ConcertInfo(string Performer) : IEventInfo;

public record ConferenceInfo(string Speaker, string Topic) : IEventInfo;

public record OnlineInfo(string Url) : IEventInfo;