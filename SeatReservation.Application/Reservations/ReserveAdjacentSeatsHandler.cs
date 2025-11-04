using CSharpFunctionalExtensions;
using SeatReservation.Application.DataBase;
using SeatReservation.Application.Events;
using SeatReservation.Application.Seats;
using SeatReservation.Contracts;
using SeatReservation.Contracts.Reservations;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Application.Reservations;

public class ReserveAdjacentSeatsHandler
{
    private readonly IReservationsRepository _reservationsRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ISeatsRepository _seatsRepository;
    private readonly ITransactionManager _transactionManager;

    public ReserveAdjacentSeatsHandler(IReservationsRepository reservationsRepository, IEventRepository eventRepository, ISeatsRepository seatsRepository, ITransactionManager transactionManager)
    {
        _reservationsRepository = reservationsRepository;
        _eventRepository = eventRepository;
        _seatsRepository = seatsRepository;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Error>> Handle(ReservedAdjacentSeatsRequest request, CancellationToken cancellationToken)
    {
        if (request.RequiredSeatsCount <= 0)
        {
            return Error.Validation("reserveAdjacent.seatsCount", "Required seats count must be greater than zero");
        }

        if (request.PreferredRowNumber is > 10)
        {
            return Error.Validation("reserveAdjacent.seatsCount", "Cannot reserve more than 10 adjacent seats at once");
        }
        
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }
        
        using var transactionScope = transactionScopeResult.Value;
        
        var (_, isFailure, @event, error) = await _eventRepository.GetByIdWithLock(new Domain.Events.EventId(request.EventId), cancellationToken);
        if (isFailure)
        {
            transactionScope.Rollback();
            return error;
        }
        
        var availableSeats = await _seatsRepository.GetAvailableSeats(
            new VenueId(request.VenueId), 
            new Domain.Events.EventId(request.EventId), 
            request.PreferredRowNumber, 
            cancellationToken);

        if (availableSeats.Count == 0)
        {
            return Error.NotFound("reserveAdjacent.seats", "No available seats found");
        }

        var selectedSeats = request.PreferredRowNumber.HasValue
            ? AdjacentSeatFinder.FindAdjacentSeats(availableSeats, request.RequiredSeatsCount,
                request.PreferredRowNumber.Value)
            : AdjacentSeatFinder.FindBestAdjacentSeats(availableSeats, request.RequiredSeatsCount);

        if (selectedSeats.Count == 0)
        {
            return Error.NotFound("reserveAdjacent.seats", "No available seats found");
        }
        
        if (selectedSeats.Count < request.RequiredSeatsCount)
        {
            return Error.Failure("reserveAdjacent.seats", "Not enough adjacent seats available");
        }
        
        var seatIds = selectedSeats.Select(s => s.Id).ToList();
        
        var reservationResult = Reservation.Create(
            request.EventId,
            request.UserId,
            seatIds.Select(id => id.Value).ToList());

        if (reservationResult.IsFailure)
        {
            return reservationResult.Error;
        }
        
        var reservation = reservationResult.Value;
        
        var addResult = await _reservationsRepository.Add(reservation, cancellationToken);

        if (addResult.IsFailure)
        {
            return addResult.Error;
        }
        
        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            return commitResult.Error;
        }

        return addResult.Value;
    }
}