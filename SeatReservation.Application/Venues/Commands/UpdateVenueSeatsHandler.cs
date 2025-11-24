using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SeatReservation.Application.DataBase;
using SeatReservation.Contracts.Venues;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Application.Venues.Commands;

public class UpdateVenueSeatsHandler
{
    private readonly IVenuesRepository _repository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateVenueSeatsHandler> _logger;

    public UpdateVenueSeatsHandler(IVenuesRepository  repository, ITransactionManager transactionManager, ILogger<UpdateVenueSeatsHandler> logger)
    {
        _repository = repository;
        _transactionManager = transactionManager;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Error>> Handle(UpdateVenueSeatsRequest request, CancellationToken cancellationToken)
    {
        var venueId = new VenueId(request.VenueId);

        var transactionScopeResult= await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }

        using var transactionScope = transactionScopeResult.Value;

        var venue = await _repository.GetById(venueId, cancellationToken);
        if (venue.IsFailure)
        {
            transactionScope.Rollback();
            return venue.Error;
        }
        
        List<Seat> seats = [];
        foreach (var seatRequest in request.Seats)
        {
            var seat = Seat.Create(venueId, seatRequest.RowNumber, seatRequest.SeatNumber);
            if (seat.IsFailure)
            {
                transactionScope.Rollback();
                return seat.Error;
            }
            
            seats.Add(seat.Value);
        }
        
        venue.Value.UpdateSeats(seats);
        
        await _repository.DeleteSeatsByVenuesId(venueId, cancellationToken);

        await _transactionManager.SaveChangeAsync(cancellationToken);

        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
        {
            return commitedResult.Error;
        }
        
        return venueId.Value;
    }
}