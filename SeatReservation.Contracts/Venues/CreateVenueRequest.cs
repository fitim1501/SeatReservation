namespace SeatReservation.Contracts.Venues;

public record CreateVenueRequest(string Name, string Prefix, int SeatsLimit, IEnumerable<CreateSeatRequest> Seats);