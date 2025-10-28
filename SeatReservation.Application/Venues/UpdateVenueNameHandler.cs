using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SeatReservation.Application.DataBase;
using SeatReservation.Contracts;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Application.Venues;

public class UpdateVenueNameHandler
{
    private readonly IVenuesRepository _repository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateVenueNameHandler> _logger;

    public UpdateVenueNameHandler(IVenuesRepository  repository, ITransactionManager transactionManager, ILogger<UpdateVenueNameHandler> logger)
    {
        _repository = repository;
        _transactionManager = transactionManager;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Error>> Handle(UpdateVenueNameRequest request, CancellationToken cancellationToken)
    {
        var venueId = new VenueId(request.Id);

        var (_, isFailure, venue, error) = await _repository.GetById(venueId, cancellationToken);
        if (isFailure)
        {
            return error;
        }

        venue.UpdateName(request.Name);

        await _transactionManager.SaveChangeAsync(cancellationToken);
        
        return venueId.Value;
    }
}