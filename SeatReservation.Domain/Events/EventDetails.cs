namespace SeatReservation.Domain.Events;

public class EventDetails
{
    private EventDetails()
    {
        
    }
    
    public EventDetails(int capacity, string description)
    {
        Capacity = capacity;
        Description = description;
    }

    public EventId EventId { get; } = null!;
    
    public int Capacity { get; private set; }
    
    public string Description { get; private set; } 
    
    public DateTime? LastReservationUtc { get; private set; }
    
    public uint Version { get; private set; }

    public void ReserveSeat()
    {
        LastReservationUtc = DateTime.UtcNow;
    }
}