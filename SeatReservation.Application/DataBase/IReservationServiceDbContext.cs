using CSharpFunctionalExtensions;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Application.DataBase;

public interface IVenuesRepository
{
    Task<Result<Guid, Error>> Add(Venue venue, CancellationToken cancellationToken = default);
    
    Task<Result<Guid, Error>> UpdateVenueName(VenueId venueId, VenueName name, CancellationToken cancellationToken);

    Task Save();

    Task<UnitResult<Error>> UpdateVenueNameByPrefix(string prefix, VenueName venueName,
        CancellationToken cancellationToken);

    Task<Result<Venue, Error>> GeyById(VenueId venueId, CancellationToken cancellationToken);
}