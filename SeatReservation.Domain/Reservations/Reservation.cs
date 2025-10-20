using SeatReservation.Domain.Venues;

namespace SeatReservation.Domain.Reservations;

public record ReservationId(Guid Value);

public class Reservation
{
    //EF Core
    private Reservation()
    {
        
    }
    
    private List<ReservationSeat> _reservedSeats;
    
    public Reservation(ReservationId id, IEnumerable<Guid> seatIds, Guid eventId, Guid userId)
    {
        Id = id;
        EventId = eventId;
        UserId = userId;
        Status = ReservationStatus.Pending;
        CreateAt = DateTime.UtcNow;

        var reservedSeats = seatIds
            .Select(seatId => new ReservationSeat(new ReservationSeatId(Guid.NewGuid()), this, new SeatId(seatId)))
            .ToList();

        _reservedSeats = reservedSeats;
    }
    
    public ReservationId Id { get; private set; }
    
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public ReservationStatus Status { get; private set; }
    
    public DateTime CreateAt { get; private set; }

    public IReadOnlyList<ReservationSeat> ReservedSeats => _reservedSeats;
}