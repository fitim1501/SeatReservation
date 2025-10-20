namespace SeatReservation.Application;

public record CreateVenueRequest(string Name, string Prefix, int SeatsLimit, IEnumerable<CreateSeatRequest> Seats);