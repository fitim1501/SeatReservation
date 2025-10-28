using CSharpFunctionalExtensions;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Application.Venues;

public interface IVenuesRepository
{
    Task<Result<Guid, Error>> Add(Venue venue, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Venue>> GetByPrefix(string prefix, CancellationToken cancellationToken);
    
    Task<Result<Guid, Error>> UpdateVenueName(VenueId venueId, VenueName name, CancellationToken cancellationToken);

    Task<UnitResult<Error>> UpdateVenueNameByPrefix(string prefix, VenueName venueName,
        CancellationToken cancellationToken);

    Task<Result<Venue, Error>> GetById(VenueId venueId, CancellationToken cancellationToken);

    Task<Result<Venue, Error>> GetByIdWithSeats(VenueId venueId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeleteSeatsByVenuesId(VenueId venueId, CancellationToken cancellationToken);
}