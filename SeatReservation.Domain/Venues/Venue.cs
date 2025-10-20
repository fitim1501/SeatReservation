using CSharpFunctionalExtensions;
using Shared;

namespace SeatReservation.Domain.Venues;

public record VenueId(Guid Value);

public class Venue
{
    private List<Seat> _seats = [];
    
    //EF Core
    private Venue()
    {
        
    }

    public Venue(VenueId id, VenueName name, int seatsLimit, IEnumerable<Seat> seats)
    {
        Id = id;
        Name = name;
        this.SeatsLimit = seatsLimit;
        _seats = seats.ToList();
    }

    public VenueId Id { get; } = null!;
    
    public VenueName Name { get; private set; }
   
    public int SeatsLimit { get; private set; }
    public int SeatsCount => _seats.Count();
    
    public string Test2 { get; set; }

    public IReadOnlyList<Seat> Seats => _seats;
    
    public UnitResult<Error> AddSeat(Seat seat)
    {
        if (SeatsCount >= SeatsLimit)
        {
            return Error.Conflict("venue.seats.limit", "");
        }
        
        _seats.Add(seat);

        return UnitResult.Success<Error>();
    }
    
    public void ExpandSeatsLimit(int newSeatsLimit) => SeatsLimit = newSeatsLimit;

    public static Result<Venue, Error> Create(
        string prefix,
        string name,
        int seatsLimit,
        IEnumerable<Seat> seats
        )
    {
        if (seatsLimit <= 0)
        {
            return Error.Validation("venue.seats.limit", "Seats limit must be greater than zero");
        }
        
        var venueNameResult = VenueName.Create(prefix, name);
        if (venueNameResult.IsFailure)
        {
            return venueNameResult.Error;
        }

        var venueSeats = seats.ToList();

        if (venueSeats.Count < 1)
        {
            return Error.Validation("venue.seats.limit", "Seats limit must be greater than or equal to the number of seats");
        }
        
        if (venueSeats.Count > seatsLimit)
        {
            return Error.Validation("venue.seats.limit", "Seats limit must be greater than or equal to the number of seats");
        }
        
        return new Venue(new VenueId(Guid.NewGuid()), venueNameResult.Value, seatsLimit, venueSeats);
    }
}