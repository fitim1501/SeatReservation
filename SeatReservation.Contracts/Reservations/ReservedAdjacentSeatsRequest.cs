namespace SeatReservation.Contracts.Reservations;

public record ReservedAdjacentSeatsRequest(
    Guid EventId,
    Guid UserId,
    Guid VenueId,
    int RequiredSeatsCount,
    int? PreferredRowNumber = null);