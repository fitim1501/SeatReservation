namespace SeatReservation.Contracts;

public record ReservedAdjacentSeatsRequest(
    Guid EventId,
    Guid UserId,
    Guid VenueId,
    int RequiredSeatsCount,
    int? PreferredRowNumber = null);