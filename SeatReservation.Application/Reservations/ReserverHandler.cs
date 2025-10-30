using System.Data;
using CSharpFunctionalExtensions;
using SeatReservation.Application.DataBase;
using SeatReservation.Application.Events;
using SeatReservation.Application.Seats;
using SeatReservation.Contracts;
using SeatReservation.Domain.Events;
using SeatReservation.Domain.Reservations;
using SeatReservation.Domain.Venues;
using Shared;

namespace SeatReservation.Application.Reservations;

public class ReserverHandler
{
    private readonly IReservationsRepository _reservationsRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ISeatsRepository _seatsRepository;
    private readonly ITransactionManager _transactionManager;

    public ReserverHandler(IReservationsRepository reservationsRepository, IEventRepository eventRepository, ISeatsRepository seatsRepository, ITransactionManager transactionManager)
    {
        _reservationsRepository = reservationsRepository;
        _eventRepository = eventRepository;
        _seatsRepository = seatsRepository;
        _transactionManager = transactionManager;
    }
    public async Task<Result<Guid, Error>> Handle(ReserveRequest request, CancellationToken cancellationToken)
    {
        //Бронирование мест на мероприятии
        
        //1. валидация входных параметров
        
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error;
        }   
        
        using var transactionScope = transactionScopeResult.Value;
        
        //2. доступно ли мероприятие для бронирования. Проверить даты. Проверить статус мероприятия.

        var eventId = new EventId(request.EventId);
        var (_, isFailure, @event, error) = await _eventRepository.GetByIdWithLock(eventId, cancellationToken);
        if (isFailure)
        {
            transactionScope.Rollback();
            return error;
        }
        
        var reservedSeatsCount = await _reservationsRepository.GetReservedSeatsCount(request.EventId, cancellationToken);
        
        if (@event.IsAvailableForReservation(reservedSeatsCount + request.SeatIds.Count()) == false)
        {
            transactionScope.Rollback();
            return Error.Failure("reservation.unavailable", "Reservation is unavailable");
        }
        
        //3. проверить что места принадлежат мероприятию и площадке
        var seatIds = request.SeatIds.Select(s => new SeatId(s)).ToList();
        var seats = await _seatsRepository.GetByIds(seatIds, cancellationToken);

        if (seats.Any(s => s.VenueId != @event.VenueId) || seats.Count == 0)
        {
            transactionScope.Rollback();
            return Error.Conflict("seat.conflict", $"Seat  does not belong to the venue");
        }
        
        //Создать Reservation с ReservedSeats
        var reservationResult = Reservation.Create(request.EventId, request.UserId, request.SeatIds);
        if (reservationResult.IsFailure)
        {
            return reservationResult.Error;
        }

        //Сохранить Reservation в базу данных
        var insertResult = await _reservationsRepository.Add(reservationResult.Value, cancellationToken);
        if (insertResult.IsFailure)
        {
            transactionScope.Rollback();
            return insertResult.Error;
        }
        
        @event.Details.ReserveSeat();

        var saveREsult = await _transactionManager.SaveChangeAsync(cancellationToken);
        if (saveREsult.IsFailure)
        {
            transactionScope.Rollback();
            return saveREsult.Error;
        }
        
        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            transactionScope.Rollback();
            return commitResult.Error;
        }

        return reservationResult.Value.Id.Value;
    }
}