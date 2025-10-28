using CSharpFunctionalExtensions;
using Shared;

namespace SeatReservation.Domain.Venues;

public record SeatId(Guid Value);
public class Seat
{
    //EF Core
    private Seat()
    {
        
    }
    private Seat(SeatId id, Venue venue, int rowNumber, int seatNumber)
    {
        Id = id;
        Venue = venue;
        RowNumber = rowNumber;
        SeatNumber = seatNumber;
    }
    
    private Seat(SeatId id, VenueId venue, int rowNumber, int seatNumber)
    {
        Id = id;
        VenueId = venue;
        RowNumber = rowNumber;
        SeatNumber = seatNumber;
    }
    
    public SeatId Id { get; }

    public Venue Venue { get; private set; } = null!;
    public VenueId VenueId { get; private set; } = null!;

    public int RowNumber { get; private set; } 
    public int SeatNumber { get; private set; }
    
    
    public static Result<Seat, Error> Create(VenueId venueId, int rowNumber, int seatNumber)
    {
        if (rowNumber <= 0 || seatNumber <= 0)
        {
            return Error.Validation("seat.rowNumber", "Row number and seat number must be greater than zero");
        }
        
        return new Seat(new SeatId(Guid.NewGuid()), venueId, rowNumber, seatNumber);
    } 
    
    public static Result<Seat, Error> Create(Venue venue, int rowNumber, int seatNumber)
    {
        if (rowNumber <= 0 || seatNumber <= 0)
        {
            return Error.Validation("seat.rowNumber", "Row number and seat number must be greater than zero");
        }
        
        return new Seat(new SeatId(Guid.NewGuid()), venue, rowNumber, seatNumber);
    }
}