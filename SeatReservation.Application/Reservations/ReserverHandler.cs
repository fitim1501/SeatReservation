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
    private readonly IReservationRepository _reservationRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ISeatsRepository _seatsRepository;

    public ReserverHandler(IReservationRepository reservationRepository, IEventRepository eventRepository, ISeatsRepository seatsRepository)
    {
        _reservationRepository = reservationRepository;
        _eventRepository = eventRepository;
        _seatsRepository = seatsRepository;
    }
    public async Task<Result<Guid, Error>> Handle(ReserveRequest request, CancellationToken cancellationToken)
    {
        //Бронирование мест на мероприятии
        
        //1. валидация входных параметров
        
        //2. доступно ли мероприятие для бронирования. Проверить даты. Проверить статус мероприятия.

        var eventId = new EventId(request.EventId);
        var (_, isFailure, @event, error) = await _eventRepository.GetById(eventId, cancellationToken);
        if (isFailure)
        {
            return error;
        }

        if (@event.IsAvailableForReservation() == false)
        {
            return Error.Failure("reservation.unavailable", "Reservation is unavailable");
        }
        
        //3. проверить что места принадлежат мероприятию и площадке
        var seatIds = request.SeatIds.Select(s => new SeatId(s)).ToList();
        var seats = await _seatsRepository.GetByIds(seatIds, cancellationToken);

        if (seats.Any(s => s.VenueId != @event.VenueId) || seats.Count == 0)
        {
            return Error.Conflict("seat.conflict", $"Seat  does not belong to the venue");
        }
        
        //4. проверить что места не забронированы
        var isSeatsReserved = await _reservationRepository.AnySeatsAlreadyReserved(request.EventId, seatIds, cancellationToken);
        if (isSeatsReserved)
        {
            return Error.Conflict("seat.reserved", "One or more seats are already reserved");
        }
        
        //Создать Reservation с ReservedSeats
        var reservationResult = Reservation.Create(request.EventId, request.UserId, request.SeatIds);
        if (reservationResult.IsFailure)
        {
            return reservationResult.Error;
        }
        
        //Сохранить Reservation в базу данных
        await _reservationRepository.Add(reservationResult.Value, cancellationToken);

        return reservationResult.Value.Id.Value;
    }
}