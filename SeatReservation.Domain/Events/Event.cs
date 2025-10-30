using CSharpFunctionalExtensions;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Domain.Events;

public record EventId(Guid Value);

public class Event
{
    private readonly IEventInfo _info;

    public Event()
    {
        
    }
    public Event(EventId id, VenueId venueId, string name, DateTime eventDate, DateTime startDate, DateTime endDate, EventDetails details, IEventInfo info)
    {
        Info = info;
        Id = id;
        VenueId = venueId;
        Name = name;
        EventDate = eventDate;
        Details = details;
        StartDate = startDate;
        EndDate = endDate;
        Status = EventStatus.Planned;
    }
    public EventId Id { get; } 
    
    public EventDetails Details { get; private set; }
    public VenueId VenueId { get; private set; }
    
    public string Name { get; private set; }
    
    public EventType Type { get; private set; }
    
    public DateTime EventDate { get; private set; }
    
    public DateTime StartDate { get; private set; }
    
    public DateTime EndDate { get; private set; }
    
    public EventStatus Status { get; private set; }
    
    public IEventInfo Info { get; set; } 
    
    public bool IsAvailableForReservation(int capacitySum) 
        => Status == EventStatus.Planned && StartDate > DateTime.UtcNow && capacitySum <= Details.Capacity;
    
    private static Result<EventDetails, Error> Validate(
        string name, 
        DateTime eventDate,
        DateTime startDate,
        DateTime endDate,
        int capacity, 
        string description)
    {
        if (startDate >= endDate || startDate <= DateTime.UtcNow || endDate <= DateTime.UtcNow)
        {
            return Error.Validation("event.time", "Event start date must be before end date and both must be in the future");
        }
            
        if (string.IsNullOrWhiteSpace(name))
        {
            return Error.Validation("event.name", "Event name cannot be empty");
        }
        
        if (eventDate <= DateTime.UtcNow)
        {
            return Error.Validation("event.eventDate", "Event date must be in the future");
        }
        
        if (capacity <= 0)
        {
            return Error.Validation("event.capacity", "Event capacity must be greater than zero");
        }
        
        if (string.IsNullOrWhiteSpace(description))
        {
            return Error.Validation("event.description", "Event description cannot be empty");
        }

        var details = new EventDetails(capacity, description);
        
        return details;
    }
    
    public static Result<Event, Error> CreateConcert(
        VenueId venueId,
        string name,
        DateTime eventDate,
        DateTime startDate,
        DateTime endDate,
        int capacity,
        string description,
        string performer)
    {
        var detailsResult = Validate(name, eventDate, startDate, endDate, capacity, description);
        if (detailsResult.IsFailure)
        {
            return detailsResult.Error;
        }

        if (string.IsNullOrWhiteSpace(performer))
        {
            return Error.Validation("event.performer", "Performer cannot be empty");
        }
        
        var concertInfo = new ConcertInfo(performer);
        
        return new Event(new EventId(Guid.NewGuid()), venueId, name, eventDate, startDate, endDate, detailsResult.Value, concertInfo);
    }
    
    public static Result<Event, Error> CreateConference(
        VenueId venueId,
        string name,
        DateTime eventDate,
        DateTime startDate,
        DateTime endDate,
        int capacity,
        string description,
        string speaker,
        string topic)
    {
        var detailsResult = Validate(name, eventDate, startDate, endDate, capacity, description);
        if (detailsResult.IsFailure)
        {
            return detailsResult.Error;
        }

        if (string.IsNullOrWhiteSpace(speaker))
        {
            return Error.Validation("event.speaker", "Speaker cannot be empty");
        }
        
        if (string.IsNullOrWhiteSpace(topic))
        {
            return Error.Validation("event.topic", "Topic cannot be empty");
        }
        
        var conferenceInfo = new ConferenceInfo(speaker, topic);
        
        return new Event(new EventId(Guid.NewGuid()), venueId, name, eventDate, startDate, endDate, detailsResult.Value, conferenceInfo);
    }
    
    public static Result<Event, Error> CreateOnline(
        VenueId venueId,
        string name,
        DateTime eventDate,
        DateTime startDate,
        DateTime endDate,
        int capacity,
        string description,
        string url)
    {
        var detailsResult = Validate(name, eventDate, startDate, endDate, capacity, description);
        if (detailsResult.IsFailure)
        {
            return detailsResult.Error;
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return Error.Validation("event.url", "URL cannot be empty");
        }
        
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return Error.Validation("event.url", "URL is not valid");
        }
        
        var onlineInfo = new OnlineInfo(url);
        
        return new Event(new EventId(Guid.NewGuid()), venueId, name, eventDate, startDate, endDate, detailsResult.Value, onlineInfo);
    }
}

public enum EventStatus
{
    Planned,
    InProgress,
    Finished,
    Cancelled
}