using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SeatReservation.Contracts.Venues;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Application.Venues.Commands;

public class UpdateVenueNameByPrefixHandler
{
    private readonly IVenuesRepository _repository;
    private readonly ILogger<UpdateVenueNameHandler> _logger;

    public UpdateVenueNameByPrefixHandler(IVenuesRepository  repository, ILogger<UpdateVenueNameHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<UnitResult<Error>> Handle(UpdateVenueNameByPrefixRequest request, CancellationToken cancellationToken)
    {
        var venueName = VenueName.CreateWithoutPerfix(request.Name);
        if (venueName.IsFailure)
        {
            return venueName.Error;
        }
        
        var result = await _repository.UpdateVenueNameByPrefix(request.Prefix, venueName.Value, cancellationToken);
        if (result.IsFailure)
        {
            return result.Error;
        }

        return UnitResult.Success<Error>();
    }
}